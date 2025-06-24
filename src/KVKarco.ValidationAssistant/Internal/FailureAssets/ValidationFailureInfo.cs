namespace KVKarco.ValidationAssistant.Internal.FailureAssets;

/// <summary>
/// Represents the structured title information for a ValidationRule(Component) failure.
/// This class encapsulates details about the failing rule, including its name,
/// where it was declared, and how influence the validation run.
/// </summary>
internal abstract class ValidationFailureInfo
{
    public ValidationFailureInfo(ReadOnlySpan<char> ruleName, int declaredOnLine, FailureSeverity severity, ComponentFailureStrategy strategy)
    {
        Title = $"""

            RuleName          : {ruleName}
            DeclaredOnLine    : {declaredOnLine}
            Severity          : {severity}
            WithMessage       : 

            """;

        Strategy = strategy;
        DeclaredOnLine = declaredOnLine;
        Severity = severity;
    }

    public int DeclaredOnLine { get; }

    public ComponentFailureStrategy Strategy { get; }

    public FailureSeverity Severity { get; }

    public string Title { get; set; }

    public static ValidationFailureInfo<T, TExternalResources, TProperty> New<T, TExternalResources, TProperty>(
        FailureMessageFactory<T, TExternalResources, TProperty> failureMessageFactory,
        ReadOnlySpan<char> ruleName,
        int declaredOnLine,
        FailureSeverity severity,
        ComponentFailureStrategy strategy)
        => new(failureMessageFactory, ruleName, declaredOnLine, severity, strategy);
}

internal sealed class ValidationFailureInfo<T, TExternalResources, TProperty> :
    ValidationFailureInfo
{
    public ValidationFailureInfo(
        FailureMessageFactory<T, TExternalResources, TProperty> failureMessageFactory,
        ReadOnlySpan<char> ruleName,
        int declaredOnLine,
        FailureSeverity severity,
        ComponentFailureStrategy strategy)
        : base(ruleName, declaredOnLine, severity, strategy)
    {
        FailureMessageFactory = failureMessageFactory;
    }

    public FailureMessageFactory<T, TExternalResources, TProperty> FailureMessageFactory { get; }
}
