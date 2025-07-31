using KVKarco.ValidationAssistant.Exceptions;
using KVKarco.ValidationAssistant.Internal.CustomValidatorAssets;
using KVKarco.ValidationAssistant.Internal.PreValidation;
using KVKarco.ValidationAssistant.Internal.ValidationFlow;
using System.Linq.Expressions;

namespace KVKarco.ValidationAssistant.UnitTests;

public class ExpressValidatorCoreBuilderTests
{
    private const string ValidatorName = "TestValidator";

    // Reset factories before each test to ensure isolation
    public ExpressValidatorCoreBuilderTests()
    {
        // Reset global defaults if any test changes them
        ValidatorsConfig.GlobalDefaults.OnRuleFailure = ValidatorFlow.Continue;
        ValidatorsConfig.GlobalDefaults.OnComponentFailure = RuleSetFlow.Continue;
    }

    [Fact]
    public void Constructor_InitializesCorrectly_WithDefaults()
    {
        var builder = new CustomValidatorCoreBuilder<TestModel, TestResources>(ValidatorName);
        var validationCore = (CustomValidatorCore<TestModel, TestResources>)builder.CreateValidatorCore();

        Assert.Equal(ValidatorName, validationCore.ValidatorName);
        Assert.Null(validationCore.SnapShots);
        Assert.Empty(validationCore.PreValidationRules);
        Assert.Empty(validationCore.Components);

        Assert.Equal(ValidatorsConfig.GlobalDefaults.OnRuleFailure, builder.RuleFailureStrategy);
        Assert.Equal(ValidatorsConfig.GlobalDefaults.OnComponentFailure, builder.RuleComponentsFailureStrategy);
    }

    [Theory]
    [InlineData(ValidatorFlow.Stop, RuleSetFlow.Exit)]
    [InlineData(ValidatorFlow.Continue, RuleSetFlow.Stop)]
    public void DefaultStrategies_CanBeSet(ValidatorFlow ruleStrategy, RuleSetFlow componentStrategy)
    {
        var builder = new CustomValidatorCoreBuilder<TestModel, TestResources>(ValidatorName);

        builder.DefaultRuleFailureStrategy(ruleStrategy);
        builder.DefaultComponentFailureStrategy(componentStrategy);

        Assert.Equal(ruleStrategy, builder.RuleFailureStrategy);
        Assert.Equal(componentStrategy, builder.RuleComponentsFailureStrategy);
    }

    [Fact]
    public void Ensure_AddsSynchronousMainInstancePreValidationRule()
    {
        var builder = new CustomValidatorCoreBuilder<TestModel, TestResources>(ValidatorName);
        PreValidationPredicate<TestModel> predicate = (m) => true;
        string explanation = "Initial check failed.";
        int lineNumber = 100;

        builder.Ensure(predicate, explanation, lineNumber);

        var validationCore = (CustomValidatorCore<TestModel, TestResources>)builder.CreateValidatorCore();

        Assert.Single(validationCore.PreValidationRules);
        var rule = Assert.IsType<MainInstancePreValidationRule<TestModel, TestResources>>(validationCore.PreValidationRules[0]);
        Assert.Equal(ValidatorName, validationCore.ValidatorName);
        Assert.True(rule.CanRunSynchronously);
    }

    [Fact]
    public void EnsureAsync_AddsAsynchronousMainInstancePreValidationRule()
    {
        var builder = new CustomValidatorCoreBuilder<TestModel, TestResources>(ValidatorName);
        AsyncPreValidationPredicate<TestModel> predicate = (m, ct) => Task.FromResult(true);
        int lineNumber = 200;

        builder.EnsureAsync(predicate, null, lineNumber);

        var validationCore = (CustomValidatorCore<TestModel, TestResources>)builder.CreateValidatorCore();

        Assert.Single(validationCore.PreValidationRules);
        var rule = Assert.IsType<MainInstancePreValidationRule<TestModel, TestResources>>(validationCore.PreValidationRules[0]);
        Assert.Equal(ValidatorName, validationCore.ValidatorName);
        Assert.False(rule.CanRunSynchronously);
    }

    [Fact]
    public void EnsureResources_AddsSynchronousResourcesPreValidationRule()
    {
        var builder = new CustomValidatorCoreBuilder<TestModel, TestResources>(ValidatorName);
        PreValidationPredicate<TestResources> predicate = (m) => true;
        string explanation = "Initial check failed.";
        int lineNumber = 100;

        builder.EnsureResources(predicate, explanation, lineNumber);

        var validationCore = (CustomValidatorCore<TestModel, TestResources>)builder.CreateValidatorCore();

        Assert.Single(validationCore.PreValidationRules);
        var rule = Assert.IsType<ResourcesPreValidationRule<TestModel, TestResources>>(validationCore.PreValidationRules[0]);
        Assert.Equal(ValidatorName, validationCore.ValidatorName);
        Assert.True(rule.CanRunSynchronously);
    }

    [Fact]
    public void EnsureResources_AddsAsynchronousResourcesPreValidationRule()
    {
        var builder = new CustomValidatorCoreBuilder<TestModel, TestResources>(ValidatorName);
        AsyncPreValidationPredicate<TestResources> predicate = (m, ct) => Task.FromResult(true);
        string explanation = "Initial check failed.";
        int lineNumber = 100;

        builder.EnsureResourcesAsync(predicate, explanation, lineNumber);

        var validationCore = (CustomValidatorCore<TestModel, TestResources>)builder.CreateValidatorCore();

        Assert.Single(validationCore.PreValidationRules);
        var rule = Assert.IsType<ResourcesPreValidationRule<TestModel, TestResources>>(validationCore.PreValidationRules[0]);
        Assert.Equal(ValidatorName, validationCore.ValidatorName);
        Assert.False(rule.CanRunSynchronously);
    }

    [Fact]
    public void Ensure_ThrowsRuleCreationException_WhenMainInstancePredicateIsNull()
    {
        var builder = new CustomValidatorCoreBuilder<TestModel, TestResources>(ValidatorName);
        PreValidationPredicate<TestModel> nullPredicate = null!;

        var exception = Assert.Throws<RuleCreationException>(() => builder.Ensure(nullPredicate));
        Assert.Contains("predicate", exception.Message);
    }

    [Fact]
    public void Ensure_ThrowsRuleCreationException_WhenMainInstanceAsyncPredicateIsNull()
    {
        var builder = new CustomValidatorCoreBuilder<TestModel, TestResources>(ValidatorName);
        AsyncPreValidationPredicate<TestModel> nullPredicate = null!;

        var exception = Assert.Throws<RuleCreationException>(() => builder.EnsureAsync(nullPredicate));
        Assert.Contains("predicate", exception.Message);
    }

    [Fact]
    public void Ensure_ThrowsRuleCreationException_WhenResourcesPredicateIsNull()
    {
        var builder = new CustomValidatorCoreBuilder<TestModel, TestResources>(ValidatorName);
        PreValidationPredicate<TestResources> nullPredicate = null!;

        var exception = Assert.Throws<RuleCreationException>(() => builder.EnsureResources(nullPredicate));
        Assert.Contains("predicate", exception.Message);
    }

    [Fact]
    public void Ensure_ThrowsRuleCreationException_WhenResourcesAsyncPredicateIsNull()
    {
        var builder = new CustomValidatorCoreBuilder<TestModel, TestResources>(ValidatorName);
        AsyncPreValidationPredicate<TestResources> nullPredicate = null!;

        var exception = Assert.Throws<RuleCreationException>(() => builder.EnsureResourcesAsync(nullPredicate));
        Assert.Contains("predicate", exception.Message);
    }

    [Fact]
    public void UseFor_CreatesNewPropertyRuleBuilderAndSetsRuleToBeAdded()
    {
        var builder = new CustomValidatorCoreBuilder<TestModel, TestResources>(ValidatorName);
        Expression<Func<TestModel, string>> propertySelector = m => m.MyProperty;
        int lineNumber = 100;
        var propertyRuleBuilder = builder.UseFor(propertySelector, lineNumber).Ensure(x => x.Contains('a'));

        var validationCore = (CustomValidatorCore<TestModel, TestResources>)builder.CreateValidatorCore();

        Assert.NotNull(propertyRuleBuilder);
        Assert.Single(validationCore.Components);
        Assert.IsType<CustomValidatorPropertyRule<TestModel, TestResources, string>>(validationCore.Components[0]);
    }

    [Fact]
    public void CallingUseForTwoTimes_ResolvesAndSetsRulesCorrectly()
    {
        var builder = new CustomValidatorCoreBuilder<TestModel, TestResources>(ValidatorName);
        Expression<Func<TestModel, string>> propertySelector1 = m => m.MyProperty;
        Expression<Func<TestModel, int>> propertySelector2 = m => m.MyIntProperty;
        int lineNumber1 = 100;
        int lineNumber2 = 101;

        var propertyRuleBuilder1 = builder.UseFor(propertySelector1, lineNumber1).Ensure(x => x.Contains('a'));
        var propertyRuleBuilder2 = builder.UseFor(propertySelector2, lineNumber2).Ensure(x => x > 4);

        var validationCore = (CustomValidatorCore<TestModel, TestResources>)builder.CreateValidatorCore();

        Assert.NotNull(propertyRuleBuilder1);
        Assert.NotNull(propertyRuleBuilder2);
        Assert.Equal(2, validationCore.Components.Length);
        Assert.IsType<CustomValidatorPropertyRule<TestModel, TestResources, string>>(validationCore.Components[0]);
        Assert.IsType<CustomValidatorPropertyRule<TestModel, TestResources, int>>(validationCore.Components[1]);
    }

    [Fact]
    public void UseWhen_AddsConditionalFlowRuleAndExecutesAction_AndTheConditionalRule_HasCorrectSkipCount()
    {
        var builder = new CustomValidatorCoreBuilder<TestModel, TestResources>(ValidatorName);
        ValidationCondition<TestModel, TestResources> condition = (ctx) => true;
        int lineNumber = 500;

        // 1. Add 2 outside the conditional flow property rules
        builder.UseFor(m => m.MyProperty, 1).Ensure(x => x.Length > 0); // Rule 0
        builder.UseFor(m => m.MyIntProperty, 2).Ensure(x => x > 0);    // Rule 1

        // 2. Then UseWhen with 2 property rules inside.
        builder.UseWhen(condition, () =>
        {
            builder.UseFor(m => m.MyProperty, 3).Ensure(x => x.Contains('b')); // Inner Rule 0 (actual index 3, but logical skip index 0 for conditional block)
            builder.UseFor(m => m.MyIntProperty, 4).Ensure(x => x < 10);      // Inner Rule 1 (actual index 4, but logical skip index 1 for conditional block)
        }, lineNumber);

        // 3. Then we create validation core
        var validationCore = (CustomValidatorCore<TestModel, TestResources>)builder.CreateValidatorCore();

        // 4. Then we need to check if the ValidationRules length is 5, and none of the rules inside are null.
        // Expected rules order: [PropertyRule1, PropertyRule2, ConditionalRule, InnerPropertyRule1, InnerPropertyRule2]
        Assert.Equal(5, validationCore.Components.Length);
        foreach (var rule in validationCore.Components)
        {
            Assert.NotNull(rule);
        }

        // 5. Then we need to check if the Conditional flow rule is on index 2
        var conditionalRule = Assert.IsType<ConditionalFlowValidatorRule<TestModel, TestResources, CustomValidatorRunCtx<TestModel, TestResources>>>(validationCore.Components[2]);

        // 6. And is of type ConditionalFlowValidatorRule<TestModel, TestResources, ExpressValidatorRunCtx<TestModel, TestResources>> (already asserted)

        // 7. And the RulesToSkip is 2 (because 2 rules were defined inside the action)
        Assert.Equal(2, conditionalRule.RulesToSkip);

        // 8. And the IsOtherwise is false
        Assert.False(conditionalRule.IsOtherwise);

        // 9. And we need to check CanRunSynchronously is true
        Assert.True(conditionalRule.CanRunSynchronously);
    }

    [Fact]
    public void UseWhenAsync_AddsConditionalFlowRuleAndExecutesAction_AndTheConditionalRule_HasCorrectSkipCount()
    {
        var builder = new CustomValidatorCoreBuilder<TestModel, TestResources>(ValidatorName);
        AsyncValidationCondition<TestModel, TestResources> condition = (ctx, ct) => Task.FromResult(true);
        int lineNumber = 600;

        // 1. Add 2 outside the conditional flow property rules
        builder.UseFor(m => m.MyProperty, 1).Ensure(x => x.Length > 0);
        builder.UseFor(m => m.MyIntProperty, 2).Ensure(x => x > 0);

        // 2. Then UseWhenAsync with 2 property rules inside.
        builder.UseWhenAsync(condition, () =>
        {
            builder.UseFor(m => m.MyProperty, 3).Ensure(x => x.Contains('c'));
            builder.UseFor(m => m.MyIntProperty, 4).Ensure(x => x < 20);
        }, lineNumber);

        // 3. Then we create validation core
        var validationCore = (CustomValidatorCore<TestModel, TestResources>)builder.CreateValidatorCore();

        // 4. Then we need to check if the ValidationRules length is 5, and none of the rules inside are null.
        Assert.Equal(5, validationCore.Components.Length);
        foreach (var rule in validationCore.Components)
        {
            Assert.NotNull(rule);
        }

        // 5. Then we need to check if the Conditional flow rule is on index 2
        var conditionalRule = Assert.IsType<ConditionalFlowValidatorRule<TestModel, TestResources, CustomValidatorRunCtx<TestModel, TestResources>>>(validationCore.Components[2]);

        // 6. And the RulesToSkip is 2
        Assert.Equal(2, conditionalRule.RulesToSkip);

        // 7. And the IsOtherwise is false
        Assert.False(conditionalRule.IsOtherwise);

        // 8. And we need to check CanRunSynchronously is false.
        Assert.False(conditionalRule.CanRunSynchronously);
    }

    [Fact]
    public void OtherwiseUse_AddsConditionalFlowRule_AndSecondConditionalFlowRuleAndExecutesAction_AndBothTheConditionalRule_HasCorrectSkipCount()
    {
        var builder = new CustomValidatorCoreBuilder<TestModel, TestResources>(ValidatorName);
        ValidationCondition<TestModel, TestResources> whenCondition = (ctx) => true;
        int whenLineNumber = 500;
        int otherwiseLineNumber = 700;

        // Rule A (outside)
        builder.UseFor(m => m.MyProperty, 1).Ensure(x => x.Length > 0); // Rule 0

        // UseWhen block with 1 inner rule
        builder.UseWhen(whenCondition, () =>
        {
            builder.UseFor(m => m.MyIntProperty, 2).Ensure(x => x > 1); // Inner Rule A (skipped by conditionalRule1)
        }, whenLineNumber) // ConditionalRule1 will be at index 1, skips 1 rule.
        // OtherwiseUse block with 2 inner rules
        .OtherwiseUse(() =>
        {
            builder.UseFor(m => m.MyProperty, 3).Ensure(x => x.Contains('d')); // Inner Rule B (skipped by conditionalRule2)
            builder.UseFor(m => m.MyIntProperty, 4).Ensure(x => x < 5);       // Inner Rule C (skipped by conditionalRule2)
        }, otherwiseLineNumber); // ConditionalRule2 will be at index 3, skips 2 rules.

        // Rule B (outside)
        builder.UseFor(m => m.MyProperty, 5).Ensure(x => x.Length > 1); // Rule 5

        var validationCore = (CustomValidatorCore<TestModel, TestResources>)builder.CreateValidatorCore();

        // Expected rules: [RuleA, ConditionalRule1, InnerRuleA, ConditionalRule2, InnerRuleB, InnerRuleC, RuleB]
        Assert.Equal(7, validationCore.Components.Length);
        foreach (var rule in validationCore.Components)
        {
            Assert.NotNull(rule);
        }

        // Assert ConditionalRule1 (from UseWhen)
        var conditionalRule1 = Assert.IsType<ConditionalFlowValidatorRule<TestModel, TestResources, CustomValidatorRunCtx<TestModel, TestResources>>>(validationCore.Components[1]);
        Assert.Equal(1, conditionalRule1.RulesToSkip);
        Assert.False(conditionalRule1.IsOtherwise);
        Assert.True(conditionalRule1.CanRunSynchronously);
        Assert.Equal(whenCondition, conditionalRule1.Condition);

        // Assert ConditionalRule2 (from OtherwiseUse)
        var conditionalRule2 = Assert.IsType<ConditionalFlowValidatorRule<TestModel, TestResources, CustomValidatorRunCtx<TestModel, TestResources>>>(validationCore.Components[3]);
        Assert.Equal(2, conditionalRule2.RulesToSkip);
        Assert.True(conditionalRule2.IsOtherwise);
        Assert.True(conditionalRule2.CanRunSynchronously);
        Assert.Equal(whenCondition, conditionalRule2.Condition); // Should reference the *original* UseWhen condition
    }

    [Fact]
    public void OtherwiseUseAsync_AddsConditionalFlowRule_AndSecondConditionalFlowRuleAndExecutesAction_AndBothTheConditionalRule_HasCorrectSkipCount()
    {
        var builder = new CustomValidatorCoreBuilder<TestModel, TestResources>(ValidatorName);
        AsyncValidationCondition<TestModel, TestResources> whenConditionAsync = (ctx, ct) => Task.FromResult(true);
        int whenLineNumber = 600;
        int otherwiseLineNumber = 800;

        // Rule A (outside)
        builder.UseFor(m => m.MyProperty, 1).Ensure(x => x.Length > 0); // Rule 0

        // UseWhenAsync block with 1 inner rule
        builder.UseWhenAsync(whenConditionAsync, () =>
        {
            builder.UseFor(m => m.MyIntProperty, 2).Ensure(x => x > 1); // Inner Rule A
        }, whenLineNumber); // ConditionalRule1 will be at index 1, skips 1 rule.

        // OtherwiseUseAsync block with 2 inner rules
        builder.OtherwiseUseAsync(() =>
        {
            builder.UseFor(m => m.MyProperty, 3).Ensure(x => x.Contains('e')); // Inner Rule B
            builder.UseFor(m => m.MyIntProperty, 4).Ensure(x => x < 5);       // Inner Rule C
        }, otherwiseLineNumber); // ConditionalRule2 will be at index 3, skips 2 rules.

        // Rule B (outside)
        builder.UseFor(m => m.MyProperty, 5).Ensure(x => x.Length > 1); // Rule 5

        var validationCore = (CustomValidatorCore<TestModel, TestResources>)builder.CreateValidatorCore();

        // Expected rules: [RuleA, ConditionalRule1, InnerRuleA, ConditionalRule2, InnerRuleB, InnerRuleC, RuleB]
        Assert.Equal(7, validationCore.Components.Length);
        foreach (var rule in validationCore.Components)
        {
            Assert.NotNull(rule);
        }

        // Assert ConditionalRule1 (from UseWhenAsync)
        var conditionalRule1 = Assert.IsType<ConditionalFlowValidatorRule<TestModel, TestResources, CustomValidatorRunCtx<TestModel, TestResources>>>(validationCore.Components[1]);
        Assert.Equal(1, conditionalRule1.RulesToSkip);
        Assert.False(conditionalRule1.IsOtherwise);
        Assert.False(conditionalRule1.CanRunSynchronously);
        Assert.Equal(whenConditionAsync, conditionalRule1.AsyncCondition);

        // Assert ConditionalRule2 (from OtherwiseUseAsync)
        var conditionalRule2 = Assert.IsType<ConditionalFlowValidatorRule<TestModel, TestResources, CustomValidatorRunCtx<TestModel, TestResources>>>(validationCore.Components[3]);
        Assert.Equal(2, conditionalRule2.RulesToSkip);
        Assert.True(conditionalRule2.IsOtherwise);
        Assert.False(conditionalRule2.CanRunSynchronously);
        Assert.Equal(whenConditionAsync, conditionalRule2.AsyncCondition); // Should reference the *original* UseWhenAsync condition
    }

    [Fact]
    public void NestedConditionalFlowRules_ResolveCorrectly_AndAllHaveTheCorrectSkipCount()
    {
        var builder = new CustomValidatorCoreBuilder<TestModel, TestResources>(ValidatorName);
        ValidationCondition<TestModel, TestResources> outerCondition = (ctx) => true;
        ValidationCondition<TestModel, TestResources> innerCondition = (ctx) => true; // Using the same innerCondition for simplicity

        // Rule 0 (outside all conditionals)        
        builder.UseFor(m => m.MyProperty).Ensure(x => x.Length > 0);

        // Conditional Block 1: Outer UseWhen branch
        builder.UseWhen(outerCondition, () =>
        {
            // Rule 2 (inside outer UseWhen, before inner block)
            builder.UseFor(m => m.MyIntProperty).Ensure(x => x > 0);

            // Conditional Block 2: Nested UseWhen/OtherwiseUse
            builder.UseWhen(innerCondition, () =>
            {
                // Rules inside inner UseWhen (Rule 4, 5)
                builder.UseFor(m => m.MyProperty).Ensure(x => x.Contains('f'));
                builder.UseFor(m => m.MyIntProperty).Ensure(x => x < 100);
            })
            .OtherwiseUse(() =>
            {
                // Rules inside inner OtherwiseUse (Rule 7, 8, 9)
                builder.UseFor(m => m.MyProperty).Ensure(x => x.StartsWith("test"));
                builder.UseFor(m => m.MyIntProperty).Ensure(x => x != 0);
                builder.UseFor(m => m.MyProperty).Ensure(x => x.EndsWith("val"));
            }); // Inner conditional block ends here. Inner Conditional Rule is at Index 3, Inner Otherwise Rule is at Index 6

            // Rule 10 (inside outer UseWhen, after inner block)
            builder.UseFor(m => m.MyProperty).Ensure(x => x.Length > 1);

        }) // Outer UseWhen block ends here. Outer Conditional Rule is at Index 1
        .OtherwiseUse(() =>
        {
            // Rules inside outer OtherwiseUse (Rule 12, 13)
            builder.UseFor(m => m.MyProperty).Ensure(x => x.Equals("Else"));
            builder.UseFor(m => m.MyIntProperty).Ensure(x => x == 99);

            // Conditional Block 3: Another nested UseWhen/OtherwiseUse in the outer OtherwiseUse branch
            builder.UseWhen(innerCondition, () =>
            {
                // Rules inside inner UseWhen of outer OtherwiseUse (Rule 15)
                builder.UseFor(m => m.MyProperty).Ensure(x => x.Length == 5);
            })
            .OtherwiseUse(() =>
            {
                // Rules inside inner OtherwiseUse of outer OtherwiseUse (Rule 17, 18)
                builder.UseFor(m => m.MyIntProperty).Ensure(x => x > 50);
                builder.UseFor(m => m.MyProperty).Ensure(x => x.Contains("z"));
            }); // Conditional Block 3 ends here. Conditional Rule is at Index 14, Otherwise Rule is at Index 16

            // Rule 19 (inside outer OtherwiseUse, after its inner block)
            builder.UseFor(m => m.MyIntProperty).Ensure(x => x > 1000);
        }); // Outer OtherwiseUse block ends here. Outer Otherwise Rule is at Index 11

        // Rule 20 (outside all conditionals, at the very end)
        builder.UseFor(m => m.MyProperty).Ensure(x => x.Length > 2);

        var validationCore = (CustomValidatorCore<TestModel, TestResources>)builder.CreateValidatorCore();

        // Total Expected Rules: 21 (1 initial + 1 (OuterWhen) + 1 (Rule2) + 1 (InnerWhen) + 2 (InnerWhen rules) + 1 (InnerOtherwise) + 3 (InnerOtherwise rules) + 1 (Rule10) + 1 (OuterOtherwise) + 2 (OuterOtherwise rules) + 1 (InnerWhen) + 1 (InnerWhen rule) + 1 (InnerOtherwise) + 2 (InnerOtherwise rules) + 1 (Rule19) + 1 (FinalRule))
        // Let's count them carefully:
        // Rule 0 (PropertyRule)
        // Rule 1 (Outer Conditional Rule - UseWhen)
        // Rule 2 (PropertyRule)
        // Rule 3 (Inner Conditional Rule - UseWhen)
        // Rule 4 (PropertyRule)
        // Rule 5 (PropertyRule)
        // Rule 6 (Inner Conditional Rule - OtherwiseUse)
        // Rule 7 (PropertyRule)
        // Rule 8 (PropertyRule)
        // Rule 9 (PropertyRule)
        // Rule 10 (PropertyRule)
        // Rule 11 (Outer Conditional Rule - OtherwiseUse)
        // Rule 12 (PropertyRule)
        // Rule 13 (PropertyRule)
        // Rule 14 (Inner Conditional Rule - UseWhen in Otherwise branch)
        // Rule 15 (PropertyRule)
        // Rule 16 (Inner Conditional Rule - OtherwiseUse in Otherwise branch)
        // Rule 17 (PropertyRule)
        // Rule 18 (PropertyRule)
        // Rule 19 (PropertyRule)
        // Rule 20 (PropertyRule)

        Assert.Equal(21, validationCore.Components.Length);

        // Verify no null rules
        foreach (var rule in validationCore.Components)
        {
            Assert.NotNull(rule);
        }

        // --- Assertions for Rule Types and Indices ---
        Assert.IsType<CustomValidatorPropertyRule<TestModel, TestResources, string>>(validationCore.Components[0]); // Rule 0

        // Outer UseWhen Conditional Block (Index 1)
        var outerUseWhenRule = Assert.IsType<ConditionalFlowValidatorRule<TestModel, TestResources, CustomValidatorRunCtx<TestModel, TestResources>>>(validationCore.Components[1]);
        Assert.IsType<CustomValidatorPropertyRule<TestModel, TestResources, int>>(validationCore.Components[2]);    // Rule 2

        // Nested UseWhen/OtherwiseUse within Outer UseWhen (Index 3 and 6)
        var innerUseWhenRule = Assert.IsType<ConditionalFlowValidatorRule<TestModel, TestResources, CustomValidatorRunCtx<TestModel, TestResources>>>(validationCore.Components[3]);
        Assert.IsType<CustomValidatorPropertyRule<TestModel, TestResources, string>>(validationCore.Components[4]); // Rule 4
        Assert.IsType<CustomValidatorPropertyRule<TestModel, TestResources, int>>(validationCore.Components[5]);    // Rule 5
        var innerOtherwiseUseRule = Assert.IsType<ConditionalFlowValidatorRule<TestModel, TestResources, CustomValidatorRunCtx<TestModel, TestResources>>>(validationCore.Components[6]);
        Assert.IsType<CustomValidatorPropertyRule<TestModel, TestResources, string>>(validationCore.Components[7]); // Rule 7
        Assert.IsType<CustomValidatorPropertyRule<TestModel, TestResources, int>>(validationCore.Components[8]);    // Rule 8
        Assert.IsType<CustomValidatorPropertyRule<TestModel, TestResources, string>>(validationCore.Components[9]); // Rule 9

        Assert.IsType<CustomValidatorPropertyRule<TestModel, TestResources, string>>(validationCore.Components[10]); // Rule 10

        // Outer OtherwiseUse Conditional Block (Index 11)
        var outerOtherwiseUseRule = Assert.IsType<ConditionalFlowValidatorRule<TestModel, TestResources, CustomValidatorRunCtx<TestModel, TestResources>>>(validationCore.Components[11]);
        Assert.IsType<CustomValidatorPropertyRule<TestModel, TestResources, string>>(validationCore.Components[12]); // Rule 12
        Assert.IsType<CustomValidatorPropertyRule<TestModel, TestResources, int>>(validationCore.Components[13]);    // Rule 13

        // Nested UseWhen/OtherwiseUse within Outer OtherwiseUse (Index 14 and 16)
        var innerUseWhenOtherwiseBranchRule = Assert.IsType<ConditionalFlowValidatorRule<TestModel, TestResources, CustomValidatorRunCtx<TestModel, TestResources>>>(validationCore.Components[14]);
        Assert.IsType<CustomValidatorPropertyRule<TestModel, TestResources, string>>(validationCore.Components[15]); // Rule 15
        var innerOtherwiseOtherwiseBranchRule = Assert.IsType<ConditionalFlowValidatorRule<TestModel, TestResources, CustomValidatorRunCtx<TestModel, TestResources>>>(validationCore.Components[16]);
        Assert.IsType<CustomValidatorPropertyRule<TestModel, TestResources, int>>(validationCore.Components[17]);    // Rule 17
        Assert.IsType<CustomValidatorPropertyRule<TestModel, TestResources, string>>(validationCore.Components[18]); // Rule 18

        Assert.IsType<CustomValidatorPropertyRule<TestModel, TestResources, int>>(validationCore.Components[19]);    // Rule 19

        Assert.IsType<CustomValidatorPropertyRule<TestModel, TestResources, string>>(validationCore.Components[20]); // Rule 20

        // --- Checks for Predicates, IsOtherwise, CanRunSynchronously, and RulesToSkip ---

        // Outer UseWhen Rule (at index 1)
        Assert.Equal(outerCondition, outerUseWhenRule.Condition);
        Assert.False(outerUseWhenRule.IsOtherwise);
        Assert.True(outerUseWhenRule.CanRunSynchronously);
        // Rules in its block: Rule 2 (1), InnerUseWhen (1), InnerWhen rules (2), InnerOtherwise (1), InnerOtherwise rules (3), Rule 10 (1) = 1+1+2+1+3+1 = 9 rules
        Assert.Equal(9, outerUseWhenRule.RulesToSkip);

        // Inner UseWhen Rule (at index 3)
        Assert.Equal(innerCondition, innerUseWhenRule.Condition);
        Assert.False(innerUseWhenRule.IsOtherwise);
        Assert.True(innerUseWhenRule.CanRunSynchronously);
        // Rules in its block: Rule 4 (1), Rule 5 (1) = 2 rules
        Assert.Equal(2, innerUseWhenRule.RulesToSkip);

        // Inner OtherwiseUse Rule (at index 6)
        Assert.Equal(innerCondition, innerOtherwiseUseRule.Condition); // Should chain from the preceding innerUseWhenRule's condition
        Assert.True(innerOtherwiseUseRule.IsOtherwise);
        Assert.True(innerOtherwiseUseRule.CanRunSynchronously);
        // Rules in its block: Rule 7 (1), Rule 8 (1), Rule 9 (1) = 3 rules
        Assert.Equal(3, innerOtherwiseUseRule.RulesToSkip);

        // Outer OtherwiseUse Rule (at index 11)
        Assert.Equal(outerCondition, outerOtherwiseUseRule.Condition); // Should chain from the initial outerUseWhenRule's condition
        Assert.True(outerOtherwiseUseRule.IsOtherwise);
        Assert.True(outerOtherwiseUseRule.CanRunSynchronously);
        // Rules in its block: Rule 12 (1), Rule 13 (1), InnerUseWhenOtherwiseBranch (1), InnerWhenOtherwiseBranch rule (1), InnerOtherwiseOtherwiseBranch (1), InnerOtherwiseOtherwiseBranch rules (2), Rule 19 (1) = 1+1+1+1+1+2+1 = 8 rules
        Assert.Equal(8, outerOtherwiseUseRule.RulesToSkip);

        // Inner UseWhen in Outer OtherwiseUse branch (at index 14)
        Assert.Equal(innerCondition, innerUseWhenOtherwiseBranchRule.Condition);
        Assert.False(innerUseWhenOtherwiseBranchRule.IsOtherwise);
        Assert.True(innerUseWhenOtherwiseBranchRule.CanRunSynchronously);
        // Rules in its block: Rule 15 (1) = 1 rule
        Assert.Equal(1, innerUseWhenOtherwiseBranchRule.RulesToSkip);

        // Inner OtherwiseUse in Outer OtherwiseUse branch (at index 16)
        Assert.Equal(innerCondition, innerOtherwiseOtherwiseBranchRule.Condition); // Should chain from the preceding innerUseWhenOtherwiseBranchRule's condition
        Assert.True(innerOtherwiseOtherwiseBranchRule.IsOtherwise);
        Assert.True(innerOtherwiseOtherwiseBranchRule.CanRunSynchronously);
        // Rules in its block: Rule 17 (1), Rule 18 (1) = 2 rules
        Assert.Equal(2, innerOtherwiseOtherwiseBranchRule.RulesToSkip);
    }
}

public class TestResources { public string ResourceValue { get; set; } = "Resource"; }

public class TestModel
{
    public string MyProperty { get; set; } = "Value";
    public int MyIntProperty { get; set; } = 123;
}