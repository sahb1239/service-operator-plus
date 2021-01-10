namespace Shared
{
    public interface IKafkaSubscriptionSourceFactory<TKey, TValue>
    {
        IKafkaSubscriptionSource<TKey, TValue> CreateSubscriptionSource(string topic);
    }
}