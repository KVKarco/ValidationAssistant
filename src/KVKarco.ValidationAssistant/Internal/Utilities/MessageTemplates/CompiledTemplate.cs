using KVKarco.ValidationAssistant.Exceptions;
using System.Collections.Immutable;
using System.Globalization;

namespace KVKarco.ValidationAssistant.Internal.Utilities.MessageTemplates;

/// <summary>
/// A compiled template consisting of alternating literal and placeholder segments.
/// Supports escaped braces: use '{{' and '}}' for literal braces.
/// </summary>
internal sealed class CompiledTemplate
{
    private CompiledTemplate(string name, CultureInfo culture, List<MessageSegment> segments)
    {
        Name = name;
        Segments = [.. segments];
        PlaceholdersAvailable = Segments.Count(x => x.Placeholder is not null);
        Culture = culture;
    }

    public string Name { get; }

    public int PlaceholdersAvailable { get; }

    // A segment is either a literal (Text != null) or a placeholder (Placeholder != null)
    public ImmutableArray<MessageSegment> Segments { get; }

    public CultureInfo Culture { get; }

    public bool IsPlaceHolderPresented(string placeholder)
    {
        for (int i = 0; i < Segments.Length; i++)
        {
            if (Segments[i].Placeholder == placeholder)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Parse raw template text into segments. Example:
    /// "'{TargetName}' must be greater than '{ComparisonValue}'."
    /// =>
    /// [Literal("'"), Placeholder(TargetName), Literal("' must be greater than '"),
    ///  Placeholder(ComparisonValue), Literal("'.")]
    /// Supports escaped braces: "{{" -> "{", "}}" -> "}".
    /// </summary>
    public static CompiledTemplate Parse(string templateName, CultureInfo culture, string template)
    {
        var list = new List<MessageSegment>(capacity: Math.Max(4, template.Length / 8));
        int i = 0, start = 0;

        while (i < template.Length)
        {
            char c = template[i];

            if (c == '{')
            {
                // Escaped "{{" -> literal "{"
                if (i + 1 < template.Length && template[i + 1] == '{')
                {
                    i += 2; // consume "{{"
                    continue; // we'll handle escaping when slicing literals below
                }

                // Flush any literal up to '{'
                if (i > start)
                {
                    list.Add(new MessageSegment(template[start..i], isLiteral: true));
                }

                // Find closing '}', considering "}}" escape
                int j = i + 1;
                while (j < template.Length && template[j] != '}')
                {
                    j++;
                }
                if (j >= template.Length)
                {
                    throw new ValidationAssistantException($"Unmatched '{{' at position {i} in template: \"{template}\"");
                }

                // Extract placeholder name between { ... }
                var name = template[(i + 1)..j];
                if (name.Length == 0)
                    throw new ValidationAssistantException("Empty placeholder {} is not allowed.");
                list.Add(new MessageSegment(name, isLiteral: false));

                // Move past '}'
                i = j + 1;
                start = i;
                continue;
            }
            else if (c == '}')
            {
                // Escaped "}}" -> literal "}"
                if (i + 1 < template.Length && template[i + 1] == '}')
                {
                    i += 2; // consume "}}"
                    continue; // handle later in literal
                }

                // A single '}' is invalid
                throw new ValidationAssistantException($"Unmatched '}}' at position {i} in template: \"{template}\"");
            }

            i++;
        }

        // Flush trailing literal
        if (start < template.Length)
        {
            list.Add(new MessageSegment(template[start..], isLiteral: true));
        }

        // Post-process: collapse escape sequences "{{" -> "{", "}}" -> "}" inside literals
        for (int k = 0; k < list.Count; k++)
        {
            if (list[k].Text is string lit)
            {
                if (lit.Contains('{', StringComparison.Ordinal) || lit.Contains('}', StringComparison.Ordinal))
                {
                    // Replace "{{"->"{" and "}}"->"}"
                    lit = lit.Replace("{{", "{", StringComparison.Ordinal).Replace("}}", "}", StringComparison.Ordinal);
                    list[k] = new MessageSegment(lit, isLiteral: true);
                }
            }
        }

        return new CompiledTemplate(templateName, culture, list);
    }
}
