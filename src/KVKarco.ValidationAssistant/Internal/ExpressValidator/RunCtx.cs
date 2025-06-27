using KVKarco.ValidationAssistant.Internal.PropertyValidation;
using System.Collections.Immutable;
using System.Globalization;

namespace KVKarco.ValidationAssistant.Internal.ExpressValidator;

/// <summary>
/// Represents a sealed, concrete implementation of <see cref="ValidatorRunCtx{T, TExternalResources}"/>
/// specifically designed for validators built using the ExpressValidator classes within the ValidationAssistant framework.
/// This context manages the state of a validation run, including the current property being validated and the accumulation of failures.
/// </summary>
/// <typeparam name="T">The type of the main object instance being validated in this run.</typeparam>
/// <typeparam name="TExternalResources">The type of external resources available during this validation run.</typeparam>
internal sealed class ExpressValidatorRunCtx<T, TExternalResources> :
    ValidatorRunCtx<T, TExternalResources>
{
    /// <summary>
    /// Represents the key of the property currently being validated. This is used to track
    /// which property's rules are being executed and to build correct property paths for failures.
    /// It is reset when switching to validate a new property.
    /// </summary>
    private PropertyKey _currentPropertyKey;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExpressValidatorRunCtx{T, TExternalResources}"/> class.
    /// </summary>
    /// <param name="fromValidator">The name of the validator initiating this run.</param>
    /// <param name="value">The main instance of <typeparamref name="T"/> being validated.</param>
    /// <param name="resources">The external resources provided for this validation run.</param>
    /// <param name="availableSnapShots">An optional immutable array of string snapshots available for this validator's core.</param>
    /// <param name="culture">The culture information to use for generating validation messages during this run.</param>
    /// <param name="result">Optional. An existing <see cref="ValidatorRunResult"/> to continue accumulating failures into. If <see langword="null"/>, a new one is created.</param>
    /// <param name="parentContext">Optional. The parent validation context if this run is part of a nested validation operation (e.g., a child validator).</param>
    public ExpressValidatorRunCtx(
        string fromValidator,
        T value,
        TExternalResources resources,
        ImmutableArray<string>? availableSnapShots,
        CultureInfo culture,
        ValidatorRunResult? result,
        ValidatorRunCtx? parentContext)
        : base(
            availableSnapShots,
            value,
            resources,
            fromValidator,
            culture,
            parentContext,
            result ?? new(fromValidator)) // If no result is provided, create a new one initialized with the validator name.
    {
        // Initialize the current property key to an empty state, indicating no specific property is currently active.
        _currentPropertyKey = PropertyKey.Empty;
    }

    /// <summary>
    /// Gets the name of the property currently being validated, derived from the <see cref="_currentPropertyKey"/>.
    /// Returns an empty span if no specific property is currently being validated (i.e., <see cref="_currentPropertyKey"/> is empty).
    /// </summary>
    public override ReadOnlySpan<char> PropertyName => _currentPropertyKey.Info is null ? [] : _currentPropertyKey.PropertyName;

    /// <summary>
    /// Gets the corrected, full path of the property currently being validated.
    /// This path is built recursively by prepending the <see cref="ValidatorRunCtx{T, TExternalResources}.CorrectPropertyPath"/>
    /// from any <see cref="ValidatorRunCtx{T, TExternalResources}.ParentContext"/> to the current property's path.
    /// </summary>
    /// <remarks>
    /// If there is a <see cref="ValidatorRunCtx{T, TExternalResources}.ParentContext"/>, the path is formed as "ParentPath.CurrentPropertyPath".
    /// Otherwise, it is just the current property's path.
    /// </remarks>
    internal override string CorrectPropertyPath => ParentContext is not null
        ? $"{ParentContext.CorrectPropertyPath}.{_currentPropertyKey.PropertyPath}"
        : _currentPropertyKey.PropertyPath;

    /// <summary>
    /// Sets the context for validating a specific property identified by the provided <paramref name="key"/>.
    /// This method updates the internal state to reflect the new active property and
    /// resets any rule-specific failure tracking from previous rule executions for a clean state.
    /// </summary>
    /// <param name="key">The <see cref="PropertyKey"/> representing the property that is about to be validated.</param>
    /// <param name="failureInfo">The <see cref="RuleFailureInfo"/> associated with the property rule that owns this property validation.
    /// This is used to track the strategy and other metadata for the current property rule.</param>
    public void ForProperty(PropertyKey key, RuleFailureInfo failureInfo)
    {
        _currentPropertyKey = key;
        _currentRuleFailure = null; // Clear previous rule failure as we are starting a new property rule.
        _currentRuleFailureInfo = failureInfo; // Set the info for the current property rule.
        _currentValidationFailureInfo = null; // Clear validation info from previous component.
        _currentPropertyRuleFailures = 0; // Reset property-specific failure count for the new property rule.
    }
}
