using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DSG_TaskReport_Service.Controller_Service.Interface
{
    public interface ITaskReportControllerService
    {
        Task<NormalResponse> GetDailySignalReport(DateTime day, string deviceId);
        Task<NormalResponse> GetDailyFreqReport(DateTime day, string deviceId);
    }
}
