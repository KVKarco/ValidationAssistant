using KVKarco.ValidationAssistant.Internal.PreValidation;

namespace KVKarco.ValidationAssistant.Internal.ExpressValidator;

/// <summary>
/// Represents the sealed, concrete implementation of the validator core specifically designed
/// for the ExpressValidator framework. This class orchestrates the execution of both
/// pre-validation rules and main validation rules, leveraging the capabilities of
/// <see cref="ValidatorCore{T, TExternalResources, TContex}"/> with the specific context type
/// <see cref="ExpressValidatorRunCtx{T, TExternalResources}"/>.
/// </summary>
/// <typeparam name="T">The type of the main object instance being validated by this core.</typeparam>
/// <typeparam name="TExternalResources">The type of external resources that can be accessed by the validation rules.</typeparam>
internal sealed class ExpressValidatorCore<T, TExternalResources> :
    // Inherits from ValidatorCore, specializing it with ExpressValidatorRunCtx as the context.
    ValidatorCore<T, TExternalResources, ExpressValidatorRunCtx<T, TExternalResources>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExpressValidatorCore{T, TExternalResources}"/> class.
    /// This constructor primarily forwards the provided rule collections and metadata to the base
    /// <see cref="ValidatorCore{T, TExternalResources, TContex}"/> constructor for compilation and setup.
    /// </summary>
    /// <param name="validatorName">The descriptive name of this validator core.</param>
    /// <param name="snapShots">An optional list of string snapshots, potentially used for debugging or capturing rule definitions state.</param>
    /// <param name="preValidationRules">A list of compiled <see cref="IPreValidationRule{T, TExternalResources}"/> instances
    /// that will be executed before the main validation rules.</param>
    /// <param name="rules">A list of compiled <see cref="IValidatorRule{T, TExternalResources, TContex}"/> instances
    /// that define the primary validation logic for this core.</param>
    public ExpressValidatorCore(
        string validatorName,
        List<string>? snapShots,
        List<IPreValidationRule<T, TExternalResources>> preValidationRules,
        List<IValidatorRule<T, TExternalResources, ExpressValidatorRunCtx<T, TExternalResources>>> rules)
        : base(validatorName, snapShots, preValidationRules, rules) // Pass all parameters to the base ValidatorCore constructor.
    {
        // No additional logic is required in this concrete constructor beyond calling the base.
    }
}
