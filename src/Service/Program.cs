using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry.Serdes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Shared;

namespace Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();

                    services.AddSingleton<IMathService, MathService>();

                    services.AddSingleton<Confluent.Kafka.IConsumer<long, string>>(serviceProvider =>
                    {
                        var config = new ConsumerConfig();
                        serviceProvider.GetService<IConfiguration>().GetSection("Kafka:ConsumerSettings").Bind(config);
                        return new ConsumerBuilder<long, string>(config)
                            .Build();
                    });
                    services.AddSingleton<Confluent.Kafka.IProducer<long, string>>(serviceProvider =>
                    {
                        var config = new ProducerConfig();
                        serviceProvider.GetService<IConfiguration>().GetSection("Kafka:ProducerSettings").Bind(config);
                        return new ProducerBuilder<long, string>(config)
                            .Build();
                    });

                    services.AddSingleton<Shared.IConsumer<long, string>, KafkaConsumer<long, string>>();
                    services.AddSingleton<Shared.IProducer<long, string>, KafkaProducer<long, string>>();
                });
    }
}
