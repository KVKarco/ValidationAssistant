namespace KVKarco.ValidationAssistant.Internal;

/// <summary>
/// Represents a common abstract base class for internal validator rules within the framework.
/// This class implements the <see cref="IValidatorRule{T, TExternalResources, TContext}"/> interface,
/// providing common properties and abstract methods that most specific rule types will implement.
/// It defines the core execution contract for synchronous and asynchronous validation.
/// </summary>
/// <typeparam name="T">The type of the main instance being validated.</typeparam>
/// <typeparam name="TExternalResources">The type of external resources available during validation.</typeparam>
/// <typeparam name="TContext">The specific type of <see cref="ValidatorRunCtx{T, TExternalResources}"/>
/// used for the validation run, ensuring context-specific operations.</typeparam>
internal abstract class ValidatorRule<T, TExternalResources, TContext> :
    IValidatorRule<T, TExternalResources, TContext>
    where TContext : ValidatorRunCtx<T, TExternalResources>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidatorRule{T, TExternalResources, TContext}"/> class.
    /// </summary>
    /// <param name="canRunSynchronously">A boolean indicating whether this rule supports synchronous execution.</param>
    protected ValidatorRule(bool canRunSynchronously)
    {
        CanRunSynchronously = canRunSynchronously;
    }

    /// <summary>
    /// Gets the failure information associated with this rule.
    /// This property must be implemented by derived classes to provide details like explanation factories
    /// or failure strategies that are applicable if this rule determines a failure.
    /// </summary>
    public abstract RuleFailureInfo Info { get; }

    /// <summary>
    /// Gets the unique or descriptive name of this validator rule.
    /// This property must be implemented by derived classes.
    /// </summary>
    public abstract ReadOnlySpan<char> RuleName { get; }

    /// <summary>
    /// Gets a value indicating whether this validator rule can be executed synchronously.
    /// This property is set during construction.
    /// </summary>
    public bool CanRunSynchronously { get; }

    /// <summary>
    /// Abstract method for synchronously executing this validator rule.
    /// Derived classes must provide the concrete validation logic here, using the provided <paramref name="context"/>
    /// to access the instance being validated, external resources, and to report failures.
    /// This method should only be called if <see cref="CanRunSynchronously"/> is <see langword="true"/>.
    /// </summary>
    /// <param name="context">The validation run context for the current operation.</param>
    public abstract void Validate(TContext context);

    /// <summary>
    /// Abstract method for asynchronously executing this validator rule.
    /// Derived classes must provide the concrete asynchronous validation logic here, using the provided <paramref name="context"/>
    /// to access the instance being validated, external resources, and to report failures.
    /// </summary>
    /// <param name="context">The validation run context for the current operation.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    public abstract ValueTask ValidateAsync(TContext context, CancellationToken ct);
}
