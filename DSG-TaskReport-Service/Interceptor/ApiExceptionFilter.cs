using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;

namespace DSG_TaskReport_Service
{
    public class ApiExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            context.Result = new ContentResult()
            {
                Content = JsonConvert.SerializeObject(new NormalResponse(NPCode.ServerError, "", context.Exception.ToString(), null)),
                StatusCode = 200
            };
        }
    }
}
