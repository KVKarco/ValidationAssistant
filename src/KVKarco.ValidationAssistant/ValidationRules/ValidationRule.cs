using KVKarco.ValidationAssistant.Abstractions;
using KVKarco.ValidationAssistant.Internal;

namespace KVKarco.ValidationAssistant.ValidationRules;

public abstract class ValidationRule<T, TExternalResources, TProperty>
{
    private protected ValidationRule(bool canRunSynchronously)
    {
        CanRunSynchronously = canRunSynchronously;
    }

    public bool CanRunSynchronously { get; }

    public abstract ReadOnlySpan<char> RuleName { get; }

    public abstract string GetDefaultFailureMessage(IMessageCtx<T, TExternalResources> context, TProperty value);

    internal abstract void Validate(
        ValidatorRunCtx<T, TExternalResources> context,
        Undefined<TProperty> property,
        ValidationRuleFailureInfo<T, TExternalResources, TProperty> failureInfo);

    internal abstract ValueTask ValidateAsync(
        ValidatorRunCtx<T, TExternalResources> context,
        Undefined<TProperty> property,
        ValidationRuleFailureInfo<T, TExternalResources, TProperty> failureInfo,
        CancellationToken ct);
}
