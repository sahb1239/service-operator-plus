namespace Shared
{
    public class KafkaSubscriptionSourceFactory<TKey, TValue> : IKafkaSubscriptionSourceFactory<TKey, TValue>
    {
        public IKafkaSubscriptionSource<TKey, TValue> CreateSubscriptionSource(string topic)
        {
            return new KafkaSubscriptionSource<TKey, TValue>(topic);
        }
    }
}