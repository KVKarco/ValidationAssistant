using KVKarco.ValidationAssistant.Exceptions;
using KVKarco.ValidationAssistant.ValidationRules;

namespace KVKarco.ValidationAssistant.Internal.PropertyValidation;

/// <summary>
/// Manages the transient state of a single validation component while it is being built
/// within a <see cref="PropertyRuleBuilder{T, TExternalResources, TProperty}"/>.
/// This class encapsulates all the configuration options (e.g., failure strategy, severity, message)
/// and the underlying validation rule or predicate, allowing for a fluent and cohesive API.
/// </summary>
/// <typeparam name="T">The type of the main instance being validated.</typeparam>
/// <typeparam name="TExternalResources">The type providing external resources or dependencies.</typeparam>
/// <typeparam name="TProperty">The type of the property for which the component is being built.</typeparam>
internal sealed class PendingComponentState<T, TExternalResources, TProperty>
{
    private int _declaredOnLine;
    private ComponentFailureStrategy _defaultOnFailure;
    private FailureSeverity _defaultSeverity;
    private ComponentFailureStrategy _onFailure;
    private FailureSeverity _severity;
    private IValidationRule<T, TExternalResources, TProperty> _rule;
    private IAsyncValidationRule<T, TExternalResources, TProperty> _asyncRule;
    private string? _failureMessage;
    private FailureMessageFactory<T, TExternalResources, TProperty>? _failureMessageFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="PendingComponentState{T, TExternalResources, TProperty}"/> class.
    /// Sets initial default failure strategy and severity, and marks the state as resolved,
    /// indicating it's ready to accept a new component definition.
    /// </summary>
    /// <param name="onFailure">The default component failure strategy.</param>
    /// <param name="severity">The default component failure severity.</param>
    public PendingComponentState(ComponentFailureStrategy onFailure, FailureSeverity severity)
    {
        _defaultOnFailure = onFailure;
        _defaultSeverity = severity;
        _rule = null!; // Will be set when a rule is added
        _asyncRule = null!; // Will be set when an async rule is added
        _declaredOnLine = 0; // Will be set when the component is declared
        _onFailure = onFailure; // Default strategy for failure
        _severity = severity; // Default severity for failure
        _failureMessage = null; // Reset failure message
        _failureMessageFactory = null; // Reset failure message factory
        IsReadyForNextComponent = true; // Initially ready to accept a new component
    }

    /// <summary>
    /// Gets a value indicating whether the current state is ready to accept a new component definition.
    /// <see langword="true"/> means the previous component (if any) has been resolved.
    /// <see langword="false"/> means a component is currently being defined and needs to be resolved first.
    /// </summary>
    public bool IsReadyForNextComponent { get; private set; }

    /// <summary>
    /// Changes the default component failure strategy and severity.
    /// These new defaults will apply to subsequent components if not overridden.
    /// </summary>
    /// <param name="onFailure">The new default component failure strategy.</param>
    /// <param name="severity">The new default component failure severity.</param>
    public void ChangeDefaults(ComponentFailureStrategy onFailure, FailureSeverity severity)
    {
        _defaultOnFailure = onFailure;
        _defaultSeverity = severity;
    }

    /// <summary>
    /// Prepares the state for defining a new synchronous validation component.
    /// Throws an exception if a component is already pending resolution.
    /// </summary>
    /// <param name="rule">The synchronous validation rule for the new component.</param>
    /// <param name="declaredOnLine">The line number where this component was declared.</param>
    /// <param name="onFailure">Optional: override the default failure strategy for this component.</param>
    /// <param name="severity">Optional: override the default severity for this component.</param>
    /// <exception cref="ValidationAssistantInternalException">Thrown if a component is already pending resolution.</exception>
    public void NextComponent(
        IValidationRule<T, TExternalResources, TProperty> rule,
        int declaredOnLine,
        ComponentFailureStrategy? onFailure = null,
        FailureSeverity? severity = null)
    {
        if (!IsReadyForNextComponent)
        {
            throw new ValidationAssistantInternalException("Cannot start a new component before the previous one is resolved.");
        }
        _rule = rule;
        _asyncRule = null!; // Reset async rule
        _declaredOnLine = declaredOnLine;
        IsReadyForNextComponent = false; // Mark as not ready for next, as a component is pending
        _onFailure = onFailure ?? _defaultOnFailure; // Use provided or default strategy
        _severity = severity ?? _defaultSeverity; // Use provided or default severity
        _failureMessage = null; // Reset failure message
        _failureMessageFactory = null; // Reset failure message factory
    }

    /// <summary>
    /// Prepares the state for defining a new asynchronous validation component.
    /// Throws an exception if a component is already pending resolution.
    /// </summary>
    /// <param name="rule">The asynchronous validation rule for the new component.</param>
    /// <param name="declaredOnLine">The line number where this component was declared.</param>
    /// <param name="onFailure">Optional: override the default failure strategy for this component.</param>
    /// <param name="severity">Optional: override the default severity for this component.</param>
    /// <exception cref="ValidationAssistantInternalException">Thrown if a component is already pending resolution.</exception>
    public void NextComponent(
        IAsyncValidationRule<T, TExternalResources, TProperty> rule,
        int declaredOnLine,
        ComponentFailureStrategy? onFailure = null,
        FailureSeverity? severity = null)
    {
        if (!IsReadyForNextComponent)
        {
            throw new ValidationAssistantInternalException("Cannot start a new component before the previous one is resolved.");
        }
        _rule = null!; // Reset  rule
        _asyncRule = rule;
        _declaredOnLine = declaredOnLine;
        IsReadyForNextComponent = false; // Mark as not ready for next, as a component is pending
        _onFailure = onFailure ?? _defaultOnFailure; // Use provided or default strategy
        _severity = severity ?? _defaultSeverity; // Use provided or default severity
        _failureMessage = null; // Reset failure message
        _failureMessageFactory = null; // Reset failure message factory
    }

    /// <summary>
    /// Overrides the failure message for the currently pending component with a static string.
    /// </summary>
    /// <param name="failureMessage">The static failure message.</param>
    public void Override(string failureMessage)
    {
        _failureMessage = failureMessage;
        _failureMessageFactory = null;
    }

    /// <summary>
    /// Overrides the failure message for the currently pending component with a dynamic factory.
    /// </summary>
    /// <param name="failureMessageFactory">The dynamic failure message factory.</param>
    public void Override(FailureMessageFactory<T, TExternalResources, TProperty> failureMessageFactory)
    {
        _failureMessage = null;
        _failureMessageFactory = failureMessageFactory;
    }

    /// <summary>
    /// Overrides the component failure strategy for the currently pending component.
    /// </summary>
    /// <param name="onFailure">The new component failure strategy.</param>
    public void Override(ComponentFailureStrategy onFailure)
    {
        _onFailure = onFailure;
    }

    /// <summary>
    /// Overrides the failure severity for the currently pending component.
    /// </summary>
    /// <param name="severity">The new failure severity.</param>
    public void Override(FailureSeverity severity)
    {
        _severity = severity;
    }

    /// <summary>
    /// Gets the compiled <see cref="PropertyRuleComponent{T, TExternalResources, TProperty}"/>
    /// for the currently pending component and marks the state as ready for the next component.
    /// Throws an exception if no component is currently pending.
    /// </summary>
    /// <returns>The compiled property rule component.</returns>
    /// <exception cref="ValidationAssistantInternalException">Thrown if no component is pending resolution.</exception>
    public PropertyRuleComponent<T, TExternalResources, TProperty> GetComponent()
    {
        if (IsReadyForNextComponent) // This means no component is pending to be resolved.
        {
            throw new ValidationAssistantInternalException("No component is pending resolution. Call NextComponent first.");
        }

        IsReadyForNextComponent = true; // Mark as resolved and ready for the next component definition.

        // Determine the failure message factory
        FailureMessageFactory<T, TExternalResources, TProperty> messageFactory;
        if (_failureMessage is not null)
        {
            string capturedMessage = _failureMessage; // Capture for closure
            messageFactory = (_, _) => capturedMessage;
        }
        else if (_failureMessageFactory is not null)
        {
            messageFactory = _failureMessageFactory;
        }
        else
        {
            // Use the default message from the rule itself
            messageFactory = _rule is not null ? _rule.GetDefaultFailureMessage : _asyncRule!.GetDefaultFailureMessage;
        }

        // Create ComponentFailureInfo with all collected options.
        var info = new ValidationRuleFailureInfo<T, TExternalResources, TProperty>(
            messageFactory,
            _rule is not null ? _rule.RuleName : _asyncRule!.RuleName,
            _declaredOnLine,
            _severity,
            _onFailure);

        // Create the PropertyRuleComponent, wrapping the actual rule/predicate.
        return _asyncRule is null
                ? new PropertyRuleComponent<T, TExternalResources, TProperty>(_rule!, info)
                : new PropertyRuleComponent<T, TExternalResources, TProperty>(_asyncRule, info);
    }
}
