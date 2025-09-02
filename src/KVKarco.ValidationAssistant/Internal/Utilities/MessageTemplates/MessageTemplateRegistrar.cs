using KVKarco.ValidationAssistant.Abstractions.MessageTemplate;
using System.Globalization;

namespace KVKarco.ValidationAssistant.Internal.Utilities.MessageTemplates;

internal sealed class MessageTemplateRegistrar : IMessageTemplateRegistrar
{
    public void Register(string templateName, CultureInfo culture, string messageTemplate, bool overwrite = true)
    {
        Ensure.TemplateCanBeRegistered(templateName, culture, messageTemplate);

        InternalCache.RegisterTemplate(templateName, culture, messageTemplate, overwrite);
    }

    public void RegisterFrom(IMessageTemplateSource source)
    {
        Ensure.NotNull(source, InternalDefaults.TemplateSourceNull);

        source.Register(this);
    }
}
