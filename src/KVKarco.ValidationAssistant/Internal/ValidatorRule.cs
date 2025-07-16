using KVKarco.ValidationAssistant.Abstractions;

namespace KVKarco.ValidationAssistant.Internal;

/// <summary>
/// Provides a base abstract implementation for <see cref="IValidatorRule{T, TExternalResources, TContext}"/>.
/// This class handles the common aspects of a validation rule, particularly the determination
/// of whether the rule can run synchronously.
/// </summary>
/// <typeparam name="T">The type of the entity or object being validated.</typeparam>
/// <typeparam name="TExternalResources">The type of external resources used during validation.</typeparam>
/// <typeparam name="TContext">The type of the validation run context.</typeparam>
internal abstract class ValidatorRule<T, TExternalResources, TContext> :
    IValidatorRule<T, TExternalResources, TContext>
    where TContext : ValidatorRunCtx<T, TExternalResources>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidatorRule{T, TExternalResources, TContext}"/> class.
    /// </summary>
    /// <param name="canRunSynchronously">
    /// A value indicating whether this rule's <see cref="Validate(TContext)"/> method can be invoked synchronously.
    /// This typically depends on whether any underlying validation logic is asynchronous.
    /// </param>
    protected ValidatorRule(bool canRunSynchronously)
    {
        CanRunSynchronously = canRunSynchronously;
    }

    /// <inheritdoc/>
    public abstract RuleFailureInfo Info { get; }

    /// <inheritdoc/>
    public abstract ReadOnlySpan<char> RuleName { get; }

    /// <inheritdoc/>
    public bool CanRunSynchronously { get; }

    /// <inheritdoc/>
    public abstract void Validate(TContext context);

    /// <inheritdoc/>
    public abstract ValueTask ValidateAsync(TContext context, CancellationToken ct);
}