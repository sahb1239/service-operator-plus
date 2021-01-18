using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared;
using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Service
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConsumer<long, string> _consumer;
        private readonly IProducer<long, string> _producer;
        private readonly IMathService _mathService;

        public Worker(ILogger<Worker> logger, IConsumer<long, string> consumer, IProducer<long, string> producer, IMathService mathService)
        {
            this._logger = logger;
            this._consumer = consumer;
            this._producer = producer;
            this._mathService = mathService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var subscription = _consumer.Subscribe("Plus");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Waiting for message at: {0}", DateTime.Now);

                var message = await subscription.ConsumeAsync();
                _logger.LogInformation("Retrieved operands: {0}", message.value);

                var output = _mathService.Execute(JsonConvert.DeserializeObject<Operation>(message.value));

                _logger.LogInformation("Returning response: {0}", JsonConvert.SerializeObject(output.Output));
                await _producer.ProduceAsync("PlusResult", JsonConvert.SerializeObject(output));
            }

            _consumer.Unsubscribe(subscription);
        }
    }
}
