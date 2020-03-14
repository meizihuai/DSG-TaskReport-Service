using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

using System.Drawing;
using Newtonsoft;
using Newtonsoft.Json;
using DSGServerCore;
using Novacode;
using Common;

namespace DSG_TaskReport_Service
{
    class FreqModuleService : ITaskReportService
    {
        public Task<NormalResponse> DoReport(UserTaskInfo taskInfo, string deviceId, string dirPath)
        {
            return Task.Run(() =>
            {
                TaskFreqModule ts = JsonConvert.DeserializeObject<TaskFreqModule>(taskInfo.TaskCode);
                if (ts == null) return new NormalResponse(false, $"任务 {taskInfo.TaskNickName},id={taskInfo.ID},在制作报告时Json转换格式非法");
                List<TaskMsgInfo> msgList = null;
                string deviceNickName = TaskUtil.GetDeviceNameByDeviceId(deviceId);
                using (var db = new DSGDbContext())
                {
                    var rt = db.TaskMsgTable.Where(a => a.TaskID == taskInfo.ID && a.DeviceID == deviceId).ToList();
                    if (rt != null)
                    {
                        msgList = rt;
                    }
                }
                if (msgList == null || msgList.Count == 0) return new NormalResponse(false, $"{deviceNickName}:在执行任务期间，没有记录任何设备消息，无法制作报告");
                List<FreqBscanInfo> freqlist = new List<FreqBscanInfo>();
                msgList.ForEach(a =>
                {
                    FreqBscanInfo finfo = JsonConvert.DeserializeObject<FreqBscanInfo>(a.DeviceMsg);
                    finfo.DateTime = a.DateTime;
                    if (finfo != null) freqlist.Add(finfo);
                });
                if (freqlist.Count == 0)
                {
                    return new NormalResponse(false, $"{deviceNickName}:在执行任务期间，有记录{msgList.Count}条设备消息，但是没有任何有效的频谱数据，无法制作报告");
                }
                string freqStart = $"{ts.FreqStart.ToString("0.000")}MHz";
                string freqStop = $"{ts.FreqStop.ToString("0.000")}MHz";
                string startTime = taskInfo.StartTime;
                string endTime = taskInfo.EndTime;

                string filePath = Path.Combine(dirPath, $"{deviceNickName}-{taskInfo.TaskNickName}.docx");
                using (DocX doc = DocX.Create(filePath))
                {
                    doc.InsertParagraph().AppendLine($"{taskInfo.TaskNickName}监测报告").Font("黑体").FontSize(25).Alignment = Alignment.center;
                    doc.InsertParagraph().AppendLine($"{deviceNickName}").Font("黑体").FontSize(25).Alignment = Alignment.center;
                    doc.InsertParagraph().AppendLine("1.概述").Font("宋体").FontSize(20);
                    doc.InsertParagraph()
                    .AppendLine($"     本次监测任务主要监测[{ts.FreqStart.ToString("0.000")}MHz,{ts.FreqStop.ToString("0.000")}MHz]频段,在{taskInfo.StartTime}到{taskInfo.EndTime}期间的频谱建模")
                    .Font("宋体").FontSize(15);

                    doc.InsertParagraph().AppendLine("2.最大值保持图").Font("宋体").FontSize(20);
                    FreqModule freqModule = new FreqModule(freqlist[0], 0, "");
                    freqlist.ForEach(a =>
                    {
                        a.DeValue();
                        freqModule.Update(a);
                    });
                    RemoteFreqModule.Add(deviceId, taskInfo, freqModule);
                    {
                        string freqLabel = $"[{freqStart},{freqStop}]频段,在{startTime}到{endTime}期间，{freqlist.Count}幅频谱最大值保持图";
                        string imgPath = TaskUtil.FreqValues2ImgFile(freqModule.Freq.Freqs, freqModule.Freq.FreqValues, freqLabel, Color.Green);
                        var nImg = doc.AddImage(imgPath);
                        var pic = nImg.CreatePicture();
                        pic.Width = 600;
                        pic.Height = 200;
                        doc.InsertParagraph().AppendPicture(pic);
                    }
                    doc.Save();
                }
                return new NormalResponse(true, filePath);
            });
        }

        public string GetTaskType()
        {
            return TaskKind.RemoteFreqModule;
        }
    }
}
