using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Common
{
    /// <summary>
    /// 缓存服务，一般来讲存在两种缓存服务，一是redis，二是mysql sys_cache_t表
    /// </summary>
    public interface ICache
    {
        bool Exists(string key);
        bool SetExpire(string key, int second);
        int GetExpire(string key);
        T GetCache<T>(string key);
        string GetCache(string key);
        void SetCache(string key, object value, int expSecond = 0);
        void RemoveCache(string key);

    }
}
