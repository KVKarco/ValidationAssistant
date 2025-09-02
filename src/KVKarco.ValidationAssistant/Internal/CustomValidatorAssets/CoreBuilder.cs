using KVKarco.ValidationAssistant.Abstractions;
using KVKarco.ValidationAssistant.Internal.Utilities;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace KVKarco.ValidationAssistant.Internal.CustomValidatorAssets;

internal sealed class CustomValidatorCoreBuilder<T, TExternalResources> :
    ValidatorCoreBuilder<T, TExternalResources, CustomValidatorRunCtx<T, TExternalResources>>,
    IValidatorValidationDefinitionBuilder<T, TExternalResources>
{
    public CustomValidatorCoreBuilder(string validatorName)
        : base(validatorName)
    {
    }

    /// <inheritdoc/>
    public IInitialPropertyRuleBuilder<T, TExternalResources, TProperty> UseFor<TProperty>(
        Expression<Func<T, TProperty>> propertySelector,
        [CallerLineNumber] int callingFileLineNumber = 0)
    {
        // Resolve the previously defined rule (if any) before starting a new one.
        ResolveLastRule();

        // Create a new ExpressValidatorPropertyRuleBuilder instance.
        // This builder will be used to fluently define the specific validation components for this property.
        var builder = new CustomeValidatorPropertyRuleBuilder<T, TExternalResources, TProperty>(
            _validatorName, // Pass the validator's name
            InternalCache.GetOrAddPropertyCtx(propertySelector),
            RuleFailureStrategy, // Pass the validator's default rule failure strategy
            RuleComponentsFailureStrategy, // Pass the validator's default component failure strategy
            callingFileLineNumber,
            _snapShots // Pass the shared snapshots list
        );

        _ruleToBeAdded = builder; // Set the current rule to be added to the builder.

        // Return the new property rule builder to allow fluent chaining.
        return builder;
    }

    /// <inheritdoc/>
    internal override ValidatorCore CreateValidatorCore()
    {
        // Resolve any last pending rule before finalizing the validator core.
        ResolveLastRule();

        // Create and return the ExpressValidatorCore using the collected pre-validation rules, main rules, and snapshots.
        return ValidatorCore.ForExpressValidator(_validatorName, _snapShots.Count == 0 ? null : _snapShots, _preValidationRules, _rules);
    }
}
