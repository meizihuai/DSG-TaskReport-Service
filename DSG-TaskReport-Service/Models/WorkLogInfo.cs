using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DSGServerCore
{
    [Table("logtable")]
    public class WorkLogInfo
    {
        public int ID{get;set;}
        public string DateTime{get;set;}
        public string Usr{get;set;}
        public string Kind{get;set;}
        public string Content{get;set;}
        public string DeviceNickName{get;set;}
        public string DeviceID{get;set;}
        public string TaskNickName{get;set;}
        public string Cata{get;set;}
        public string RelateID{get;set;}
    }
}
