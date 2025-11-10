using System;
using System.Threading.Tasks;

namespace ComputerStore.Services
{
    public interface IRedisCacheService
    {
    Task SetAsync<T>(string key, T value, TimeSpan expiration);
        Task<T?> GetAsync<T>(string key);
    }
}