using System.Diagnostics.CodeAnalysis;

namespace KVKarco.ValidationAssistant.Abstractions.MessageTemplate;

/// <summary>
/// Implement this in consumer code to supply/override templates.
/// Call <see cref="FailureMessages.RegisterFrom"/> at app start.
/// </summary>
public interface IMessageTemplateSource
{
    void Register([NotNull] IMessageTemplateRegistrar registrar);
}
