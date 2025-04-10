namespace InfonetCSCR.Web.Services;

public static class CachingServices
{
  public static async Task<T> GetCacheAsync<T>(this IDistributedCache distributedCache, string cacheKey, CancellationToken token = default(CancellationToken))
  {
    if (distributedCache == null)
    {
      throw new ArgumentNullException("distributedCache");
    }
    if (cacheKey == null)
    {
      throw new ArgumentNullException("cacheKey");
    }

    byte[] utf8Byte = await distributedCache.GetAsync(cacheKey, token).ConfigureAwait(continueOnCapturedContext: false);
    if (utf8Byte != null)
    {
      return JsonSerializer.Deserialize<T>(utf8Byte);
    }

    return default(T);
  }

  public static async Task RemoveCacheAsync(this IDistributedCache distributedCache, string cacheKey, CancellationToken token = default(CancellationToken))
  {
    if (distributedCache == null)
    {
      throw new ArgumentNullException("distributedCache");
    }
    if (cacheKey == null)
    {
      throw new ArgumentNullException("cacheKey");
    }
    await distributedCache.RemoveAsync(cacheKey, token).ConfigureAwait(continueOnCapturedContext: false);
  }

  public static async Task SetCacheAsync<T>(this IDistributedCache distributedCache, string cacheKey, T obj, int cacheExpirationInMinutes = 180, CancellationToken token = default(CancellationToken))
  {
    if (distributedCache == null)
    {
      throw new ArgumentNullException("distributedCache");
    }
    if (cacheKey == null)
    {
      throw new ArgumentNullException("cacheKey");
    }
    if (obj == null)
    {
      throw new ArgumentNullException("obj");
    }

    DistributedCacheEntryOptions options = new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(cacheExpirationInMinutes));
    byte[] utf8Bytes = JsonSerializer.SerializeToUtf8Bytes(obj);
    await distributedCache.SetAsync(cacheKey, utf8Bytes, options, token).ConfigureAwait(continueOnCapturedContext: false);
  }
}
