using System.Text;

namespace KVKarco.ValidationAssistant.Internal.ValidationFlow;

/// <summary>
/// Represents a rule failure that is not tied to a specific property path but rather to a
/// higher-level logical condition or a validator-wide check.
/// Logical rule failures do not contain nested <see cref="ValidationFailure"/> instances.
/// </summary>
internal sealed class LogicalRuleFailure : RuleFailure
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LogicalRuleFailure"/> class.
    /// </summary>
    /// <param name="info">Metadata and configuration details for this logical rule failure.</param>
    /// <param name="explanation">A textual explanation of why the logical rule failed.</param>
    public LogicalRuleFailure(RuleFailureInfo info, string explanation) : base("", info, explanation)
    {
    }

    /// <summary>
    /// Gets a value indicating that a logical rule failure never has nested validation failures.
    /// Always returns <see langword="false"/>.
    /// </summary>
    public sealed override bool HasValidationFailures => false;

    /// <summary>
    /// This property is not implemented for <see cref="LogicalRuleFailure"/> as it does not track
    /// granular <see cref="ValidationFailure"/> instances. Accessing it will result in an exception.
    /// </summary>
    /// <exception cref="NotImplementedException">Always thrown when attempting to access this property.</exception>
    public sealed override IReadOnlyCollection<ValidationFailure> ValidationFailures => throw new NotImplementedException();

    /// <summary>
    /// This property is not implemented for <see cref="LogicalRuleFailure"/> as it does not track
    /// granular validation messages. Accessing it will result in an exception.
    /// </summary>
    /// <exception cref="NotImplementedException">Always thrown when attempting to access this property.</exception>
    public sealed override IReadOnlyCollection<string> ValidationFailuresMessages => throw new NotImplementedException();

    /// <summary>
    /// This method is not implemented for <see cref="LogicalRuleFailure"/> as it does not track
    /// granular <see cref="ValidationFailure"/> instances. Calling it will result in an exception.
    /// </summary>
    /// <param name="failure">The validation failure to add (will cause an exception).</param>
    /// <exception cref="NotImplementedException">Always thrown when attempting to call this method.</exception>
    public sealed override void AddValidationFailure(ValidationFailure failure) => throw new NotImplementedException();

    /// <summary>
    /// Appends a formatted explanation of this logical rule failure to the provided <see cref="StringBuilder"/>.
    /// It includes separators, the rule's title, and its explanation.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to which the explanation will be appended.</param>
    public sealed override void AttachToExplanation(StringBuilder sb)
    {
        sb.AppendLine("---------------------------------------------------------------------------");
        sb.AppendLine(Info.Title);
        sb.Append(Explanation);
        sb.AppendLine("---------------------------------------------------------------------------");
    }
}
