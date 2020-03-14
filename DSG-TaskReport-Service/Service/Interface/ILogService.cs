using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DSG_TaskReport_Service
{
    public interface ILogService
    {
        void Info(string serviceId, string str, int level);
        void Warn(string serviceId, string str, int level);
        void Error(string serviceId, string str, int level);
    }
}
