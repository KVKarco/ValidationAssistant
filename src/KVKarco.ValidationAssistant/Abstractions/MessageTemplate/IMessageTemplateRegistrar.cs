using System.Globalization;

namespace KVKarco.ValidationAssistant.Abstractions.MessageTemplate;

/// <summary>
/// Provided to <see cref="IMessageTemplateSource"/> to register templates.
/// </summary>
public interface IMessageTemplateRegistrar
{
    /// <summary>
    /// Register one template. If overwrite=false, existing entries are preserved.
    /// </summary>
    void Register(string templateName, CultureInfo culture, string messageTemplate, bool overwrite = true);

    /// <summary>
    /// Register from user defined source.
    /// </summary>
    /// <param name="source"></param>
    void RegisterFrom(IMessageTemplateSource source);
}
