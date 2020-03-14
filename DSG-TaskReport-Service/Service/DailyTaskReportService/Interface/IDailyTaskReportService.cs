using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DSG_TaskReport_Service.Service.DailyTaskReportService.Interface
{
    public interface IDailyTaskReportService
    {
        string WorkType { get; }
        Task<NormalResponse> DoReport(DateTime day,string deviceId);
    }
}
