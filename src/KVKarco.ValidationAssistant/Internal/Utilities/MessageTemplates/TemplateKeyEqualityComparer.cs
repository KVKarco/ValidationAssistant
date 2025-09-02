using System.Diagnostics.CodeAnalysis;

namespace KVKarco.ValidationAssistant.Internal.Utilities.MessageTemplates;

internal sealed class TemplateKeyEqualityComparer : IEqualityComparer<TemplateKey>
{
    public bool Equals(TemplateKey x, TemplateKey y)
    {
        return x.Name == y.Name && x.Culture == y.Culture;
    }

    public int GetHashCode([DisallowNull] TemplateKey obj)
    {
        return HashCode.Combine(obj.Name, obj.Culture);
    }
}
