using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DSGServerCore
{
    [Table("usertasktable")]
    public class UserTaskInfo
    {
        public int ID{get;set;}
        public string DateTime{get;set;}
        public string Status { get; set; }
        public string UserName{get;set;}
        public string TaskKind{get;set;}//任务类型
        public string TaskNickName{get;set;}
        public string TaskBg{get;set;}  //任务背景
        public string DeviceID{get;set;}
        [NotMapped]
        public List<string> DeviceIDList { get; set; }
        public int DeviceCount { get; set; }
        public string DeviceName{get;set;}
        public string StartTime{get;set;}
        public string EndTime { get; set; }
        [NotMapped]
        public DateTime StartDateTime { get; set; }     
        [NotMapped]
        public DateTime EndDateTime { get; set; }
        public string PushEmailUserName{get;set;}
        public string TaskCode{get;set;} //任务详细参数代码
        public int StatusCode { get;set;} //任务状态代码，0=未开启，1=正在进行，2=制作报告，3=错误，100=已完成
        public string ResultReportUrl{get;set;}      
        public int IsClosed{get;set;}
        public string TaskDeviceKind{get;set;}
        public string Guid { get; set; }
        public string ReportLog { get; set; }
       
    }
}
