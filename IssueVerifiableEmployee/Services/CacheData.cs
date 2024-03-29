using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IssuerVerifiableEmployee.Services;

public class CacheData
{
    [JsonPropertyName("status")]
    public string? Status { get; set; } = string.Empty;
    [JsonPropertyName("message")]
    public string? Message { get; set; } = string.Empty;
    [JsonPropertyName("expiry")]
    public string? Expiry { get; set; } = string.Empty;
    [JsonPropertyName("payload")]
    public string? Payload { get; set; } = string.Empty;

    public static void AddToCache(string key, IDistributedCache cache, CacheData cacheData)
    {
        var cacheExpirationInDays = 1;

        var options = new DistributedCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromDays(cacheExpirationInDays));

        cache.SetString(key, JsonSerializer.Serialize(cacheData), options);
    }

    public static CacheData? GetFromCache(string key, IDistributedCache cache)
    {
        var item = cache.GetString(key);
        if (item != null)
        {
            return JsonSerializer.Deserialize<CacheData>(item);
        }

        return null;
    }
}
