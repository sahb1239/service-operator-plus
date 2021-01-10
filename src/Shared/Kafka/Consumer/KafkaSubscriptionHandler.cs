using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace Shared
{
    internal class KafkaSubscriptionHandler<TKey, TValue> : IKafkaSubscriptionHandler<TKey, TValue>
    {
        public event EventHandler SubscriptionChanged;
        public IEnumerable<string> SubscribedTopics => _subscriptions.Keys;

        private readonly IKafkaSubscriptionSourceFactory<TKey, TValue> _factory;
        private readonly ILogger<KafkaSubscriptionHandler<TKey, TValue>> _logger;
        private readonly ConcurrentDictionary<string, IKafkaSubscriptionSource<TKey, TValue>> _subscriptions = new ConcurrentDictionary<string, IKafkaSubscriptionSource<TKey, TValue>>();

        public KafkaSubscriptionHandler(IKafkaSubscriptionSourceFactory<TKey, TValue> factory, ILogger<KafkaSubscriptionHandler<TKey, TValue>> logger)
        {
            this._factory = factory ?? throw new ArgumentNullException(nameof(factory));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public ISubscription<TKey, TValue> Subscribe(string topic)
        {
            var subscription = _factory.CreateSubscriptionSource(topic);
            AddSubscriptionToList(subscription);
            OnSubscriptionChanged();
            return subscription.Subscription;
        }

        public void Unsubscribe(ISubscription<TKey, TValue> subscription)
        {
            RemoveSubscriptionFromList(subscription);
            OnSubscriptionChanged();
        }

        public void HandleConsumeResult(ConsumeResult<TKey, TValue> result)
        {
            var subscriptions = GetSubscriptionSources(result.Topic);
            foreach (var kafkaSubscriptionSource in subscriptions)
            {
                kafkaSubscriptionSource.HandleMessage(result);
            }
        }

        private void AddSubscriptionToList(IKafkaSubscriptionSource<TKey, TValue> subscription)
        {
            if (!_subscriptions.TryAdd(subscription.Topic, subscription))
            {
                throw new NotSupportedException("Multiple subscriptions on same topic not supported");
            }
        }

        private IKafkaSubscriptionSource<TKey, TValue>[] GetSubscriptionSources(string topic)
        {
            if (_subscriptions.TryGetValue(topic, out var output))
            {
                return new[]
                {
                    output
                };
            }
            else
            {
                _logger.LogWarning("No subscription sources found for topic {0}", topic);
                return new IKafkaSubscriptionSource<TKey, TValue>[] { };
            }
        }

        private void RemoveSubscriptionFromList(ISubscription<TKey, TValue> subscription)
        {
            _subscriptions.TryRemove(subscription.Topic, out _);
        }

        protected virtual void OnSubscriptionChanged()
        {
            SubscriptionChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}