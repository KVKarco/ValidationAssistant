using System.Globalization;

namespace KVKarco.ValidationAssistant.Internal.Utilities.MessageTemplates;

internal readonly record struct TemplateKey
{
    public TemplateKey(string name, CultureInfo culture)
    {
        Name = name;
        Culture = culture;
    }

    public readonly string Name { get; }
    public readonly CultureInfo Culture { get; }
}