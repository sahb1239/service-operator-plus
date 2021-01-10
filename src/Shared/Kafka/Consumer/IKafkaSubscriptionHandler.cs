using System;
using System.Collections.Generic;
using Confluent.Kafka;

namespace Shared
{
    public interface IKafkaSubscriptionHandler<TKey, TValue>
    {
        event EventHandler SubscriptionChanged;
        IEnumerable<string> SubscribedTopics { get; }
        ISubscription<TKey, TValue> Subscribe(string topic);
        void Unsubscribe(ISubscription<TKey, TValue> subscription);
        void HandleConsumeResult(ConsumeResult<TKey, TValue> result);
    }
}