using KVKarco.ValidationAssistant.Internal.Utilities.MessageTemplates;
using KVKarco.ValidationAssistant.Internal.Utilities.TargetAssets;
using Microsoft.Extensions.ObjectPool;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq.Expressions;

namespace KVKarco.ValidationAssistant.Internal.Utilities;

internal static class InternalCache
{
    private static DefaultObjectPool<MessageFormatter> _formattersPool = new(new MessageFormatterPolicy());
    private static readonly ConcurrentDictionary<TemplateKey, CompiledTemplate> _templates = new(new TemplateKeyEqualityComparer());
    private static readonly ConcurrentDictionary<string, TargetCtx> _membersCtxCache = [];
    //private static readonly ConcurrentDictionary<Type, Lazy<ValidatorCore>> _coresCache = [];


    public static MessageFormatter RentFormatter() => _formattersPool.Get();

    public static void ReturnFormatter(MessageFormatter formatter) => _formattersPool.Return(formatter);


    public static void RegisterTemplate(string templateName, CultureInfo culture, string messageTemplate, bool overwrite)
    {
        TemplateKey key = new(templateName, culture);

        if (overwrite || !_templates.ContainsKey(key))
        {
            _templates[key] = CompiledTemplate.Parse(templateName, culture, messageTemplate);
        }
    }

    public static CompiledTemplate? ResolveTemplate(string name, CultureInfo culture)
    {
        // 1) Exact culture
        if (_templates.TryGetValue(new(name, culture), out CompiledTemplate? value))
            return value;

        // 2) Parent culture (e.g., "fr-CA" -> "fr")
        var parent = culture.Parent;
        if (_templates.TryGetValue(new(name, culture.Parent), out var parentT))
            return parentT;

        // 3) Invariant culture
        if (_templates.TryGetValue(new(name, CultureInfo.InvariantCulture), out var inv))
            return inv;

        return null;
    }

    public static void ClearTemplates()
    {
        _templates.Clear();
    }


    public static TargetCtx<TSubject, TTarget> GetOrExtractPropertyCtx<TSubject, TTarget>(
        Expression<Func<TSubject, TTarget>> memberSelector,
        bool removeStartName = false,
        bool isForCollection = false)
    {
        // Normalize the expression to a string key for caching.
        // If the parameter is already named "x", use its ToString() directly.
        // Otherwise, rewrite it to use "x" as the parameter name for consistent hashing.
        string propSelectorDefinition = memberSelector.Parameters[0].Name == "x"
            ? memberSelector.ToString()
            : memberSelector.Rewrite().ToString();

        // Get or add the PropertyCtx to the cache.
        // The factory function receives the key and the original expression as state.
        TargetCtx context = _membersCtxCache.GetOrAdd(
            propSelectorDefinition,
            (key, expression) => ExpressionFactory.CreateCtx(expression, removeStartName, isForCollection),
            memberSelector); // Pass the original memberSelector as state

        // Cast the non-generic PropertyCtx to its specific generic type.
        // This cast is safe because the factory method creates the correct generic type.
        return (TargetCtx<TSubject, TTarget>)context;
    }

    //TODO: Implement TryGetCore(for validators constructor use) and TryAddCore(from all ready build core in validate methods)
}
