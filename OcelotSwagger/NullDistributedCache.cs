namespace OcelotSwagger
{
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Caching.Distributed;

    public class NullDistributedCache : IDistributedCache
    {
        public byte[] Get(string key)
        {
            return null;
        }

        public Task<byte[]> GetAsync(string key, CancellationToken token = new CancellationToken())
        {
            return Task.FromResult<byte[]>(null);
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            // Do nothing
        }

        public Task SetAsync(
            string key,
            byte[] value,
            DistributedCacheEntryOptions options,
            CancellationToken token = new CancellationToken())
        {
            return Task.CompletedTask;
        }

        public void Refresh(string key)
        {
            // Do nothing
        }

        public Task RefreshAsync(string key, CancellationToken token = new CancellationToken())
        {
            return Task.CompletedTask;
        }

        public void Remove(string key)
        {
            // Do nothing
        }

        public Task RemoveAsync(string key, CancellationToken token = new CancellationToken())
        {
            return Task.CompletedTask;
        }
    }
}
