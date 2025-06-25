using KVKarco.ValidationAssistant.ValidationRules;

namespace KVKarco.ValidationAssistant.Internal.GenericValidationRules;

/// <summary>
/// Represents a synchronous validation rule that either captures the current validation state as a snapshot
/// or checks the validity of a previously captured snapshot to influence the validation flow.
/// </summary>
/// <typeparam name="T">The type of the main object instance being validated.</typeparam>
/// <typeparam name="TExternalResources">The type of external resources or dependencies available to the rule.</typeparam>
/// <typeparam name="TProperty">The type of the property being validated by this rule (though its value might not be directly used for snapshot logic).</typeparam>
internal sealed class SnapShotValidationRule<T, TExternalResources, TProperty> :
    IValidationRule<T, TExternalResources, TProperty>
{
    /// <summary>
    /// The unique identifier for the snapshot this rule operates on.
    /// </summary>
    private readonly string _snapShotIdentifier;

    /// <summary>
    /// A flag indicating whether this rule should capture the current validation result as a snapshot (<see langword="true"/>)
    /// or check the validity of an existing snapshot (<see langword="false"/>).
    /// </summary>
    private readonly bool _isCapturingResult;

    /// <summary>
    /// Initializes a new instance of the <see cref="SnapShotValidationRule{T, TExternalResources, TProperty}"/> class.
    /// </summary>
    /// <param name="snapShotIdentifier">The unique identifier for the snapshot.</param>
    /// <param name="isCapturingResult">If <see langword="true"/>, this rule will capture the current validation result for the specified snapshot.
    /// If <see langword="false"/>, it will check the validity of the existing snapshot.</param>
    public SnapShotValidationRule(string snapShotIdentifier, bool isCapturingResult)
    {
        _snapShotIdentifier = snapShotIdentifier;
        _isCapturingResult = isCapturingResult;
    }

    /// <summary>
    /// Gets the unique name of this snapshot validation rule. This is typically a predefined constant
    /// from <see cref="DefaultNaming"/>.
    /// </summary>
    public ReadOnlySpan<char> RuleName => DefaultNaming.SnapShotValRule;

    /// <summary>
    /// Provides the default failure message for this validation rule, specifically when a checked
    /// snapshot is found to be invalid. It retrieves the message from the global default messages configuration.
    /// </summary>
    /// <param name="context">The message context, providing access to the validated object, external resources, and culture.</param>
    /// <param name="value">The actual value of the property associated with the component (can be ignored if not applicable to the message).</param>
    /// <returns>A string explaining that the snapshot is invalid and has caused the validation rule set to stop.</returns>
    public string GetDefaultFailureMessage(IMessageCtx<T, TExternalResources> context, TProperty value)
        => ValidatorsConfig.GlobalDefaults.Messages.InvalidSnapShotComponentExplanation(context, value, _snapShotIdentifier);

    /// <summary>
    /// Executes the logic for the snapshot rule: either captures the result or checks its validity.
    /// </summary>
    /// <param name="context">The validation run context, providing access to the main object, external dependencies,
    /// and methods for managing snapshots.</param>
    /// <param name="value">The value of the property to validate (not directly used by this rule's core logic).</param>
    /// <returns>
    /// <see langword="true"/> if <see cref="_isCapturingResult"/> is <see langword="true"/> (as capturing a result is not a "failure" of the rule itself),
    /// or if <see cref="_isCapturingResult"/> is <see langword="false"/> and the checked snapshot is valid;
    /// otherwise, <see langword="false"/> if <see cref="_isCapturingResult"/> is <see langword="false"/> and the snapshot is invalid.
    /// </returns>
    public bool IsValid(ValidatorRunCtx<T, TExternalResources> context, TProperty value)
    {
        if (_isCapturingResult)
        {
            // When capturing, the rule itself always "succeeds" in its task
            // The validity of the snapshot is determined by the current state of property rule failures.
            context.CalculateAndAddSnapShotResult(_snapShotIdentifier);
            return true;
        }

        // When not capturing, the rule's validity depends on the snapshot's validity
        // Assuming IsSnapShotValidInternal is an internal helper for IsSnapShotValid or a similar check.
        return context.IsSnapShotValidInternal(_snapShotIdentifier);
    }
}
