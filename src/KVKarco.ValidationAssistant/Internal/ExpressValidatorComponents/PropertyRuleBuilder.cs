using KVKarco.ValidationAssistant.Abstractions;
using KVKarco.ValidationAssistant.Internal.PropertyValidation;

namespace KVKarco.ValidationAssistant.Internal.ExpressValidatorComponents;

internal sealed class ExpressValidatorPropertyRuleBuilder<T, TExternalResources, TProperty> :
    PropertyRuleBuilder<T, TExternalResources, TProperty>,
    IBuildableRule<T, TExternalResources, ExpressValidatorRunCtx<T, TExternalResources>>
    where T : notnull
    where TExternalResources : notnull
    where TProperty : notnull
{
    private readonly PropertyCtx<T, TProperty> _propertyCtx;
    private readonly string _validatorName;

    public ExpressValidatorPropertyRuleBuilder(
        string validatorName,
        PropertyCtx<T, TProperty> propertyCtx,
        RuleFailureStrategy defaultRuleFailureStrategy,
        ComponentFailureStrategy defaultComponentFailureStrategy,
        int declaredOnLine,
        List<string> snapShots)
        : base(defaultRuleFailureStrategy, defaultComponentFailureStrategy, declaredOnLine, snapShots)
    {
        _validatorName = validatorName;
        _propertyCtx = propertyCtx;
    }

    public IValidatorRule<T, TExternalResources, ExpressValidatorRunCtx<T, TExternalResources>> Build()
    {
        ResolveLastComponent();
        return new ExpressValidatorPropertyRule<T, TExternalResources, TProperty>(_propertyCtx, _validatorName, _onRuleFailure, _ruleDeclaredOnLine, _ruleComponents);
    }
}
