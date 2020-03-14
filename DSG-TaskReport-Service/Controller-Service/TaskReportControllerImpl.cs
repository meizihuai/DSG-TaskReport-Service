using Common;
using DSG_TaskReport_Service.Controller_Service.Interface;
using DSG_TaskReport_Service.Service.DailyTaskReportService.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DSG_TaskReport_Service.Controller_Service
{
    public class TaskReportControllerImpl : ITaskReportControllerService
    {
        private List<IDailyTaskReportService> services;

        public TaskReportControllerImpl(IEnumerable<IDailyTaskReportService> services)
        {
            this.services = services.ToList();
        }

        public Task<NormalResponse> GetDailyFreqReport(DateTime day, string deviceId)
        {
            var service = services.Where(a => a.WorkType == "DailyFreq").FirstOrDefault();
            if (service == null) return Task.FromResult(new NormalResponse(false, "没有对应的报告服务"));
            return service.DoReport(day, deviceId);
        }

        public Task<NormalResponse> GetDailySignalReport(DateTime day, string deviceId)
        {
            var service = services.Where(a => a.WorkType == "DailySignal").FirstOrDefault();
            if (service == null) return Task.FromResult(new NormalResponse(false, "没有对应的报告服务"));
            return service.DoReport(day, deviceId);
        }
    }
}
