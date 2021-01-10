using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Shared;

namespace Tester
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IProducer<long, string> _producer;

        public Worker(ILogger<Worker> logger, IProducer<long, string> producer)
        {
            _logger = logger;
            _producer = producer;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);

                await _producer.ProduceAsync("Plus",
                    JsonConvert.SerializeObject(new Operation() {Operands = new List<decimal>() {1, 2}}));
            }
        }

        public class Operation
        {
            public IEnumerable<decimal> Operands { get; set; }
        }
    }
}
