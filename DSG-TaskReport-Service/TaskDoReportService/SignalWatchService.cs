using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

using System.Drawing;
using Newtonsoft;
using Newtonsoft.Json;
using Novacode;
using DSGServerCore;
using Common;

namespace DSG_TaskReport_Service
{
    class SignalWatchService : ITaskReportService
    {
        public Task<NormalResponse> DoReport(UserTaskInfo taskInfo, string deviceId, string dirPath)
        {
            return Task.Run(()=>
            {
                TaskSignalWatch ts = JsonConvert.DeserializeObject<TaskSignalWatch>(taskInfo.TaskCode);
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
                    if (finfo != null) freqlist.Add(finfo);
                });
                if (freqlist.Count == 0)
                {
                    return new NormalResponse(false, $"{deviceNickName}:在执行任务期间，有记录{msgList.Count}条设备消息，但是没有任何有效的频谱数据，无法制作报告");
                }

                string freqStart = $"{ts.FreqStart.ToString("0.00000")}MHz";
                string freqStop = $"{ts.FreqStop.ToString("0.00000")}MHz";
                string startTime = taskInfo.StartTime;
                string endTime = taskInfo.EndTime;

                string filePath = Path.Combine(dirPath, $"{deviceNickName}-{taskInfo.TaskNickName}.docx");
                using (DocX doc = DocX.Create(filePath))
                {
                    doc.InsertParagraph().AppendLine($"{taskInfo.TaskNickName}监测报告").Font("黑体").FontSize(25).Alignment = Alignment.center;
                    doc.InsertParagraph().AppendLine($"{deviceNickName}").Font("黑体").FontSize(25).Alignment = Alignment.center;
                    doc.InsertParagraph()
                    .AppendLine($"     本次监测任务主要监测[{ts.FreqStart.ToString("0.00000")}MHz,{ts.FreqStop.ToString("0.00000")}MHz]频段,在{taskInfo.StartTime}到{taskInfo.EndTime}期间的台站监督信息")
                    .Font("宋体").FontSize(15);
                    doc.InsertParagraph().AppendLine("2.最大值保持图").Font("宋体").FontSize(20);
                    FreqModule freqModule = new FreqModule(freqlist[0], 0, "");
                    List<SignalInfo> signalList = new List<SignalInfo>();
                    freqlist.ForEach(a =>
                    {
                        a.DeValue();
                        freqModule.Update(a);
                        List<SignalInfo> list = SignalInfo.Parse(a, ts.Width, ts.Range, ts.Threshold);
                        if (list == null) list = new List<SignalInfo>();
                        foreach (double d in ts.WatchPoints)
                        {
                            bool isFind = false;
                            foreach (var itm in list)
                            {
                                if (itm.Freq == d)
                                {
                                    isFind = true;
                                    itm.IsWhite = true;
                                    break;
                                }
                            }
                            if (!isFind)
                            {
                                list.Add(new SignalInfo(d, a.GetFreqPointValue(d, -100)) { IsWhite = true });
                            }
                        }
                        foreach (var newItm in list)
                        {
                            bool isFind = false;
                            foreach (var itm in signalList)
                            {
                                if (itm.Freq == newItm.Freq)
                                {
                                    itm.Update(newItm.Value);
                                    isFind = true;
                                    break;
                                }
                            }
                            if (!isFind)
                            {
                                newItm.ValueList = new List<double>();
                                signalList.Add(newItm);
                            }
                        }
                        foreach (var itm in signalList)
                        {
                            itm.ValueList.Add(a.GetFreqPointValue(itm.Freq, -100));
                        }
                    });
                    foreach (var itm in signalList)
                    {
                        itm.WatchCount = itm.ValueList.Count;
                        itm.OverCount = itm.ValueList.Where(a => a >= ts.Threshold).Count();
                        itm.Occupy = Math.Round((100 * ((double)itm.OverCount / (double)itm.WatchCount)), 1);
                        itm.StatusMark = "正常";
                        if (itm.OverCount >= ts.HoldSecond) itm.StatusMark = "超标";
                    }
                    signalList = signalList.Where(a => a.IsWhite || (a.WatchCount >= ts.HoldSecond && a.MaxValue >= ts.Threshold)).ToList();
                    signalList = signalList.OrderBy(a => a.IsWhite ? 0 : 1).ThenByDescending(a => a.Occupy).ToList();
                    {
                        string freqLabel = $"[{freqStart},{freqStop}]频段,在{startTime}到{endTime}期间，{freqlist.Count}幅频谱最大值保持图";
                        string imgPath = TaskUtil.FreqValues2ImgFile(freqModule.Freq.Freqs, freqModule.Freq.FreqValues, freqLabel, Color.Green);
                        var nImg = doc.AddImage(imgPath);
                        var pic = nImg.CreatePicture();
                        pic.Width = 600;
                        pic.Height = 200;
                        doc.InsertParagraph().AppendPicture(pic);
                    }

                    doc.InsertParagraph().AppendLine("3.信号占用度直方图").Font("宋体").FontSize(20);
                    if (signalList == null || signalList.Count == 0)
                    {
                        doc.InsertParagraph().AppendLine($" ● 任务期间没有出现任信号").Font("宋体").FontSize(12);
                        doc.Save();
                        return new NormalResponse(true, filePath);
                    }
                    {
                        string imgPath = TaskUtil.SignalList2ImgFile(signalList);
                        var nImg = doc.AddImage(imgPath);
                        var pic = nImg.CreatePicture();
                        pic.Width = 600;
                        pic.Height = 200;
                        doc.InsertParagraph().AppendPicture(pic);
                    }
                    doc.InsertParagraph().AppendLine("4.信号信息统计表").Font("宋体").FontSize(20);
                    DocTable tab = new DocTable(doc, signalList.Count, new string[]
                     {
                        "序号","频率(MHz)","最大值","最小值","平均值","状态","占用度","是否关注"
                     });
                    for (int i = 0; i < signalList.Count; i++)
                    {
                        SignalInfo s = signalList[i];
                        object[] obj = new object[]
                        {
                            i+1,s.Freq.ToString("0.00000"),s.MaxValue.ToString("0.0"),s.MinValue.ToString("0.0"),
                            s.AvgValue.ToString("0.0"),s.StatusMark,$"{s.Occupy} %",s.IsWhite?"是":""
                        };
                        tab.InsertRow(obj);
                    }
                    doc.InsertParagraph().InsertTableAfterSelf(tab.tab);
                    doc.InsertParagraph().AppendLine("5.信号频谱时序图").Font("宋体").FontSize(20);
                    foreach (var itm in signalList)
                    {
                        string focus = itm.IsWhite ? "[关注]" : "";
                        string label = $"{itm.Freq.ToString("0.00000")}MHz{focus},{taskInfo.StartTime} 到 {taskInfo.EndTime} 时序图，占用度 {itm.Occupy}";
                        doc.InsertParagraph().AppendLine($" ● {label}").Font("宋体").FontSize(12);
                        string imgPath = TaskUtil.FreqTimeValues2ImgFile(itm.ValueList.ToArray(), label, ts.Threshold, Color.Blue);
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
            return TaskKind.SignalWatch;
        }

        //Task<NormalResponse> ITaskReportService.DoReport(UserTaskInfo taskInfo, string deviceId, string dirPath)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
