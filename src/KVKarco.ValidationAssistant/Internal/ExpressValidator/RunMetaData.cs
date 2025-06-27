using KVKarco.ValidationAssistant.Exceptions;
using KVKarco.ValidationAssistant.Internal;
using System.Globalization;

namespace KVKarco.ValidationAssistant.Internal.ExpressValidator;

public readonly record struct ExpressValidatorRunMetaData
{
    public ExpressValidatorRunMetaData()
    {
        throw new RuleCreationException("Creating empty ExpressValidatorRunMetaData can be created only by ExpressValidators.");
    }

    private ExpressValidatorRunMetaData(CultureInfo culture, ValidatorRunResult? result = null, ValidatorRunCtx? parentContext = null)
    {
        Culture = culture;
        Result = result;
        ParentContext = parentContext;
    }

    internal readonly CultureInfo Culture { get; }

    internal readonly ValidatorRunResult? Result { get; }

    internal readonly ValidatorRunCtx? ParentContext { get; }

    internal static ExpressValidatorRunMetaData ForMainValidator(CultureInfo culture) => new(culture);

    internal static ExpressValidatorRunMetaData ForChildValidator(CultureInfo culture, ValidatorRunCtx parentContext)
        => new(culture, parentContext.Result, parentContext);
}