using KVKarco.ValidationAssistant.Exceptions;
using KVKarco.ValidationAssistant.Internal.PropertyValidation;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace KVKarco.ValidationAssistant.Internal.Utilities;

/// <summary>
/// Provides static factory methods for creating and manipulating expression trees,
/// primarily focused on extracting property information and generating null-safe
/// property access expressions suitable for dynamic value resolution in validation contexts.
/// This class is central to converting user-defined property selectors into executable logic.
/// </summary>
internal static class ExpressionFactory
{
    /// <summary>Represents a constant expression for <see langword="null"/>.</summary>
    private static readonly ConstantExpression _forNull = Expression.Constant(null);
    /// <summary>Represents a constant expression for <see langword="true"/>.</summary>
    private static readonly ConstantExpression _forTrue = Expression.Constant(true);
    /// <summary>Represents a constant expression for <see langword="false"/>.</summary>
    private static readonly ConstantExpression _forFalse = Expression.Constant(false);

    /// <summary>
    /// Analyzes a lambda expression representing a property selection and extracts
    /// the <see cref="MemberInfo"/> of the deepest (last) member in the chain,
    /// along with the total count of members in the access path. This is useful
    /// for identifying the target property and understanding the depth of the expression.
    /// </summary>
    /// <param name="lambdaExpression">The lambda expression to analyze, e.g., <c>x => x.Prop1.Prop2</c>.</param>
    /// <returns>
    /// A tuple containing:
    /// <list type="bullet">
    ///     <item><term><c>info</c></term><description>The <see cref="MemberInfo"/> of the last property/field in the chain.</description></item>
    ///     <item><term><c>count</c></term><description>The total number of members in the property access chain.</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="RuleCreationException">
    /// Thrown if any part of the expression body is not a valid member or property access expression.
    /// </exception>
    public static (MemberInfo info, int count) GetLastMemberAndMembersCount(LambdaExpression lambdaExpression)
    {
        MemberExpression? member = lambdaExpression.Body as MemberExpression;
        MemberInfo memberInfo = null!; // Will be assigned in the loop
        int count = 0;

        // Traverse the member expression chain from the deepest member upwards
        while (member is not null)
        {
            memberInfo = member.Member; // Keep track of the current member's info
            RuleCreationException.ThrowIfNotMemberOrPropertyExpression(member); // Validate it's a member access

            member = member.Expression as MemberExpression; // Move up to the parent expression
            count++; // Increment count for each member
        }

        return (memberInfo, count); // Return the last member's info and the total count
    }

    /// <summary>
    /// Creates a <see cref="PropertyCtx{T, TProperty}"/> instance from a given property selector expression.
    /// This method analyzes the expression, builds a null-safe access path, compiles a
    /// <see cref="PropertyValueResolver{T, TProperty}"/> delegate, and constructs a <see cref="PropertyKey"/>
    /// to fully describe the target property for validation.
    /// </summary>
    /// <typeparam name="T">The type of the main instance being validated.</typeparam>
    /// <typeparam name="TProperty">The type of the property being selected.</typeparam>
    /// <param name="propertySelector">The expression selecting the property (e.g., <c>x => x.User.Address.Street</c>).</param>
    /// <param name="removeStartName">
    /// If <see langword="true"/>, the name of the first member in the property path (e.g., "User" in "User.Address.Street")
    /// will be removed from the resulting <see cref="PropertyKey.PropertyPath"/>. This is useful for nested validators
    /// where the root of the sub-validator should not include the parent's property name.
    /// </param>
    /// <param name="isForCollection">
    /// A flag indicating whether the property being selected represents a collection. This information
    /// is passed to the <see cref="PropertyKey"/> for later use in collection-specific validation rules.
    /// </param>
    /// <returns>A fully configured <see cref="PropertyCtx{T, TProperty}"/> instance.</returns>
    /// <exception cref="RuleCreationException">
    /// Thrown if the <paramref name="propertySelector"/> is not a valid member access expression
    /// or if any intermediate expression is invalid.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the required constructor for <see cref="Undefined{TProperty}"/> cannot be found via reflection.
    /// </exception>
    public static PropertyCtx<T, TProperty> CreatePropertyCtx<T, TProperty>(
        Expression<Func<T, TProperty>> propertySelector,
        bool removeStartName = false,
        bool isForCollection = false)
    {
        // Parameter expression to be used in the compiled lambda (e.g., 'x' in 'x => x.Prop').
        ParameterExpression parameterExpression = propertySelector.Parameters[0];

        // List to collect all member expressions in the property selector chain.
        List<MemberExpression> internalMembers = [];
        StringBuilder sb = new(); // Used to build the property path string.

        // Start traversing from the body of the lambda (which should be a MemberExpression).
        MemberExpression? member = propertySelector.Body as MemberExpression;

        // Store the MemberInfo of the final property. Will be used for PropertyKey.
        MemberInfo memberInfo = member!.Member; // Null-forgiving as it's validated by ThrowIfNotMemberOrPropertyExpression.

        // Index for internal tracking during traversal.
        int index = 0;

        // Traverse the member expression chain from the deepest member upwards to the parameter.
        while (member is not null)
        {
            // Add the current member to the list.
            internalMembers.Add(member);

            // Build the property path string (e.g., "Prop1.Prop2.Prop3").
            // Special handling for ".Value" property of Nullable<T> if it's not the root.
            // This ensures the path is "Prop1" not "Prop1.Value".
            if (member.Member.Name != "Value" || internalMembers[index].IsForNullableType())
            {
                if (sb.Length == 0)
                {
                    sb.Insert(0, member.Member.Name);
                }
                else
                {
                    sb.Insert(0, '.');
                    sb.Insert(0, member.Member.Name);
                }
            }

            // Validate that the current expression is indeed a member or property access.
            RuleCreationException.ThrowIfNotMemberOrPropertyExpression(member);

            // Move up the expression tree to the parent member's expression.
            member = member.Expression as MemberExpression;
            index++;
        }

        // Reverse the order of members so they are from root to leaf (e.g., [x.Prop1, x.Prop1.Prop2, x.Prop1.Prop2.Prop3]).
        internalMembers.Reverse();

        // Define binding flags for reflection to find the Undefined<TProperty> constructor.
        BindingFlags bf = BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance;

        // Get the constructor of the Undefined<TProperty> class that takes (TProperty value, bool hasValue, bool isNull).
        ConstructorInfo constructorInfo = typeof(Undefined<TProperty>).GetConstructor(bf, [typeof(TProperty), typeof(bool), typeof(bool)])
            ?? throw new InvalidOperationException($"Undefined<{typeof(TProperty).Name}> constructor with (TProperty, bool, bool) parameters not found.");

        // Create an expression for the default constructor of Undefined<TProperty>,
        // which signifies a missing/undefined value.
        NewExpression defaultReturn = Expression.New(typeof(Undefined<TProperty>));

        // Create the main lambda expression for the PropertyValueResolver delegate.
        // This lambda's body will be built recursively by CreateNullSafePropagationBody.
        Expression<PropertyValueResolver<T, TProperty>> valueGetter = Expression.Lambda<PropertyValueResolver<T, TProperty>>(
            CreateNullSafePropagationBody(
                // Start with the first member's access from the parameter
                Expression.PropertyOrField(parameterExpression, internalMembers[0].Member.Name),
                internalMembers,
                constructorInfo,
                defaultReturn,
                // Start processing from the second member in the chain (index 1), as the first is handled above.
                1,
                false), // isResolved is false initially, indicating we're not yet "inside" a resolved segment
            parameterExpression); // The parameter of the lambda is the validation instance.

        // If removeStartName is true, remove the first member's name from the generated path.
        if (removeStartName)
        {
            // Ensure there's a name to remove and it matches the expected length.
            if (sb.Length >= internalMembers[0].Member.Name.Length)
            {
                sb.Remove(0, internalMembers[0].Member.Name.Length);
                // If the path started with "Name.SubName" and "Name" is removed, it becomes ".SubName". Remove the leading dot.
                if (sb.Length > 0 && sb[0] == '.')
                {
                    sb.Remove(0, 1);
                }
            }
        }

        // Create the PropertyKey with the extracted member info, built path, and collection flag.
        PropertyKey key = PropertyKey.Create(memberInfo, sb.ToString(), isForCollection);

        // Create and return the PropertyCtx with the PropertyKey and the compiled value resolver.
        return PropertyCtx.Create(key, valueGetter.Compile());
    }

    /// <summary>
    /// Recursively constructs an expression tree that safely navigates a property access chain,
    /// handling intermediate null values using conditional expressions. If an intermediate property
    /// is null, the propagation stops, and the expression evaluates to the <paramref name="defaultReturn"/>
    /// (an <see cref="Undefined{TProperty}"/> representing no value). Otherwise, it continues
    /// accessing the next member in the chain.
    /// </summary>
    /// <param name="currentExpression">The current expression in the property access chain being processed.</param>
    /// <param name="members">A list of all <see cref="MemberExpression"/>s in the property access path, ordered from root to leaf.</param>
    /// <param name="constructorInfo">The <see cref="ConstructorInfo"/> for <see cref="Undefined{TProperty}"/>.</param>
    /// <param name="defaultReturn">The <see cref="NewExpression"/> representing a default <see cref="Undefined{TProperty}"/> (missing value).</param>
    /// <param name="toContinueFrom">The index in the <paramref name="members"/> list from which to continue processing.</param>
    /// <param name="isResolved">
    /// A flag indicating whether the <paramref name="currentExpression"/> has already been
    /// resolved and assigned to a local variable (typically when handling nullable intermediate types).
    /// </param>
    /// <returns>An <see cref="Expression"/> representing the null-safe property access logic.</returns>
    private static Expression CreateNullSafePropagationBody(
        Expression currentExpression,
        List<MemberExpression> members,
        ConstructorInfo constructorInfo,
        NewExpression defaultReturn,
        int toContinueFrom,
        bool isResolved)
    {
        // Base case: If we are at the end of the member chain (only one member left to process from the original list).
        if (members.Count == 1) // This handles expressions like `x => x.Prop` directly
        {
            // If the final property is nullable (e.g., string, int?), we need an IsNull check.
            if (currentExpression.IsForNullableType())
            {
                // `currentExpression == null`
                Expression isNullCheck = Expression.Equal(currentExpression, _forNull);
                // Create Undefined<TProperty>(value, true, isNullCheck)
                return Expression.New(constructorInfo, currentExpression, _forTrue, isNullCheck);
            }

            // If the final property is a non-nullable value type, it cannot be null by itself.
            // Create Undefined<TProperty>(value, true, false)
            return Expression.New(constructorInfo, currentExpression, _forTrue, _forFalse);
        }

        // Recursive step: If `isResolved` is true, it means we are already processing
        // a member that was successfully resolved (not null) from a previous nullable check.
        if (isResolved)
        {
            // Access the next property/field in the chain.
            currentExpression = Expression.PropertyOrField(currentExpression, members[toContinueFrom].Member.Name);
            toContinueFrom++;

            // If this is the last member after advancing, handle it as the final property.
            if (toContinueFrom == members.Count)
            {
                if (currentExpression.IsForNullableType())
                {
                    Expression isNullCheck = Expression.Equal(currentExpression, _forNull);
                    return Expression.New(constructorInfo, currentExpression, _forTrue, isNullCheck);
                }
                return Expression.New(constructorInfo, currentExpression, _forTrue, _forFalse);
            }

            // Recursively call for the remaining members, keeping isResolved as false for the next iteration's nullable check.
            return CreateNullSafePropagationBody(
                currentExpression,
                members,
                constructorInfo,
                defaultReturn,
                toContinueFrom,
                false); // Reset isResolved, as the newly accessed 'currentExpression' might be nullable.
        }
        else // `isResolved` is false: We need to check if the current segment is nullable and handle null propagation.
        {
            // If the current expression's type is nullable (e.g., a reference type or Nullable<T>).
            if (currentExpression.IsForNullableType())
            {
                // Declare a local variable to hold the result of the current expression.
                ParameterExpression caller = Expression.Variable(currentExpression.Type, ToLowerFirstLatter(members[toContinueFrom - 1].Member.Name));
                // Assign the current expression's value to the local variable.
                BinaryExpression assign = Expression.Assign(caller, currentExpression);

                // Recursively create the access logic for the next member using the local variable.
                // Pass `true` for `isResolved` because `caller` is now a known, resolved value (even if null).
                Expression access = CreateNullSafePropagationBody(caller, members, constructorInfo, defaultReturn, toContinueFrom, true);

                // Create a conditional expression: `if (caller == null) return defaultReturn else return access`.
                BinaryExpression test = Expression.Equal(caller, _forNull);
                NewExpression ifTrue = defaultReturn; // If current segment is null, return Undefined (missing value).
                Expression ifFalse = access; // If not null, proceed with the 'access' expression.

                ConditionalExpression ternary = Expression.Condition(test, ifTrue, ifFalse);

                // Return a block expression: { var caller = currentExpression; return caller == null ? defaultReturn : access; }
                return Expression.Block(type: access.Type, variables: [caller], expressions: [assign, ternary]);
            }
            else // The current expression's type is a non-nullable value type (struct, int, etc.).
            {
                // Loop through subsequent non-nullable members.
                while (currentExpression.IsForNotNullableStruct() && toContinueFrom < members.Count)
                {
                    currentExpression = Expression.PropertyOrField(currentExpression, members[toContinueFrom].Member.Name);
                    toContinueFrom++;

                    // If we reached the end of the members list, return the final value creation.
                    if (toContinueFrom == members.Count)
                    {
                        if (currentExpression.IsForNullableType())
                        {
                            Expression isNullCheck = Expression.Equal(currentExpression, _forNull);
                            return Expression.New(constructorInfo, currentExpression, _forTrue, isNullCheck);
                        }
                        return Expression.New(constructorInfo, currentExpression, _forTrue, _forFalse);
                    }
                }

                // If there are more members to resolve, and we've stopped at a nullable or the end,
                // call the method recursively to handle the next segment.
                return CreateNullSafePropagationBody(
                    currentExpression,
                    members,
                    constructorInfo,
                    defaultReturn,
                    toContinueFrom,
                    false); // isResolved remains false if we just traversed non-nullables without assigning a variable.
            }
        }
    }

    /// <summary>
    /// Converts the first letter of a given character span to lowercase, preserving the rest of the string.
    /// This method is used internally, for example, to generate variable names for expression trees.
    /// </summary>
    /// <param name="input">The input <see cref="ReadOnlySpan{Char}"/>.</param>
    /// <returns>A new <see cref="string"/> with the first letter converted to lowercase.</returns>
    private static string ToLowerFirstLatter(ReadOnlySpan<char> input)
    {
        if (input.IsEmpty)
        {
            return string.Empty;
        }

        Span<char> result = stackalloc char[input.Length];
        input.CopyTo(result); // Copy the original characters

        result[0] = char.ToLower(input[0], CultureInfo.InvariantCulture); // Only modify the first character

        return new string(result);
    }
}