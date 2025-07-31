using KVKarco.ValidationAssistant.Abstractions;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace KVKarco.ValidationAssistant;

public static class ValidatorsConfig
{
    public static readonly DefaultConfiguration GlobalDefaults = new();
}

public sealed class DefaultConfiguration
{
    public CultureInfo DefaultCulture { get; set; } = CultureInfo.GetCultureInfo("en-US");

    public DefaultMessages Messages { get; set; } = new DefaultMessages();

    public FlowEffect DefaultComponentFailureFlowEffect { get; set; } = FlowEffect.Proceed;

    public FlowEffect DefaultRuleFailureFlowEffect { get; set; } = FlowEffect.Proceed;

    public Severity DefaultRuleFailureSeverity { get; set; } = Severity.Error;
}

public class DefaultMessages
{
    public virtual string LogicalComponentStopExplanation<T, TExternalResources, TProperty>(
        [NotNull] IMessageCtx<T, TExternalResources> context,
        TProperty value)
    {

        return $"PropertyRule for {context.PropertyName} is stopped from failed ValidationRule.";
    }

    public virtual string ValidationComponentError<T, TExternalResources, TProperty>(
        [NotNull] IMessageCtx<T, TExternalResources> context,
        TProperty value)
    {
        return $"The specified condition was not met for {context.PropertyName}.";
    }

    public virtual string ChildValidatorComponentExplanation<T, TExternalResources, TProperty>(
        [NotNull] IMessageCtx<T, TExternalResources> context,
        TProperty value)
    {
        return $"PropertyValidator failed validating property: {context.PropertyName}.";
    }

    public virtual string InvalidSnapShotComponentExplanation<T, TExternalResources, TProperty>(
       [NotNull] IMessageCtx<T, TExternalResources> context,
       TProperty value,
       ReadOnlySpan<char> snapShotIdentifier)
    {
        return $"SnapShot {snapShotIdentifier} is not valid ValidationRuleSet(PropertyRule) stop executing.";
    }


    public virtual string PropertyValueMissingExplanation<T, TExternalResources>(
        [NotNull] IMessageCtx<T, TExternalResources> context)
    {
        return $"Property : {context.PropertyName} value cant be extracted is missing.";
    }

    public virtual string ConditionalFlowBlockSkipExplanation<T, TExternalResources>(
        [NotNull] IMessageCtx<T, TExternalResources> context,
        int rulesSkipped)
    {

        return $"The failure of the ConditionalFlowRule caused the validation run to skip {rulesSkipped} rules.";
    }
}