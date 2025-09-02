namespace KVKarco.ValidationAssistant.Internal.Utilities.MessageTemplates;

internal readonly struct MessageSegment
{
    public MessageSegment(string data, bool isLiteral)
    {
        if (isLiteral)
        {
            Text = data;
            Placeholder = null;
        }
        else
        {
            Text = null;
            Placeholder = data;
        }
    }

    public string? Text { get; }          // literal text if not null
    public string? Placeholder { get; }   // placeholder name if not null
}
