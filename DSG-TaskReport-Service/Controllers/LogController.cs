using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DSG_TaskReport_Service.Controllers
{
    public class LogController : Controller
    {
        public IActionResult Index()
        {
            return Redirect("/staticfiles/html/log.html");
        }
    }
}