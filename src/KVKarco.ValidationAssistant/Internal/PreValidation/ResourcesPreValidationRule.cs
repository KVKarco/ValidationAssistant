using KVKarco.ValidationAssistant.Exceptions;

namespace KVKarco.ValidationAssistant.Internal.PreValidation;

/// <summary>
/// Represents a sealed concrete pre-validation rule that applies a predicate (synchronous or asynchronous)
/// directly to the external resources (<typeparamref name="TExternalResources"/>) available during validation.
/// This rule is designed for fundamental checks on the external resources, such as nullability or basic initialization,
/// before the main validation rules run.
/// </summary>
/// <typeparam name="T">The type of the main instance being validated (used for context).</typeparam>
/// <typeparam name="TExternalResources">The type of external resources being validated.</typeparam>
internal sealed class ResourcesPreValidationRule<T, TExternalResources> :
    PreValidationRule<T, TExternalResources>
{
    /// <summary>
    /// The synchronous predicate (function) to execute for validation, if this rule is synchronous.
    /// This predicate operates on <typeparamref name="TExternalResources"/>.
    /// </summary>
    private readonly PreValidationPredicate<TExternalResources> _predicate;

    /// <summary>
    /// The asynchronous predicate (function) to execute for validation, if this rule is asynchronous.
    /// This predicate operates on <typeparamref name="TExternalResources"/>.
    /// </summary>
    private readonly AsyncPreValidationPredicate<TExternalResources> _asyncPredicate;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourcesPreValidationRule{T, TExternalResources}"/> class
    /// for a synchronous pre-validation predicate.
    /// </summary>
    /// <param name="predicate">The synchronous predicate to apply to the external resources.</param>
    /// <param name="info">The failure information associated with this pre-validation rule, passed to the base class.</param>
    /// <param name="failureExplanation">A user-defined explanation message for the failure, passed to the base class.</param>
    public ResourcesPreValidationRule(
        PreValidationPredicate<TExternalResources> predicate,
        PreValidationRuleFailureInfo<T, TExternalResources> info,
        string failureExplanation)
        : base(info, true, failureExplanation)
    {
        _predicate = predicate;
        _asyncPredicate = null!; // Explicitly null for synchronous variant.
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourcesPreValidationRule{T, TExternalResources}"/> class
    /// for an asynchronous pre-validation predicate.
    /// </summary>
    /// <param name="asyncPredicate">The asynchronous predicate to apply to the external resources.</param>
    /// <param name="info">The failure information associated with this pre-validation rule, passed to the base class.</param>
    /// <param name="failureExplanation">A user-defined explanation message for the failure, passed to the base class.</param>
    public ResourcesPreValidationRule(
        AsyncPreValidationPredicate<TExternalResources> asyncPredicate,
        PreValidationRuleFailureInfo<T, TExternalResources> info,
        string failureExplanation)
        : base(info, false, failureExplanation)
    {
        _predicate = null!; // Explicitly null for asynchronous variant.
        _asyncPredicate = asyncPredicate;
    }

    /// <summary>
    /// Gets the name of this pre-validation rule. It dynamically indicates whether the rule is synchronous or asynchronous.
    /// </summary>
    public sealed override ReadOnlySpan<char> RuleName => CanRunSynchronously
        ? DefaultNaming.ExternalResourcesPreValidationName
        : DefaultNaming.ExternalResourcesAsyncPreValidationName;

    /// <summary>
    /// Retrieves the default error message for this pre-validation rule when it fails.
    /// This delegates to a global default message factory in <see cref="ValidatorsConfig"/>.
    /// </summary>
    /// <param name="context">The validation run context.</param>
    /// <returns>A string representing the default error message.</returns>
    public sealed override string GetDefaultErrorMessage(ValidatorRunCtx<T, TExternalResources> context)
        => ValidatorsConfig.GlobalDefaults.Messages.ExternalResourcesPreValidationError(context);

    /// <summary>
    /// Synchronously executes the pre-validation logic by applying the predicate to the external resources.
    /// If the predicate returns <see langword="false"/>, a pre-validation failure is added to the context.
    /// </summary>
    /// <param name="context">The validation run context.</param>
    /// <exception cref="ValidationRunException">Thrown if this method is called when the rule wraps an asynchronous predicate.</exception>
    public sealed override void PreValidate(ValidatorRunCtx<T, TExternalResources> context)
    {
        ValidationRunException.ThrowIfAsyncRuleIsCalledSynchronously(CanRunSynchronously, RuleName);

        // Executes the synchronous predicate on the external resources. The null-forgiving operator (!) is safe here
        // because CanRunSynchronously being true implies _predicate is not null.
        if (!_predicate(context.Resources))
        {
            // If validation fails, add a pre-validation failure to the context.
            // The message is generated via GetDefaultErrorMessage, and specific failure info (_info) is used from the base class.
            context.AddPreValidationFailure(_info, ValidationFailure.ForPreValidationRule(new(_info.DeclaredOnLine), GetDefaultErrorMessage(context)));
        }
    }

    /// <summary>
    /// Asynchronously executes the pre-validation logic by applying the predicate to the external resources.
    /// If the predicate returns <see langword="false"/>, a pre-validation failure is added to the context.
    /// </summary>
    /// <param name="context">The validation run context.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    public sealed override async ValueTask PreValidateAsync(ValidatorRunCtx<T, TExternalResources> context, CancellationToken ct)
    {
        bool isValid = CanRunSynchronously
            ? _predicate(context.Resources) // Execute sync predicate if it's a sync rule
            : await _asyncPredicate(context.Resources, ct).ConfigureAwait(false); // Await async predicate for async rule

        if (!isValid)
        {
            // If validation fails, add a pre-validation failure to the context.
            // The message is generated via GetDefaultErrorMessage, and specific failure info (_info) is used from the base class.
            context.AddPreValidationFailure(_info, ValidationFailure.ForPreValidationRule(new(_info.DeclaredOnLine), GetDefaultErrorMessage(context)));
        }
    }
}
