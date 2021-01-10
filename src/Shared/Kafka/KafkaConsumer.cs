using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Confluent.Kafka;

namespace Shared
{
    public class KafkaConsumer<TKey, TValue> : IConsumer<TKey, TValue>
    {
        private readonly Confluent.Kafka.IConsumer<TKey, TValue> _kafkaConsumer;
        private readonly IDictionary<string, KafkaSubscription> _topicsSubscribed = new Dictionary<string, KafkaSubscription>();
        private CancellationTokenSource _cancellationTokenSource;

        public KafkaConsumer(Confluent.Kafka.IConsumer<TKey, TValue> kafkaConsumer)
        {
            this._kafkaConsumer = kafkaConsumer;
        }

        public ISubscription<TKey, TValue> Subscribe(string topic)
        {
            var subscription = new KafkaSubscription(topic, this);
            AddToTopicsSubscribed(topic, subscription);
            return subscription;
        }

        private void Unsubscribe(KafkaSubscription subscription)
        {
            RemoveFromTopicsSubscribed(subscription);
        }

        private void AddToTopicsSubscribed(string topic, KafkaSubscription subscription)
        {
            _topicsSubscribed[topic] = subscription;
            UpdateSubscription();
        }

        private void RemoveFromTopicsSubscribed(KafkaSubscription subscription)
        {
            _topicsSubscribed.Remove(subscription.Topic);
            UpdateSubscription();
        }

        private void UpdateSubscription()
        {
            if (_topicsSubscribed.Any())
            {
                _kafkaConsumer.Subscribe(_topicsSubscribed.Keys);

                if (_cancellationTokenSource == null)
                {
                    _cancellationTokenSource = new CancellationTokenSource();
                    new Thread(() => StartConsumerLoop(_cancellationTokenSource.Token)).Start();
                }
            }
            else
            {
                _kafkaConsumer.Unsubscribe();
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = null;
            }
        }

        private void StartConsumerLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var cr = this._kafkaConsumer.Consume(token);

                    HandleMessage(cr);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (ConsumeException e)
                {
                    // Consumer errors should generally be ignored (or logged) unless fatal.
                    Console.WriteLine($"Consume error: {e.Error.Reason}");

                    if (e.Error.IsFatal)
                    {
                        // https://github.com/edenhill/librdkafka/blob/master/INTRODUCTION.md#fatal-consumer-errors
                        break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Unexpected error: {e}");
                    break;
                }
            }
        }

        private void HandleMessage(ConsumeResult<TKey, TValue> cr)
        {
            var subscription = _topicsSubscribed[cr.Topic];
            subscription.OnMessageReceived(cr.Message.Key, cr.Message.Value);
        }

        private class KafkaSubscription : ISubscription<TKey, TValue>
        {
            private KafkaConsumer<TKey, TValue> consumer;

            public KafkaSubscription(string topic, KafkaConsumer<TKey, TValue> consumer)
            {
                Topic = topic ?? throw new ArgumentNullException(nameof(topic));
                this.consumer = consumer ?? throw new ArgumentNullException(nameof(consumer));
            }

            public string Topic { get; }

            public event MessageReceived<TKey, TValue> MessageReceived;
            
            public void Unsubscribe()
            {
                consumer.Unsubscribe(this);
            }

            public void OnMessageReceived(TKey key, TValue value)
            {
                MessageReceived?.Invoke(key, value);
            }
        }
    }
}
