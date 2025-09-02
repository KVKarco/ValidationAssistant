using KVKarco.ValidationAssistant.Abstractions;
using KVKarco.ValidationAssistant.Internal;
using KVKarco.ValidationAssistant.Internal.CustomValidatorAssets;
using KVKarco.ValidationAssistant.Internal.Utilities;
using KVKarco.ValidationAssistant.Results;
using System.Globalization;

namespace KVKarco.ValidationAssistant;

// for validatorFactoryDelegate, the TExternalResources is not known;
public abstract class CustomValidator<T>
{
    internal abstract ValidationCtx CreateContextForElements(ValidationCtx parentCtx, T firstValue);

    internal abstract bool IsValid(ValidationCtx context, T value, int? index);

    internal abstract Task<bool> IsValidAsync(ValidationCtx context, T value, int? index, CancellationToken ct = default);

}
/// <summary>
/// Represents the abstract base class for a high-performance, fluent-style validator.
/// This class serves as the primary entry point for users to define and execute validation rules
/// for a given instance of <typeparamref name="T"/> with optional external resources <typeparamref name="TExternalResources"/>.
/// It leverages a caching mechanism for compiled validation rules to optimize performance on repeated validations.
/// </summary>
/// <typeparam name="T">The type of the instance that this validator will validate.</typeparam>
/// <typeparam name="TExternalResources">The type of external resources or services that the validation rules might depend on.</typeparam>
public abstract class CustomValidator<T, TExternalResources>
{
    /// <summary>
    /// Stores the exact type of the concrete validator derived from <see cref="CustomValidator{T, TExternalResources}"/>.
    /// Used as a key for caching the compiled validator core.
    /// </summary>
    private readonly Type _validatorType;

    /// <summary>
    /// Holds the external resources provided to this validator instance, which can be accessed by validation rules.
    /// </summary>
    private protected readonly TExternalResources _resources;

    /// <summary>
    /// The compiled and cached validator core instance, containing all pre-validation and main validation rules.
    /// This core is retrieved once per validator type and reused across instances for efficiency.
    /// </summary>
    private protected readonly CustomValidatorCore<T, TExternalResources> _core;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomValidator{T, TExternalResources}"/> class.
    /// This constructor is called by derived classes and is responsible for
    /// retrieving or compiling the validation rules from the cache.
    /// </summary>
    /// <param name="resources">The external resources that this validator instance will use during validation.
    /// These resources are immutable for the lifetime of this validator instance.</param>
    protected CustomValidator(TExternalResources resources)
    {
        _resources = resources;
        _validatorType = GetType(); // Get the runtime type of the derived validator.
        // Retrieve or add the compiled ExpressValidatorCore from the internal cache.
        // The ExpressPreValidationRules and ExpressRules methods define the rules for this validator type.
        _core = InternalCache.GetOrAddExpressValidatorCore<T, TExternalResources>(_validatorType, ComposePreAndPostValidation, ComposeValidation);
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
    /// <param name="culture">Optional. The culture information to use for generating validation messages. Defaults to <see cref="ValidationAssistantConfig.GlobalDefaults.DefaultCulture"/> if not provided.</param>
    /// <returns>A <see cref="ValidatorRunResult"/> containing any validation failures.</returns>
    public ValidatorRunResult Validate(T value, CultureInfo? culture = null)
    {
        //// Create a new validation context for this specific run.// the core have the validation logic pre checks snapshot template and clean up logic.
        using var context = ValidationCtx.ForNewRunAsMainValidator(_core, value, _resources, culture);

        // Execute the setup, pre validation, validation logic of the compiled core/ context will execute the clean up logic.
        await _core.InternalValidate(contex, ct);

        return contex.Result; // Return the accumulated results.
    }

    /// <summary>
    /// Asynchronously validates the specified <paramref name="value"/> using the defined rules.
    /// This method will execute all pre-validation rules first, and if they pass,
    /// then execute the main validation rules. Supports cancellation.
    /// </summary>
    /// <param name="value">The instance of <typeparamref name="T"/> to validate.</param>
    /// <param name="culture">Optional. The culture information to use for generating validation messages. Defaults to <see cref="ValidationAssistantConfig.GlobalDefaults.DefaultCulture"/> if not provided.</param>
    /// <param name="ct">Optional. A <see cref="CancellationToken"/> to observe while waiting for the validation to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous validation operation,
    /// yielding a <see cref="ValidatorRunResult"/> containing any validation failures.</returns>
    public async Task<ValidatorRunResult> ValidateAsync(T value, CultureInfo? culture = null, CancellationToken ct = default)
    {

        //// Create a new validation context for this specific run.// the core have the validation logic pre checks snapshot template and clean up logic.
        using var context = ValidationCtx.ForNewRunAsMainValidator(_core, value, _resources, culture);

        // Execute the asynchronous setup, pre validation, validation logic of the compiled core/ context will execute the clean up logic.
        await _core.InternalValidateAsync(contex, ct).ConfigureAwait(false);

        return contex.Result; // Return the accumulated results.
    }

    //so a parent validator can create a child validator context, parent validator dont know the child validator TExternalResources.
    internal override ValidationCtx CreateContextForElements(ValidationCtx parentCtx, T firstValue)
    {
        return ValidationCtx.ForNewRunAsChildValidator(_core, value, _resources, parentCtx, 0);
    }

    //if its for elements we dont use using for validation, because the parent validator used the CreateContextForElements method to create this context.
    // and put using on the reusable element context, so we only refresh the context with the new value and index, and validate.
    internal override bool IsValid(ValidationCtx context, T value, int? index)
    {
        //this means is for elements, so the parent validator used the CreateContextForElements method to create this context.
        //so we only refresh the context with the new value and index, and validate.
        if (context is CustomValidatorRunCtx<T, TExternalResources> concreteContext && index.HasValue)
        {
            concreteContext.ForNext(value, index.Value);
            _core.InternalValidate(concreteContext);
            return concreteContext.IsRunValid;
        }

        // this is for single property validation from parent validator, so we create a new context for the child validator. with using
        using var ctx = ValidationCtx.ForNewRunAsChildValidator(_core, value, _resources, context);
        _core.InternalValidate(ctx);
        return ctx.IsRunValid;
    }

    internal async ValueTask<bool> IsValidAsync(ValidationCtx context, T value, int? index, CancellationToken ct = default)
    {
        //this means is for elements, so the parent validator used the CreateContextForElements method to create this context.
        //so we only refresh the context with the new value and index, and validate.
        if (context is CustomValidatorRunCtx<T, TExternalResources> concreteContext && index.HasValue)
        {
            concreteContext.ForNext(value, index.Value);
            await _core.InternalValidateAsync(concreteContext, ct).ConfigureAwait(false);
            return concreteContext.IsRunValid;
        }

        // this is for single property validation from parent validator, so we create a new context for the child validator. with using
        using var ctx = ValidationCtx.ForNewRunAsChildValidator(_core, value, _resources, context);
        await _core.InternalValidateAsync(ctx, ct).ConfigureAwait(false);
        return ctx.IsRunValid;
    }

    protected abstract void ConfigureValidation(IValidatorPreValidationDefinitionBuilder<T, TExternalResources> builder);

    protected abstract void ComposeValidation(IValidatorValidationDefinitionBuilder<T, TExternalResources> builder);
}
