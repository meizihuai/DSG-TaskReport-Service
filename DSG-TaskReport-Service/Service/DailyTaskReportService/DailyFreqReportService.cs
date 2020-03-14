using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using DSG_TaskReport_Service.Service.DailyTaskReportService.Interface;
using DSGServerCore;

namespace DSG_TaskReport_Service.TaskDoReportService
{
    public class DailyFreqReportService : IDailyTaskReportService
    {
        public string WorkType => "DailyFreq";

        public Task<NormalResponse> DoReport(DateTime day, string deviceId)
        {
            throw new NotImplementedException();
        }
    }
}
