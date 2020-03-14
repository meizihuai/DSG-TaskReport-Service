using Common;
using DSGServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSG_TaskReport_Service
{
    public interface ITaskReportService
    {
        string GetTaskType();
        Task<NormalResponse> DoReport(UserTaskInfo taskInfo, string deviceId, string dirPath);
    }
}
