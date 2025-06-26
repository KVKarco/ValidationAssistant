using KVKarco.ValidationAssistant.Exceptions;
using KVKarco.ValidationAssistant.Internal.PreValidation;
using KVKarco.ValidationAssistant.Internal.PropertyValidation;
using KVKarco.ValidationAssistant.Internal.ValidationFlow;
using System.Collections.Immutable;
using System.Globalization;

namespace KVKarco.ValidationAssistant.Internal;

/// <summary>
/// Represents the abstract base class for a validator's execution context.
/// This context holds state and provides utilities during a validation run,
/// allowing validators to track progress, access culture information,
/// and manage validation results and failures.
/// </summary>
public abstract class ValidatorRunCtx
{
    /// <summary>
    /// Stores the name or identifier of the validator from which this context originated.
    /// </summary>
    private protected readonly string _fromValidator;

    /// <summary>
    /// Stores information about the most recent rule failure encountered during the validation run.
    /// This is used to track the specific <see cref="RuleFailure"/> instance.
    /// </summary>
    private protected RuleFailure? _currentRuleFailure;

    /// <summary>
    /// Stores metadata about the configuration of the most recent rule failure.
    /// This includes strategy for stopping or exiting rules.
    /// </summary>
    private protected RuleFailureInfo? _currentRuleFailureInfo;

    /// <summary>
    /// Stores metadata about the configuration of the most recent validation failure
    /// that applies to a specific property.
    /// </summary>
    private protected ComponentFailureInfo? _currentValidationFailureInfo;

    /// <summary>
    /// Tracks the total count of validation failures accumulated during the entire validation run.
    /// </summary>
    private protected int _totalValidationFailures;

    /// <summary>
    /// Tracks the count of failures for the currently executing property rule.
    /// </summary>
    private protected int _currentPropertyRuleFailures;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidatorRunCtx"/> class.
    /// This constructor sets up the foundational context for a validation run.
    /// </summary>
    /// <param name="fromValidator">The name or identifier of the validator initiating this context.</param>
    /// <param name="culture">The <see cref="CultureInfo"/> to be used for validation, especially for message formatting.</param>
    /// <param name="parentContext">An optional reference to a parent <see cref="ValidatorRunCtx"/> for nested validations.</param>
    /// <param name="result">The <see cref="ValidatorRunResult"/> instance where validation outcomes will be recorded.</param>
    private protected ValidatorRunCtx(string fromValidator, CultureInfo culture, ValidatorRunCtx? parentContext, ValidatorRunResult result)
    {
        _fromValidator = fromValidator;
        Culture = culture;
        ParentContext = parentContext;
        Result = result;

        _totalValidationFailures = 0;
        _currentPropertyRuleFailures = 0;
    }

    /// <summary>
    /// Gets the <see cref="CultureInfo"/> associated with the current validation run.
    /// </summary>
    public CultureInfo Culture { get; }

    /// <summary>
    /// Gets the name of the property currently being validated as a read-only span of characters.
    /// This property must be implemented by derived classes.
    /// </summary>
    public abstract ReadOnlySpan<char> PropertyName { get; }

    /// <summary>
    /// Gets an optional reference to the parent validation context if this is a nested validation run.
    /// </summary>
    internal ValidatorRunCtx? ParentContext { get; }

    /// <summary>
    /// Gets the <see cref="ValidatorRunResult"/> where all validation successes and failures are aggregated.
    /// </summary>
    internal ValidatorRunResult Result { get; }

    /// <summary>
    /// Gets a value indicating whether the overall validation run is currently considered valid (i.e., no total failures recorded).
    /// </summary>
    internal bool IsRunValid => _totalValidationFailures == 0;

    /// <summary>
    /// Gets the correct or full path of the property currently being validated within the object graph.
    /// This property must be implemented by derived classes.
    /// </summary>
    internal abstract string CorrectPropertyPath { get; }
}

/// <summary>
/// Represents the generic base class for a validator's execution context,
/// providing access to the instance being validated and any external resources.
/// </summary>
/// <typeparam name="T">The type of the main instance being validated.</typeparam>
/// <typeparam name="TExternalResources">The type of external resources available during validation.</typeparam>
public abstract class ValidatorRunCtx<T, TExternalResources> :
    ValidatorRunCtx,
    IConditionCtx<T, TExternalResources>,
    IMessageCtx<T, TExternalResources>
{
    /// <summary>
    /// Stores the state of available validation snapshots, mapped by their identifier.
    /// Each tuple contains the snapshot identifier and a nullable boolean indicating its validity.
    /// A null value means the snapshot result has not yet been calculated.
    /// </summary>
    private readonly (string identifaer, bool? isValid)[]? _availableSnapShots;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidatorRunCtx{T, TExternalResources}"/> class.
    /// This constructor sets up the context with the specific validation instance and resources.
    /// </summary>
    /// <param name="availableSnapShots">An optional immutable array of snapshot identifiers that this validator expects to capture.
    /// Each identified snapshot's validity will initially be set to null.</param>
    /// <param name="value">The instance of <typeparamref name="T"/> currently being validated.</param>
    /// <param name="resources">The external resources of type <typeparamref name="TExternalResources"/> available during validation.</param>
    /// <param name="fromValidator">The name or identifier of the validator initiating this context.</param>
    /// <param name="culture">The <see cref="CultureInfo"/> to be used for validation.</param>
    /// <param name="parentContext">An optional reference to a parent <see cref="ValidatorRunCtx"/> for nested validations.</param>
    /// <param name="result">The <see cref="ValidatorRunResult"/> instance where validation outcomes will be recorded.</param>
    private protected ValidatorRunCtx(
        ImmutableArray<string>? availableSnapShots,
        T value,
        TExternalResources resources,
        string fromValidator,
        CultureInfo culture,
        ValidatorRunCtx? parentContext,
        ValidatorRunResult result)
        : base(fromValidator, culture, parentContext, result)
    {
        if (availableSnapShots is not null)
        {
            _availableSnapShots = new (string, bool?)[availableSnapShots.Value.Length];

            for (int i = 0; i < availableSnapShots.Value.Length; i++)
            {
                _availableSnapShots[i] = (availableSnapShots.Value[i], null);
            }
        }

        ValidationInstance = value;
        Resources = resources;
    }

    /// <summary>
    /// Gets the main instance of type <typeparamref name="T"/> currently undergoing validation.
    /// </summary>
    public T ValidationInstance { get; }

    /// <summary>
    /// Gets the external resources of type <typeparamref name="TExternalResources"/> available for the current validation run.
    /// </summary>
    public TExternalResources Resources { get; }

    /// <summary>
    /// Determines whether the current property rule execution should be exited based on the
    /// <see cref="ComponentFailureStrategy"/> of the current validation failure.
    /// </summary>
    /// <returns><see langword="true"/> if the property rule should exit; otherwise, <see langword="false"/>.</returns>
    internal bool ToExitPropertyRule()
    {
        return _currentValidationFailureInfo is not null
               && (_currentValidationFailureInfo.Strategy == ComponentFailureStrategy.Stop
                   || _currentValidationFailureInfo.Strategy == ComponentFailureStrategy.Exit);
    }

    /// <summary>
    /// Determines whether the entire validation process should stop based on the
    /// <see cref="RuleFailureStrategy"/> of the current rule failure or the
    /// <see cref="ComponentFailureStrategy"/> of the current validation failure.
    /// </summary>
    /// <returns><see langword="true"/> if the validation should stop entirely; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ValidationAssistantInternalException">
    /// Thrown if <see cref="_currentRuleFailureInfo"/> or <see cref="_currentValidationFailureInfo"/> are unexpectedly null.
    /// This indicates an internal logic error within the validation assistant.
    /// </exception>
    internal bool ToStopValidation()
    {
        if (_currentRuleFailure is null)
        {
            return false;
        }

        if (_currentRuleFailureInfo is null || _currentValidationFailureInfo is null)
        {
            throw new ValidationAssistantInternalException("Debug how RuleFailureInfo or ValidationFailureInfo is null in ToStopValidation.");
        }

        return _currentRuleFailureInfo.Strategy == RuleFailureStrategy.Stop || _currentValidationFailureInfo.Strategy == ComponentFailureStrategy.Stop;
    }

    /// <summary>
    /// Calculates the index for the next rule to be executed, potentially skipping rules
    /// if the current rule failure dictates a "logical rule" skip.
    /// </summary>
    /// <param name="currentIndex">The current index of the rule being processed.</param>
    /// <returns>The index of the next rule to process.</returns>
    /// <exception cref="ValidationAssistantInternalException">
    /// Thrown if <see cref="_currentRuleFailureInfo"/> is unexpectedly null when calculating the next index.
    /// This indicates an internal logic error.
    /// </exception>
    internal int CalculateNextIndex(int currentIndex)
    {
        if (_currentRuleFailure is null)
        {
            return currentIndex + 1; // No failure, proceed to the next rule
        }

        if (_currentRuleFailureInfo is null)
        {
            throw new ValidationAssistantInternalException("Debug how RuleFailureInfo is null in CalculateNextIndex.");
        }

        // If rules should be skipped due to a logical rule failure, calculate the new index
        // Otherwise, just move to the next rule
        return _currentRuleFailureInfo.RulesToSkip == 0 ? currentIndex + 1 : currentIndex + _currentRuleFailureInfo.RulesToSkip + 1;
    }

    /// <summary>
    /// Resets the internal state for processing a new logical rule.
    /// This typically clears any active rule or validation failure information.
    /// </summary>
    internal virtual void ForLogicalRule()
    {
        _currentRuleFailure = null;
        _currentRuleFailureInfo = null;
        _currentValidationFailureInfo = null;
        _currentPropertyRuleFailures = 0;
    }

    /// <summary>
    /// Adds a failure related to a logical rule to the validation result.
    /// This populates <see cref="_currentRuleFailure"/> and adds it to the overall <see cref="Result"/>.
    /// </summary>
    /// <param name="failureInfo">Information about the logical rule failure, including its explanation factory.</param>
    internal void AddLogicalRuleFailure(LogicalRuleFailureInfo<T, TExternalResources> failureInfo)
    {
        // Create a new LogicalRuleFailure based on the provided info and its explanation
        _currentRuleFailure = new LogicalRuleFailure(failureInfo, failureInfo.ExplanationFactory(this, failureInfo.RulesToSkip));
        Result.AddRuleFailure(_currentRuleFailure); // Add the failure to the main result collection
    }

    /// <summary>
    /// Adds a validation failure associated with a specific property to the validation result.
    /// This updates failure counters and manages the current rule failure context.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property that failed validation.</typeparam>
    /// <param name="property">The <see cref="Undefined{TProperty}"/> instance representing the property and its value.</param>
    /// <param name="failureInfo">Information about the validation failure, including its message factory and strategy.</param>
    /// <exception cref="ValidationAssistantInternalException">
    /// Thrown if <see cref="_currentRuleFailureInfo"/> is not of the expected type or is null when adding a property validation failure.
    /// This indicates an internal logic error.
    /// </exception>
    internal void AddPropertyRuleComponentFailure<TProperty>(Undefined<TProperty> property, ComponentFailureInfo<T, TExternalResources, TProperty> failureInfo)
    {
        _currentPropertyRuleFailures++; // Increment failures for the current property rule
        _totalValidationFailures++;     // Increment total failures for the entire run
        _currentValidationFailureInfo = failureInfo; // Update the current validation failure info

        // If there's no current RuleFailure (meaning this is the first failure for a property rule),
        // create a new PropertyRuleFailure and add it to the result.
        if (_currentRuleFailure is null)
        {
            if (_currentRuleFailureInfo is PropertyRuleFailureInfo<T, TExternalResources, TProperty> info)
            {
                // Create a new PropertyRuleFailure using the property, its path, and an explanation
                _currentRuleFailure = new PropertyRuleFailure<TProperty>(
                    property, CorrectPropertyPath, info, info.ExplanationFactory(this, property.Value));
                Result.AddRuleFailure(_currentRuleFailure); // Add this new rule failure to the overall result
            }
            else
            {
                // Internal error: RuleFailureInfo should be a PropertyRuleFailureInfo at this point
                throw new ValidationAssistantInternalException("Debug how PropertyRuleFailureInfo is wrong type or null in AddValidationFailure.");
            }
        }

        // Add the specific validation failure (for the property) to the current rule failure
        _currentRuleFailure.AddValidationFailure(ValidationFailure.ForPropertyComponent(failureInfo, failureInfo.FailureMessageFactory(this, property.Value)));
    }

    internal void AddPreValidationFailure(PreValidationRuleFailureInfo<T, TExternalResources> failureInfo, ValidationFailure validationFailure)
    {
        _totalValidationFailures++;
        // Create a new LogicalRuleFailure based on the provided info and its explanation
        _currentRuleFailure = new PreValidationRuleFailure(CorrectPropertyPath, failureInfo, failureInfo.ExplanationFactory(this, failureInfo.RulesToSkip));
        _currentRuleFailure.AddValidationFailure(validationFailure);
        Result.AddRuleFailure(_currentRuleFailure); // Add the failure to the main result collection
    }

    /// <summary>
    /// Calculates the index for the next rule to be executed, potentially skipping rules
    /// if the current rule failure dictates a "logical rule" skip.
    /// </summary>
    /// <param name="currentIndex">The current index of the rule being processed.</param>
    /// <returns>The index of the next rule to process.</returns>
    /// <exception cref="ValidationAssistantInternalException">
    /// Thrown if <see cref="_currentRuleFailureInfo"/> is unexpectedly null when calculating the next index.
    /// This indicates an internal logic error.
    /// </exception>
    internal void CalculateAndAddSnapShotResult(string snapShotIdentifier)
    {
        if (_availableSnapShots is null)
        {
            throw new ValidationRunException($"Validator: {_fromValidator} does not contain any snap shots.");
        }

        int index = Array.FindIndex(_availableSnapShots, s => s.identifaer == snapShotIdentifier);

        if (index < 0)
        {
            throw new ValidationRunException($"Snap shot: {snapShotIdentifier} does not exist in validator: {_fromValidator}.");
        }

        var (identifier, isValid) = _availableSnapShots[index];

        if (isValid.HasValue)
        {
            throw new ValidationRunException($"Snap shot: {snapShotIdentifier} value is already set.");
        }

        _availableSnapShots[index] = (snapShotIdentifier, _currentPropertyRuleFailures == 0);
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1033:Interface methods should be callable by child types", Justification = "<Pending>")]
    bool IConditionCtx<T, TExternalResources>.IsSnapShotValid(string snapShotIdentifier)
        => IsSnapShotValidInternal(snapShotIdentifier);

    /// <summary>
    /// Retrieves the validity status of a previously calculated validation snapshot.
    /// This allows subsequent rules or components to make decisions based on past validation outcomes.
    /// </summary>
    /// <param name="snapShotIdentifier">The unique identifier of the snapshot whose validity is to be retrieved.</param>
    /// <returns><see langword="true"/> if the snapshot was valid (no property rule failures at its capture point); otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ValidationRunException">
    /// Thrown if the validator does not support snapshots, if the <paramref name="snapShotIdentifier"/> does not exist,
    /// or if the snapshot's value has not yet been set.
    /// </exception>
    internal bool IsSnapShotValidInternal(string snapShotIdentifier)
    {
        if (_availableSnapShots is null)
        {
            throw new ValidationRunException($"Validator: {_fromValidator} does not contain any snap shots.");
        }

        int index = Array.FindIndex(_availableSnapShots, s => s.identifaer == snapShotIdentifier);

        if (index < 0)
        {
            throw new ValidationRunException($"Snap shot: {snapShotIdentifier} does not exist in validator: {_fromValidator}.");
        }

        bool? isValid = _availableSnapShots[index].isValid;

        if (isValid.HasValue)
        {
            return isValid.Value;
        }

        throw new ValidationRunException($"Snap shot: {snapShotIdentifier} value is not set.");
    }
}

