namespace KVKarco.ValidationAssistant.Internal.ValidationLeafAssets;

internal interface IValidationLeafOptionsBuilder<TSubject, TResources, TTarget>
{
    IValidationLeafOptions<TSubject, TResources, TTarget> Create();
}
