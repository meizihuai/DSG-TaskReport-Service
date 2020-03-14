using DSG_TaskReport_Service;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DSGServerCore
{
    [Table("whiteradiotable")]
    public class WhiteRadioInfo
    {
        public int ID{get;set;}
        public string DateTime{get;set;}
        public double Freq{get;set;}//MHz
        public double Width{get;set;}//KHz
        public string RadioName{get;set;}//电台名称
        public string Mark{get;set;}//备注
        public string RecordUsr{get;set;}
        public string Province{get;set;}
        public string City{get;set;}
        public string District{get;set;}
       
    }
}
