using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

using System.Drawing;
using Newtonsoft;
using Newtonsoft.Json;
using DSGServerCore;
using System.Diagnostics;
using Novacode;
using DSG_TaskReport_Service.Classes;
using Common;

namespace DSG_TaskReport_Service
{
    class IllegalSignalCatch : ITaskReportService
    {

        public Task<NormalResponse> DoReportBak(UserTaskInfo taskInfo, string deviceId, string dirPath)
        {
            return Task.Run(() =>
            {
                TaskillegalSignalCatch ts = JsonConvert.DeserializeObject<TaskillegalSignalCatch>(taskInfo.TaskCode);
                if (ts == null) return new NormalResponse(false, $"任务 {taskInfo.TaskNickName},id={taskInfo.ID},在制作报告时Json转换格式非法");
                string deviceNickName = TaskUtil.GetDeviceNameByDeviceId(deviceId);
                Module.Log("查询历史设备消息数量...");
                List<TaskMsgInfo> msglist = new List<TaskMsgInfo>();
                using (var db = new DSGDbContext())
                {
                    var rt = db.TaskMsgTable.Where(a => a.TaskID == taskInfo.ID && a.DeviceID == deviceId).ToList();
                    msglist = rt;
                }
                if (msglist == null || msglist.Count == 0) return new NormalResponse(false, $"{deviceNickName}:在执行任务期间，没有记录任何设备消息，无法制作报告");
                Module.Log($"查询到历史设备消息数量: {msglist.Count}");
                FreqModule freqModule = null;
                List<FreqBscanInfo> freqBscanInfolist = new List<FreqBscanInfo>();
                List<SignalInfo> signalList = new List<SignalInfo>();

                for (int m = 0; m < msglist.Count; m++)
                {

                    var msg = msglist[m];
                    FreqBscanInfo finfo = JsonConvert.DeserializeObject<FreqBscanInfo>(msg.DeviceMsg);
                    if (finfo == null) continue;
                    finfo.DeValue();
                    freqBscanInfolist.Add(finfo);
                    if (freqModule == null)
                    {
                        freqModule = new FreqModule(finfo, 0, "");
                    }
                    freqModule.Update(finfo);
                    List<SignalInfo> mlist = SignalInfo.Parse(finfo, ts.Width, ts.Range, ts.Threshold);
                    if (mlist == null || mlist.Count == 0)
                    {
                        continue;
                    }
                    mlist.ForEach(itm =>
                    {
                        bool flagExist = false;
                        itm.WatchCount = 1;
                        itm.OverCount = 1;
                        foreach (var signal in signalList)
                        {
                            if (signal.Freq == itm.Freq)
                            {
                                flagExist = true;
                                signal.WatchCount++;
                                signal.OverCount++;
                                if (itm.Value > signal.MaxValue)
                                {
                                    signal.MaxValue = itm.Value;
                                }
                                break;
                            }
                        }
                        if (!flagExist)
                        {
                            signalList.Add(itm);
                        }
                    });
                }
                int freqCount = freqBscanInfolist.Count;
                int msgCount = msglist.Count;
                msglist = null;
                if (msgCount == 0)
                {
                    return new NormalResponse(false, $"{deviceNickName}:在执行任务期间，有记录{msgCount}条设备消息，但是没有任何有效的频谱数据，无法制作报告");
                }
                // signalList = signalList.Where(a => a.OverCount >= ts.IllegalCount && a.MaxValue >= ts.Threshold).ToList();
                //signalList = signalList.Where(a => a.IsWhite || a.Occupy > 20).ToList();

                for (int j = 0; j < signalList.Count; j++)
                {
                    //  Module.Log($"计算进度: {j + 1} / {signalList.Count}");
                    var itm = signalList[j];
                    itm.ValueList = new List<double>();
                    foreach (var finfo in freqBscanInfolist)
                    {
                        itm.SumValue = 0;
                        itm.WatchCount = msgCount;
                        double value = finfo.GetFreqPointValue(itm.Freq, itm.Value);

                        itm.ValueList.Add(value);
                        if (value > itm.MaxValue) itm.MaxValue = value;
                        if (value < itm.MinValue) itm.MinValue = value;
                        itm.SumValue += value;

                        itm.AvgValue = Math.Round(itm.SumValue / itm.ValueList.Count, 1);
                        int sumCount = itm.ValueList.Count;
                        int overCount = itm.ValueList.Where(a => a >= ts.Threshold).Count();
                        double per = (double)overCount / (double)sumCount;
                        per = Math.Round(per * 100, 1);
                        itm.Occupy = per;
                        itm.StatusMark = "正常";
                        itm.OverCount = overCount;
                        if (itm.OverCount >= ts.IllegalCount) itm.StatusMark = "超标";
                        itm.IsWhite = (WhiteRadioUtils.IsWhite(itm.Freq));
                    }
                }

                signalList = signalList.Where(a => a.OverCount >= ts.IllegalCount && a.MaxValue >= ts.Threshold).ToList();
                signalList = signalList.Where(a => a.IsWhite || a.Occupy > 10).ToList();


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
                                            .AppendLine($"     本次监测任务主要监测[{ts.FreqStart.ToString("0.000")}MHz,{ts.FreqStop.ToString("0.000")}MHz]频段,在{taskInfo.StartTime}到{taskInfo.EndTime}期间的台站监督信息")
                                            .Font("宋体").FontSize(15);
                    doc.InsertParagraph().AppendLine("2.最大值保持图").Font("宋体").FontSize(20);


                    signalList = signalList.OrderBy(a => a.IsWhite ? 0 : 1).ThenBy(a => a.Freq).ToList();
                    {
                        string freqLabel = $"[{freqStart},{freqStop}]频段,在{startTime}到{endTime}期间，{freqCount}幅频谱最大值保持图";
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
                        doc.InsertParagraph().AppendLine($" ● 任务期间没有出现任何非法信号").Font("宋体").FontSize(12);
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
                        "序号","频率(MHz)","最大值","最小值","平均值","状态","信号","占用度"
                     });
                    for (int i = 0; i < signalList.Count; i++)
                    {
                        SignalInfo s = signalList[i];
                        object[] obj = new object[]
                        {
                            i+1,s.Freq.ToString("0.000"),s.MaxValue.ToString("0.0"),s.MinValue.ToString("0.0"),
                            s.AvgValue.ToString("0.0"),s.StatusMark,s.IsWhite?"合法":"待查",$"{s.Occupy} %"
                        };
                        tab.InsertRow(obj);
                    }
                    doc.InsertParagraph().InsertTableAfterSelf(tab.tab);

                    doc.InsertParagraph().AppendLine("5.任务期间预警信息").Font("宋体").FontSize(20);
                    List<WarnInfo> warnlist = null;
                    using (var db = new DSGDbContext())
                    {
                        warnlist = db.WarnTable.Where(a => a.TaskID == taskInfo.ID && a.DeviceID == deviceId).ToList();
                    }
                    if (warnlist != null && warnlist.Count > 0)
                    {
                        warnlist = warnlist.OrderBy(a => a.Freq).ToList();
                        tab = new DocTable(doc, warnlist.Count, new string[]
                                                    {
                            "序号","预警时间","设备","频率(MHz)","场强(dBm)","预警类别","预警内容"
                                                    });
                        for (int i = 0; i < warnlist.Count; i++)
                        {
                            var itm = warnlist[i];
                            object[] obj = new object[]
                            {
                                i+1,itm.DateTime,itm.DeviceID,itm.Freq.ToString("0.00"),
                                itm.Value.ToString("0.0"),itm.Type,itm.Body
                            };
                            tab.InsertRow(obj);
                        }
                        doc.InsertParagraph().InsertTableAfterSelf(tab.tab);
                    }
                    else
                    {
                        doc.InsertParagraph().AppendLine($" ● 任务期间没有产生任何预警").Font("宋体").FontSize(12);
                    }

                    doc.InsertParagraph().AppendLine("6.信号频谱时序图").Font("宋体").FontSize(20);
                    foreach (var itm in signalList)
                    {
                        string focus = itm.IsWhite ? "[关注]" : "";
                        string label = $"{itm.Freq.ToString("0.00")}MHz{focus},{taskInfo.StartTime} 到 {taskInfo.EndTime} 时序图，占用度 {itm.Occupy}";
                        doc.InsertParagraph().AppendLine($" ● {label}").Font("宋体").FontSize(12);
                        string imgPath = TaskUtil.FreqTimeValues2ImgFile(itm.ValueList.ToArray(), label, ts.Threshold, Color.Blue);
                        var nImg = doc.AddImage(imgPath);
                        var pic = nImg.CreatePicture();
                        pic.Width = 600;
                        pic.Height = 200;
                        doc.InsertParagraph().AppendPicture(pic);
                    }
                    doc.Save();
                    return new NormalResponse(true, filePath);
                }
            });
        }

        public Task<NormalResponse> DoReport(UserTaskInfo taskInfo, string deviceId, string dirPath)
        {
            return Task.Run(() =>
            {
                TaskillegalSignalCatch ts = JsonConvert.DeserializeObject<TaskillegalSignalCatch>(taskInfo.TaskCode);
                if (ts == null) return new NormalResponse(false, $"任务 {taskInfo.TaskNickName},id={taskInfo.ID},在制作报告时Json转换格式非法");
                string deviceNickName = TaskUtil.GetDeviceNameByDeviceId(deviceId);

                using (var db = new DSGDbContext())
                {
                    Module.Log("查询历史设备消息数量...");
                    var tmplist = db.TaskMsgTable.Where(a => a.TaskID == taskInfo.ID && a.DeviceID == deviceId).Select(a => new { a.ID }).ToList();
                    if (tmplist == null || tmplist.Count == 0)
                    {
                        Module.Log($"查询到历史设备消息数量: {"没有任何消息"}");
                        return new NormalResponse(false, $"{deviceNickName}:在执行任务期间，没有记录任何设备消息，无法制作报告");
                    }
                    Module.Log($"查询到历史设备消息数量: {tmplist.Count}");
                    List<int> idlist = new List<int>();
                    tmplist.ForEach(a => idlist.Add(a.ID));
                    tmplist = null;
                    Module.Log("开始逐条消息查询...");
                    //  Module.Log("[0%]  0 / 0 ");
                    int reportMsgCount = 0;
                    FreqModule freqModule = null;
                    List<SignalInfo> signalList = new List<SignalInfo>();
                    List<double> signalFreqlist = new List<double>();
                    int perCount = 500;
                    string lastWorkTimespan = "";
                    int msgCount = idlist.Count;
                    List<FreqBscanInfo> freqBscanInfolist = new List<FreqBscanInfo>();
                    for (int i = 0; i < idlist.Count; i += perCount)
                    {
                        // if (i >= perCount) break;
                        double dp = (double)(100 * (i + 1)) / (double)idlist.Count;
                        string logstr = $"[{dp.ToString("0.0")} %]  {i + 1} / {idlist.Count}";
                        // Module.Log(logstr);
                        var rt = db.TaskMsgTable.Where(a => a.TaskID == taskInfo.ID && a.DeviceID == deviceId)
                                  .OrderBy(a => a.ID).Skip(i).Take(perCount).ToList();
                        if (rt == null)
                        {
                            Module.Log(logstr + "  已查询结果，但结果为空");
                            continue;
                        }

                        if (string.IsNullOrEmpty(lastWorkTimespan))
                        {
                            logstr = logstr + $"  已查询结果，Count={rt.Count},计算数据...";
                        }
                        else
                        {
                            logstr = logstr + $"  已查询结果，Count={rt.Count},上一步耗时 {lastWorkTimespan}，当前信号数量: {signalList.Count} 正在进行Next...";
                        }
                        // Module.Log(logstr);
                        Stopwatch stopwatch = new Stopwatch();
                        stopwatch.Start();
                        for (int m = 0; m < rt.Count; m++)
                        {
                            string tmplogstr = logstr + $" [{m}/{rt.Count}]";
                            //  Module.Log(tmplogstr);
                            var msg = rt[m];
                            FreqBscanInfo finfo = JsonConvert.DeserializeObject<FreqBscanInfo>(msg.DeviceMsg);
                            if (finfo == null) continue;
                            reportMsgCount++;
                            finfo.DeValue();
                            if (freqModule == null)
                            {
                                freqModule = new FreqModule(finfo, 0, "");
                            }
                            freqModule.Update(finfo);
                            List<SignalInfo> mlist = SignalInfo.Parse(finfo, ts.Width, ts.Range, ts.Threshold);
                            if (mlist == null || mlist.Count == 0)
                            {
                                continue;
                            }

                            mlist.Where(a => !signalFreqlist.Contains(a.Freq)).ToList().ForEach(a =>
                             {
                                 signalFreqlist.Add(a.Freq);
                                 signalList.Add(a);
                             });

                            freqBscanInfolist.Add(finfo);

                        }
                        stopwatch.Stop();
                        lastWorkTimespan = stopwatch.Elapsed.ToString();
                        //  Module.Log(logstr + $",计算耗时 {stopwatch.Elapsed.ToString()},Next...");
                        Module.Log($"计算耗时 {stopwatch.Elapsed.ToString()}");
                    }

                    for (int j = 0; j < signalList.Count; j++)
                    {
                        //  Module.Log($"计算进度: {j + 1} / {signalList.Count}");
                        var itm = signalList[j];
                        itm.ValueList = new List<double>();
                        foreach (var finfo in freqBscanInfolist)
                        {
                            itm.SumValue = 0;
                            itm.WatchCount = msgCount;
                            double value = finfo.GetFreqPointValue(itm.Freq, itm.Value);

                            itm.ValueList.Add(value);
                            if (value > itm.MaxValue) itm.MaxValue = value;
                            if (value < itm.MinValue) itm.MinValue = value;
                            itm.SumValue += value;

                            itm.AvgValue = Math.Round(itm.SumValue / itm.ValueList.Count, 1);
                            int sumCount = itm.ValueList.Count;
                            int overCount = itm.ValueList.Where(a => a >= ts.Threshold).Count();
                            double per = (double)overCount / (double)sumCount;
                            per = Math.Round(per * 100, 1);
                            itm.Occupy = per;
                            itm.StatusMark = "正常";
                            itm.OverCount = overCount;
                            if (itm.OverCount >= ts.IllegalCount) itm.StatusMark = "超标";
                            itm.IsWhite = (WhiteRadioUtils.IsWhite(itm.Freq));
                        }
                    }



                    if (reportMsgCount == 0)
                    {
                        // Module.Log($"{deviceNickName}:在执行任务期间，有记录{idlist.Count}条设备消息，但是没有任何有效的频谱数据，无法制作报告");
                        return new NormalResponse(false, $"{deviceNickName}:在执行任务期间，有记录{idlist.Count}条设备消息，但是没有任何有效的频谱数据，无法制作报告");
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
                                                .AppendLine($"     本次监测任务主要监测[{ts.FreqStart.ToString("0.000")}MHz,{ts.FreqStop.ToString("0.000")}MHz]频段,在{taskInfo.StartTime}到{taskInfo.EndTime}期间的台站监督信息")
                                                .Font("宋体").FontSize(15);
                        doc.InsertParagraph().AppendLine("2.最大值保持图").Font("宋体").FontSize(20);


                        signalList = signalList.OrderBy(a => a.IsWhite ? 0 : 1).ThenBy(a => a.Freq).ToList();
                        {
                            string freqLabel = $"[{freqStart},{freqStop}]频段,在{startTime}到{endTime}期间，{idlist.Count}幅频谱最大值保持图";
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
                            doc.InsertParagraph().AppendLine($" ● 任务期间没有出现任何非法信号").Font("宋体").FontSize(12);
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
                        "序号","频率(MHz)","最大值","最小值","平均值","状态","信号","占用度"
                         });
                        for (int i = 0; i < signalList.Count; i++)
                        {
                            SignalInfo s = signalList[i];
                            object[] obj = new object[]
                            {
                            i+1,s.Freq.ToString("0.000"),s.MaxValue.ToString("0.0"),s.MinValue.ToString("0.0"),
                            s.AvgValue.ToString("0.0"),s.StatusMark,s.IsWhite?"合法":"待查",$"{s.Occupy} %"
                            };
                            tab.InsertRow(obj);
                        }
                        doc.InsertParagraph().InsertTableAfterSelf(tab.tab);

                        doc.InsertParagraph().AppendLine("5.任务期间预警信息").Font("宋体").FontSize(20);
                        List<WarnInfo> warnlist = null;
                        warnlist = db.WarnTable.Where(a => a.TaskID == taskInfo.ID && a.DeviceID == deviceId).ToList();
                        if (warnlist != null && warnlist.Count > 0)
                        {
                            warnlist = warnlist.OrderBy(a => a.Freq).ToList();
                            tab = new DocTable(doc, warnlist.Count, new string[]
                                                        {
                            "序号","预警时间","设备","频率(MHz)","场强(dBm)","预警类别","预警内容"
                                                        });
                            for (int i = 0; i < warnlist.Count; i++)
                            {
                                var itm = warnlist[i];
                                object[] obj = new object[]
                                {
                                i+1,itm.DateTime,itm.DeviceID,itm.Freq.ToString("0.00"),
                                itm.Value.ToString("0.0"),itm.Type,itm.Body
                                };
                                tab.InsertRow(obj);
                            }
                            doc.InsertParagraph().InsertTableAfterSelf(tab.tab);
                        }
                        else
                        {
                            doc.InsertParagraph().AppendLine($" ● 任务期间没有产生任何预警").Font("宋体").FontSize(12);
                        }

                        doc.InsertParagraph().AppendLine("6.信号频谱时序图").Font("宋体").FontSize(20);
                        foreach (var itm in signalList)
                        {
                            string focus = itm.IsWhite ? "[关注]" : "";
                            string label = $"{itm.Freq.ToString("0.00")}MHz{focus},{taskInfo.StartTime} 到 {taskInfo.EndTime} 时序图，占用度 {itm.Occupy}";
                            doc.InsertParagraph().AppendLine($" ● {label}").Font("宋体").FontSize(12);
                            string imgPath = TaskUtil.FreqTimeValues2ImgFile(itm.ValueList.ToArray(), label, ts.Threshold, Color.Blue);
                            var nImg = doc.AddImage(imgPath);
                            var pic = nImg.CreatePicture();
                            pic.Width = 600;
                            pic.Height = 200;
                            doc.InsertParagraph().AppendPicture(pic);
                        }
                        doc.Save();
                        return new NormalResponse(true, filePath);
                    }
                }


            });
        }

        public string GetTaskType()
        {
            return TaskKind.IllegalSignalCatch;
        }
    }
}
