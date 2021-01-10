using System;
using System.Threading;
using System.Threading.Tasks;

namespace Shared
{
    public static class SubscriptionExtentions
    {
        public static Task<(TKey key, TValue value)> ConsumeAsync<TKey, TValue>(this ISubscription<TKey, TValue> subscription, CancellationToken cancellationToken = default)
        {
            if (subscription == null) throw new ArgumentNullException(nameof(subscription));
            var wrapper = new SubscriptionWrapper<TKey, TValue>(subscription);
            return wrapper.ConsumeAsync();
        }

        private class SubscriptionWrapper<TKey, TValue>
        {
            private TaskCompletionSource<(TKey, TValue)> completionSource = new TaskCompletionSource<(TKey,TValue)>();
            private ISubscription<TKey, TValue> subscription;

            public SubscriptionWrapper(ISubscription<TKey, TValue> subscription)
            {
                this.subscription = subscription;
                this.subscription.MessageReceived += SubscriptionOnMessageReceived;
            }

            private void SubscriptionOnMessageReceived(TKey key, TValue value)
            {
                completionSource.TrySetResult((key, value));
                this.subscription.MessageReceived -= SubscriptionOnMessageReceived;
            }   

            public Task<(TKey key, TValue value)> ConsumeAsync()
            {
                return completionSource.Task;
            }
        }
    }
}
