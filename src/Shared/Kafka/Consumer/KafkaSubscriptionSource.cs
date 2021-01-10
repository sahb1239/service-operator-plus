using Confluent.Kafka;

namespace Shared
{
    public class KafkaSubscriptionSource<TKey, TValue> : IKafkaSubscriptionSource<TKey, TValue>
    {
        private readonly KafkaSubscription<TKey, TValue> _subscription;

        public KafkaSubscriptionSource(string topic)
        {
            Topic = topic;
            _subscription = new KafkaSubscription<TKey, TValue>(this);
        }

        public string Topic { get; }

        public ISubscription<TKey, TValue> Subscription => _subscription;

        public void HandleMessage(ConsumeResult<TKey, TValue> result)
        {
            _subscription.HandleMessage(result.Message.Key, result.Message.Value);
        }
    }
}