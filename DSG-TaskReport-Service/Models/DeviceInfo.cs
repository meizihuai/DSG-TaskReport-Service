using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace DSGServerCore
{
    [Table("devicetable")]
    public class DeviceInfo
    {
        public int ID { get; set; }
        public string DeviceID { get; set; }
        [NotMapped]
        public string Guid { get; set; }
        public string DeviceNickName { get; set; }
        public string Func { get; set; }//设备当前的任务类型
        public string Status { get; set; }//设备当前状态
        [NotMapped]
        public bool IsTasking { get; set; }
        [NotMapped]
        public UserTaskInfo UserTask { get; set; }
        [NotMapped]
        public  bool IsReadyForWork { get; set; }
        public string DeviceOrder { get; set; }//DeviceOrder设备最后一次接收的命令结构体，json
       // public string OrderObject { get; set; }//DeviceOrder设备最后一次接收的命令结构体的关联object，如离散扫描是基于频谱扫描命令的，这里记录离散扫描参数
        public string Kind { get; set; }
        public string OnlineTime { get; set; }
        public double Lng { get; set; }
        public double Lat { get; set; }
       // public string Address { get; set; }
        public string IP { get; set; }
        public int Port { get; set; }
        public string RunKind { get; set; }
        public string Province { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string Address { get; set; }
        //public string LocationAddress { get; set; }
        public string DSGWGDeviceId { get; set; }
        public double TodayFlow { get; set; }
        public double MonthFlow { get; set; }
        public double YearFlow { get; set; }
        [NotMapped]
        public bool IsOnline { get; set; }
    }
}
