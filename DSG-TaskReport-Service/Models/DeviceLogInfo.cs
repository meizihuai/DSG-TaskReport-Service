using DSG_TaskReport_Service;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DSGServerCore
{
    [Table("devicelogtable")]
    public class DeviceLogInfo
    {
        public int ID { get; set; }
        [Column("Time")]
        public string DateTime { get; set; }
        public string DeviceID { get; set; }
        public string DeviceNickName { get; set; }
        public string Address { get; set; }
        public string Log { get; set; }
        public string Result { get; set; }
        public string Status { get; set; }
        public static void AddLog(DeviceInfo device, string body, string result = "成功", string status = "在线")
        {
            if (device == null) return;
            Task.Run(() =>
            {
                try
                {
                    DeviceLogInfo log = new DeviceLogInfo();
                    log.DateTime = TimeUtil.Now().ToString("yyyy-MM-dd HH:mm:ss");
                    log.DeviceID = device.DeviceID;                   
                    log.DeviceNickName = device.DeviceNickName;
                    log.Address = device.Province + "," + device.City + "," + device.District;
                    if (string.IsNullOrEmpty(log.Address.Replace(",", ""))) log.Address = "";
                    log.Log = body;
                    log.Result = result;
                    log.Status = status;
                    using(DSGDbContext db=new DSGDbContext())
                    {
                        db.DeviceLogTable.Add(log);
                        db.SaveChanges();
                    }                 
                }
                catch (Exception e)
                {
                   // Module.Log(e.ToString());
                }
            });
        }
        public static void AddLog(string deviceId,string deviceNickName, string body, string result = "成功", string status = "在线")
        {
            if (string.IsNullOrEmpty(deviceId)) return;
            Task.Run(() =>
            {
                try
                {
                    DeviceLogInfo log = new DeviceLogInfo();
                    log.DeviceID = deviceId;
                    log.DeviceNickName = deviceNickName;
                    log.Log = body;
                    log.Result = result;
                    log.Status = status;
                    using (DSGDbContext db = new DSGDbContext())
                    {
                        db.DeviceLogTable.Add(log);
                        db.SaveChanges();
                    }
                }
                catch (Exception e)
                {

                }
            });
        }
    }
}
