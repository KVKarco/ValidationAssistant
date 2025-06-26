namespace KVKarco.ValidationAssistant.Internal.PreValidation;

internal sealed class PreValidationFailureInfo :
    ComponentFailureInfo
{
    public PreValidationFailureInfo(int declaredOnLine)
        : base(declaredOnLine, FailureSeverity.Error, ComponentFailureStrategy.Stop)
    {
        Title = $"""

            Severity         : {FailureSeverity.Error}
            WithErrorMessage : 
            """;
    }

    public sealed override string Title { get; }
}