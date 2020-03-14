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
    public class ModelActionFilter : IAlwaysRunResultFilter
    {

        public void OnResultExecuted(ResultExecutedContext context)
        {

        }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {

                string msg = "";
                foreach (var item in context.ModelState.Values)
                {
                    foreach (var error in item.Errors)
                    {
                        msg += error.ErrorMessage + ";";
                    }
                }
                context.Result = new ContentResult()
                {
                    Content = JsonConvert.SerializeObject(new NormalResponse(NPCode.ModelError, msg)),
                    StatusCode = 200
                };
            }
        }
    }
}
