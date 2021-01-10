using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace Shared
{
    public class KafkaConsumer<TKey, TValue> : IConsumer<TKey, TValue>
    {
        private readonly Confluent.Kafka.IConsumer<TKey, TValue> _kafkaConsumer;
        private readonly IKafkaSubscriptionHandler<TKey, TValue> _subscriptionHandler;
        private readonly ILogger<KafkaConsumer<TKey, TValue>> _logger;
        private CancellationTokenSource _cancellationTokenSource;
        private object _locker = new object();

        public KafkaConsumer(Confluent.Kafka.IConsumer<TKey, TValue> kafkaConsumer, IKafkaSubscriptionHandler<TKey, TValue> subscriptionHandler, ILogger<KafkaConsumer<TKey, TValue>> logger)
        {
            this._kafkaConsumer = kafkaConsumer ?? throw new ArgumentNullException(nameof(kafkaConsumer));
            this._subscriptionHandler = subscriptionHandler ?? throw new ArgumentNullException(nameof(subscriptionHandler));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));

            this._subscriptionHandler.SubscriptionChanged += OnSubscriptionChanged;
        }

        public ISubscription<TKey, TValue> Subscribe(string topic)
        {
            return this._subscriptionHandler.Subscribe(topic);
        }

        public void Unsubscribe(ISubscription<TKey, TValue> subscription)
        {
            this._subscriptionHandler.Unsubscribe(subscription);
        }

        private void OnSubscriptionChanged(object? sender, EventArgs e)
        {
            lock (_locker)
            {
                if (_subscriptionHandler.SubscribedTopics.Any())
                {
                    _kafkaConsumer.Subscribe(_subscriptionHandler.SubscribedTopics);

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
        }

        private void StartConsumerLoop(CancellationToken token)
        {
            _logger.LogInformation("Started Kafka Consumer loop");

            while (!token.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = this._kafkaConsumer.Consume(token);
                    _subscriptionHandler.HandleConsumeResult(consumeResult);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (ConsumeException e)
                {
                    // Consumer errors should generally be ignored (or logged) unless fatal.
                    if (e.Error.IsFatal)
                    {
                        _logger.LogError("Consume error: {0}", e.Error.Reason);
                        // https://github.com/edenhill/librdkafka/blob/master/INTRODUCTION.md#fatal-consumer-errors
                        break;
                    }
                    else
                    {
                        _logger.LogInformation("Consume error: {0}", e.Error.Reason);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogInformation($"Unexpected error: {e}");
                    break;
                }
            }
        }
    }
}
