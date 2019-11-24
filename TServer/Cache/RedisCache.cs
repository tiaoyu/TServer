using System;
namespace Common.Cache
{
    public class RedisCache
    {
        public RedisCache()
        {
            var csredis = new CSRedis.CSRedisClient("127.0.0.1:6379,defaultDatabase=13,prefix=T_");
            RedisHelper.Initialization(csredis);
        }

        public static void Set(string key, byte[] value)
        {
            RedisHelper.Set(key, value);
        }

        public static byte[] Get(string key)
        {
            return RedisHelper.Get<byte[]>(key);
        }
    }
}
