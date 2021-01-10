using System.Threading.Tasks;

namespace Shared
{
    public interface IProducer<TKey, TValue>
    {
        Task<TKey> ProduceAsync(string topic, TValue value);
    }
}
