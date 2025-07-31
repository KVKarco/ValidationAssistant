using KVKarco.ValidationAssistant.Internal.CustomValidatorAssets;
using KVKarco.ValidationAssistant.Internal.PropertyValidation;
using KVKarco.ValidationAssistant.Internal.Utilities;
using KVKarco.ValidationAssistant.Tests.Models;
using System.Globalization;

namespace KVKarco.ValidationAssistant.UnitTests;

public class ExpressValidatorPropertyRuleBuilderTests
{
    private class DummyResources { }

    [Fact]
    public void Build_CreatesPropertyRule_WithCorrectComponentsAndStrategies()
    {
        // Arrange
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");

        var propertyCtx = ExpressionFactory.CreatePropertyCtx<TestUser, string?>(u => u.Name, false, false);
        var snapshots = new List<string>();

        var builder = new CustomeValidatorPropertyRuleBuilder<TestUser, DummyResources, string?>(
            validatorName: "TestUserValidator",
            propertyCtx: propertyCtx,
            defaultRuleFailureStrategy: ValidatorFlow.Continue,
            defaultComponentFailureStrategy: RuleSetFlow.Continue,
            declaredOnLine: 123,
            snapShots: snapshots
        );

        builder
            .Ensure(name => !string.IsNullOrWhiteSpace(name))
                .WithMessage("Name is required")
                .OnFailure(RuleSetFlow.Exit)
                .Severity(FailureSeverity.Error)
            .Ensure((ctx, name) => name == null || name.Length <= 50)
                .WithMessage("Name must be 50 characters or less")
                .OnFailure(RuleSetFlow.Continue)
                .Severity(FailureSeverity.Warning)
            .EnsureAsync(async (name, ct) => await System.Threading.Tasks.Task.FromResult(name == null || !name.Contains("!")))
                .WithMessage("Name must not contain exclamation mark")
                .OnFailure(RuleSetFlow.Continue)
                .Severity(FailureSeverity.Error);

        // Act
        var rule = builder.Build();

        // Assert
        var typedRule = Assert.IsType<CustomValidatorPropertyRule<TestUser, DummyResources, string?>>(rule);
        Assert.Equal(ValidatorFlow.Continue, typedRule.Info.Strategy);
        Assert.Equal(123, typedRule.Info.DeclaredOnLine);

        // Use reflection to get the private _ruleComponents field
        var ruleComponentsField = typeof(CustomValidatorPropertyRule<TestUser, DummyResources, string?>)
            .BaseType! // PropertyRule
            .GetField("_ruleComponents", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var components = (System.Collections.IEnumerable)ruleComponentsField!.GetValue(typedRule)!;
        var componentList = components.Cast<PropertyRuleComponent<TestUser, DummyResources, string?>>().ToList();

        Assert.Equal(3, componentList.Count);

        // First component: sync, Exit, Error
        Assert.True(componentList[0].CanRunSynchronously);
        Assert.Equal(RuleSetFlow.Exit, componentList[0].Info.Strategy);
        Assert.Equal(FailureSeverity.Error, componentList[0].Info.Severity);

        // Second component: sync, Continue, Warning
        Assert.True(componentList[1].CanRunSynchronously);
        Assert.Equal(RuleSetFlow.Continue, componentList[1].Info.Strategy);
        Assert.Equal(FailureSeverity.Warning, componentList[1].Info.Severity);

        // Third component: async, Continue, Error
        Assert.False(componentList[2].CanRunSynchronously);
        Assert.Equal(RuleSetFlow.Continue, componentList[2].Info.Strategy);
        Assert.Equal(FailureSeverity.Error, componentList[2].Info.Severity);

        // Prepare a context for message factories
        var ctx = new CustomValidatorRunCtx<TestUser, DummyResources>(
            fromValidator: "TestUserValidator",
            value: new TestUser { Name = "Test" },
            resources: new DummyResources(),
            availableSnapShots: null,
            culture: CultureInfo.GetCultureInfo("en-US"),
            result: null,
            parentContext: null);

        // Check messages using the context and a sample value
        Assert.Equal("Name is required", componentList[0].Info.FailureMessageFactory(ctx, "Test"));
        Assert.Equal("Name must be 50 characters or less", componentList[1].Info.FailureMessageFactory(ctx, "Test"));
        Assert.Equal("Name must not contain exclamation mark", componentList[2].Info.FailureMessageFactory(ctx, "Test"));
    }
}