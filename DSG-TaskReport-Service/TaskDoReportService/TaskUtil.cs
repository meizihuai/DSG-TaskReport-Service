using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

using System.Drawing;
using Newtonsoft;
using Newtonsoft.Json;
using DSGServerCore;
using FrmChartDemo.Charting;

namespace DSG_TaskReport_Service
{
    public static class TaskUtil
    {
        public static string FreqValues2ImgFile(double[] xx, double[] yy, string labelStr, Color Seriescolor)
        {
            Chart chart = new Chart(1000, 300);
            chart.ChartArea.AxisX.LineColor = Color.Gray;
            chart.ChartArea.AxisY.LineColor = Color.Gray;
            chart.MinY = -150;
            chart.MaxY = -30;
            chart.ChartArea.AxisY.GridCount = 6;
            //  chart.ChartArea.AxisX.GridCount = 6;
            Series ser = new Series();
            ser.Color = Color.Blue;
            ser.ChartType = SeriesChartType.Line;



            for (var i = 0; i <= yy.Count() - 1; i++)
                ser.Points.Add(new ChartPoint(xx[i], yy[i]));
            ser.Color = Seriescolor;
          

            string guid = Guid.NewGuid().ToString("N");
            string dirPath = $"Taskimages";
            CheckDir(dirPath);
            string tmpPath = Path.Combine(dirPath, guid + ".jpg");
            if (File.Exists(tmpPath))
                File.Delete(tmpPath);
            chart.Series.Add(ser);
         
            chart.SaveImage(tmpPath, System.Drawing.Imaging.ImageFormat.Jpeg);
            System.Drawing.Image img = System.Drawing.Image.FromFile(tmpPath);
            Bitmap bmp = new Bitmap(img);
            img.Dispose();
            File.Delete(tmpPath);
            Graphics g = Graphics.FromImage(bmp);
            g.DrawString(labelStr, new System.Drawing.Font("宋体", 12), Brushes.Red, 100, 20);
            g.Save();
            string path = Path.Combine(dirPath, guid + "(Edit).jpg");
            if (File.Exists(path))
                File.Delete(path);
            bmp.Save(path);
            return path;
        }
        public static string FreqTimeValues2ImgFile(double[] yy, string labelStr, double threshold, Color Seriescolor)
        {
            Chart chart = new Chart(1000, 300);
            chart.ChartArea.AxisX.LineColor = Color.Gray;
            chart.ChartArea.AxisY.LineColor = Color.Gray;
            chart.MinY = -150;
            chart.MaxY = -30;
            chart.ChartArea.AxisY.GridCount = 6;
            Series ser = new Series();
            ser.Color = Color.Blue;
            ser.ChartType = SeriesChartType.Line;

            Series ser2 = new Series();
            ser.Color = Color.Red;
            ser.ChartType = SeriesChartType.Line;

            for (var i = 0; i <= yy.Count() - 1; i++)
            {
                ser.Points.Add(new ChartPoint(i, yy[i]));
                ser2.Points.Add(new ChartPoint(i, threshold));
            }
                
            ser.Color = Seriescolor;

            chart.Series.Add(ser);
            chart.Series.Add(ser2);

            string guid = Guid.NewGuid().ToString("N");
            string dirPath = $"Taskimages";
            CheckDir(dirPath);
            string tmpPath = Path.Combine(dirPath, guid + ".jpg");
            if (File.Exists(tmpPath))
                File.Delete(tmpPath);
            chart.SaveImage(tmpPath, System.Drawing.Imaging.ImageFormat.Jpeg);
            System.Drawing.Image img = System.Drawing.Image.FromFile(tmpPath);
            Bitmap bmp = new Bitmap(img);
            img.Dispose();
            File.Delete(tmpPath);
            Graphics g = Graphics.FromImage(bmp);
            g.DrawString(labelStr, new System.Drawing.Font("宋体", 12), Brushes.Red, 100, 20);
            g.Save();
            string path = Path.Combine(dirPath, guid + "(Edit).jpg");
            if (File.Exists(path))
                File.Delete(path);
            bmp.Save(path);
            return path;
        }

        public static string SignalList2ImgFile(List<SignalInfo> signalList)
        {
            if (signalList == null || signalList.Count == 0) return "";
            double[] xx = new double[signalList.Count];
            double[] yy = new double[signalList.Count];
            for (int i = 0; i < signalList.Count; i++)
            {
                xx[i] = signalList[i].Freq;
                yy[i] = signalList[i].Occupy;
            }
            Chart chart = new Chart(1000, 300);
            chart.ChartArea.AxisX.LineColor = Color.Gray;
            chart.ChartArea.AxisY.LineColor = Color.Gray;
            chart.MinY = 0;
            chart.MaxY = 100;
            chart.ChartArea.AxisY.GridCount = 6;
            Series ser = new Series();
            ser.Color = Color.Blue;
            ser.ChartType = SeriesChartType.Column;


            for (var i = 0; i <= yy.Count() - 1; i++)
                ser.Points.Add(new ChartPoint(Math.Round(xx[i], 3), yy[i]));
            ser.Color = Color.Blue;        
            chart.Series.Add(ser);
            string guid = Guid.NewGuid().ToString("N");
            string dirPath = $"Taskimages";
            CheckDir(dirPath);
            string tmpPath = Path.Combine(dirPath, guid + ".jpg");
            if (File.Exists(tmpPath))
                File.Delete(tmpPath);
          
            chart.SaveImage(tmpPath, System.Drawing.Imaging.ImageFormat.Jpeg);
            System.Drawing.Image img = System.Drawing.Image.FromFile(tmpPath);
            Bitmap bmp = new Bitmap(img);
            img.Dispose();
            File.Delete(tmpPath);
            // Dim g As Graphics = Graphics.FromImage(img)
            // g.DrawString(labelStr, New Font("宋体", 12), Brushes.Red, 100, 20)
            // g.Save()
            string path = Path.Combine(dirPath, guid + "(Edit).jpg");
            if (File.Exists(path))
                File.Delete(path);
            bmp.Save(path);
            return path;
        }

        public static void CheckDir(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
        public static string GetDeviceNameByDeviceId(string deviceId)
        {
            string deviceNickName = deviceId;

            using (var db = new DSGDbContext())
            {
                var deviceInfo = db.DeviceTable.Where(a => a.DeviceID == deviceId).FirstOrDefault();
                if (deviceInfo != null)
                {
                    deviceNickName = deviceInfo.DeviceNickName;
                }
                return deviceNickName;
            }
        }
    }
}
