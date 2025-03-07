using System.Reflection;

namespace KVKarco.ValidationAssistant.Common;

internal readonly record struct PropertyKey
{
    public PropertyKey(MemberInfo info, string? pathToProperty) : this()
    {
        MemberInfo = info;
        PathToProperty = pathToProperty;
        FullPath = pathToProperty == null ? MemberInfo.Name : $"{pathToProperty}.{info.Name}";
    }

    public readonly MemberInfo MemberInfo { get; }

    public readonly string? PathToProperty { get; }

    public string FullPath { get; }
}