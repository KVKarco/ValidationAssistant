using KVKarco.ValidationAssistant.Abstractions;
using KVKarco.ValidationAssistant.Exceptions;
using KVKarco.ValidationAssistant.Internal.PreValidation;
using KVKarco.ValidationAssistant.Internal.ValidationFlow;
using System.Runtime.CompilerServices;

namespace KVKarco.ValidationAssistant.Internal;

/// <summary>
/// Represents the abstract base class for a validator core builder. This class provides
/// the foundational structure and common functionalities for defining and collecting
/// pre-validation rules. It also manages global validation strategies for the builder.
/// Concrete implementations will extend this to include the core validation rule definitions.
/// </summary>
/// <typeparam name="T">The type of the main instance being validated.</typeparam>
/// <typeparam name="TExternalResources">The type of external resources available during validation.</typeparam>
/// <typeparam name="TContext">The specific type of <see cref="ValidatorRunCtx{T, TExternalResources}"/>
/// that this builder will use when compiling the validator core.</typeparam>
internal abstract class ValidatorCoreBuilder<T, TExternalResources, TContext> :
    IPreValidationDefinitionBuilder<T, TExternalResources>,
    IConditionalFlowRuleBuilder<T, TExternalResources>, // Added for UseWhen/UseWhenAsync
    IOtherwiseConditionalFlowRuleBuilder<T, TExternalResources>,
    IOtherwiseConditionalAsyncFlowRuleBuilder<T, TExternalResources>
    where TContext : ValidatorRunCtx<T, TExternalResources>
{
    /// <summary>
    /// The name of the validator being built. This name is used for identification and reporting.
    /// </summary>
    protected readonly string _validatorName;

    /// <summary>
    /// A list of snapshot identifiers defined within this validator. Snapshots allow
    /// capturing the validation state at specific points for conditional rule execution.
    /// </summary>
    protected readonly List<string> _snapShots;

    /// <summary>
    /// A list of <see cref="IPreValidationRule{T, TExternalResources}"/> instances that will be
    /// executed before the main validation rules. These rules are typically used for
    /// initial checks or prerequisites.
    /// </summary>
    protected readonly List<IPreValidationRule<T, TExternalResources>> _preValidationRules;

    /// <summary>
    /// A list of <see cref="IValidatorRule{T, TExternalResources, TContext}"/> instances that constitute
    /// the main validation logic. This list will be populated by the concrete builder's
    /// implementation of <see cref="ICoreValidationDefinitionBuilder{T, TExternalResources}"/>.
    /// </summary>
    protected readonly List<IValidatorRule<T, TExternalResources, TContext>> _rules;

    /// <summary>
    /// Holds the <see cref="IBuildableRule{T, TExternalResources, TContext}"/> instance that is currently
    /// being configured via the fluent API. This allows subsequent fluent calls to apply configurations
    /// to the most recently defined rule. It is set to <see langword="null"/> after the rule is built and added.
    /// </summary>
    protected IBuildableRule<T, TExternalResources, TContext>? _ruleToBeAdded;

    /// <summary>
    /// Stores the condition delegate (sync or async) from the correct UseWhen/UseWhenAsync call.
    /// This is used by the subsequent OtherwiseUse/OtherwiseUseAsync call to create the corresponding
    /// ConditionalFlowValidatorRule. This field supports the chaining of conditional blocks.
    /// </summary>
    protected Delegate? _conditionToChain;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidatorCoreBuilder{T, TExternalResources, TContext}"/> class.
    /// </summary>
    /// <param name="validatorName">The name of the validator being built.</param>
    protected ValidatorCoreBuilder(string validatorName)
    {
        _validatorName = validatorName;
        _snapShots = [];
        _preValidationRules = [];
        _rules = []; // Initialize the new _rules list

        // Initialize default failure strategies from global configuration.
        RuleFailureStrategy = ValidatorsConfig.GlobalDefaults.OnRuleFailure;
        RuleComponentsFailureStrategy = ValidatorsConfig.GlobalDefaults.OnComponentFailure;
    }

    public RuleFailureStrategy RuleFailureStrategy { get; set; }

    public ComponentFailureStrategy RuleComponentsFailureStrategy { get; set; }

    /// <inheritdoc/>
    public void DefaultRuleFailureStrategy(RuleFailureStrategy ruleFailureStrategy)
        => RuleFailureStrategy = ruleFailureStrategy;

    /// <inheritdoc/>
    public void DefaultComponentFailureStrategy(ComponentFailureStrategy componentFailureStrategy)
        => RuleComponentsFailureStrategy = componentFailureStrategy;

    /// <inheritdoc/>
    public void Ensure(
        PreValidationPredicate<T> predicate,
        string? explanationMessage = null,
        [CallerLineNumber] int callingFileLineNumber = 0)
    {
        RuleCreationException.ThrowIfNull(predicate);

        // Create a new MainInstancePreValidationRule and add it to the list of pre-validation rules.
        MainInstancePreValidationRule<T, TExternalResources> rule = new(_validatorName, callingFileLineNumber, predicate, explanationMessage);
        _preValidationRules.Add(rule);
    }

    /// <inheritdoc/>
    public void EnsureAsync(
        AsyncPreValidationPredicate<T> predicate,
        string? explanationMessage = null,
        [CallerLineNumber] int callingFileLineNumber = 0)
    {
        RuleCreationException.ThrowIfNull(predicate);

        // Create a new MainInstancePreValidationRule (asynchronous version) and add it.
        MainInstancePreValidationRule<T, TExternalResources> rule = new(_validatorName, callingFileLineNumber, predicate, explanationMessage);
        _preValidationRules.Add(rule);
    }

    /// <inheritdoc/>
    public void EnsureResources(
        PreValidationPredicate<TExternalResources> predicate,
        string? explanationMessage = null,
        [CallerLineNumber] int callingFileLineNumber = 0)
    {
        RuleCreationException.ThrowIfNull(predicate);

        // Create a new ResourcesPreValidationRule and add it to the list of pre-validation rules.
        ResourcesPreValidationRule<T, TExternalResources> rule = new(_validatorName, callingFileLineNumber, predicate, explanationMessage);
        _preValidationRules.Add(rule);
    }

    /// <inheritdoc/>
    public void EnsureResourcesAsync(
        AsyncPreValidationPredicate<TExternalResources> predicate,
        string? explanationMessage = null,
        [CallerLineNumber] int callingFileLineNumber = 0)
    {
        RuleCreationException.ThrowIfNull(predicate);

        // Create a new ResourcesPreValidationRule (asynchronous version) and add it.
        ResourcesPreValidationRule<T, TExternalResources> rule = new(_validatorName, callingFileLineNumber, predicate, explanationMessage);
        _preValidationRules.Add(rule);
    }

    /// <inheritdoc/>
    public IOtherwiseConditionalFlowRuleBuilder<T, TExternalResources> UseWhen(
        ValidationCondition<T, TExternalResources> condition,
        Action rulesToUseWhenConditionIsMet,
        [CallerLineNumber] int callingFileLineNumber = 0)
    {
        ResolveLastRule();

        RuleCreationException.ThrowIfNull(condition);
        RuleCreationException.ThrowIfNull(rulesToUseWhenConditionIsMet);

        int reservationIndex = ReserveSlot(); // Reserve a spot for the ConditionalFlowValidatorRule
        rulesToUseWhenConditionIsMet(); // Execute the action to populate rules for the 'when' block
        ResolveLastRule(); // Resolve the last rule added from the action

        // Create the ConditionalFlowValidatorRule and place it in the reserved slot
        SetRuleToReservedSlot(
            new ConditionalFlowValidatorRule<T, TExternalResources, TContext>(
                _validatorName.AsSpan(), // Convert string to ReadOnlySpan<char>
                _rules.Count - reservationIndex - 1, // This is the skip count if condition is false
                false, // This is a 'when' block
                condition,
                callingFileLineNumber),
            reservationIndex);

        _conditionToChain = condition; // Store the condition for a potential OtherwiseUse call

        return this; // Return 'this' to allow chaining to OtherwiseUse
    }

    /// <inheritdoc/>
    public IOtherwiseConditionalAsyncFlowRuleBuilder<T, TExternalResources> UseWhenAsync(
        AsyncValidationCondition<T, TExternalResources> condition,
        Action rulesToUseWhenConditionIsMet,
        [CallerLineNumber] int callingFileLineNumber = 0)
    {
        ResolveLastRule();

        RuleCreationException.ThrowIfNull(condition);
        RuleCreationException.ThrowIfNull(rulesToUseWhenConditionIsMet);

        int reservationIndex = ReserveSlot(); // Reserve a spot for the ConditionalFlowValidatorRule
        rulesToUseWhenConditionIsMet(); // Execute the action to populate rules for the 'when' block
        ResolveLastRule(); // Resolve the last rule added from the action

        // Create the ConditionalFlowValidatorRule and place it in the reserved slot
        SetRuleToReservedSlot(
            new ConditionalFlowValidatorRule<T, TExternalResources, TContext>(
                _validatorName.AsSpan(), // Convert string to ReadOnlySpan<char>
                _rules.Count - reservationIndex - 1, // This is the skip count if condition is false
                false, // This is a 'when' block
                condition, // Pass the AsyncValidationCondition
                callingFileLineNumber),
            reservationIndex);

        _conditionToChain = condition; // Store the condition for a potential OtherwiseUseAsync call

        return this; // Return 'this' to allow chaining to OtherwiseUseAsync
    }

    /// <inheritdoc/>
    public void OtherwiseUse(
        Action rulesToUseWhenConditionIsNotMet,
        [CallerLineNumber] int callingFileLineNumber = 0)
    {
        ResolveLastRule(); // Resolve any rule immediately preceding OtherwiseUse

        RuleCreationException.ThrowIfNull(rulesToUseWhenConditionIsNotMet);

        // Ensure a UseWhen was called previously and the condition is synchronous
        if (_conditionToChain is not ValidationCondition<T, TExternalResources> syncCondition)
        {
            throw new ValidationAssistantInternalException("OtherwiseUse must follow a synchronous UseWhen call.");
        }

        int reservationIndex = ReserveSlot(); // Reserve a spot for the ConditionalFlowValidatorRule

        rulesToUseWhenConditionIsNotMet(); // Execute the action to populate rules for the 'otherwise' block
        ResolveLastRule(); // Resolve the last rule added from the action

        // Create and place the 'Otherwise' ConditionalFlowValidatorRule in the reserved slot.
        // This rule will be executed if the 'when' condition was false.
        SetRuleToReservedSlot(
            new ConditionalFlowValidatorRule<T, TExternalResources, TContext>(
                _validatorName.AsSpan(),
                _rules.Count - reservationIndex - 1, // This is the skip count if condition is true
                true, // This is an 'otherwise' block
                syncCondition, // Use the original synchronous condition
                callingFileLineNumber),
            reservationIndex); // Place it at the start of the 'otherwise' block

        _conditionToChain = null; // Clear the chained condition as the block is complete
    }

    /// <inheritdoc/>
    public void OtherwiseUseAsync(
        Action rulesToUseWhenConditionIsNotMet,
        [CallerLineNumber] int callingFileLineNumber = 0)
    {
        ResolveLastRule(); // Resolve any rule immediately preceding OtherwiseUseAsync

        RuleCreationException.ThrowIfNull(rulesToUseWhenConditionIsNotMet);

        // Ensure a UseWhenAsync was called previously and the condition is asynchronous
        if (_conditionToChain is not AsyncValidationCondition<T, TExternalResources> asyncCondition)
        {
            throw new ValidationAssistantInternalException("OtherwiseUseAsync must follow an asynchronous UseWhenAsync call.");
        }

        int reservationIndex = ReserveSlot(); // Reserve a spot for the ConditionalFlowValidatorRule

        rulesToUseWhenConditionIsNotMet(); // Execute the action to populate rules for the 'otherwise' block
        ResolveLastRule(); // Resolve the last rule added from the action

        // Create and place the 'Otherwise' ConditionalFlowValidatorRule in the reserved slot.
        // This rule will be executed if the 'when' condition was false.
        SetRuleToReservedSlot(
            new ConditionalFlowValidatorRule<T, TExternalResources, TContext>(
                _validatorName.AsSpan(),
                _rules.Count - reservationIndex - 1, // This is the skip count if 'otherwise' condition is true (meaning 'when' was true)
                true, // This is an 'otherwise' block
                asyncCondition, // Use the original asynchronous condition
                callingFileLineNumber),
            reservationIndex); // Place it at the start of the 'otherwise' block

        _conditionToChain = null; // Clear the chained condition as the block is complete
    }

    /// <summary>
    /// Abstract method that concrete builder implementations must override to create
    /// and return the final <see cref="ValidatorCore"/> instance, encapsulating all
    /// defined pre-validation and core validation rules.
    /// </summary>
    /// <returns>A compiled <see cref="ValidatorCore"/> instance.</returns>
    internal abstract ValidatorCore CreateValidatorCore();

    /// <summary>
    /// Reserves a slot in the main rules list for a rule that will be placed later (e.g., a conditional flow rule).
    /// This prevents issues with index shifts when rules are added dynamically within fluent chains.
    /// </summary>
    /// <returns>The index of the reserved slot.</returns>
    protected virtual int ReserveSlot()
    {
        int reservationIndex = _rules.Count;
        _rules.Add(default!); // Add a default/null placeholder
        return reservationIndex;
    }

    /// <summary>
    /// Resolves the last rule that was being built via the fluent API and adds it
    /// to the internal list of validation rules. This method is typically called
    /// implicitly by the fluent API whenever a new rule definition begins,
    /// or explicitly when the rule definition is complete.
    /// </summary>
    protected virtual void ResolveLastRule()
    {
        // Clear the chained condition when a new rule is resolved, as it marks the end of a conditional block chain.
        _conditionToChain = null;
        if (_ruleToBeAdded is not null)
        {
            // Capture the builder instance and null out the field to prepare for the next rule.
            IBuildableRule<T, TExternalResources, TContext> builder = _ruleToBeAdded;
            _ruleToBeAdded = null;

            // Build the concrete ValidatorRule from the IBuildableRule and add it to the list.
            _rules.Add(builder.Build());
        }
    }

    /// <summary>
    /// Sets a compiled validator rule into a previously reserved slot in the main rules list,
    /// or adds it to the end if no reservation index is provided.
    /// </summary>
    /// <param name="rule">The compiled validator rule to place.</param>
    /// <param name="reservationIndex">Optional. The index of the reserved slot. If null, the rule is added to the end.</param>
    protected virtual void SetRuleToReservedSlot(IValidatorRule<T, TExternalResources, TContext> rule, int? reservationIndex = null)
    {
        if (reservationIndex is not null)
        {
            _rules[reservationIndex.Value] = rule;
        }
        else
        {
            _rules.Add(rule);
        }
    }
}
