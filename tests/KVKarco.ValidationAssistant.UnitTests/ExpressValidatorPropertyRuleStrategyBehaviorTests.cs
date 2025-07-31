using KVKarco.ValidationAssistant.Abstractions;
using KVKarco.ValidationAssistant.Internal;
using KVKarco.ValidationAssistant.Internal.CustomValidatorAssets;
using KVKarco.ValidationAssistant.Internal.PropertyValidation;
using KVKarco.ValidationAssistant.ValidationRules;
using System.Globalization;

namespace KVKarco.ValidationAssistant.UnitTests;

public class ExpressValidatorPropertyRuleStrategyBehaviorTests
{
    private class DummyResources { }

    private static PropertyCtx<object, object> CreatePropertyCtx(string propertyName = "TestProp")
    {
        // Use any property for the PropertyKey (e.g., Length of string)
        var propertyKey = PropertyKey.Create(typeof(string).GetProperty("Length")!, propertyName, false);
        // Value resolver always returns a defined value (not missing, not null)
        return PropertyCtx.Create<object, object>(propertyKey, _ => Undefined.For<object>(new object(), true, false));
    }

    private class CountingValidationRule : IValidationRule<object, DummyResources, object>
    {
        private readonly bool _shouldFail;
        private readonly Action? _onRun;
        public int RunCount { get; private set; }

        public CountingValidationRule(bool shouldFail, Action? onRun = null)
        {
            _shouldFail = shouldFail;
            _onRun = onRun;
        }

        public ReadOnlySpan<char> RuleName => "CountingValidationRule";
        public string GetDefaultFailureMessage(IMessageCtx<object, DummyResources> context, object value) => _shouldFail ? "fail" : "ok";
        public bool IsValid(ValidatorRunCtx<object, DummyResources> context, object value)
        {
            RunCount++;
            _onRun?.Invoke();
            return !_shouldFail;
        }
    }

    private static CustomValidatorPropertyRule<object, DummyResources, object> CreatePropertyRule(
        ValidatorFlow ruleStrategy,
        List<(CountingValidationRule rule, RuleSetFlow strategy)> rulesAndStrategies,
        string propertyName = "TestProp")
    {
        var propertyCtx = CreatePropertyCtx(propertyName);
        var components = new List<PropertyRuleComponent<object, DummyResources, object>>();
        foreach (var (rule, strategy) in rulesAndStrategies)
        {
            var info = ValidationRuleFailureInfo.New<object, DummyResources, object>(
                (ctx, val) => rule.GetDefaultFailureMessage(ctx, val),
                "TestRule", 1, FailureSeverity.Error, strategy);
            components.Add(new PropertyRuleComponent<object, DummyResources, object>(rule, info));
        }
        return new ExpressValidatorPropertyRule<object, DummyResources, object>(
            propertyCtx,
            "TestValidator",
            ruleStrategy,
            1,
            components
        );
    }

    [Fact]
    public void Component_With_Stop_Strategy_Stops_Entire_Validation_Process_And_Skips_Second_Rule()
    {
        // Arrange
        bool secondRuleRun = false;
        var stopRule = new CountingValidationRule(true);
        var continueRule = new CountingValidationRule(false);

        var firstRule = CreatePropertyRule(ValidatorFlow.Continue, new List<(CountingValidationRule, RuleSetFlow)>
        {
            (stopRule, RuleSetFlow.Stop),
            (continueRule, RuleSetFlow.Continue)
        });

        var secondRule = CreatePropertyRule(ValidatorFlow.Continue, new List<(CountingValidationRule, RuleSetFlow)>
        {
            (new CountingValidationRule(false, () => secondRuleRun = true), RuleSetFlow.Continue)
        }, propertyName: "SecondProp");

        var core = new CustomValidatorCore<object, DummyResources>(
            "TestValidator",
            null,
            new List<IPreValidationRule<object, DummyResources>>(),
            new List<IValidatorComponent<object, DummyResources, CustomValidatorRunCtx<object, DummyResources>>>
            {
                firstRule,
                secondRule
            });

        var ctx = new CustomValidatorRunCtx<object, DummyResources>(
            "TestValidator", new object(), new DummyResources(), null, CultureInfo.GetCultureInfo("en-US"), null, null);

        // Act
        core.InternalValidate(ctx);

        // Assert
        Assert.Equal(1, stopRule.RunCount);
        Assert.Equal(0, continueRule.RunCount);
        Assert.False(secondRuleRun);
        Assert.False(ctx.IsRunValid);
    }

    [Fact]
    public void Component_With_Exit_Strategy_Stops_PropertyRule_At_Third_Component_And_Skips_Five_Components()
    {
        // Arrange
        int componentsRun = 0;
        var rulesAndStrategies = new List<(CountingValidationRule, RuleSetFlow)>();
        for (int i = 0; i < 8; i++)
        {
            if (i == 2)
                rulesAndStrategies.Add((new CountingValidationRule(true, () => componentsRun++), RuleSetFlow.Exit));
            else
                rulesAndStrategies.Add((new CountingValidationRule(false, () => componentsRun++), RuleSetFlow.Continue));
        }

        var propertyRule = CreatePropertyRule(ValidatorFlow.Continue, rulesAndStrategies);

        var core = new CustomValidatorCore<object, DummyResources>(
            "TestValidator",
            null,
            new List<IPreValidationRule<object, DummyResources>>(),
            new List<IValidatorComponent<object, DummyResources, CustomValidatorRunCtx<object, DummyResources>>>
            {
                propertyRule
            });

        var ctx = new CustomValidatorRunCtx<object, DummyResources>(
            "TestValidator", new object(), new DummyResources(), null, CultureInfo.GetCultureInfo("en-US"), null, null);

        // Act
        core.InternalValidate(ctx);

        // Assert
        Assert.Equal(3, componentsRun); // Only first 3 components should run
        for (int i = 0; i < 8; i++)
        {
            if (i < 3)
                Assert.Equal(1, ((CountingValidationRule)rulesAndStrategies[i].Item1).RunCount);
            else
                Assert.Equal(0, ((CountingValidationRule)rulesAndStrategies[i].Item1).RunCount);
        }
        Assert.False(ctx.IsRunValid);
    }

    [Fact]
    public void PropertyRule_With_Stop_Strategy_And_Continue_Components_Stops_Validation_After_Second_PropertyRule()
    {
        // Arrange
        var rule1Components = new List<(CountingValidationRule, RuleSetFlow)>
        {
            (new CountingValidationRule(false), RuleSetFlow.Continue)
        };
        var rule2Components = new List<(CountingValidationRule, RuleSetFlow)>();
        for (int i = 0; i < 7; i++)
        {
            if (i == 4)
                rule2Components.Add((new CountingValidationRule(true), RuleSetFlow.Continue)); // Only this one fails
            else
                rule2Components.Add((new CountingValidationRule(false), RuleSetFlow.Continue));
        }
        var rule3Components = new List<(CountingValidationRule, RuleSetFlow)>
        {
            (new CountingValidationRule(false), RuleSetFlow.Continue)
        };

        var rule1 = CreatePropertyRule(ValidatorFlow.Continue, rule1Components, propertyName: "Rule1Prop");
        var rule2 = CreatePropertyRule(ValidatorFlow.Stop, rule2Components, propertyName: "Rule2Prop");
        var rule3 = CreatePropertyRule(ValidatorFlow.Continue, rule3Components, propertyName: "Rule3Prop");

        var core = new CustomValidatorCore<object, DummyResources>(
            "TestValidator",
            null,
            new List<IPreValidationRule<object, DummyResources>>(),
            new List<IValidatorComponent<object, DummyResources, CustomValidatorRunCtx<object, DummyResources>>>
            {
                rule1,
                rule2,
                rule3
            });

        var ctx = new CustomValidatorRunCtx<object, DummyResources>(
            "TestValidator", new object(), new DummyResources(), null, CultureInfo.GetCultureInfo("en-US"), null, null);

        // Act
        core.InternalValidate(ctx);

        // Assert
        Assert.Equal(1, rule1Components[0].Item1.RunCount);
        for (int i = 0; i < 7; i++)
            Assert.Equal(1, rule2Components[i].Item1.RunCount);
        Assert.Equal(0, rule3Components[0].Item1.RunCount);
        Assert.False(ctx.IsRunValid);
    }
}