using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Api.Infra.Caching
{
    public class RedisCachingService : IRedisCachingService
    {
        private readonly IDistributedCache _cache;
        private bool _redisAvailable = true;

        public RedisCachingService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<T> GetAsync<T>(string key)
        {
            if (!_redisAvailable) return default;

            try
            {
                var cachedData = await _cache.GetStringAsync(key);
                if (string.IsNullOrEmpty(cachedData))
                    return default;

                return JsonSerializer.Deserialize<T>(cachedData);
            }
            catch (Exception)
            {
                // Redis indisponível - desativa temporariamente
                _redisAvailable = false;
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            if (!_redisAvailable) return;

            try
            {
                var options = new DistributedCacheEntryOptions();

                if (expiration.HasValue)
                    options.AbsoluteExpirationRelativeToNow = expiration;
                else
                    options.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);

                var serializedData = JsonSerializer.Serialize(value);
                await _cache.SetStringAsync(key, serializedData, options);

                // Se conseguiu salvar, Redis está disponível
                _redisAvailable = true;
            }
            catch (Exception)
            {
                _redisAvailable = false;
            }
        }

        public async Task RemoveAsync(string key)
        {
            if (!_redisAvailable) return;

            try
            {
                await _cache.RemoveAsync(key);
                _redisAvailable = true;
            }
            catch (Exception)
            {
                _redisAvailable = false;
            }
        }

        public async Task<bool> ExistsAsync(string key)
        {
            if (!_redisAvailable) return false;

            try
            {
                var data = await _cache.GetStringAsync(key);
                _redisAvailable = true;
                return !string.IsNullOrEmpty(data);
            }
            catch (Exception)
            {
                _redisAvailable = false;
                return false;
            }
        }
    }
}