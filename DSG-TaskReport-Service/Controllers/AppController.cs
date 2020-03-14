using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using DSG_TaskReport_Service.RunningLog;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DSG_TaskReport_Service.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AppController : ControllerBase
    {
        /// <summary>
        /// 获取运行日志
        /// </summary>
        /// <param name="getCount"></param>
        /// <returns></returns>
        [HttpGet]
        public NormalResponse GetLogs(int getCount = 0)
        {
            return new NormalResponse(true, "", "", RuningLog.GetLogs(getCount));
        }
    }
}