using KVKarco.ValidationAssistant.Internal;
using KVKarco.ValidationAssistant.Internal.ExpressValidator;
using KVKarco.ValidationAssistant.Internal.Utilities;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace KVKarco.ValidationAssistant;

/// <summary>
/// Represents the abstract base class for a high-performance, fluent-style validator.
/// This class serves as the primary entry point for users to define and execute validation rules
/// for a given instance of <typeparamref name="T"/> with optional external resources <typeparamref name="TExternalResources"/>.
/// It leverages a caching mechanism for compiled validation rules to optimize performance on repeated validations.
/// </summary>
/// <typeparam name="T">The type of the instance that this validator will validate.</typeparam>
/// <typeparam name="TExternalResources">The type of external resources or services that the validation rules might depend on.</typeparam>
public abstract class ExpressValidator<T, TExternalResources> :
    ICoreValidator<T> // Implements the public contract for a core validator.
{
    /// <summary>
    /// Stores the exact type of the concrete validator derived from <see cref="ExpressValidator{T, TExternalResources}"/>.
    /// Used as a key for caching the compiled validator core.
    /// </summary>
    private readonly Type _validatorType;

    /// <summary>
    /// Holds the external resources provided to this validator instance, which can be accessed by validation rules.
    /// </summary>
    private readonly TExternalResources _resources;

    /// <summary>
    /// The compiled and cached validator core instance, containing all pre-validation and main validation rules.
    /// This core is retrieved once per validator type and reused across instances for efficiency.
    /// </summary>
    private readonly ExpressValidatorCore<T, TExternalResources> _core;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExpressValidator{T, TExternalResources}"/> class.
    /// This constructor is called by derived classes and is responsible for
    /// retrieving or compiling the validation rules from the cache.
    /// </summary>
    /// <param name="resources">The external resources that this validator instance will use during validation.
    /// These resources are immutable for the lifetime of this validator instance.</param>
    protected ExpressValidator(TExternalResources resources)
    {
        _resources = resources;
        _validatorType = GetType(); // Get the runtime type of the derived validator.
        // Retrieve or add the compiled ExpressValidatorCore from the internal cache.
        // The ExpressPreValidationRules and ExpressRules methods define the rules for this validator type.
        _core = InternalCache.GetOrAddExpressValidatorCore<T, TExternalResources>(_validatorType, ExpressPreValidationRules, ExpressRules);
    }

    /// <summary>
    /// Gets the unique name of this validator, derived from its compiled core.
    /// </summary>
    public string ValidatorName => _core.ValidatorName;

    /// <summary>
    /// Gets a value indicating whether this validator can be executed synchronously.
    /// This is determined by the capabilities of its compiled core, which in turn depends
    /// on whether any of its pre-validation or main rules are asynchronous.
    /// </summary>
    public bool CanRunSynchronously => _core.CanRunSynchronously;

    /// <summary>
    /// Synchronously validates the specified <paramref name="value"/> using the defined rules.
    /// This method will execute all pre-validation rules first, and if they pass,
    /// then execute the main validation rules.
    /// </summary>
    /// <param name="value">The instance of <typeparamref name="T"/> to validate.</param>
    /// <param name="culture">Optional. The culture information to use for generating validation messages. Defaults to <see cref="ValidatorsConfig.GlobalDefaults.DefaultCulture"/> if not provided.</param>
    /// <returns>A <see cref="ValidatorRunResult"/> containing any validation failures.</returns>
    public ValidatorRunResult Validate(T value, CultureInfo? culture = null)
    {
        // Create a new validation context for this specific run.
        var contex = ValidatorRunCtx.ForNewValidatorCoreRun(_core, value, _resources, culture);
        // Execute the synchronous validation logic of the compiled core.
        _core.InternalValidate(contex);
        return contex.Result; // Return the accumulated results.
    }

    /// <summary>
    /// Asynchronously validates the specified <paramref name="value"/> using the defined rules.
    /// This method will execute all pre-validation rules first, and if they pass,
    /// then execute the main validation rules. Supports cancellation.
    /// </summary>
    /// <param name="value">The instance of <typeparamref name="T"/> to validate.</param>
    /// <param name="culture">Optional. The culture information to use for generating validation messages. Defaults to <see cref="ValidatorsConfig.GlobalDefaults.DefaultCulture"/> if not provided.</param>
    /// <param name="ct">Optional. A <see cref="CancellationToken"/> to observe while waiting for the validation to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous validation operation,
    /// yielding a <see cref="ValidatorRunResult"/> containing any validation failures.</returns>
    public async Task<ValidatorRunResult> ValidateAsync(T value, CultureInfo? culture = null, CancellationToken ct = default)
    {
        // Create a new validation context for this specific run.
        var contex = ValidatorRunCtx.ForNewValidatorCoreRun(_core, value, _resources, culture);
        // Execute the asynchronous validation logic of the compiled core.
        await _core.InternalValidateAsync(contex, ct).ConfigureAwait(false);
        return contex.Result; // Return the accumulated results.
    }

    /// <summary>
    /// When overridden in a derived class, this method allows for the definition of pre-validation rules.
    /// Pre-validation rules are executed before any main validation rules and are typically used
    /// for fundamental checks (e.g., ensuring the instance or resources are not null).
    /// By default, it ensures the instance and external resources are not null.
    /// </summary>
    /// <param name="builder">A builder that provides fluent methods for defining pre-validation rules.</param>
    protected virtual void ExpressPreValidationRules([NotNull] IPreValidationRuleExpressionBuilder<T, TExternalResources> builder)
    {
        // Default pre-validation: ensure the main instance is not null.
        builder.Ensure(x => x is not null);
        // Default pre-validation: ensure the external resources are not null.
        builder.EnsureResources(x => x is not null);
    }

    /// <summary>
    /// When overridden in a derived class, this abstract method must be implemented
    /// to define the main validation rules for the <typeparamref name="T"/> instance.
    /// This is where the core business logic validation rules are specified using the provided builder.
    /// </summary>
    /// <param name="builder">A builder that provides fluent methods for defining main validation rules (e.g., for properties).</param>
    protected abstract void ExpressRules(IRuleExpressionBuilder<T, TExternalResources> builder);
}

/// <summary>
/// Provides a convenient abstract base class for validators that do not require any external resources.
/// It simplifies the instantiation of <see cref="ExpressValidator{T, TExternalResources}"/> by automatically
/// providing an <see cref="EmptyValidationResources"/> instance.
/// </summary>
/// <typeparam name="T">The type of the instance that this validator will validate.</typeparam>
public abstract class ExpressValidator<T> : ExpressValidator<T, EmptyValidationResources>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExpressValidator{T}"/> class.
    /// It automatically passes an instance of <see cref="EmptyValidationResources.Empty"/>
    /// to the base <see cref="ExpressValidator{T, TExternalResources}"/> constructor.
    /// </summary>
    protected ExpressValidator() : base(EmptyValidationResources.Empty)
    {
    }
}

/// <summary>
/// Defines the public contract for a compiled or configured validator, primarily intended for consumption via Dependency Injection (DI).
/// While generic, this interface is designed to expose the validation capabilities of validators that originate from the ExpressValidator,
/// allowing for type-agnostic interaction with validation services and results without direct dependency on the concrete ExpressValidator implementation types.
/// </summary>
/// <typeparam name="T">The type of the instance that this validator is designed to validate.</typeparam>
public interface ICoreValidator<T>
{
    /// <summary>
    /// Gets the unique name of the validator.
    /// </summary>
    string ValidatorName { get; }

    /// <summary>
    /// Gets a value indicating whether this validator can be executed synchronously.
    /// </summary>
    bool CanRunSynchronously { get; }

    /// <summary>
    /// Synchronously validates the specified <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The instance of <typeparamref name="T"/> to validate.</param>
    /// <param name="culture">Optional. The culture information to use for generating validation messages. Defaults to <see cref="ValidatorsConfig.GlobalDefaults.DefaultCulture"/> if not provided.</param>
    /// <returns>A <see cref="ValidatorRunResult"/> containing any validation failures.</returns>
    ValidatorRunResult Validate(T value, CultureInfo? culture = null);

    /// <summary>
    /// Asynchronously validates the specified <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The instance of <typeparamref name="T"/> to validate.</param>
    /// <param name="culture">Optional. The culture information to use for generating validation messages. Defaults to <see cref="ValidatorsConfig.GlobalDefaults.DefaultCulture"/> if not provided.</param>
    /// <param name="ct">Optional. A <see cref="CancellationToken"/> to observe while waiting for the validation to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous validation operation,
    /// yielding a <see cref="ValidatorRunResult"/> containing any validation failures.</returns>
    Task<ValidatorRunResult> ValidateAsync(T value, CultureInfo? culture = null, CancellationToken ct = default);
}

public interface IInitialPropertyRuleBuilder<T, TExternalResources, out TProperty>
{
    //empty for now
}
