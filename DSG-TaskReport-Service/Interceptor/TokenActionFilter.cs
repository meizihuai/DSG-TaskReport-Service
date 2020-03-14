using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;


namespace DSG_TaskReport_Service
{
    public class TokenActionFilter : Attribute, IAsyncActionFilter
    {
        public int PowerLevel = 1;

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            bool flagSuccess = false;
            bool flagRightPower = false;
            context.HttpContext.Request.Query.TryGetValue("token", out StringValues token);
            if (!string.IsNullOrEmpty(token))
            {
                flagSuccess = true;
                flagRightPower = true;
                //if(mAccountService==null) mAccountService = (IAccountService)Module.ServiceProvider.GetService(typeof(IAccountService));
                //NormalResponse np = mAccountService.GetTokenInfo(token);
                //if (np.result)
                //{
                //    UserAccount userInfo = np.Parse<UserAccount>();
                //    if (userInfo != null)
                //    {
                //        flagSuccess = true;
                //        context.HttpContext.Items["UserInfo"] = userInfo;
                //        flagRightPower = userInfo.Power >= PowerLevel;
                //    }
                //}
            }

            if (!flagSuccess)
            {
                context.Result = new ContentResult()
                {
                    Content = JsonConvert.SerializeObject(new NormalResponse(NPCode.TokenError, "token无效")),
                    StatusCode = 200
                };
                return;
            }
            if (!flagRightPower)
            {
                context.Result = new ContentResult()
                {
                    Content = JsonConvert.SerializeObject(new NormalResponse(NPCode.PowerError, "权限不足")),
                    StatusCode = 200
                };
                return;
            }

            await next();
        }
    }
}
