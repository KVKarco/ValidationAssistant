using KVKarco.ValidationAssistant.Abstractions;
using KVKarco.ValidationAssistant.Exceptions;
using KVKarco.ValidationAssistant.Internal.CustomValidatorAssets;
using System.Collections.Immutable;
using System.Globalization;

namespace KVKarco.ValidationAssistant.Internal;

internal abstract class ValidatorRunCtx
{
    protected readonly string _validatorName;

    private readonly (string identifaer, bool? isValid)[]? _availableSnapShots;

    protected int _totalFailures;

    protected FlowEffect? _onValidatorFlowImpact;

    protected int _componentsToSkip;

    protected ValidatorComponentLog? _executionLog;

    protected FlowEffect? _onPropertyValidatorFlowImpact;

    protected int _componentFailures;

    protected ValidatorRunCtx(
        string fromValidator,
        CultureInfo culture,
        ValidatorRunCtx? parentContext,
        ValidatorRunResult result,
        ImmutableArray<string>? availableSnapShots)
    {
        _validatorName = fromValidator;
        Culture = culture;
        ParentContext = parentContext;
        Result = result;

        _totalFailures = 0;
        _componentFailures = 0;

        _onValidatorFlowImpact = FlowEffect.Proceed;

        _onPropertyValidatorFlowImpact = FlowEffect.Proceed;

        _componentsToSkip = 0;

        if (availableSnapShots.HasValue)
        {
            _availableSnapShots = new (string, bool?)[availableSnapShots.Value.Length];

            for (int i = 0; i < availableSnapShots.Value.Length; i++)
            {
                _availableSnapShots[i] = (availableSnapShots.Value[i], null);
            }
        }
    }

    public ValidatorRunResult Result { get; }

    public CultureInfo Culture { get; }

    public ValidatorRunCtx? ParentContext { get; }

    public abstract ReadOnlySpan<char> PropertyName { get; }

    public abstract string CorrectPropertyPath { get; }

    public bool IsRunValid => _totalFailures == 0;

    public void AttachResultToSnapShot(string snapShot)
    {
        if (_availableSnapShots is null)
        {

            throw new ValidationRunException($"Validator: {_validatorName} does not contain any snapshots.");
        }

        int index = Array.FindIndex(_availableSnapShots, s => s.identifaer == snapShot);

        if (index < 0)
        {
            throw new ValidationRunException($"Snapshot: {snapShot} does not exist in validator: {_validatorName}.");
        }

        var (identifier, isValid) = _availableSnapShots[index];

        if (isValid.HasValue)
        {
            throw new ValidationRunException($"Snap shot: {snapShot} value is already set.");
        }

        _availableSnapShots[index].isValid = _componentFailures == 0;
    }

    public bool IsSnapShotValid(string snapShot)
    {
        if (_availableSnapShots is null)
        {
            throw new ValidationRunException($"Validator: {_validatorName} does not contain any snapshots.");
        }

        int index = Array.FindIndex(_availableSnapShots, s => s.identifaer == snapShot);

        if (index < 0)
        {
            throw new ValidationRunException($"Snapshot: {snapShot} does not exist in validator: {_validatorName}.");
        }

        bool? isValid = _availableSnapShots[index].isValid;

        if (isValid.HasValue)
        {
            return isValid.Value;
        }

        throw new ValidationRunException($"Snapshot: {snapShot} value is not set.");
    }

    public static CustomValidatorRunCtx<T, TExternalResources> ForNewRunAsMainValidator<T, TExternalResources>(
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
            culture ?? ValidatorsConfig.GlobalDefaults.DefaultCulture,
            null,
            null);
    }

    public static CustomValidatorRunCtx<T, TExternalResources> ForNewRunAsChildValidator<T, TExternalResources>(
        CustomValidatorCore<T, TExternalResources> core,
        T value,
        TExternalResources resources,
        ValidatorRunCtx parentCtx)
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
}

internal abstract class ValidatorRunCtx<T, TExternalResources> :
    ValidatorRunCtx,
    IConditionCtx<T, TExternalResources>,
    IMessageCtx<T, TExternalResources>
{
    protected ValidatorRunCtx(
        ImmutableArray<string>? availableSnapShots,
        T value,
        TExternalResources resources,
        string fromValidator,
        CultureInfo culture,
        ValidatorRunCtx? parentContext,
        ValidatorRunResult result)
        : base(fromValidator, culture, parentContext, result, availableSnapShots)
    {
        Value = value!;
        Resources = resources;
    }

    public T Value { get; }

    public TExternalResources Resources { get; }

    public bool ToStopPropertyValidator() => _executionLog is null
            ? throw new ValidationAssistantInternalException("Debug how ToStopPropertyValidator is call with null componentLog")
            : _onPropertyValidatorFlowImpact.HasValue && _onPropertyValidatorFlowImpact == FlowEffect.Stop;


    public bool ToStopRun() => _executionLog is null
            ? throw new ValidationAssistantInternalException("Debug how ToStopRun is call with null componentLog")
            : _onValidatorFlowImpact.HasValue && _onValidatorFlowImpact == FlowEffect.Stop;

    public int CalculateNextIndex(int currentIndex) => currentIndex + 1 + _componentsToSkip;
}
