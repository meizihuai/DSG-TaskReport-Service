using Common;
using DSG_TaskReport_Service.Controller_Service.Interface;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DSG_TaskReport_Service.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TaskReportController : ControllerBase
    {
        private ITaskReportControllerService service;

        public TaskReportController(ITaskReportControllerService service)
        {
            this.service = service;
        }

        /// <summary>
        /// 获取每日信道日报
        /// </summary>
        /// <param name="day"></param>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        [HttpGet]
        public Task<NormalResponse> GetDailySignalReport(DateTime day,string deviceId)
        {
            return service.GetDailySignalReport(day, deviceId);
        }

        /// <summary>
        /// 获取每日频谱日报
        /// </summary>
        /// <param name="day"></param>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        [HttpGet]
        public Task<NormalResponse> GetDailyFreqReport(DateTime day,string deviceId)
        {
            return service.GetDailyFreqReport(day, deviceId);
        }
    }
}
