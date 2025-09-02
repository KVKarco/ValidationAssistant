using KVKarco.ValidationAssistant.Abstractions;
using KVKarco.ValidationAssistant.Abstractions.MessageTemplate;
using KVKarco.ValidationAssistant.Abstractions.ValidationContexts;
using KVKarco.ValidationAssistant.Exceptions;
using KVKarco.ValidationAssistant.Internal.CustomValidatorAssets;
using KVKarco.ValidationAssistant.Internal.ValidationLeafAssets;
using KVKarco.ValidationAssistant.Internal.ValidationRules;
using KVKarco.ValidationAssistant.Results;
using System.Collections.Immutable;
using System.Globalization;

namespace KVKarco.ValidationAssistant.Internal;

public abstract class ValidationCtx :
    ICleanUpCtx
{
    // the name of the validator that is being run, this is used to identify the validator in the logs and results.
    internal string ForValidator { get; }

    // the snapshots that are available for this validator run, these are used to capture the state of the validation components at a certain point in time.
    // all snapshots are set to false by default, and will reset in run time.
    private readonly (string identifaer, bool isValid)[]? _availableSnapShots;

    // the total number of failures that occurred during the validation run, this is used to determine if the run is valid or not.
    private protected int _totalFailures;

    // the flow impact of the ongoing component(PropertyValidator), if its considered to be failed.
    private protected FlowEffect _ongoingComponentFlowEffect;

    // a skip count for the ongoing component(PropertyValidator) that can set if the component have ability to skip components in the Validator.
    private protected int _ongoingComponentWillSkip;

    // the log of the ongoing component, this is used to log the execution of the component and its rules(if any).
    private protected ValidationComponentExecutionLog? _ongoingComponentLog;

    // the flow impact of the ongoing rule, this is used to determine if the rule execution should be stopped or continued in the ongoing component(PropertyValidator).
    private protected FlowEffect _ongoingRuleFlowEffect;

    // the number of failures that occurred in the ongoing component(PropertyValidator), this is used to determine if the component is considered failed.
    // for components that are not PropertyValidator, this is set to 0 by default, and should be set manually when the component is failed.
    private protected int _ongoingComponentFailures;
    private bool _disposedValue;

    private protected ValidationCtx(
        string fromValidator,
        CultureInfo culture,
        ValidationCtx? parentContext,
        ValidatorRunResult result,
        ImmutableArray<string>? availableSnapShots)
    {
        ForValidator = fromValidator;
        Culture = culture;
        ParentContext = parentContext;
        Result = result;

        _totalFailures = 0;
        _ongoingComponentFailures = 0;

        _ongoingComponentFlowEffect = FlowEffect.Proceed;

        _ongoingRuleFlowEffect = FlowEffect.Proceed;

        _ongoingComponentWillSkip = 0;

        if (availableSnapShots.HasValue)
        {
            _availableSnapShots = new (string, bool)[availableSnapShots.Value.Length];

            for (int i = 0; i < availableSnapShots.Value.Length; i++)
            {
                _availableSnapShots[i] = (availableSnapShots.Value[i], false);
            }
        }
    }

    // the shared Result(by all validators present in the main validator) object that contains the logs and the validation status of the run.
    internal ValidatorRunResult Result { get; }

    // the CultureInfo that is used for localization of the validation messages, this is set by the validator or global defaults.
    public CultureInfo Culture { get; }

    // if the ongoing Validator is a child validator, this will be set to the parent context.
    // used to share the Result object,culture, and property paths and names.
    internal ValidationCtx? ParentContext { get; }

    // the extracted property name from the ongoing component(PropertyValidator),
    public abstract string PropertyName { get; }

    // the correct property path that is used to identify the property in the validation logs and results.
    internal abstract string CorrectPropertyPath { get; }

    // flag that indicates if the run is valid or not, this is determined by the total number of failures that occurred during the validation run.
    internal bool IsRunValid => _totalFailures == 0;

    internal void AttachResultToSnapShot(string snapShot)
    {
        // we are sure the index exists, because we are checking on creation time if the snapshot exists in the validator.
        int index = Array.FindIndex(_availableSnapShots!, s => s.identifaer == snapShot);

        // we are safe to set the isValid value, because we are sure this cant happen more than once, because we guard against this in the validator creation.
        _availableSnapShots![index].isValid = _ongoingComponentFailures == 0;
    }

    public bool IsSnapShotValid(string snapShot)
    {
        // we are sure the index exists, because we are checking on creation time if the snapshot exists in the validator.
        int index = Array.FindIndex(_availableSnapShots!, s => s.identifaer == snapShot);

        // we are safe to get the isValid value, because we are sure the snapshot exists in the validator, we guard against this in the validator creation.
        return _availableSnapShots![index].isValid;
    }

    internal bool ToAttachHintToFailures { get; set; }


    // this method is used to determine if the execution of a validation rule in the ongoing component(PropertyValidator) should be stopped.
    internal bool ToStopPropertyValidator() => _ongoingComponentLog is null
            ? throw new ValidationAssistantInternalException("Debug how ToStopPropertyValidator is call with null componentLog")
            : _ongoingRuleFlowEffect == FlowEffect.Stop;

    internal void RuleSkipped(IRuleOptions core)
    {
        if (_ongoingComponentLog is null)
        {
            throw new ValidationAssistantInternalException("Debug how AddRuleFailure is call with null componentLog(PropertyValidator log)");
        }

        _ongoingComponentLog.AttachToLog(core, ExecutionStatus.Skipped, null);
    }

    internal void RuleFailed(IRuleOptions core, string message)
    {
        if (_ongoingComponentLog is null)
        {
            throw new ValidationAssistantInternalException("Debug how AddRuleFailure is call with null componentLog(PropertyValidator log)");
        }

        _ongoingRuleFlowEffect = core.FailureLocalFlowImpact;
        _ongoingComponentLog.Explanation = "One or more rules failed.";

        if (core.IsValidationRule)
        {
            _ongoingComponentFailures++;
            _totalFailures++;
            _ongoingComponentLog.Status = ExecutionStatus.Failed;
        }

        _ongoingComponentLog.AttachToLog(core, ExecutionStatus.Failed, message);
    }

    internal void RulePassed(IRuleOptions core)
    {
        if (_ongoingComponentLog is null)
        {
            throw new ValidationAssistantInternalException("Debug how AddRuleFailure is call with null componentLog(PropertyValidator log)");
        }

        _ongoingComponentLog.AttachToLog(core, ExecutionStatus.Passed, null);
    }

    // this method is used to determine if the run should be stopped(the component flow impact to be cascaded to the run),
    // if there is a failure in the ongoing component.
    // TODO: _ongoingComponentFailures are set by default when a component is PropertyValidator,
    // for logical or different single rules i have to set it manually,
    // or come up with a better way to handle this.
    internal bool ToStopRun() => _ongoingComponentLog is null
            ? throw new ValidationAssistantInternalException("Debug how ToStopRun is call with null componentLog")
            : _ongoingComponentFailures > 0 && _ongoingComponentFlowEffect == FlowEffect.Stop;

    internal static CustomValidatorRunCtx<T, TExternalResources> ForNewRunAsMainValidator<T, TExternalResources>(
        CustomValidatorCore<T, TExternalResources> core,
         T value,
        TExternalResources resources,
        CultureInfo? culture)
    {
        return new CustomValidatorRunCtx<T, TExternalResources>(
            core.ValidatorName,
            value,
            resources,
            core.SnapShots,
            culture ?? ValidationAssistantConfig.GlobalDefaults.DefaultCulture,
            null,
            null);
    }

    internal static CustomValidatorRunCtx<T, TExternalResources> ForNewRunAsChildValidator<T, TExternalResources>(
        CustomValidatorCore<T, TExternalResources> core,
        T value,
        TExternalResources resources,
        ValidationCtx parentCtx)
    {
        return new CustomValidatorRunCtx<T, TExternalResources>(
            core.ValidatorName,
            value,
            resources,
            core.SnapShots,
            parentCtx.Culture,
            parentCtx.Result,
            parentCtx);
    }

    public void RegisterForCleanup(IDisposable cleanup)
    {
        throw new NotImplementedException();
    }

    public void RegisterForCleanup(Action cleanup)
    {
        throw new NotImplementedException();
    }

    public void RegisterForCleanupAsync(IAsyncDisposable cleanup)
    {
        throw new NotImplementedException();
    }

    public void RegisterForCleanupAsync(AsyncAction cleanup)
    {
        throw new NotImplementedException();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~ValidationCtx()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

public abstract class ValidationCtx<T, TResources> :
    ValidationCtx,
    IMessageCtx<T, TResources>,
    IConditionCtx<T, TResources>
{
    private protected ValidationCtx(
        ImmutableArray<string>? availableSnapShots,
        T value,
        TResources resources,
        string fromValidator,
        CultureInfo culture,
        ValidationCtx? parentContext,
        ValidatorRunResult result)
        : base(fromValidator, culture, parentContext, result, availableSnapShots)
    {
        Subject = value!;
        Resources = resources;
    }

    // the value of the object being validated, this is the main object that is being validated by the validator
    // NOTE: i need a better name for this property, because it is not a context, but rather the value of the object being validated.
    public T Subject { get; }

    // the resources that are injected into the validator, these are the external resources that can be used by the validation rules.
    public TResources Resources { get; }


    internal int CalculateNextIndex(int currentIndex) => currentIndex + 1 + _ongoingComponentWillSkip;



    internal void AddRuleSkipped<TProperty>(IValidationLeafOptions<T, TResources, TProperty> options)
    {
        if (_ongoingComponentLog is null)
        {
            throw new ValidationAssistantInternalException("Debug how AddRuleSkipped is call with null componentLog(PropertyValidator log)");
        }

        _ongoingComponentLog.AttachToLog(new(options.Code, ExecutionStatus.Informational, options.DeclarationLine, "Rule is skipped from emended condition failure.", null));
    }

    public IMessageResolver GetTemplate(string templateName)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
