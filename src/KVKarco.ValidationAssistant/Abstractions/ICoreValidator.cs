using System.Globalization;

namespace KVKarco.ValidationAssistant.Abstractions;

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
