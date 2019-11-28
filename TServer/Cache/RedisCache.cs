using System;
namespace Common.Cache
{
    public class RedisCache : Singleton<RedisCache>
    {
        public RedisCache()
        {
            var csredis = new CSRedis.CSRedisClient("127.0.0.1:6379,defaultDatabase=13,prefix=T_");
            RedisHelper.Initialization(csredis);
        }

        public void Set(string key, byte[] value)
        {
            RedisHelper.Set(key, value);
        }

        public void Set(string key, string value)
        {
            RedisHelper.Set(key, value);
        }

        public T Get<T>(string key)
        {
            return RedisHelper.Get<T>(key);
        }
    }
}
