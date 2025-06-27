using KVKarco.ValidationAssistant.Abstractions;
using KVKarco.ValidationAssistant.Internal.ExpressValidator;
using KVKarco.ValidationAssistant.Internal.PropertyValidation;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace KVKarco.ValidationAssistant.Internal.Utilities;

/// <summary>
/// Provides a static, thread-safe caching mechanism for compiled <see cref="ExpressValidatorCore{T, TExternalResources}"/> instances
/// and <see cref="PropertyCtx"/> objects. This cache optimizes performance by storing and reusing
/// compiled validator cores and property contexts once they have been built, avoiding redundant
/// compilation of validation rules and property access delegates.
/// </summary>
internal static class InternalCache
{
    /// <summary>
    /// A thread-safe dictionary that caches <see cref="Lazy{T}"/> instances of <see cref="ValidatorCore"/>,
    /// keyed by the concrete <see cref="Type"/> of the <see cref="ExpressValidator{T, TExternalResources}"/> that owns them.
    /// The <see cref="Lazy{T}"/> ensures that the validator core is built only once, atomically, when first requested.
    /// </summary>
    private static readonly ConcurrentDictionary<Type, Lazy<ValidatorCore>> _coresCache = [];

    /// <summary>
    /// A thread-safe dictionary that caches <see cref="PropertyCtx"/> instances,
    /// keyed by a string representation of the property selector expression.
    /// This prevents redundant compilation of property access delegates and metadata extraction.
    /// </summary>
    private static readonly ConcurrentDictionary<string, PropertyCtx> _membersCtxCache = [];

    /// <summary>
    /// Retrieves a compiled <see cref="ExpressValidatorCore{T, TExternalResources}"/> from the cache,
    /// or builds and adds it to the cache if it does not already exist for the given validator type.
    /// This method ensures that validator core compilation happens only once per validator type across the application lifetime.
    /// </summary>
    /// <typeparam name="T">The type of the instance being validated by the validator core.</typeparam>
    /// <typeparam name="TExternalResources">The type of external resources used by the validator core.</typeparam>
    /// <param name="validatorType">The concrete <see cref="Type"/> of the <see cref="ExpressValidator{T, TExternalResources}"/>.</param>
    /// <param name="preValidationRuleCreator">An action that defines the pre-validation rules for the validator,
    /// typically provided by the <c>ExpressPreValidationRules</c> method of <see cref="ExpressValidator{T, TExternalResources}"/>.</param>
    /// <param name="ruleCreator">An action that defines the main validation rules for the validator,
    /// typically provided by the abstract <c>ExpressRules</c> method of <see cref="ExpressValidator{T, TExternalResources}"/>.</param>
    /// <returns>A compiled and cached instance of <see cref="ExpressValidatorCore{T, TExternalResources}"/>.</returns>
    /// <remarks>
    /// This method uses <see cref="Lazy{T}"/> to ensure thread-safe, one-time initialization of the validator core.
    /// The actual compilation logic is encapsulated within the <see cref="ExpressValidatorCoreBuilder{T, TExternalResources}"/>,
    /// which implements the rule expression builder interfaces.
    /// </remarks>
    public static ExpressValidatorCore<T, TExternalResources> GetOrAddExpressValidatorCore<T, TExternalResources>(
        Type validatorType,
        Action<IPreValidationDefinitionBuilder<T, TExternalResources>> preValidationRuleCreator,
        Action<ICoreValidationDefinitionBuilder<T, TExternalResources>> ruleCreator)
    {
        // TODO: The ExpressValidatorCoreBuilder class needs to be implemented.
        // It should implement both IPreValidationDefinitionBuilder<T, TExternalResources> and ICoreValidationDefinitionBuilder<T, TExternalResources>.
        // This builder is responsible for collecting the rules defined by the user in ExpressPreValidationRules and ExpressRules,
        // and then compiling them into an ExpressValidatorCore.

        Lazy<ValidatorCore> validationCore = _coresCache.GetOrAdd(
            validatorType,
            (type, action) => new Lazy<ValidatorCore>(() =>
            {
                // Get validator name from attribute or fallback to type name
                string name = Attribute.GetCustomAttribute(type, typeof(ValidatorNameAttribute))
                is not ValidatorNameAttribute myAttribute
                ? type.Name
                : myAttribute.ValidatorName;

                // Create an instance of the builder, which will gather the rules
                // ExpressValidatorCoreBuilder<T, TExternalResources> builder = new(name); // Uncomment once implemented
                // action.preValidationRuleCreator(builder); // Uncomment once implemented
                // action.ruleCreator(builder); // Uncomment once implemented
                // return builder.CreateValidatorCore(); // Uncomment once implemented

                // Placeholder return - replace with actual builder creation and core return
                throw new NotImplementedException("ExpressValidatorCoreBuilder needs to be implemented to create the validator core.");
            }),
            (preValidationRuleCreator, ruleCreator)); // Passed as state to the factory delegate

        // Cast the generic ValidatorCore to the specific ExpressValidatorCore type.
        // This cast is safe because the Lazy<ValidatorCore> will contain an ExpressValidatorCore.
        return (ExpressValidatorCore<T, TExternalResources>)validationCore.Value;
    }

    /// <summary>
    /// Retrieves a <see cref="PropertyCtx{T, TProperty}"/> from the cache, or creates and adds it
    /// if it does not already exist for the given property selector expression.
    /// This method leverages <see cref="ExpressionFactory.CreatePropertyCtx{T, TProperty}(Expression{Func{T, TProperty}}, bool, bool)"/>
    /// to generate the context and then caches it for future reuse.
    /// </summary>
    /// <typeparam name="T">The type of the main instance from which the property value is extracted.</typeparam>
    /// <typeparam name="TProperty">The type of the property whose context is being managed.</typeparam>
    /// <param name="memberSelector">The expression selecting the property (e.g., <c>x => x.User.Address.Street</c>).</param>
    /// <param name="removeLastMember">
    /// If <see langword="true"/>, the name of the last member in the property path (e.g., "Street" in "User.Address.Street")
    /// will be removed from the resulting <see cref="PropertyKey.PropertyPath"/>. This is useful for scenarios
    /// where only the parent path is desired. (Note: The parameter name might be intended as `removeStartName`
    /// to align with `ExpressionFactory`, but its current name suggests removing the last member. Clarification needed if different from `removeStartName` in `ExpressionFactory`).
    /// **Correction:** This parameter name `removeLastMember` seems to be a mismatch with `removeStartName` in `ExpressionFactory`.
    /// Assuming it maps to `ExpressionFactory.removeStartName` for consistency, as that's what's passed to `CreatePropertyCtx`.
    /// </param>
    /// <param name="isForCollection">
    /// A flag indicating whether the property being selected represents a collection. This information
    /// is passed to the <see cref="PropertyKey"/> for later use in collection-specific validation rules.
    /// </param>
    /// <returns>A compiled and cached instance of <see cref="PropertyCtx{T, TProperty}"/>.</returns>
    /// <remarks>
    /// The property selector expression is normalized to a string key using <see cref="SelectorRewriter.Rewrite{T, TProperty}(Expression{Func{T, TProperty}})"/>
    /// to ensure consistent caching regardless of the parameter name used in the lambda (e.g., `user => user.Name` and `x => x.Name` map to the same key).
    /// </remarks>
    public static PropertyCtx<T, TProperty> GetOrAddPropertyCtx<T, TProperty>(
        Expression<Func<T, TProperty>> memberSelector,
        bool removeLastMember = false, // Renamed from original to be descriptive if it affects the last member.
                                       // If it means 'removeStartName' from ExpressionFactory, the naming is inconsistent.
                                       // Assuming it's `removeStartName` as per the call to ExpressionFactory.
        bool isForCollection = false)
    {
        // Normalize the expression to a string key for caching.
        // If the parameter is already named "x", use its ToString() directly.
        // Otherwise, rewrite it to use "x" as the parameter name for consistent hashing.
        string propSelectorDefinition = memberSelector.Parameters[0].Name == "x"
            ? memberSelector.ToString()
            : memberSelector.Rewrite().ToString();

        // Get or add the PropertyCtx to the cache.
        // The factory function receives the key and the original expression as state.
        PropertyCtx context = _membersCtxCache.GetOrAdd(
            propSelectorDefinition,
            (key, expression) => ExpressionFactory.CreatePropertyCtx((Expression<Func<T, TProperty>>)expression, removeLastMember, isForCollection),
            memberSelector); // Pass the original memberSelector as state

        // Cast the non-generic PropertyCtx to its specific generic type.
        // This cast is safe because the factory method creates the correct generic type.
        return (PropertyCtx<T, TProperty>)context;
    }
}
