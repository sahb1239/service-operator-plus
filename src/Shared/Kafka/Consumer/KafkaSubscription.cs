using System;

namespace Shared
{
    public class KafkaSubscription<TKey, TValue> : ISubscription<TKey, TValue>
    {
        private readonly IKafkaSubscriptionSource<TKey, TValue> _source;

        public KafkaSubscription(IKafkaSubscriptionSource<TKey, TValue> source)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
        }

        public string Topic => _source.Topic;

        public event MessageReceived<TKey, TValue> MessageReceived;

        public void HandleMessage(TKey key, TValue value)
        {
            OnMessageReceived(key, value);
        }

        protected virtual void OnMessageReceived(TKey key, TValue value)
        {
            MessageReceived?.Invoke(key, value);
        }
    }
}