namespace Shared
{
    public interface ISubscription<TKey, TValue>
    {
        string Topic { get; }

        event MessageReceived<TKey, TValue> MessageReceived;
    }
}