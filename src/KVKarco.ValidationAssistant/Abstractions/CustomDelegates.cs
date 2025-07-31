using KVKarco.ValidationAssistant.Internal.PropertyValidation;

namespace KVKarco.ValidationAssistant.Abstractions;

public delegate bool Predicate<T, TExternalResources, in TMember>(
    IValidationCtx<T, TExternalResources> context,
    TMember value);

public delegate Task<bool> AsyncPredicate<T, TExternalResources, in TMember>(
    IValidationCtx<T, TExternalResources> context,
    TMember value,
    CancellationToken ct);

public delegate bool Condition<T, TExternalResources>(
    IConditionCtx<T, TExternalResources> context);

public delegate bool Condition<T, TExternalResources, in TMember>(
    IConditionCtx<T, TExternalResources> context, TMember value);

public delegate Task<bool> AsyncCondition<T, TExternalResources>(
    IConditionCtx<T, TExternalResources> context,
    CancellationToken ct);

public delegate Task<bool> AsyncCondition<T, TExternalResources, in TMember>(
    IConditionCtx<T, TExternalResources> context,
    TMember value,
    CancellationToken ct);

public delegate string MessageFactory<T, TExternalResources, in TMember>(
    IMessageCtx<T, TExternalResources> context,
    TMember value);


/// <summary>
/// Defines an internal delegate responsible for resolving (extracting) the value of a specific property
/// from a given validation instance. This delegate is typically created by compiling an
/// expression (e.g., a property selector like `x => x.SomeProperty`) and is used internally
/// by property rules to efficiently access the values they need to validate.
/// </summary>
/// <typeparam name="T">The type of the main validation instance from which the property value is to be extracted.</typeparam>
/// <typeparam name="TProperty">The type of the property whose value is to be extracted.</typeparam>
/// <param name="validationInstance">The instance of type <typeparamref name="T"/> from which the property value will be resolved.</param>
/// <returns>
/// An <see cref="Undefined{TProperty}"/> instance representing the resolved property value.
/// This wrapper can distinguish between a property that is genuinely <see langword="null"/>
/// and one that is conceptually "undefined" or not present due to, for example, a null intermediate
/// in a property chain (e.g., `x.Address.Street` where `Address` is null).
/// </returns>
internal delegate Undefined<TProperty> PropertyValueResolver<T, TProperty>(T validationInstance);