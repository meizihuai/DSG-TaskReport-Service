using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Common;
using DSG_TaskReport_Service.Classes;
using DSG_TaskReport_Service.TaskModels;
using DSGServerCore;
using Newtonsoft.Json;
using Novacode;

namespace DSG_TaskReport_Service.TaskDoReportService
{
    public class UseAssessService : ITaskReportService
    {
        public Task<NormalResponse> DoReport(UserTaskInfo taskInfo, string deviceId, string dirPath)
        {
            return Task.Run(() =>
            {
                TaskUseAssess ts = JsonConvert.DeserializeObject<TaskUseAssess>(taskInfo.TaskCode);
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
                List<SignalInfo> signallist = new List<SignalInfo>();
                msgList.ForEach(a =>
                {
                    List<SignalInfo> finfos = JsonConvert.DeserializeObject<List<SignalInfo>>(a.DeviceMsg);
                    if (finfos != null)
                    {
                        foreach (var newItm in finfos)
                        {
                            bool isFind = false;
                            foreach (var itm in signallist)
                            {
                                if (itm.Freq == newItm.Freq)
                                {
                                    itm.MaxValue = itm.MaxValue > newItm.Value ? itm.MaxValue : newItm.Value;
                                    itm.MinValue = itm.MinValue < newItm.Value ? itm.MinValue : newItm.Value;
                                    itm.SumValue += itm.Value;
                                    itm.ValueList.Add(newItm.Value);
                                    if (itm.ModuleMinValue > newItm.ModuleMinValue) itm.ModuleMinValue = newItm.ModuleMinValue;
                                    if (itm.ModuleValue < newItm.ModuleValue) itm.ModuleValue = newItm.ModuleValue;
                                    isFind = true;
                                    break;
                                }
                            }
                            if (!isFind)
                            {
                                newItm.ValueList = new List<double>();
                                newItm.ValueList.Add(newItm.Value);
                                newItm.MaxValue = newItm.Value;
                                newItm.MinValue = newItm.Value;
                                newItm.SumValue = newItm.Value;
                                signallist.Add(newItm);
                            }
                        }
                    }
                });
                if (signallist.Count == 0)
                {
                    return new NormalResponse(false, $"{deviceNickName}:在执行任务期间，有记录{msgList.Count}条设备消息，但是没有任何有效的离散数据，无法制作报告");
                }
                foreach (var itm in signallist)
                {
                    itm.WatchCount = itm.ValueList.Count;
                    itm.OverCount = itm.ValueList.Where(a => a >= itm.ModuleValue).Count();
                    itm.LowerCount = itm.ValueList.Where(a => a <= itm.ModuleMinValue).Count();
                    itm.Occupy = Math.Round((100 * (double)itm.OverCount / (double)itm.WatchCount), 1);
                    itm.StatusMark = "正常";
                    itm.AvgValue = Math.Round(itm.SumValue / itm.WatchCount, 1);
                    if (itm.OverCount >= ts.HoldSecond) itm.StatusMark = "超标";
                    if (itm.LowerCount >= ts.HoldSecond) itm.StatusMark = "故障";
                    itm.IsWhite = (WhiteRadioUtils.IsWhite(itm.Freq));
                }
                string filePath = Path.Combine(dirPath, $"{deviceNickName}-{taskInfo.TaskNickName}.docx");
                using (DocX doc = DocX.Create(filePath))
                {
                    doc.InsertParagraph().AppendLine($"{taskInfo.TaskNickName}监测报告").Font("黑体").FontSize(25).Alignment = Alignment.center;
                    doc.InsertParagraph().AppendLine($"{deviceNickName}").Font("黑体").FontSize(25).Alignment = Alignment.center;
                    doc.InsertParagraph().AppendLine("1.概述").Font("宋体").FontSize(20);
                    doc.InsertParagraph()
                    .AppendLine($"     本次监测任务主要监测{JsonConvert.SerializeObject(ts.WatchPoints)}MHz频点列表,在{taskInfo.StartTime}到{taskInfo.EndTime}期间的可用评估信息")
                    .Font("宋体").FontSize(15);
                    doc.InsertParagraph().AppendLine("2.信号占用度统计图").Font("宋体").FontSize(20);
                    if (signallist == null || signallist.Count == 0)
                    {
                        doc.InsertParagraph().AppendLine($" ● 任务期间没有出现任何信号").Font("宋体").FontSize(12);
                        doc.Save();
                        return new NormalResponse(true, filePath);
                    }
                    {
                        string imgPath = TaskUtil.SignalList2ImgFile(signallist);
                        var nImg = doc.AddImage(imgPath);
                        var pic = nImg.CreatePicture();
                        pic.Width = 600;
                        pic.Height = 200;
                        doc.InsertParagraph().AppendPicture(pic);
                    }
                    doc.InsertParagraph().AppendLine("4.信号信息统计表").Font("宋体").FontSize(20);
                    DocTable tab = new DocTable(doc, signallist.Count, new string[]
                    {
                        "序号","频率(MHz)","最大值","最小值","平均值","状态","占用度","是否可用"
                    });
                    int maxOccupy = 25;
                    for (int i = 0; i < signallist.Count; i++)
                    {
                        SignalInfo s = signallist[i];
                        bool isCanUse = s.Occupy > maxOccupy;
                        object[] obj = new object[]
                        {
                            i+1,s.Freq.ToString("0.00000"),s.MaxValue.ToString("0.0"),s.MinValue.ToString("0.0"),
                            s.AvgValue.ToString("0.0"),s.StatusMark,$"{s.Occupy} %",isCanUse?"不可用":"可用"
                        };
                        
                        tab.InsertRow(obj);
                    }
                    doc.InsertParagraph().InsertTableAfterSelf(tab.tab);

                    //doc.InsertParagraph().AppendLine("5.任务期间预警信息").Font("宋体").FontSize(20);
                    //List<WarnInfo> warnlist = null;
                    //using (var db = new DSGDbContext())
                    //{
                    //    warnlist = db.WarnTable.Where(a => a.TaskID == taskInfo.ID && a.DeviceID == deviceId).ToList();
                    //}
                    //if (warnlist != null && warnlist.Count > 0)
                    //{
                    //    tab = new DocTable(doc, warnlist.Count, new string[]
                    //     {
                    //        "序号","预警时间","设备","频率(MHz)","场强(dBm)","预警类别","预警内容"
                    //     });
                    //    for (int i = 0; i < warnlist.Count; i++)
                    //    {
                    //        var itm = warnlist[i];
                    //        object[] obj = new object[]
                    //        {
                    //            i+1,itm.DateTime,itm.DeviceID,itm.Freq.ToString("0.00000"),
                    //            itm.Value.ToString("0.0"),itm.Type,itm.Body
                    //        };
                    //        tab.InsertRow(obj);
                    //    }
                    //    doc.InsertParagraph().InsertTableAfterSelf(tab.tab);
                    //}
                    //else
                    //{
                    //    doc.InsertParagraph().AppendLine($" ● 任务期间没有产生任何预警").Font("宋体").FontSize(12);
                    //}

                    doc.InsertParagraph().AppendLine("5.信号频谱时序图").Font("宋体").FontSize(20);
                    foreach (var itm in signallist)
                    {
                        string focus = itm.IsWhite ? "[关注]" : "";
                        string label = $"{itm.Freq.ToString("0.00000")}MHz{focus},{taskInfo.StartTime} 到 {taskInfo.EndTime} 时序图，占用度 {itm.Occupy}";
                        doc.InsertParagraph().AppendLine($" ● {label}").Font("宋体").FontSize(12);
                        string imgPath = TaskUtil.FreqTimeValues2ImgFile(itm.ValueList.ToArray(), label, itm.ModuleValue, Color.Blue);
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
            return TaskKind.UseAssess;
        }
    }
}
