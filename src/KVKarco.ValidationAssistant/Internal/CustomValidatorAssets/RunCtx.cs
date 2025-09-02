using CommunityToolkit.HighPerformance.Buffers;
using KVKarco.ValidationAssistant.Exceptions;
using KVKarco.ValidationAssistant.Internal.Utilities.TargetAssets;
using KVKarco.ValidationAssistant.Results;
using System.Collections.Immutable;
using System.Globalization;

namespace KVKarco.ValidationAssistant.Internal.CustomValidatorAssets;

// only used from Validators created inheriting the CustomValidator base class.
internal sealed class CustomValidatorRunCtx<T, TExternalResources> :
    ValidationCtx<T, TExternalResources>
{
    private TargetKey _propertyKey;

    public CustomValidatorRunCtx(
        string fromValidator,
        T value,
        TExternalResources resources,
        ImmutableArray<string>? availableSnapShots,
        CultureInfo culture,
        ValidatorRunResult? result,
        ValidationCtx? parentContext)
        : base(
            availableSnapShots,
            value,
            resources,
            fromValidator,
            culture,
            parentContext,
            result ?? new(fromValidator))
    {
        _propertyKey = TargetKey.Empty;
    }

    // the property name defined in the object graph, for the ongoing property being validated.
    public override ReadOnlySpan<char> PropertyName => _propertyKey.Info is null ? [] : _propertyKey.PropertyName;

    // the property path defined in the object graph, for the ongoing property being validated.
    //public override string CorrectPropertyPath => ParentContext is not null
    //    ? $"{ParentContext.CorrectPropertyPath}.{_propertyKey.PropertyPath}"
    //    : _propertyKey.PropertyPath;

    private string _currentPropertyPath = string.Empty;

    public override string CorrectPropertyPath
    {
        get
        {
            // this is ok if there is no parent context, i have the path in the _propertyKey i return from cache to not create a new string every time.
            if (ParentContext is null)
            {
                ReadOnlySpan<char> path = _propertyKey.PropertyPath.AsSpan();
                return StringPool.Shared.GetOrAdd(path);
            }

            // now lets check if the path is already cached.
            if (_currentPropertyPath is not null)
            {
                return _currentPropertyPath;
            }

            // now if i have a parent context and the full path to here is not cached, i need to build the full path and cache it here.

            // 2. Recursive step: Get the parent's cached path first./ let the parent handle his cache and return the path.
            ReadOnlySpan<char> parentPropertyPath = ParentContext.CorrectPropertyPath.AsSpan();
            ReadOnlySpan<char> propertyPath = _propertyKey.PropertyPath.AsSpan();

            // 3. Now that we have the parent's cached path, build the full path on the stack.
            int totalLength = parentPropertyPath.Length + 1 + propertyPath.Length;
            Span<char> targetSpan = stackalloc char[totalLength];

            parentPropertyPath.CopyTo(targetSpan);
            targetSpan[parentPropertyPath.Length] = '.';
            propertyPath.CopyTo(targetSpan.Slice(parentPropertyPath.Length + 1));

            // 4. Use StringPool to cache and retrieve the combined path.
            // This is the final, globally cached path string.
            // and the correct path to local cache and return it.
            _currentPropertyPath = StringPool.Shared.GetOrAdd(targetSpan);

            return _currentPropertyPath;
        }
    }


    // sets the context to track the validation of a specific property, from specified component(PropertyValidator).
    public void ForProperty(TargetKey key, int declarationLine, FlowEffect componentFlowImact)
    {
        // the PropertyKey associated with the current property path of the validation object.
        _propertyKey = key;

        // reset the ongoing component state for the new property validation, and set the skip effect to 0 because the component is not skipping components.
        _ongoingComponentFailures = 0;
        _ongoingComponentWillSkip = 0;

        //reset the ongoing rule flow effect to Proceed, and set the ongoing component flow effect to the specified effect,
        //to be use if component is considered as failed.
        _ongoingRuleFlowEffect = FlowEffect.Proceed;
        _ongoingComponentFlowEffect = componentFlowImact;

        // start the log for the current component, and add it to the result.
        _ongoingComponentLog = ValidationComponentExecutionLog.ForProperty(ForValidator, CorrectPropertyPath, declarationLine);
        Result.AddComponentLog(_ongoingComponentLog);
    }

    public bool ToAttachHintOnFailures => true;

    public void ForSchemaGuard(int declarationLine)
    {
        _propertyKey = TargetKey.Empty;

        _ongoingComponentFailures = 0;
        _ongoingComponentWillSkip = 0;
        _ongoingComponentFlowEffect = FlowEffect.Stop;
        _ongoingRuleFlowEffect = FlowEffect.Proceed;
        _ongoingComponentLog = new ValidationComponentExecutionLog(
            ComponentType.SchemaGuard, ForValidator, "", declarationLine, ExecutionStatus.Passed, null);

        Result.AddComponentLog(_ongoingComponentLog);
    }

    public void ForNextSchemaGuardPart(TargetKey key)
    {
        _propertyKey = key;
    }

    public void PathBroken(string hint)
    {
        if (_ongoingComponentLog is null)
        {
            throw new ValidationAssistantInternalException("Debug how PathBroken is call with null componentLog(PropertyValidator log)");
        }

        _ongoingComponentFailures++;
        _totalFailures++;
        _ongoingComponentLog.Status = ExecutionStatus.Failed;

        _ongoingComponentLog.Explanation ??= "The schema is broken,one or more members are not found in to the object graph.";

        _ongoingComponentLog.AttachToLog(
            new ValidationRuleExecutionLog(
                "SchemaGuard",
                ExecutionStatus.Failed,
                _ongoingComponentLog.DeclaredOnLine,
                hint,
                new ValidationRuleFailure("SchemaGuard", Severity.Critical, "Internal error validation failed.", ToAttachHintToFailures ? hint : null)));
    }
}
