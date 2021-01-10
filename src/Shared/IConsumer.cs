namespace Shared
{
    public interface IConsumer<TKey, TValue>
    {
        ISubscription<TKey, TValue> Subscribe(string topic);
    }

    public interface ISubscription<TKey, TValue>
    {
        string Topic { get; }

        event MessageReceived<TKey, TValue> MessageReceived;

        void Unsubscribe();
    }

    public delegate void MessageReceived<TKey, TValue>(TKey key, TValue value);
}
