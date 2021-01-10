namespace Shared
{
    public interface IConsumer<TKey, TValue>
    {
        ISubscription<TKey, TValue> Subscribe(string topic);
        void Unsubscribe(ISubscription<TKey, TValue> subscription);
    }
}
