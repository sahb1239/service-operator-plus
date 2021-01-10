using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Shared;

namespace Tester
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

                    services.AddSingleton<Confluent.Kafka.IProducer<long, string>>(serviceProvider =>
                    {
                        var config = new ProducerConfig();
                        serviceProvider.GetService<IConfiguration>().GetSection("Kafka:ProducerSettings").Bind(config);
                        return new ProducerBuilder<long, string>(config)
                            .Build();
                    });

                    services.AddSingleton<Shared.IProducer<long, string>, KafkaProducer<long, string>>();
                });
    }
}
