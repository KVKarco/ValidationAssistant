using Microsoft.Extensions.ObjectPool;

namespace KVKarco.ValidationAssistant.Internal.Utilities.MessageTemplates;

internal class MessageFormatterPolicy : IPooledObjectPolicy<MessageFormatter>
{
    public MessageFormatterPolicy()
    {
    }

    // Creates a new instance of MessageFormatter.
    public MessageFormatter Create()
    {
        return new MessageFormatter();
    }

    // Called when an object is returned to the pool.
    // Use this to reset the object's state.
    public bool Return(MessageFormatter formatter)
    {
        formatter.Clear();
        return true; // Return true to indicate the object can be pooled.
    }
}
