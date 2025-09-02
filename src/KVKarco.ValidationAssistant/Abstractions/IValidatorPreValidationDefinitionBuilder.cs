using KVKarco.ValidationAssistant.Exceptions;
using KVKarco.ValidationAssistant.Internal.CustomValidatorAssets;
using KVKarco.ValidationAssistant.Internal.CustomValidatorAssets.Abstractions;
using KVKarco.ValidationAssistant.Internal.Utilities;
using KVKarco.ValidationAssistant.Internal.Utilities.TargetAssets;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;

namespace KVKarco.ValidationAssistant.Abstractions;

//BIG NOTE: add flag in the rules options to know if its a failure rule or information failure so we can know if we need log or log with failure.
//and do we ValidationRunException and to give the power to user to choose do we throw or return messages.


/// <summary>
/// Provides methods to configure the pre-validation stage of a validator.
/// This stage includes setting defaults, defining guards, and enriching resources.
/// </summary>
/// <typeparam name="TSubject">The type of the instance to be validated.</typeparam>
/// <typeparam name="TResources">The type of the external resources.</typeparam>
public interface IValidatorPreValidationDefinitionBuilder<TSubject, TResources>
{
    IValidationDefaultsConfigurator Defaults { get; }

    ISchemaGuardBuilder<TSubject> EnsureSchema([CallerLineNumber] int callingFileLineNumber = 0);

    IResourceGuardBuilder<TResources, TResource> Guard<TResource>(
        Expression<Func<TResources, TResource>> selector,
        [CallerLineNumber] int callingFileLineNumber = 0);

    void EnrichResources(
        Action<IPreValidationCtx<TResources>> action,
        [CallerLineNumber] int callingFileLineNumber = 0);

    void EnrichResourcesAsync(
        AsyncAction<IPreValidationCtx<TResources>> action,
        [CallerLineNumber] int callingFileLineNumber = 0);
}

internal abstract class ValidationCoreBuilder : IValidationDefaultsConfigurator
{
    protected CultureInfo _defaultCulture;
    protected FlowEffect _defaultValidatorLevelFlowEffect;
    protected FlowEffect _defaultRuleLevelFlowEffect;
    protected Severity _defaultRuleFailureSeverity;

    /// <summary>
    /// Stores the condition delegate (sync or async) from the correct UseWhen/UseWhenAsync call.
    /// This is used by the subsequent OtherwiseUse/OtherwiseUseAsync call to create the corresponding
    /// ConditionalFlowValidatorRule. This field supports the chaining of conditional blocks.
    /// </summary>
    protected Delegate? _conditionToChain;

    protected ValidationCoreBuilder()
    {
        _defaultCulture = ValidationAssistantConfig.GlobalDefaults.DefaultCulture;
        _defaultValidatorLevelFlowEffect = ValidationAssistantConfig.GlobalDefaults.DefaultValidatorLevelFlowEffect;
        _defaultRuleLevelFlowEffect = ValidationAssistantConfig.GlobalDefaults.DefaultRuleLevelFlowEffect;
        _defaultRuleFailureSeverity = ValidationAssistantConfig.GlobalDefaults.DefaultRuleFailureSeverity;
    }

    #region Defaults

    public IValidationDefaultsConfigurator Defaults => this;

    public IValidationDefaultsConfigurator SetCulture(CultureInfo culture)
    {
        _defaultCulture = culture;
        return this;
    }

    public IValidationDefaultsConfigurator SetValidatorLevelFlowEffect(FlowEffect effect)
    {
        _defaultValidatorLevelFlowEffect = effect;
        return this;
    }

    public IValidationDefaultsConfigurator SetRuleLevelFlowEffect(FlowEffect effect)
    {
        _defaultRuleLevelFlowEffect = effect;
        return this;
    }

    public IValidationDefaultsConfigurator SetSeverity(Severity severity)
    {
        _defaultRuleFailureSeverity = severity;
        return this;
    }

    #endregion
}

internal sealed class ValidatorCoreBuilderV1<TSubject, TResources> :
    ValidationCoreBuilder,
    IValidatorPreValidationDefinitionBuilder<TSubject, TResources>,
    ISchemaGuardBuilder<TSubject>
{
    private readonly List<IValidatorNode<TSubject, TResources>> _validatorComponents = [];

    private IValidatorComponentBuilder<TSubject, TResources>? _previousComponentBuilder;


    public ValidatorCoreBuilderV1()
    {

    }



    #region schema guard

    public ISchemaGuardBuilder<TSubject> EnsureSchema([CallerLineNumber] int callingFileLineNumber = 0)
    {
        ResolveLastRule();
        _previousComponentBuilder = new SchemaGuardBuilder<TSubject, TResources>(callingFileLineNumber);
        return this;
    }

    public ISchemaGuardBuilder<TSubject> Contains<TTarget>(Expression<Func<TSubject, TTarget>> selector)
    {
        // is it valid selector.
        Ensure.IsValidSelector(selector);

        // we get the meta for the target.
        TargetCtx<TSubject, TTarget> ctx = InternalCache.GetOrExtractPropertyCtx(selector);

        if (_previousComponentBuilder is SchemaGuardBuilder<TSubject, TResources> schemaGuardBuilder)
        {
            schemaGuardBuilder.AddTargetToGuard(ctx);
        }
        else
        {
            throw new ValidationAssistantInternalException("Debug how schemaGuardBuilder is null");
        }

        return this;
    }

    #endregion

    #region Resources Gurds and enrichment

    public IResourceGuardBuilder<TResources, TResource> Guard<TResource>(
        Expression<Func<TResources, TResource>> selector,
        [CallerLineNumber] int callingFileLineNumber = 0)
    {
        //ensures the selector is valid.
        Ensure.IsValidSelector(selector);

        //creates a component builder for the specific guard. similar to Target builder.
        throw new NotImplementedException();
    }

    public void EnrichResources(Action<IPreValidationCtx<TResources>> action, [CallerLineNumber] int callingFileLineNumber = 0)
    {
        throw new NotImplementedException();
    }

    public void EnrichResourcesAsync(AsyncAction<IPreValidationCtx<TResources>> action, [CallerLineNumber] int callingFileLineNumber = 0)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region not done


    #endregion

    /// <summary>
    /// Resolves the last rule that was being built via the fluent API and adds it
    /// to the internal list of validation rules. This method is typically called
    /// implicitly by the fluent API whenever a new rule definition begins,
    /// or explicitly when the rule definition is complete.
    /// </summary>
    private void ResolveLastRule()
    {
        // Clear the chained condition when a new rule is resolved, as it marks the end of a conditional block chain.
        _conditionToChain = null;
        if (_previousComponentBuilder is not null)
        {
            // Capture the builder instance and null out the field to prepare for the next rule. 
            IValidatorComponentBuilder<TSubject, TResources> builder = _previousComponentBuilder;
            _previousComponentBuilder = null;

            // Build the concrete ValidatorRule from the IBuildableRule and add it to the list.
            var component = builder.Build();
            if (component is not null)
            {
                _validatorComponents.Add(component);
            }
        }
    }

}

internal interface IValidatorComponentBuilder<TSubject, TResources>
{
    IValidatorNode<TSubject, TResources>? Build();
}

internal sealed class ResourcesGuardValidatorNodeBuilder<TSubject, TResources, TTarget> :
    IResourceGuardBuilder<TResources, TTarget>
{
    private readonly int _declaredOnLine;
    private readonly TargetCtx<TResources, TTarget> _targetCtx;
    private readonly List<IValidatorLeaf<TSubject, TResources, TTarget>> _validationLeaves;

    public ResourcesGuardValidatorNodeBuilder(
        int declaredOnLine,
        TargetCtx<TResources, TTarget> targetCtx,
        List<IValidatorLeaf<TSubject, TResources, TTarget>> validationLeaves)
    {
        _declaredOnLine = declaredOnLine;
        _targetCtx = targetCtx;
        _validationLeaves = validationLeaves;
    }

    #region Resources Guards
    public IResourceGuardBuilder<TResources, TTarget> Against(
        Predicate<TTarget> predicate,
        [CallerLineNumber] int callingFileLineNumber = 0)
    {
        //ensure is valid predicate.
        Ensure.PredicateNotNullOrHasClosure(predicate);


        throw new NotImplementedException();
    }

    public IResourceGuardBuilder<TResources, TTarget> AgainstAsync(
        AsyncPredicate<IPreValidationCtx<TTarget>> predicate,
        [CallerLineNumber] int callingFileLineNumber = 0)
    {
        throw new NotImplementedException();
    }

    public IResourceGuardBuilder<TResources, TTarget> AgainstNull(
        [CallerLineNumber] int callingFileLineNumber = 0)
    {
        throw new NotImplementedException();
    }

    #endregion
}

internal sealed class SchemaGuardBuilder<TSubject, TResources> :
    IValidatorComponentBuilder<TSubject, TResources>
{
    private readonly int _declaredOnLine;
    private List<TargetCtx<TSubject>>? _pathsToGuardFor;

    public SchemaGuardBuilder(int declaredOnLine) => _declaredOnLine = declaredOnLine;

    public void AddTargetToGuard(TargetCtx<TSubject> context)
    {
        _pathsToGuardFor ??= [];
        _pathsToGuardFor.Add(context);
    }

    public IValidatorNode<TSubject, TResources>? Build()
    {
        if (_pathsToGuardFor is null)
        {
            return null;
        }

        return new SchemaGuardValidatorComponent<TSubject, TResources>(_pathsToGuardFor, _declaredOnLine);
    }
}

internal sealed class SchemaGuardValidatorComponent<TSubject, TResources> :
    IValidatorNode<TSubject, TResources>
{
    private readonly int _declaredOnLine;
    private readonly ImmutableArray<TargetCtx<TSubject>> _pathsToGuard;

    public SchemaGuardValidatorComponent(List<TargetCtx<TSubject>> targetContext, int declaredOnLine)
    {
        _pathsToGuard = [.. targetContext];
        _declaredOnLine = declaredOnLine;
    }

    public bool CanExecuteSynchronously => true;

    public void Execute(CustomValidatorRunCtx<TSubject, TResources> context)
    {
        StringBuilder? sb = null;
        context.ForSchemaGuard(_declaredOnLine);

        for (int i = 0; i < _pathsToGuard.Length; i++)
        {
            (bool isUndefined, string? missingMember) = _pathsToGuard[i].Check(context.Subject);
            if (isUndefined)
            {
                if (sb is null)
                {
                    sb = new();
                    sb.AppendLine("One or more required members can`t be accessed:");
                    sb.AppendLine("Broken paths:");
                }

                context.ForProperty(_pathsToGuard[i].Key, 0, FlowEffect.Stop);
                sb.AppendLine();
                sb.AppendLine("Broken path: ");
                sb.Append(context.CorrectPropertyPath);
                sb.AppendLine("Null member in the path: ");
                sb.Append(missingMember);
            }
        }

        if (sb is not null)
        {
            context.PathBroken(sb.ToString());
        }
    }

    public ValueTask ExecuteAsync(CustomValidatorRunCtx<TSubject, TResources> context, CancellationToken ct)
    {
        Execute(context);
        return ValueTask.CompletedTask;
    }
}









