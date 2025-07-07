using KVKarco.ValidationAssistant.Abstractions;
using KVKarco.ValidationAssistant.Exceptions;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace KVKarco.ValidationAssistant.Internal.ExpressValidatorComponents;

internal sealed class ExpressValidatorCoreBuilder<T, TExternalResources> :
    ValidatorCoreBuilder<T, TExternalResources, ExpressValidatorRunCtx<T, TExternalResources>>,
    ICoreValidationDefinitionBuilder<T, TExternalResources>
{
    public ExpressValidatorCoreBuilder(string validatorName)
        : base(validatorName)
    {
    }

    public IInitialPropertyRuleBuilder<T, TExternalResources, TProperty> UseFor<TProperty>(
        Expression<Func<T, TProperty>> propertySelector,
        [CallerLineNumber] int callingFileLineNumber = 0)
    {
        ResolveLastRule();

        RuleCreationException.ThrowIfInvalidSelector(propertySelector);

        return null!; // This method should return an instance of a builder for property rules.
    }

    internal override ValidatorCore CreateValidatorCore()
    {
        ResolveLastRule();

        return ValidatorCore.ForExpressValidator(_validatorName, _snapShots, _preValidationRules, _rules);
    }
}
