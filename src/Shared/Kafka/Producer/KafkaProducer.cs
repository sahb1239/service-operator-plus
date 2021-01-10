using System.Threading.Tasks;

namespace Shared
{
    public class KafkaProducer<TKey, TValue> : IProducer<TKey, TValue>
    {
        private readonly Confluent.Kafka.IProducer<TKey, TValue> _kafkaProducer;

        public KafkaProducer(Confluent.Kafka.IProducer<TKey, TValue> kafkaProducer)
        {
            this._kafkaProducer = kafkaProducer;
        }

        public async Task<TKey> ProduceAsync(string topic, TValue value)
        {
            var message = GetMessage(value);
            var result = await _kafkaProducer.ProduceAsync(topic, message);
            return result.Key;
        }

        private Confluent.Kafka.Message<TKey, TValue> GetMessage(TValue value)
        {
            return new Confluent.Kafka.Message<TKey, TValue>
            {
                Value = value,
            };
        }
    }
}
