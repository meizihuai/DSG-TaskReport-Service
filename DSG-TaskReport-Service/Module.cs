using Common;
using DSG_TaskReport_Service.RunningLog;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ThuInjecter;

namespace DSG_TaskReport_Service
{
    public class Module
    {
        public static readonly string APPName = "DSG_TaskReport_Service";
        public static readonly bool IsLogger = false;
        public static string Version = "1.0.0";
        public static string MysqlConnstr = "";
        public static ServiceProvider ServiceProvider;
        public static string StaticFilePath = "staticfiles";
        public static IConfiguration Configuration { get; set; }
        public static ILogService LogService { get; set; }
        private static bool useRemoteLogService = false;

        public static string ServerRootUrl = "";
        public static string ReportRootPath = "";
        public static string ReportUrlPath = "";
        public static int WorkInterval = 5;


        public static void Init(IConfiguration Configuration, ServiceProvider serviceProvider)
        {
            Console.Title = $"{APPName} {Version}";
            Module.ServiceProvider = serviceProvider;
            Module.Configuration = Configuration;
            MysqlConnstr = Configuration.GetSection("MysqlConnection").Value;
           
            LogService = serviceProvider.GetService<ILogService>();         
            ServiceFactory.Init();

            ServerRootUrl =Configuration.GetSection("ServerRootUrl").Value;
            ReportRootPath =Configuration.GetSection("ReportRootPath").Value;
            ReportUrlPath =Configuration.GetSection("ReportUrlPath").Value;
            WorkInterval = int.Parse(Configuration.GetSection("WorkInterval").Value);
       

            if (!Directory.Exists(ReportRootPath)) Directory.CreateDirectory(ReportRootPath);


            Start();
        }
        public static void Start()
        {
            Log("================程序启动================");
            //MysqlEntityCoding mysqlEntityCoding = new MysqlEntityCoding("DSG_TaskReport_Service", "Entity", "QoEDataDB", MysqlConnstr);
            //mysqlEntityCoding.WriteTableEntityToClassFile("UserAccount");

            Task.Run(async () =>
            {
                while (true)
                {
                    // Log("=========Working 1=========");
                    await UserTaskLoopWorker.DoWork();
                    //  Log("=========Working 2=========");
                    await Task.Delay(Module.WorkInterval * 1000);
                }
            });


        }
        public static void Stop()
        {
            Log("================程序关闭================");
        }
        public static void Log(string str, string serviceId = "", int level = 0)
        {
            string log = TimeUtil.Now().ToString("[HH:mm:ss] ") + str;
            Console.WriteLine(log);
            RuningLog.Log(str, serviceId, level);
            if (useRemoteLogService)
            {
                if (string.IsNullOrEmpty(serviceId)) serviceId = APPName;
                LogService.Info(serviceId, str, level);
            }
        }
    }
}
