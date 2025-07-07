using KVKarco.ValidationAssistant.Abstractions;
using KVKarco.ValidationAssistant.Exceptions;
using KVKarco.ValidationAssistant.Internal.PreValidation;
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
    IPreValidationDefinitionBuilder<T, TExternalResources>
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
    /// A list of <see cref="ValidatorRule{T, TExternalResources, TContext}"/> instances that constitute
    /// the main validation logic. This list will be populated by the concrete builder's
    /// implementation of <see cref="ICoreValidationDefinitionBuilder{T, TExternalResources}"/>.
    /// </summary>
    protected readonly List<ValidatorRule<T, TExternalResources, TContext>> _validationRules;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidatorCoreBuilder{T, TExternalResources, TContext}"/> class.
    /// </summary>
    /// <param name="validatorName">The name of the validator being built.</param>
    protected ValidatorCoreBuilder(string validatorName)
    {
        _validatorName = validatorName;
        _snapShots = [];
        _preValidationRules = [];
        _validationRules = [];

        // Initialize default failure strategies from global configuration.
        RuleFailureStrategy = ValidatorsConfig.GlobalDefaults.OnRuleFailure;
        RuleComponentsFailureStrategy = ValidatorsConfig.GlobalDefaults.OnComponentFailure;
    }

    /// <summary>
    /// Gets or sets the default strategy to apply when a validation rule fails.
    /// This strategy dictates how the validation process should continue or stop.
    /// </summary>
    public RuleFailureStrategy RuleFailureStrategy { get; set; }

    /// <summary>
    /// Gets or sets the default strategy to apply when a component within a property rule fails.
    /// This strategy dictates how the property rule's component execution should continue or exit.
    /// </summary>
    public ComponentFailureStrategy RuleComponentsFailureStrategy { get; set; }

    /// <summary>
    /// Defines a synchronous pre-validation rule that checks a predicate against the main validation instance.
    /// If the predicate returns <see langword="false"/>, a pre-validation failure is recorded.
    /// </summary>
    /// <param name="predicate">The synchronous predicate function to execute against the main instance.</param>
    /// <param name="explanationMessage">An optional explanation message for the failure. If null, a default message is used.</param>
    /// <param name="callingFileLineNumber">The line number in the source file where this method was called, used for debugging and reporting.</param>
    /// <exception cref="RuleCreationException">Thrown if the <paramref name="predicate"/> is <see langword="null"/>.</exception>
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

    /// <summary>
    /// Defines an asynchronous pre-validation rule that checks a predicate against the main validation instance.
    /// If the predicate returns <see langword="false"/>, a pre-validation failure is recorded.
    /// </summary>
    /// <param name="predicate">The asynchronous predicate function to execute against the main instance.</param>
    /// <param name="explanationMessage">An optional explanation message for the failure. If null, a default message is used.</param>
    /// <param name="callingFileLineNumber">The line number in the source file where this method was called, used for debugging and reporting.</param>
    /// <exception cref="RuleCreationException">Thrown if the <paramref name="predicate"/> is <see langword="null"/>.</exception>
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

    /// <summary>
    /// Defines a synchronous pre-validation rule that checks a predicate against the external resources.
    /// If the predicate returns <see langword="false"/>, a pre-validation failure is recorded.
    /// </summary>
    /// <param name="predicate">The synchronous predicate function to execute against the external resources.</param>
    /// <param name="explanationMessage">An optional explanation message for the failure. If null, a default message is used.</param>
    /// <param name="callingFileLineNumber">The line number in the source file where this method was called, used for debugging and reporting.</param>
    /// <exception cref="RuleCreationException">Thrown if the <paramref name="predicate"/> is <see langword="null"/>.</exception>
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

    /// <summary>
    /// Defines an asynchronous pre-validation rule that checks a predicate against the external resources.
    /// If the predicate returns <see langword="false"/>, a pre-validation failure is recorded.
    /// </summary>
    /// <param name="predicate">The asynchronous predicate function to execute against the external resources.</param>
    /// <param name="explanationMessage">An optional explanation message for the failure. If null, a default message is used.</param>
    /// <param name="callingFileLineNumber">The line number in the source file where this method was called, used for debugging and reporting.</param>
    /// <exception cref="RuleCreationException">Thrown if the <paramref name="predicate"/> is <see langword="null"/>.</exception>
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
}