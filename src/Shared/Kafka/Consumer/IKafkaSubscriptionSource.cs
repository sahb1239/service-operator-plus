using Confluent.Kafka;

namespace Shared
{
    public interface IKafkaSubscriptionSource<TKey, TValue>
    {
        string Topic { get; }
        ISubscription<TKey, TValue> Subscription { get; }
        void HandleMessage(ConsumeResult<TKey, TValue> result);
    }
}