using KVKarco.ValidationAssistant.Abstractions;
using KVKarco.ValidationAssistant.Internal.PropertyValidation;

namespace KVKarco.ValidationAssistant.Internal.PropertyValidation.GenericValidationRules;

internal sealed class SnapShotValidationRule<T, TExternalResources, TProperty> :
    ValidationComponent<T, TExternalResources, TProperty>
{
    private readonly string _snapShotIdentifier;

    private readonly bool _isCapturingResult;

    public SnapShotValidationRule(string snapShotIdentifier, bool isCapturingResult)
        : base(true)
    {
        _snapShotIdentifier = snapShotIdentifier;
        _isCapturingResult = isCapturingResult;
    }

    public sealed override ReadOnlySpan<char> RuleName => DefaultCodes.SnapShotValRule;

    public sealed override string GetDefaultFailureMessage(IMessageCtx<T, TExternalResources> context, TProperty value)
        => ValidatorsConfig.GlobalDefaults.Messages.InvalidSnapShotComponentExplanation(context, value, _snapShotIdentifier);

    public bool IsValid(ValidatorRunCtx<T, TExternalResources> context, TProperty value)
    {
        if (_isCapturingResult)
        {
            // When capturing, the rule itself always "succeeds" in its task
            // The validity of the snapshot is determined by the current state of property rule failures.
            context.CalculateAndAddSnapShotResult(_snapShotIdentifier);
            return true;
        }

        // When not capturing, the rule's validity depends on the snapshot's validity
        // Assuming IsSnapShotValidInternal is an internal helper for IsSnapShotValid or a similar check.
        return context.IsSnapShotValidInternal(_snapShotIdentifier);
    }

    internal sealed override void Validate(
        ValidatorRunCtx<T, TExternalResources> context,
        Undefined<TProperty> property,
        ValidationRuleFailureInfo<T, TExternalResources, TProperty> failureInfo)
    {
        if (_isCapturingResult)
        {
            context.CalculateAndAddSnapShotResult(_snapShotIdentifier);
        }
        else if (!context.IsSnapShotValidInternal(_snapShotIdentifier))
        {
            context.AddValidationRuleFailure(property, failureInfo);
        }
    }

    internal sealed override ValueTask ValidateAsync(
        ValidatorRunCtx<T, TExternalResources> context,
        Undefined<TProperty> property,
        ValidationRuleFailureInfo<T, TExternalResources, TProperty> failureInfo,
        CancellationToken ct)
    {
        Validate(context, property, failureInfo);
        return ValueTask.CompletedTask;
    }
}

internal class SomeContext
{
    private List<string> _errors = new List<string>();
    public IReadOnlyList<string> Errors => _errors;

    public void AddError(string error)
    {
        _errors.Add(error);
    }

    public bool HasErrors => _errors.Count > 0;
}

internal class Child
{
    public Child(Func<object, bool> isValid)
    {
        IsValid = isValid;
        CanRunSynchronously = true;
    }

    public Child(Func<object, Task<bool>> isValidAsync)
    {
        IsValidAsync = isValidAsync;
        CanRunSynchronously = false;
    }

    public Func<object, bool>? IsValid { get; }
    public Func<object, Task<bool>>? IsValidAsync { get; }

    public bool CanRunSynchronously { get; }

    internal void Validate(SomeContext context, object value)
    {
        if (CanRunSynchronously)
        {
            if (!IsValid!(value))
            {
                context.AddError("Child synchronous validation failed.");
            }
        }
        else
        {
            throw new InvalidOperationException("Cannot call synchronous Validate when only an async delegate is provided.");
        }
    }

    internal ValueTask ValidateAsync(SomeContext context, object value, CancellationToken ct)
    {
        if (CanRunSynchronously)
        {
            if (!IsValid!(value))
            {
                context.AddError("Child asynchronous validation failed (sync delegate).");
            }
            return ValueTask.CompletedTask;
        }

        return ValidateAsyncWithDelegate(context, value);

        async ValueTask ValidateAsyncWithDelegate(SomeContext ctx, object val)
        {
            // We await the result of IsValidAsync.
            if (!await IsValidAsync!(val).ConfigureAwait(false))
            {
                ctx.AddError("Child asynchronous validation failed.");
            }
        }
    }
}

internal class Parent
{
    private readonly List<Child> _components;

    public Parent(List<Child> components)
    {
        _components = components;
        CanRunSynchronously = _components.All(c => c.CanRunSynchronously);
    }

    public bool CanRunSynchronously { get; }

    internal void Validate(SomeContext context, object value)
    {
        //or we live it to the child to throw an exception for better exception message
        //if (!CanRunSynchronously)
        //{
        //    throw new InvalidOperationException("Cannot call synchronous Validate if any child component is asynchronous.");
        //}

        foreach (var component in _components)
        {
            component.Validate(context, value);
        }
    }

    internal ValueTask ValidateAsync(SomeContext context, object value, CancellationToken ct)
    {
        if (CanRunSynchronously)
        {
            foreach (var component in _components)
            {
                component.Validate(context, value);
            }

            return ValueTask.CompletedTask;
        }

        return ValidateAsyncComponents(context, value, ct);


        async ValueTask ValidateAsyncComponents(SomeContext ctx, object val, CancellationToken token)
        {
            for (int i = 0; i < _components.Count; i++)
            {
                var component = _components[i];
                token.ThrowIfCancellationRequested();

                if (component.CanRunSynchronously)
                {
                    component.Validate(ctx, val);
                }
                else
                {
                    await component.ValidateAsync(ctx, val, token).ConfigureAwait(false);
                }
            }
        }
    }
}

public class GrandParent
{
    private readonly List<Parent> _components;

    internal GrandParent(List<Parent> components)
    {
        _components = components;
        CanRunSynchronously = _components.All(c => c.CanRunSynchronously);
    }

    public bool CanRunSynchronously { get; }

    public bool Validate(object value)
    {
        SomeContext context = new SomeContext();

        foreach (var component in _components)
        {
            component.Validate(context, value);
        }

        return !context.HasErrors;
    }

    public Task<bool> ValidateAsync(object value, CancellationToken ct = default)
    {
        SomeContext context = new SomeContext();

        if (CanRunSynchronously)
        {
            foreach (var component in _components)
            {
                component.Validate(context, value);
                // No need to check ct here?
            }
            return Task.FromResult(!context.HasErrors);
        }

        return ValidateAsyncInternal();

        async Task<bool> ValidateAsyncInternal()
        {
            for (int i = 0; i < _components.Count; i++)
            {
                var component = _components[i];
                ct.ThrowIfCancellationRequested();

                if (component.CanRunSynchronously)
                {
                    component.Validate(context, value);
                }
                else
                {
                    await component.ValidateAsync(context, value, ct).ConfigureAwait(false);
                }
            }

            return !context.HasErrors;
        }
    }
}

// and usage example:

// var grandParent = someCache.getGrandParent();
// bool isValidSync = grandParent.Validate(new object()); // Synchronous validation
// bool isValidAsync = await grandParent.ValidateAsync(new object()); // Asynchronous validation