
using Common;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DSG_TaskReport_Service
{
    public class TokenRenewalMiddleware
    {
        private readonly RequestDelegate next;

        private readonly string[] excludePaths = new string[]
        {
            "/api/account"
        };
        private readonly string[] includePaths = new string[]
        {
            "/api/account/GetTokenInfo"
        };
        public TokenRenewalMiddleware(RequestDelegate next)
        {
            this.next = next;

        }
        public Task Invoke(HttpContext context)
        {
            var token = context.Request.Query["token"].ToString();
            if (string.IsNullOrEmpty(token)) return this.next(context);

            string path = context.Request.Path.ToString().ToLower();
            bool flagNeedRenewal = false;
            int count = excludePaths.ToList().Where(a => path.StartsWith(a.ToLower())).Count();
            flagNeedRenewal = count == 0;
            if (!flagNeedRenewal)
            {
                int includeCount = includePaths.ToList().Where(a => path.StartsWith(a.ToLower())).Count();
                flagNeedRenewal = includeCount > 0;
            }
            if (!flagNeedRenewal)
            {
                return this.next(context);
            }
            // Task.Run(() => Api.RenewalToken(token));
            return this.next(context);
        }
    }
}
