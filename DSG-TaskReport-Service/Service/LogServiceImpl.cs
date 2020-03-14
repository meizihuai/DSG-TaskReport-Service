using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DSG_TaskReport_Service
{
    public class LogServiceImpl : ILogService
    {
        private string logServiceUrl = Module.Configuration.GetSection("MicroServiceUrl:LogService").Value;
        public async void Info(string serviceId, string str, int level)
        {
            LogInfo info = new LogInfo()
            {
                ServiceId = serviceId,
                Content = str,
                Time = TimeUtil.Now().ToString("yyyy-MM-dd HH:mm:ss"),
                Level = level,
                Type = "Info"
            };
            await HttpHelper.Post($"{logServiceUrl}/api/log/UploadLog", info);
        }

        public async void Warn(string serviceId, string str, int level)
        {
            LogInfo info = new LogInfo()
            {
                ServiceId = serviceId,
                Content = str,
                Time = TimeUtil.Now().ToString("yyyy-MM-dd HH:mm:ss"),
                Level = level,
                Type = "Warn"
            };
            await HttpHelper.Post($"{logServiceUrl}/api/log/UploadLog", info);
        }
        public async void Error(string serviceId, string str, int level)
        {
            LogInfo info = new LogInfo()
            {
                ServiceId = serviceId,
                Content = str,
                Time = TimeUtil.Now().ToString("yyyy-MM-dd HH:mm:ss"),
                Level = level,
                Type = "Error"
            };
            await HttpHelper.Post($"{logServiceUrl}/api/log/UploadLog", info);
        }
    }
}
