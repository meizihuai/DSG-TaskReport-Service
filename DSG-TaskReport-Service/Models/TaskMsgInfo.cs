using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DSGServerCore    
{
    [Table("devicemsgtable")]
    public class TaskMsgInfo
    {
        public int ID { get; set; }
        public string DateTime { get; set; }
        public string Day { get; set; }
        public int TaskID { get; set; }
        public string Func { get; set; }
        public string DeviceMsg { get; set; }
        public string DeviceID { get; set; }
    }
}
