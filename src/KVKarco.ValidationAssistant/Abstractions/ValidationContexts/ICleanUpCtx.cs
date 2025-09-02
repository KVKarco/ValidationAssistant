namespace KVKarco.ValidationAssistant.Abstractions.ValidationContexts;

using System;

/// <summary>
/// Defines a context capable of managing cleanup actions that should be executed after validation completes.
/// Supports both synchronous and asynchronous cleanup registration.
/// </summary>
/// <remarks>
/// Use this context to register resources such as <see cref="IDisposable"/> or <see cref="IAsyncDisposable"/> instances
/// that should be disposed once validation is finished.
/// </remarks>
public interface ICleanUpCtx : IDisposable
{
    /// <summary>
    /// Registers a synchronous cleanup action to be disposed when validation completes.
    /// </summary>
    /// <param name="cleanup">The disposable resource to register.</param>
    void RegisterForCleanup(IDisposable cleanup);

    /// <summary>
    /// Registers a synchronous cleanup callback to be executed when validation completes.
    /// </summary>
    /// <param name="cleanup">The cleanup action to execute.</param>
    void RegisterForCleanup(Action cleanup);

    /// <summary>
    /// Registers an asynchronous cleanup action to be disposed when validation completes.
    /// </summary>
    /// <param name="cleanup">The asynchronous disposable resource to register.</param>
    void RegisterForCleanupAsync(IAsyncDisposable cleanup);

    /// <summary>
    /// Registers an asynchronous cleanup callback to be executed when validation completes.
    /// </summary>
    /// <param name="cleanup">The asynchronous cleanup action to execute.</param>
    void RegisterForCleanupAsync(AsyncAction cleanup);
}
