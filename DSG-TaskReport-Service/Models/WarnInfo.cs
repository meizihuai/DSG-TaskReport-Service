using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DSGServerCore
{
    [Table("newwarntable")]
    public class WarnInfo
    {
        public int ID{get;set;}
        public string DateTime{get;set;}
        public int TaskID { get; set; }
        public string DeviceID{get;set;}
        public double Lng{get;set;}
        public double Lat{get;set;}
        public string Type{get;set;}
        public int IsClosed{get;set;}
        public string Mark{get;set;}
        public string Body{get;set;}
        public string Province{get;set;}
        public string City{get;set;}
        public string District{get;set;}
        public double Freq{get;set;}
        public double Width {get;set;}
        public double Value {get;set;}
        public int Times{get;set;}

      
    }
}
