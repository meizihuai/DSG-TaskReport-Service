using Common;
using DSG_TaskReport_Service.TaskDoReportService;
using DSGServerCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSG_TaskReport_Service
{
    class UserTaskLoopWorker
    {
        private static List<ITaskReportService> _TaskReportServiceList;
        public static List<ITaskReportService> TaskReportServiceList
        {
            get
            {
                if (_TaskReportServiceList == null) _TaskReportServiceList = Module.ServiceProvider.GetServices<ITaskReportService>().ToList();
                return _TaskReportServiceList;
            }
        }
       

        public static async Task DoWork()
        {
            try
            {
                string nowTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                //临时修改TaskID = 36 的记录，方便执行报告操作
                //121
                //using (var db = new DSGDbContext())
                //{
                //    var rt = db.UserTaskTable.Find(210);
                //    if (rt != null)
                //    {
                //        rt.StatusCode = 1;
                //        rt.IsClosed = 0;
                //        rt.ReportLog = "";
                //    }
                //    db.Update(rt, a => a.StatusCode, a => a.IsClosed, a => a.ReportLog);
                //}

                //查询和修改未开启且已经可以开始的任务
                using (var db = new DSGDbContext())
                {
                    var rt = db.UserTaskTable.Where(a => a.IsClosed == 0 && a.StatusCode == 0 && a.StartTime.CompareTo(nowTime) <= 0).ToList();
                    if (rt != null && rt.Count > 0)
                    {
                        foreach (var itm in rt)
                        {
                            itm.Status = "正在执行";
                            itm.StatusCode = 1;
                        }
                        int updateCount = db.SaveChanges();
                    }
                }
                //查询和修改未结束且已经开始的任务，且已经可以结束的任务
                using (var db = new DSGDbContext())
                {
                   var rt = db.UserTaskTable.Where(a => a.IsClosed == 0 && a.StatusCode == 1 && a.EndTime.CompareTo(nowTime) <= 0).ToList();
                   // var rt = db.UserTaskTable.Where(a => a.ID == 126).ToList();
                    if (rt != null && rt.Count > 0)
                    {
                        foreach (var itm in rt)
                        {
                            itm.Status = "制作报告";
                            itm.StatusCode = 2;
                        }
                        db.SaveChanges();
                        
                        foreach (var itm in rt)
                        {
                            Module.Log($">>制作报告中,id={itm.ID},{itm.TaskNickName}");
                            Stopwatch sp = new Stopwatch();
                            sp.Start();
                            await DoReport(itm);
                            sp.Stop();
                            Module.Log($"    -->[Done]耗时 {sp.Elapsed.ToString()}");
                        }                                         
                    }
                }
            }
            catch (Exception e)
            {
                string txt = e.ToString();
                Module.Log(txt);
            }
        }
        /// <summary>
        /// 制作报告
        /// </summary>
        /// <param name="info"></param>
        public static async Task DoReport(UserTaskInfo info)
        {
            try
            {


                string dirPath = Path.Combine(Module.ReportRootPath, "UserTaskReports", info.ID + "-" + info.Guid);
                string report = "";
                CheckDir($"{Module.ReportRootPath}/");
                CheckDir($"{Path.Combine(Module.ReportRootPath, "UserTaskReports")}");
                CheckDir(dirPath);
                string[] deviceids = JsonConvert.DeserializeObject<string[]>(info.DeviceID);
                string[] deviceNickNames= JsonConvert.DeserializeObject<string[]>(info.DeviceName);
                if (deviceids == null) return;
                if (deviceNickNames == null) return;
                if (deviceNickNames .Length!= deviceids.Length) return;
                StringBuilder sb = new StringBuilder();
                List<string> exportFileNamelist = new List<string>();
                for(int i=0;i<deviceids.Length;i++)
                {
                    string itm = deviceids[i];
                    Module.Log($" 制作 {itm}   {i+1} / {deviceids.Length} 数据...");
                    string deviceNickName = deviceNickNames[i];
                    NormalResponse np = null;
                    ITaskReportService service = TaskReportServiceList.Where(a => a.GetTaskType() == info.TaskKind).FirstOrDefault();
                    if (service != null)
                    {
                        np = await service.DoReport(info, itm, dirPath);
                    }                   
                    if (np!=null)
                    {
                        if (!np.result)
                        {
                            Module.Log($"    -->制作报告事件,{np.msg}");
                            sb.AppendLine(np.msg);
                        }
                        else
                        {
                            exportFileNamelist.Add(np.msg);
                        }                    
                    }                 
                }
                string zipFileName = info.TaskNickName + ".zip";
                string zipFilePath = Path.Combine(dirPath, zipFileName);
                string fileExt = "zip";
                if (exportFileNamelist.Count>1)
                {
                    ZipHelper.Zip(dirPath, zipFilePath);
                }
                else if(exportFileNamelist.Count ==1)
                {
                    fileExt = ".docx";
                    zipFileName = new FileInfo(exportFileNamelist[0]).Name;
                    zipFilePath = Path.Combine(dirPath, zipFileName);
                }
               
                string url = "";
                if (File.Exists(zipFilePath))
                {
                    url = $"{Module.ServerRootUrl}/{Module.ReportUrlPath}/{info.ID + "-" + info.Guid}/{zipFileName}";
                }
                else
                {
                    sb.AppendLine("该任务不需制作报告，或报告文件不存在");
                }
                using (var db = new DSGDbContext())
                {
                    var rt = db.UserTaskTable.Find(info.ID);
                    if (rt != null)
                    {
                        rt.ReportLog = sb.ToString();
                        rt.ResultReportUrl = url;
                        rt.Status = "已完成";
                        rt.IsClosed = 1;
                        rt.StatusCode = 100;
                    }
                    db.SaveChanges();
                }
            
                Hashtable ht = new Hashtable();
                string emailBody = "您好，您的监测任务已经完成，系统自动推送报告至您的邮箱，请查阅附件，谢谢！";
                if (File.Exists(zipFilePath))
                {
                    ht.Add($"{info.ID}-{info.TaskNickName}监测报告.{fileExt}", zipFilePath);
                }
                else
                {
                    emailBody = "您好，您的监测任务已经完成，此次监测任务没有报告产生，请登录系统查看详情，谢谢！";
                }
               
               
                EmailHelper.SendMail(info.PushEmailUserName, $"{info.ID}-{info.TaskNickName}监测报告", emailBody, ht);
               
            }
            catch (Exception e)
            {
                Module.Log(e.ToString());
            }
        }
        private static void CheckDir(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}
