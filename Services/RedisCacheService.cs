using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace ComputerStore.Services
{
    public class RedisCacheService : IRedisCacheService
    {
        private readonly IDistributedCache _cache;

   public RedisCacheService(IDistributedCache cache)
   {
   _cache = cache;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan expiration)
   {
   var options = new DistributedCacheEntryOptions
   {
       AbsoluteExpirationRelativeToNow = expiration
   };

 var jsonData = JsonSerializer.Serialize(value);
 await _cache.SetStringAsync(key, jsonData, options);
        }

  public async Task<T?> GetAsync<T>(string key)
   {
   var jsonData = await _cache.GetStringAsync(key);
       return jsonData == null ? default : JsonSerializer.Deserialize<T>(jsonData);
   }
    }
}