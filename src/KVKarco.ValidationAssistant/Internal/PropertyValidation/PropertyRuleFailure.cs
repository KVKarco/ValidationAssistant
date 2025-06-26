using System.Collections.ObjectModel;
using System.Text;

namespace KVKarco.ValidationAssistant.Internal.PropertyValidation;

/// <summary>
/// Represents a rule failure specifically associated with a property during validation.
/// This type of rule failure can contain one or more granular <see cref="ValidationFailure"/> instances
/// that specify issues found within the property's value.
/// </summary>
/// <typeparam name="TProperty">The type of the property that the rule is validating.</typeparam>
internal sealed class PropertyRuleFailure<TProperty> : RuleFailure
{
    /// <summary>
    /// A private list to store individual <see cref="ValidationFailure"/> instances for this property rule.
    /// This list is lazily initialized when the first validation failure is added.
    /// </summary>
    private List<ValidationFailure>? _validationFailures;

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyRuleFailure{TProperty}"/> class.
    /// </summary>
    /// <param name="property">The <see cref="Undefined{TProperty}"/> instance representing the property and its value.</param>
    /// <param name="path">The property path associated with this rule failure.</param>
    /// <param name="info">Metadata and configuration details for this property rule failure.</param>
    /// <param name="explanation">A high-level textual explanation of why the property rule failed.</param>
    public PropertyRuleFailure(Undefined<TProperty> property, string path, RuleFailureInfo info, string explanation) : base(path, info, explanation)
    {
        Property = property;
    }

    /// <summary>
    /// Gets the <see cref="Undefined{TProperty}"/> instance that encapsulates the property's value
    /// and its state (present/missing, null).
    /// </summary>
    public Undefined<TProperty> Property { get; }

    /// <summary>
    /// Gets a value indicating whether this property rule failure contains any specific <see cref="ValidationFailure"/> instances.
    /// Returns <see langword="true"/> if there are collected validation failures; otherwise, <see langword="false"/>.
    /// </summary>
    public sealed override bool HasValidationFailures => _validationFailures is not null
        && _validationFailures.Exists(x => x.Severity == FailureSeverity.Error || x.Severity == FailureSeverity.Warning);

    /// <summary>
    /// Gets a read-only collection of <see cref="ValidationFailure"/> instances that occurred for this property.
    /// Returns an empty collection if no validation failures have been added.
    /// </summary>
    public sealed override IReadOnlyCollection<ValidationFailure> ValidationFailures => _validationFailures is not null
        ? _validationFailures.Where(x => x.Severity == FailureSeverity.Error || x.Severity == FailureSeverity.Warning).ToArray()
        : [];

    /// <summary>
    /// Gets a read-only collection of error messages from the <see cref="ValidationFailure"/> instances
    /// associated with this property rule, specifically filtering for failures with <see cref="FailureSeverity.Error"/>.
    /// Returns an empty collection if no error messages are present.
    /// </summary>
    public sealed override IReadOnlyCollection<string> ValidationFailuresMessages =>
        _validationFailures is not null
        ? new ReadOnlyCollection<string>(_validationFailures.Where(x => x.Severity == FailureSeverity.Error).Select(x => x.Message).ToArray())
        : [];

    /// <summary>
    /// Adds a <see cref="ValidationFailure"/> to the collection of failures for this property rule.
    /// The internal list of validation failures is initialized if it does not already exist.
    /// </summary>
    /// <param name="failure">The <see cref="ValidationFailure"/> instance to add.</param>
    public sealed override void AddValidationFailure(ValidationFailure failure)
    {
        _validationFailures ??= []; // Initialize list if null
        _validationFailures.Add(failure); // Add the new validation failure
    }

    /// <summary>
    /// Appends a formatted explanation of this property rule failure to the provided <see cref="StringBuilder"/>.
    /// It includes separators, the rule's title, its explanation, the property's value,
    /// and then appends explanations for any nested <see cref="ValidationFailure"/> instances.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to which the explanation will be appended.</param>
    public sealed override void AttachToExplanation(StringBuilder sb)
    {
        sb.AppendLine(DefaultNaming.Lines);
        sb.AppendLine(Info.Title);
        sb.Append(Explanation);

        if (_validationFailures is not null && _validationFailures.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Property value: ");
            sb.AppendLine(Property.ToString()); // Append string representation of the property's value

            for (int i = 0; i < _validationFailures.Count; i++)
            {
                _validationFailures[i].AttachToExplanation(sb); // Append explanation for each nested validation failure
            }
        }

        sb.AppendLine(DefaultNaming.Lines);
    }
}
