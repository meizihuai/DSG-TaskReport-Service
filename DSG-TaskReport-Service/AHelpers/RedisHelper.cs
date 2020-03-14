
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Common
{
    public static class RedisConfig
    {
        public static string Configname { get; set; }
        public static string Connection { get; set; }
        public static int DefaultDatabase { get; set; }
        public static string InstanceName { get; set; }
    }
    public class RedisHelper : IDisposable
    {

        private static IDatabase _cache;

        private static ConnectionMultiplexer _connection;

        private static string _instanceName;

        private static ISubscriber _sub;

        private static object instanceLock = new object();
        private static RedisHelper instance;
        public static RedisHelper Redis
        {
            get
            {
                if (instance == null)
                {
                    lock (instanceLock)
                    {
                        if (instance == null) instance = new RedisHelper();
                    }
                }
                return instance;
            }
        }

        public RedisHelper(/*RedisCacheOptions options, int database = 0*/)//这里可以做成依赖注入，但没打算做成通用类库，所以直接把连接信息直接写在帮助类里
        {
            int database = RedisConfig.DefaultDatabase;
            _connection = ConnectionMultiplexer.Connect(RedisConfig.Connection);
            _cache = _connection.GetDatabase(database);
            _instanceName = RedisConfig.InstanceName;
            _sub = _connection.GetSubscriber();
        }

        /// <summary>
        /// 取得redis的Key名称
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private string GetKeyForRedis(string key)
        {
            return _instanceName + key;
        }
        /// <summary>
        /// 判断当前Key是否存在数据
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Exists(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
            return _cache.KeyExists(GetKeyForRedis(key));
        }

        /// <summary>
        /// 设置key过期时间
        /// </summary>
        /// <param name="key"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public bool SetExpire(string key, int second)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
            return _cache.KeyExpire(GetKeyForRedis(key), TimeSpan.FromSeconds(second));
        }
        public int GetExpire(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
            var ts = _cache.KeyTimeToLive(GetKeyForRedis(key));
            if (ts == null) return 0;
            return (int)ts.Value.TotalSeconds;
        }

        /// <summary>
        /// 取得缓存数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T GetCache<T>(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
            var value = _cache.StringGet(GetKeyForRedis(key));
            if (!value.HasValue)
                return default(T);
            return JsonConvert.DeserializeObject<T>(value);
        }
        public string GetCache(string key)
        {
            return GetCache<string>(key);
        }
        /// <summary>
        /// 设置缓存数据
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetCache(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (Exists(GetKeyForRedis(key)))
                RemoveCache(GetKeyForRedis(key));

            _cache.StringSet(GetKeyForRedis(key), JsonConvert.SerializeObject(value));
        }
        /// <summary>
        /// 设置绝对过期时间
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiressAbsoulte"></param>
        public void SetCache(string key, object value, DateTime expiressAbsoulte)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (Exists(GetKeyForRedis(key)))
                RemoveCache(GetKeyForRedis(key));
            TimeSpan t = expiressAbsoulte - TimeUtil.Now();
            _cache.StringSet(GetKeyForRedis(key), JsonConvert.SerializeObject(value), t);
        }
        /// <summary>
        /// 设置相对过期时间
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expSecond"></param>
        public void SetCache(string key, object value, int expSecond)
        {
            if (Exists(GetKeyForRedis(key)))
                RemoveCache(GetKeyForRedis(key));
            if (expSecond == 0)
            {
                _cache.StringSet(GetKeyForRedis(key), JsonConvert.SerializeObject(value));
            }
            else
            {
                TimeSpan ts = TimeSpan.FromSeconds(expSecond);
                _cache.StringSet(GetKeyForRedis(key), JsonConvert.SerializeObject(value), ts);
            }

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="endPoint"></param>
        /// <param name="database"></param>
        /// <param name="timeountseconds"></param>
        public void KeyMigrate(string key, EndPoint endPoint, int database, int timeountseconds)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
            _cache.KeyMigrate(GetKeyForRedis(key), endPoint, database, timeountseconds);
        }
        /// <summary>
        /// 移除redis
        /// </summary>
        /// <param name="key"></param>
        public void RemoveCache(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            _cache.KeyDelete(GetKeyForRedis(key));
        }

        /// <summary>
        /// 销毁连接
        /// </summary>
        public void Dispose()
        {
            if (_connection != null)
                _connection.Dispose();
            GC.SuppressFinalize(this);
        }

        public void GetMssages(string channelName)
        {
            using (_connection = ConnectionMultiplexer.Connect(RedisConfig.Connection))
                _sub.Subscribe(channelName, (channel, message) =>
                {
                    string result = message;
                });
        }

        public void Publish(string channelName, string msg)
        {
            using (_connection = ConnectionMultiplexer.Connect(RedisConfig.Connection))
                _sub.Publish(channelName, msg);
        }

    }
}
