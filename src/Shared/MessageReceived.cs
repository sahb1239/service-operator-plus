namespace Shared
{
    public delegate void MessageReceived<TKey, TValue>(TKey key, TValue value);
}