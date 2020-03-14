using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DSG_TaskReport_Service
{
    public class LogInfo
    {
        public string ServiceId { get; set; }
        /// <summary>
        /// 日志类型，Info,Warn,Error
        /// </summary>
        public string Type { get; set; }
        public string Time { get; set; }
        public string Content { get; set; }
        public int Level { get; set; }
    }
}
