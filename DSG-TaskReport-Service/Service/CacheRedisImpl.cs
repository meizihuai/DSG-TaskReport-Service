using System;
using System.Collections.Generic;
using System.Text;

namespace Common
{
    public class CacheRedisImpl : ICache
    {
        public bool Exists(string key)
        {
            return RedisHelper.Redis.Exists(key);
        }

        public T GetCache<T>(string key)
        {
            return RedisHelper.Redis.GetCache<T>(key);
        }

        public string GetCache(string key)
        {
            return RedisHelper.Redis.GetCache(key);
        }

        public int GetExpire(string key)
        {
            return RedisHelper.Redis.GetExpire(key);
        }

        public void RemoveCache(string key)
        {
            RedisHelper.Redis.RemoveCache(key);
        }

        public void SetCache(string key, object value, int expSecond = 0)
        {
            if (expSecond > 0)
                RedisHelper.Redis.SetCache(key, value);
            else
                RedisHelper.Redis.SetCache(key, value, expSecond);

        }

        public bool SetExpire(string key, int second)
        {
            return RedisHelper.Redis.SetExpire(key, second);
        }
    }
}
