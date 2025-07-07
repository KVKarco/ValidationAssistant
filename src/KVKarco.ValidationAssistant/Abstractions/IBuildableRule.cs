using KVKarco.ValidationAssistant.Internal;

namespace KVKarco.ValidationAssistant.Abstractions;

/// <summary>
/// Defines an interface for a rule that is in the process of being built via a fluent API.
/// Implementations of this interface represent a rule definition that can be finalized
/// into a concrete <see cref="IValidatorRule{T, TExternalResources, TContext}"/> instance
/// once all necessary configurations have been applied.
/// This allows the main validator builder to collect and later compile these "buildable" rules.
/// </summary>
/// <typeparam name="T">The type of the main instance being validated.</typeparam>
/// <typeparam name="TExternalResources">The type of external resources available during validation.</typeparam>
/// <typeparam name="TContext">The specific type of <see cref="ValidatorRunCtx{T, TExternalResources}"/>
/// that the generated rule will operate with.</typeparam>
internal interface IBuildableRule<T, TExternalResources, TContext>
    where TContext : ValidatorRunCtx<T, TExternalResources>
{
    /// <summary>
    /// Builds and returns a fully configured <see cref="IValidatorRule{T, TExternalResources, TContext}"/>
    /// based on the definitions provided through the fluent API.
    /// </summary>
    /// <returns>A concrete instance of <see cref="IValidatorRule{T, TExternalResources, TContext}"/>.</returns>
    IValidatorRule<T, TExternalResources, TContext> Build();
}
