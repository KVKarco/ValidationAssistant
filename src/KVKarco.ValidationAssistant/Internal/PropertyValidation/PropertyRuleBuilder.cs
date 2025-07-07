using KVKarco.ValidationAssistant.Abstractions;
using KVKarco.ValidationAssistant.Exceptions;
using KVKarco.ValidationAssistant.Internal.GenericValidationRules;
using System.Runtime.CompilerServices;

namespace KVKarco.ValidationAssistant.Internal.PropertyValidation;

/// <summary>
/// Represents the abstract base class for building and configuring property-specific validation rules.
/// This class implements the fluent interfaces <see cref="IInitialPropertyRuleBuilder{T, TExternalResources, TProperty}"/>
/// and <see cref="IValidationComponentFailureOptionsBuilder{T, TExternalResources, TProperty}"/>,
/// providing the foundational structure for defining validation logic for a single property.
/// Concrete implementations will provide the actual logic for adding and configuring validation components.
/// </summary>
/// <typeparam name="T">The type of the main instance being validated.</typeparam>
/// <typeparam name="TExternalResources">The type providing external resources or dependencies.</typeparam>
/// <typeparam name="TProperty">The type of the property for which the rule is being built.</typeparam>
internal abstract class PropertyRuleBuilder<T, TExternalResources, TProperty> :
    IInitialPropertyRuleBuilder<T, TExternalResources, TProperty>,
    IValidationComponentFailureOptionsBuilder<T, TExternalResources, TProperty>
{
    // Fields to manage the state of the current property rule being built.
    protected readonly int _ruleDeclaredOnLine;
    protected readonly List<string> _snapShots; // Reference to the validator's snapshots
    protected readonly List<PropertyRuleComponent<T, TExternalResources, TProperty>> _ruleComponents = []; // Components added to this specific property rule

    // Rule-level default failure strategies
    protected RuleFailureStrategy _onRuleFailure;

    private readonly PendingComponentState<T, TExternalResources, TProperty> _componentResolver;

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyRuleBuilder{T, TExternalResources, TProperty}"/> class.
    /// </summary>
    /// <param name="defaultRuleFailureStrategy">The default rule failure strategy inherited from the validator builder.</param>
    /// <param name="defaultComponentFailureStrategy">The default component failure strategy inherited from the validator builder.</param>
    /// <param name="declaredOnLine">The line number where this property rule was declared in the source code.</param>
    /// <param name="snapShots">A reference to the list of snapshots from the main validator builder.</param>
    protected PropertyRuleBuilder(
        RuleFailureStrategy defaultRuleFailureStrategy,
        ComponentFailureStrategy defaultComponentFailureStrategy,
        int declaredOnLine,
        List<string> snapShots)
    {
        _ruleDeclaredOnLine = declaredOnLine;
        _snapShots = snapShots; // Keep a reference to the shared snapshots list
        _onRuleFailure = defaultRuleFailureStrategy;
        _componentResolver = new PendingComponentState<T, TExternalResources, TProperty>(
            defaultComponentFailureStrategy,
            FailureSeverity.Error); // Default severity for components is Error
    }

    /// <summary>
    /// Resolves the currently pending component and adds it to the list of rule components.
    /// This method is called internally by the fluent API whenever a new component is added
    /// or when the rule definition is finalized. It applies the collected failure options
    /// to the component before adding it.
    /// </summary>
    protected void ResolveLastComponent()
    {
        if (_componentResolver.IsReadyForNextComponent)
        {
            // If the last component is already resolved, we can skip further processing.
            return;
        }

        _ruleComponents.Add(_componentResolver.GetComponent()); // Add the built component to the rule's list
    }

    #region default failure strategies

    /// <inheritdoc/>
    public IInitialPropertyRuleBuilder<T, TExternalResources, TProperty> DefaultOnRuleFailure(
        RuleFailureStrategy onFailure)
    {
        // No need to ResolveLastComponent here as these are initial rule-level settings.
        _onRuleFailure = onFailure;
        return this;
    }

    /// <inheritdoc/>
    public IInitialPropertyRuleBuilder<T, TExternalResources, TProperty> DefaultOnComponentFailure(
        ComponentFailureStrategy onFailure,
        FailureSeverity severity = FailureSeverity.Error)
    {
        // No need to ResolveLastComponent here as these are initial rule-level settings.
        _componentResolver.ChangeDefaults(onFailure, severity);
        return this;
    }

    #endregion

    #region conditional flow rules

    /// <inheritdoc/>
    public IPropertyRuleBuilder<T, TExternalResources, TProperty> CaptureSnapShot(
        string snapShotIdentifier,
        [CallerLineNumber] int callingFileLineNumber = 0)
    {
        ResolveLastComponent(); // Resolve any previous component before adding this one.
        RuleCreationException.ThrowIfNullOrWhiteSpaces(snapShotIdentifier);

        // Ensure the snapshot identifier has not been used before in this validator.
        if (_snapShots.Exists(x => x == snapShotIdentifier))
        {
            throw new RuleCreationException($"SnapShot with identifier {snapShotIdentifier} declared on line : {callingFileLineNumber} already is in use.");
        }
        _snapShots.Add(snapShotIdentifier); // Add the new snapshot to the validator's list.

        _componentResolver.NextComponent(
            new SnapShotValidationRule<T, TExternalResources, TProperty>(snapShotIdentifier, true),
            callingFileLineNumber,
            ComponentFailureStrategy.Continue,
            FailureSeverity.Info);

        return this;
    }

    /// <inheritdoc/>
    public IPropertyRuleBuilder<T, TExternalResources, TProperty> ContinueWhen(
        string snapShotIdentifier,
        string? failureInfoMessage = null,
        [CallerLineNumber] int callingFileLineNumber = 0)
    {
        ResolveLastComponent(); // Resolve any previous component before adding this one.
        RuleCreationException.ThrowIfNullOrWhiteSpaces(snapShotIdentifier);

        // Ensure the snapshot identifier has been declared at the validator level.
        if (!_snapShots.Contains(snapShotIdentifier))
        {
            throw new RuleCreationException($"SnapShot with identifier '{snapShotIdentifier}' declared on line: {callingFileLineNumber} is never captured at the validator level. Please ensure it's defined using SnapShot() on the main validator builder.");
        }

        _componentResolver.NextComponent(
            new SnapShotValidationRule<T, TExternalResources, TProperty>(snapShotIdentifier, false),
            callingFileLineNumber,
            ComponentFailureStrategy.Exit,
            FailureSeverity.Info);

        if (failureInfoMessage is not null)
        {
            _componentResolver.Override(failureInfoMessage); // Override the failure message if provided.
        }

        return this;
    }

    /// <inheritdoc/>
    public IPropertyRuleBuilder<T, TExternalResources, TProperty> ContinueWhen(
        ValidationCondition<T, TExternalResources> condition,
        string? failureInfoMessage = null,
        [CallerLineNumber] int callingFileLineNumber = 0)
    {
        ResolveLastComponent(); // Resolve any previous component before adding this one.
        RuleCreationException.ThrowIfNull(condition);

        _componentResolver.NextComponent(
            new ConditionalFlowValidationRule<T, TExternalResources, TProperty>(condition),
            callingFileLineNumber,
            ComponentFailureStrategy.Exit,
            FailureSeverity.Info);

        if (failureInfoMessage is not null)
        {
            _componentResolver.Override(failureInfoMessage); // Override the failure message if provided.
        }

        return this;
    }

    /// <inheritdoc/>
    public IPropertyRuleBuilder<T, TExternalResources, TProperty> ContinueWhenAsync(
        AsyncValidationCondition<T, TExternalResources> asyncCondition,
        string? failureInfoMessage = null,
        [CallerLineNumber] int callingFileLineNumber = 0)
    {
        ResolveLastComponent(); // Resolve any previous component before adding this one.
        RuleCreationException.ThrowIfNull(asyncCondition);

        _componentResolver.NextComponent(
            new ConditionalFlowAsyncValidationRule<T, TExternalResources, TProperty>(asyncCondition),
            callingFileLineNumber,
            ComponentFailureStrategy.Exit,
            FailureSeverity.Info);

        if (failureInfoMessage is not null)
        {
            _componentResolver.Override(failureInfoMessage); // Override the failure message if provided.
        }

        return this;
    }

    #endregion

    #region validation rules

    /// <inheritdoc/>
    public IValidationComponentFailureOptionsBuilder<T, TExternalResources, TProperty> Ensure(
        PropertyPredicate<TProperty> predicate,
        [CallerLineNumber] int callingFileLineNumber = 0)
    {
        ResolveLastComponent(); // Resolve any previous component before adding this one.
        RuleCreationException.ThrowIfNull(predicate);

        _componentResolver.NextComponent(new PredicateValidationRule<T, TExternalResources, TProperty>(predicate), callingFileLineNumber);

        // Return 'this' to allow chaining of failure options.
        return this;
    }

    /// <inheritdoc/>
    public IValidationComponentFailureOptionsBuilder<T, TExternalResources, TProperty> Ensure(
        ContextAwarePropertyPredicate<T, TExternalResources, TProperty> predicate,
        [CallerLineNumber] int callingFileLineNumber = 0)
    {
        ResolveLastComponent(); // Resolve any previous component before adding this one.
        RuleCreationException.ThrowIfNull(predicate);

        _componentResolver.NextComponent(new ContextAwarePredicateValidationRule<T, TExternalResources, TProperty>(predicate), callingFileLineNumber);

        // Return 'this' to allow chaining of failure options.
        return this;
    }

    /// <inheritdoc/>
    public IValidationComponentFailureOptionsBuilder<T, TExternalResources, TProperty> EnsureAsync(
        AsyncPropertyPredicate<TProperty> asyncPredicate,
        [CallerLineNumber] int callingFileLineNumber = 0)
    {
        ResolveLastComponent(); // Resolve any previous component before adding this one.
        RuleCreationException.ThrowIfNull(asyncPredicate);

        _componentResolver.NextComponent(new PredicateAsyncValidationRule<T, TExternalResources, TProperty>(asyncPredicate), callingFileLineNumber);

        // Return 'this' to allow chaining of failure options.
        return this;
    }

    /// <inheritdoc/>
    public IValidationComponentFailureOptionsBuilder<T, TExternalResources, TProperty> EnsureAsync(
        AsyncContextAwarePropertyPredicate<T, TExternalResources, TProperty> asyncPredicate,
        [CallerLineNumber] int callingFileLineNumber = 0)
    {
        ResolveLastComponent(); // Resolve any previous component before adding this one.
        RuleCreationException.ThrowIfNull(asyncPredicate);

        _componentResolver.NextComponent(new ContextAwarePredicateAsyncValidationRule<T, TExternalResources, TProperty>(asyncPredicate), callingFileLineNumber);

        // Return 'this' to allow chaining of failure options.
        return this;
    }

    /// <inheritdoc/>
    public IValidationComponentFailureOptionsBuilder<T, TExternalResources, TProperty> UseRule(
        IValidationRule<T, TExternalResources, TProperty> rule,
        [CallerLineNumber] int callingFileLineNumber = 0)
    {
        ResolveLastComponent(); // Resolve any previous component before adding this one.
        RuleCreationException.ThrowIfNull(rule);

        _componentResolver.NextComponent(rule, callingFileLineNumber);

        // Return 'this' to allow chaining of failure options.
        return this;
    }

    /// <inheritdoc/>
    public IValidationComponentFailureOptionsBuilder<T, TExternalResources, TProperty> UseRuleAsync(
        IAsyncValidationRule<T, TExternalResources, TProperty> asyncRule,
        [CallerLineNumber] int callingFileLineNumber = 0)
    {
        ResolveLastComponent(); // Resolve any previous component before adding this one.
        RuleCreationException.ThrowIfNull(asyncRule);

        _componentResolver.NextComponent(asyncRule, callingFileLineNumber);

        // Return 'this' to allow chaining of failure options.
        return this;
    }

    #endregion

    #region validation component failure options

    /// <inheritdoc/>
    public IValidationComponentFailureOptionsBuilder<T, TExternalResources, TProperty> OnFailure(
       ComponentFailureStrategy onFailure)
    {
        _componentResolver.Override(onFailure);
        return this;
    }

    /// <inheritdoc/>
    public IValidationComponentFailureOptionsBuilder<T, TExternalResources, TProperty> Severity(
        FailureSeverity failureSeverity)
    {
        _componentResolver.Override(failureSeverity);
        return this;
    }

    /// <inheritdoc/>
    public IValidationComponentFailureOptionsBuilder<T, TExternalResources, TProperty> WithMessage(
        string failureMessage)
    {
        RuleCreationException.ThrowIfNullOrWhiteSpaces(failureMessage);
        _componentResolver.Override(failureMessage);
        return this;
    }

    /// <inheritdoc/>
    public IValidationComponentFailureOptionsBuilder<T, TExternalResources, TProperty> WithMessage(
        FailureMessageFactory<T, TExternalResources, TProperty> failureMessageFactory)
    {
        RuleCreationException.ThrowIfNull(failureMessageFactory);
        _componentResolver.Override(failureMessageFactory);
        return this;
    }

    #endregion
}
