using KVKarco.ValidationAssistant.Exceptions;
using KVKarco.ValidationAssistant.Internal.Utilities.MessageTemplates;
using KVKarco.ValidationAssistant.Internal.ValidationRules;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace KVKarco.ValidationAssistant.Internal.Utilities;

internal static class Ensure
{
    public static void NotNull<T>(T value, string exceptionMessage)
    {
        if (value is null)
            ThrowCompositionException(exceptionMessage);
    }

    public static void NotNullOrEmpty(string? value, string exceptionMessage)
    {
        if (string.IsNullOrEmpty(value))
            ThrowCompositionException(exceptionMessage);
    }

    public static void HasNoClosure(Delegate del, string exceptionMessage)
    {
        if (del.Target is not null)
            ThrowCompositionException(exceptionMessage);
    }

    #region message template guards

    public static void TemplateCanBeRegistered(string templateName, CultureInfo culture, string messageTemplate)
    {
        NotNullOrEmpty(templateName, InternalDefaults.TemplateNameNullOrEmpty);
        NotNullOrEmpty(messageTemplate, InternalDefaults.TemplateStringNullOrEmpty);
        NotNull(culture, InternalDefaults.TemplateCultureNull);
    }

    public static void TemplateCanBeFormatted(CompiledTemplate template, int placeholdersReplaced)
    {
        NotNull(template, InternalDefaults.TemplateNotSet);
        if (template.PlaceholdersAvailable != placeholdersReplaced)
            ThrowCompositionException(InternalDefaults.TemplateMissingPlaceholderReplacementsCantBeFormatted);
    }

    public static void TemplatePlaceholderCanBeReplaced(CompiledTemplate template, string placeholder)
    {
        NotNull(template, InternalDefaults.TemplateNotSet);
        NotNullOrEmpty(placeholder, InternalDefaults.TemplatePlaceholderReplacementNullOrEmpty);
        if (!template.IsPlaceHolderPresented(placeholder))
            ThrowCompositionException(InternalDefaults.TemplatePlaceholderReplacementNotInTemplate);
    }

    #endregion

    #region validation components guards

    public static void SnapShotNotEmptyAndIsCaptured(string? snapShot, List<string>? snapShots)
    {
        NotNullOrEmpty(snapShot, InternalDefaults.SnapShotNullOrEmpty);

        if (snapShots is null || !snapShots.Exists(x => x == snapShot))
            ThrowWithMessage(InternalDefaults.SnapShotIsNotCaptured);
    }

    public static void MessageFactoryNotNullOrHasClosure(Delegate messageFactory)
    {
        NotNull(messageFactory, InternalDefaults.MessageFactoryNull);
        HasNoClosure(messageFactory, InternalDefaults.MessageFactoryClosure);
    }

    public static void RuleCanExecuteSynchronously(IRuleOptions options)
    {
        if (!options.CanExceedSynchronously)
            ThrowWithMessage(InternalDefaults.AsyncRuleCalledSync(options));
    }

    public static void PropertyValueIsDefined(
        ValidationCtx context,
        string? missingMember)
    {
        if (missingMember is not null)
            ThrowWithMessage(InternalDefaults.PropertyValueUndefined(context, missingMember));
    }

    public static void PredicateNotNullOrHasClosure(Delegate del)
    {
        NotNull(del, InternalDefaults.PredicateCannotBeNull);
        HasNoClosure(del, InternalDefaults.PredicateHasClosure);
    }

    public static void ConditionNotNullOrHasClosure(Delegate del)
    {
        NotNull(del, InternalDefaults.ConditionCannotBeNull);
        HasNoClosure(del, InternalDefaults.ConditionHasClosure);
    }

    #endregion

    #region selectors guards

    // guards against null or invalid selector expressions in debug builds.
    // - Cannot be null.
    // - Reject constants anywhere.
    // - Reject static properties/fields accessed anywhere.
    // - Reject .Value only if it's the last member accessed
    // - Reject method calls anywhere.
    // - Reject indexer access anywhere.
    // - Reject all casts (implicit or explicit) anywhere.
    // - Reject assignment/mutation expressions anywhere.
    // - Reject conditionals anywhere.
    // - Must be pure property or field access chain or the main instance it self.
    public static void IsValidSelector(LambdaExpression? selector)
    {
        string? message = null;

        if (selector is null)
        {
            message = InternalDefaults.SelectorNull;
        }
        else
        {
            Expression body = selector!.Body;

            if (body is ConstantExpression)
            {
                message = InternalDefaults.ConstantsNotAllowed;
            }
            else
            {
                message = ValidateExpressionTree(body, isRoot: true);
            }
        }

        if (message is not null)
        {
            ThrowCompositionException(message);
        }
    }

    private static string? ValidateExpressionTree(Expression expression, bool isRoot)
    {
        switch (expression)
        {
            case MemberExpression memberExpr:
                {
                    bool isStatic = memberExpr.Member switch
                    {
                        PropertyInfo pi => pi.GetMethod?.IsStatic ?? false,
                        FieldInfo fi => fi.IsStatic,
                        _ => false
                    };
                    if (isStatic)
                    {
                        return InternalDefaults.StaticMemberNotAllowed;
                    }

                    if (IsValueProperty(memberExpr.Member) && isRoot)
                    {
                        return InternalDefaults.ValuePropertyAtRootNotAllowed;
                    }

                    return ValidateExpressionTree(memberExpr.Expression!, isRoot: false);
                }

            case ParameterExpression:
                return null;
            case ConstantExpression:
                return InternalDefaults.ConstantsNotAllowed;
            case MethodCallExpression:
                return InternalDefaults.MethodCallsNotAllowed;
            case UnaryExpression u when u.NodeType is ExpressionType.Convert or ExpressionType.ConvertChecked:
                return InternalDefaults.CastsNotAllowed;
            case IndexExpression:
                return InternalDefaults.IndexersNotAllowed;
            case BinaryExpression b:
                if (b.NodeType is ExpressionType.Assign)
                {
                    return InternalDefaults.AssignmentsNotAllowed;
                }
                return InternalDefaults.BinaryExpressionsNotAllowed;
            case InvocationExpression:
                return InternalDefaults.InvocationNotAllowed;
            case NewExpression:
                return InternalDefaults.NewObjectsNotAllowed;
            case ConditionalExpression:
                return InternalDefaults.ConditionalsNotAllowed;
            case DefaultExpression:
                return InternalDefaults.DefaultsNotAllowed;
            default:
                return InternalDefaults.UnsupportedExpression;
        }
    }

    #endregion

    #region privet methods

    private static bool IsValueProperty(MemberInfo member)
    {
        return member is PropertyInfo pi && pi.Name == "Value";
    }

    [DoesNotReturn]
    private static void ThrowWithMessage(string message)
    {
        throw new ValidationAssistantException(message);
    }

    [DoesNotReturn]
    private static void ThrowCompositionException(string message)
    {
        throw new ValidationCompositionException(message);
    }

    #endregion
}
