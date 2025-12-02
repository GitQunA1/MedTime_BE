using Microsoft.Extensions.Caching.Memory;

namespace MedTime.Services
{
    public class TokenCacheService
    {
        private readonly IMemoryCache _cache;

        public TokenCacheService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public void StoreRefreshToken(int userId, string refreshToken, DateTime expiryTime)
        {
            var cacheKey = $"refresh_token_{userId}";
            _cache.Set(cacheKey, (refreshToken, expiryTime), expiryTime);
        }

        public (string? Token, DateTime? ExpiryTime) GetRefreshToken(int userId)
        {
            var cacheKey = $"refresh_token_{userId}";
            if (_cache.TryGetValue(cacheKey, out (string Token, DateTime ExpiryTime) tokenInfo))
            {
                return tokenInfo;
            }
            return (null, null);
        }

        public void RemoveRefreshToken(int userId)
        {
            var cacheKey = $"refresh_token_{userId}";
            _cache.Remove(cacheKey);
        }
    }
}
