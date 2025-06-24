namespace KVKarco.ValidationAssistant.Internal.FailureAssets;

/// <summary>
/// Represents the structured title information for a ValidationRule(Component) failure.
/// This class encapsulates details about the failing rule, including its name,
/// where it was declared, and how influence the validation run.
/// </summary>
internal sealed class ValidationFailureInfo : IEquatable<ValidationFailureInfo?>
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
    }

    public int DeclaredOnLine { get; }

    public ComponentFailureStrategy Strategy { get; }

    public string Title { get; set; }

    public static ValidationFailureInfo New(ReadOnlySpan<char> ruleName, int declaredOnLine, FailureSeverity severity, ComponentFailureStrategy strategy)
        => new(ruleName, declaredOnLine, severity, strategy);

    public override bool Equals(object? obj)
    {
        return Equals(obj as ValidationFailureInfo);
    }

    public bool Equals(ValidationFailureInfo? other)
    {
        return other is not null &&
               DeclaredOnLine == other.DeclaredOnLine &&
               Strategy == other.Strategy &&
               Title == other.Title;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(DeclaredOnLine, Strategy, Title);
    }

    public static bool operator ==(ValidationFailureInfo? left, ValidationFailureInfo? right)
    {
        return EqualityComparer<ValidationFailureInfo>.Default.Equals(left, right);
    }

    public static bool operator !=(ValidationFailureInfo? left, ValidationFailureInfo? right)
    {
        return !(left == right);
    }
}
