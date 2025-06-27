using KVKarco.ValidationAssistant.Abstractions;
using KVKarco.ValidationAssistant.Internal.ExpressValidator;
using System.Collections.Concurrent;

namespace KVKarco.ValidationAssistant.Internal.Utilities;

/// <summary>
/// Provides a static, thread-safe caching mechanism for compiled <see cref="ExpressValidatorCore{T, TExternalResources}"/> instances.
/// This cache optimizes performance by storing and reusing validator cores once they have been built,
/// avoiding redundant compilation of validation rules for the same validator type.
/// </summary>
internal static class InternalCache
{
    /// <summary>
    /// A thread-safe dictionary that caches <see cref="Lazy{T}"/> instances of <see cref="ValidatorCore{T, TExternalResources, TContext}"/>,
    /// keyed by the concrete <see cref="Type"/> of the <see cref="ExpressValidator{T, TExternalResources}"/> that owns them.
    /// The <see cref="Lazy{T}"/> ensures that the validator core is built only once, atomically, when first requested.
    /// </summary>
    private static readonly ConcurrentDictionary<Type, Lazy<ValidatorCore>> _coresCache = [];

    /// <summary>
    /// Retrieves a compiled <see cref="ExpressValidatorCore{T, TExternalResources}"/> from the cache,
    /// or builds and adds it to the cache if it does not already exist for the given validator type.
    /// This method ensures that validator core compilation happens only once per validator type across the application lifetime.
    /// </summary>
    /// <typeparam name="T">The type of the instance being validated by the validator core.</typeparam>
    /// <typeparam name="TExternalResources">The type of external resources used by the validator core.</typeparam>
    /// <param name="validatorType">The concrete <see cref="Type"/> of the <see cref="ExpressValidator{T, TExternalResources}"/>.</param>
    /// <param name="preValidationRuleCreator">An action that defines the pre-validation rules for the validator,
    /// typically provided by the <c>ExpressPreValidationRules</c> method of <see cref="ExpressValidator{T, TExternalResources}"/>.</param>
    /// <param name="ruleCreator">An action that defines the main validation rules for the validator,
    /// typically provided by the abstract <c>ExpressRules</c> method of <see cref="ExpressValidator{T, TExternalResources}"/>.</param>
    /// <returns>A compiled and cached instance of <see cref="ExpressValidatorCore{T, TExternalResources}"/>.</returns>
    /// <remarks>
    /// This method uses <see cref="Lazy{T}"/> to ensure thread-safe, one-time initialization of the validator core.
    /// The actual compilation logic is encapsulated within the <see cref="ExpressValidatorCoreBuilder{T, TExternalResources}"/>,
    /// which implements the rule expression builder interfaces.
    /// </remarks>
    public static ExpressValidatorCore<T, TExternalResources> GetOrAddExpressValidatorCore<T, TExternalResources>(
        Type validatorType,
        Action<IPreValidationDefinitionBuilder<T, TExternalResources>> preValidationRuleCreator,
        Action<ICoreValidationDefinitionBuilder<T, TExternalResources>> ruleCreator)
    {
        // TODO: The ExpressValidatorCoreBuilder class needs to be implemented.
        // It should implement both IPreValidationRuleExpressionBuilder<T, TExternalResources> and IRuleExpressionBuilder<T, TExternalResources>.
        // This builder is responsible for collecting the rules defined by the user in ExpressPreValidationRules and ExpressRules,
        // and then compiling them into an ExpressValidatorCore.

        Lazy<ValidatorCore> validationCore = _coresCache.GetOrAdd(
            validatorType,
            (type, action) => new Lazy<ValidatorCore>(() =>
            {
                // Get validator name from attribute or fallback to type name
                string name = Attribute.GetCustomAttribute(type, typeof(ValidatorNameAttribute))
                is not ValidatorNameAttribute myAttribute
                ? type.Name
                : myAttribute.ValidatorName;

                // Create an instance of the builder, which will gather the rules
                // ExpressValidatorCoreBuilder<T, TExternalResources> builder = new(name); // Uncomment once implemented
                // action.preValidationRuleCreator(builder); // Uncomment once implemented
                // action.ruleCreator(builder); // Uncomment once implemented
                // return builder.CreateValidatorCore(); // Uncomment once implemented

                // Placeholder return - replace with actual builder creation and core return
                throw new NotImplementedException("ExpressValidatorCoreBuilder needs to be implemented to create the validator core.");
            }),
            (preValidationRuleCreator, ruleCreator)); // Passed as state to the factory delegate

        // Cast the generic ValidatorCore to the specific ExpressValidatorCore type.
        // This cast is safe because the Lazy<ValidatorCore> will contain an ExpressValidatorCore.
        return (ExpressValidatorCore<T, TExternalResources>)validationCore.Value;
    }
}
