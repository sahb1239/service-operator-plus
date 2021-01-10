using System;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Kafka
{
    public static class KafkaServiceCollectionExtensions
    {
        public static IServiceCollection AddKafkaConsumer<TKey, TValue>(this IServiceCollection services,
            Func<IServiceProvider, Confluent.Kafka.IConsumer<TKey, TValue>> func)
        {
            return services
                .AddSingleton(func)
                .AddSingleton<IConsumer<TKey, TValue>, KafkaConsumer<TKey, TValue>>()
                .AddTransient<IKafkaSubscriptionHandler<TKey, TValue>, KafkaSubscriptionHandler<TKey, TValue>>()
                .AddSingleton<IKafkaSubscriptionSourceFactory<TKey, TValue>,
                    KafkaSubscriptionSourceFactory<TKey, TValue>>();
        }

        public static IServiceCollection AddKafkaConsumer<TKey, TValue>(this IServiceCollection services,
            string configurationSection)
        {
            return services.AddKafkaConsumer(serviceProvider =>
            {
                var config = new ConsumerConfig();
                serviceProvider.GetService<IConfiguration>().GetSection(configurationSection).Bind(config);
                return new ConsumerBuilder<TKey, TValue>(config)
                    .Build();

            });
        }

        public static IServiceCollection AddKafkaProducer<TKey, TValue>(this IServiceCollection services,
            Func<IServiceProvider, Confluent.Kafka.IProducer<TKey, TValue>> func)
        {
            return services
                .AddSingleton(func)
                .AddSingleton<IProducer<TKey, TValue>, KafkaProducer<TKey, TValue>>();
        }

        public static IServiceCollection AddKafkaProducer<TKey, TValue>(this IServiceCollection services,
            string configurationSection)
        {
            return services.AddKafkaProducer(serviceProvider =>
            {
                var config = new ProducerConfig();
                serviceProvider.GetService<IConfiguration>().GetSection(configurationSection).Bind(config);
                return new ProducerBuilder<TKey, TValue>(config)
                    .Build();
            });
        }
    }
}
