using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using DSG_TaskReport_Service.Model;
using DSG_TaskReport_Service.Service.DailyTaskReportService.Interface;
using DSGServerCore;
using Newtonsoft.Json;

namespace DSG_TaskReport_Service.TaskDoReportService
{
    public class DailySignalReportService : IDailyTaskReportService
    {
        public string WorkType => "DailySignal";

        public Task<NormalResponse> DoReport(DateTime day, string deviceId)
        {
            using (var db = new DSGDbContext())
            {
                var device = db.DeviceTable.Where(a => a.DeviceID == deviceId).FirstOrDefault();
                if (device == null) return Task.FromResult(new NormalResponse(false, "设备不存在"));
                var msglist = db.TaskMsgTable.Where(a => a.DeviceID == deviceId && a.Day == day.Date.ToString("yyyy-MM-dd") && a.Func == "MRYP" && a.TaskID == 0).ToList();
                if(msglist==null || msglist.Count==0) return Task.FromResult(new NormalResponse(false, "没有任何信道数据"));
                return Task.FromResult(MakeReport(msglist, device));
            }
        }

        private NormalResponse MakeReport(List<TaskMsgInfo> msglist,DeviceInfo device)
        {
            List<FreqValueInfo> freqValueInfos = msglist.Select(a => JsonConvert.DeserializeObject<FreqValueInfo>(a.DeviceMsg)).ToList();
            if (freqValueInfos == null || freqValueInfos.Count == 0) return new NormalResponse(false, "没有任何有效的信道数据");
            return new NormalResponse(true, "", "", "");
        }
    }
}
