using KVKarco.ValidationAssistant;
using KVKarco.ValidationAssistant.Internal;
using KVKarco.ValidationAssistant.Internal.ExpressValidatorComponents;
using KVKarco.ValidationAssistant.Internal.PropertyValidation;
using System.Globalization;

public class ExpressValidatorRunCtxPropertyPathAndNameTests
{
    private class DummyResources { }
    private class DummyRuleFailureInfo : RuleFailureInfo
    {
        public DummyRuleFailureInfo() : base("Validator", "Rule", 1, RuleFailureStrategy.Continue, 0) { }
    }

    [Fact]
    public void PropertyName_And_CorrectPropertyPath_Are_Empty_By_Default()
    {
        var ctx = new ExpressValidatorRunCtx<object, DummyResources>(
            fromValidator: "TestValidator",
            value: new object(),
            resources: new DummyResources(),
            availableSnapShots: null,
            culture: CultureInfo.InvariantCulture,
            result: null,
            parentContext: null);

        Assert.True(ctx.PropertyName.IsEmpty);
        Assert.Equal(string.Empty, ctx.CorrectPropertyPath);
    }

    [Fact]
    public void ForProperty_Sets_CurrentPropertyKey_And_Resets_State()
    {
        var ctx = new ExpressValidatorRunCtx<object, DummyResources>(
            fromValidator: "TestValidator",
            value: new object(),
            resources: new DummyResources(),
            availableSnapShots: null,
            culture: CultureInfo.InvariantCulture,
            result: null,
            parentContext: null);

        var key = PropertyKey.Create(typeof(string).GetProperty("Length")!, "Length", false);
        var ruleInfo = new DummyRuleFailureInfo();

        ctx.ForProperty(key, ruleInfo);

        Assert.Equal("Length", ctx.PropertyName.ToString());
        Assert.Equal("Length", ctx.CorrectPropertyPath);
    }

    [Fact]
    public void CorrectPropertyPath_Composes_Multiple_Nested_Contexts()
    {
        var rootCtx = new ExpressValidatorRunCtx<object, DummyResources>(
            fromValidator: "RootValidator",
            value: new object(),
            resources: new DummyResources(),
            availableSnapShots: null,
            culture: CultureInfo.InvariantCulture,
            result: null,
            parentContext: null);

        var midCtx = new ExpressValidatorRunCtx<object, DummyResources>(
            fromValidator: "MidValidator",
            value: new object(),
            resources: new DummyResources(),
            availableSnapShots: null,
            culture: CultureInfo.InvariantCulture,
            result: null,
            parentContext: rootCtx);

        var leafCtx = new ExpressValidatorRunCtx<object, DummyResources>(
            fromValidator: "LeafValidator",
            value: new object(),
            resources: new DummyResources(),
            availableSnapShots: null,
            culture: CultureInfo.InvariantCulture,
            result: null,
            parentContext: midCtx);

        var rootKey = PropertyKey.Create(typeof(string).GetProperty("Length")!, "Root", false);
        var midKey = PropertyKey.Create(typeof(string).GetProperty("Length")!, "Mid", false);
        var leafKey = PropertyKey.Create(typeof(string).GetProperty("Length")!, "Leaf", false);

        rootCtx.ForProperty(rootKey, new DummyRuleFailureInfo());
        midCtx.ForProperty(midKey, new DummyRuleFailureInfo());
        leafCtx.ForProperty(leafKey, new DummyRuleFailureInfo());

        Assert.Equal("Root.Mid.Leaf", leafCtx.CorrectPropertyPath);
    }

    [Fact]
    public void CorrectPropertyPath_Empty_When_PropertyKey_Is_Empty()
    {
        var ctx = new ExpressValidatorRunCtx<object, DummyResources>(
            fromValidator: "TestValidator",
            value: new object(),
            resources: new DummyResources(),
            availableSnapShots: null,
            culture: CultureInfo.InvariantCulture,
            result: null,
            parentContext: null);

        // Do not set property key
        Assert.Equal(string.Empty, ctx.CorrectPropertyPath);
    }
}