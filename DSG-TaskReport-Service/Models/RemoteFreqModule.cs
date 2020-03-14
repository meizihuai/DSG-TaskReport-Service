using DSGServerCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSG_TaskReport_Service
{
    [Table("freqmoduletable")]
    public class RemoteFreqModule
    {
        public int ID { get; set; }
        public string DateTime { get; set; }
        public int TaskID { get; set; }
        public string DeviceID { get; set; }
        public string DeviceNickName { get; set; }
        public int KeepSecond { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public double Lng { get; set; }
        public double Lat { get; set; }
        public int Grid { get; set; }
        public double FreqStart { get; set; }
        public double FreqStop { get; set; }
        public double FreqStep { get; set; }
        public string FreqData { get; set; }
        public string Province { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string Address { get; set; }

        public static void Add(string deviceId,UserTaskInfo taskInfo,FreqModule freqModule)
        {
            Task.Run(()=>
            {
                if (string.IsNullOrEmpty(deviceId)) return;
                if (taskInfo == null) return;
                if (freqModule == null) return;
                using(var db =new DSGDbContext())
                {
                    var deviceInfo = db.DeviceTable.Where(a => a.DeviceID == deviceId).FirstOrDefault();
                    if (deviceInfo == null) return;
                    DateTime startDateTime = System.DateTime.Parse(taskInfo.StartTime);
                    DateTime stopDateTime = System.DateTime.Parse(taskInfo.EndTime);
                    RemoteFreqModule info = new RemoteFreqModule();
                    info.DateTime = TimeUtil.Now().ToString("yyyy-MM-dd HH:mm:ss");
                    info.TaskID = taskInfo.ID;
                    info.DeviceID = deviceId;
                    info.DeviceNickName = deviceInfo.DeviceNickName;
                    info.KeepSecond =(int) (stopDateTime - startDateTime).TotalSeconds;
                    info.StartTime = taskInfo.StartTime;
                    info.EndTime = taskInfo.EndTime;
                    info.Lng = deviceInfo.Lng;
                    info.Lat = deviceInfo.Lat;
                    info.Grid = 0;
                    info.FreqStart =(double) freqModule.Freq.FreqStart;
                    info.FreqStop = (double)freqModule.Freq.FreqStop;
                    info.FreqStep = (double)freqModule.Freq.FreqStep;
                    info.FreqData = freqModule.Freq.CompressFreqData(freqModule.Freq.FreqValues);
                    info.Province = deviceInfo.Province;
                    info.City = deviceInfo.City;
                    info.District = deviceInfo.District;
                    info.Address = deviceInfo.Address;
                    db.FreqModuleTable.Add(info);
                    db.SaveChanges();
                }
            });
        }

    }
}
