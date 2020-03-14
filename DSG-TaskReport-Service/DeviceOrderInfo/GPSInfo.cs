using DSG_TaskReport_Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DSGServerCore
{
    public class GPSInfo
    {
        public string Time { get; set; }
        public double Lng { get; set; }
        public double Lat { get; set; }
        public GPSInfo()
        {

        }
        public GPSInfo(double lng,double lat)
        {
            this.Lng = lng;
            this.Lat = lat;
            this.Time = TimeUtil.Now().ToString("yyyy-MM-dd HH:mm:ss");
        }
        public GPSInfo Copy()
        {
            if (this == null) return null;
            return (GPSInfo)this.MemberwiseClone();
        }
    }
}
