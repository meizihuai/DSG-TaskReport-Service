using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DSGServerCore
{
    public class DeviceMsgList
    {
        public int CurrentMsgId { get; set; }
        public int Count { get; set; }
        public List<DeviceMsgInfo> Data { get; set; }
        public DeviceMsgList(List<DeviceMsgInfo> data,int currentMsgId)
        {
            this.CurrentMsgId = currentMsgId;
            this.Data = data;
            this.Count = 0;
            if (data != null)
            {
                this.Count = data.Count;
            }
           
        }
    }
    public class DeviceMsgInfo
    {
        public int ID { get; set; }
        public string DateTime{ get; set; }
        public string Day { get; set; }
        public string DeviceID { get; set; }
        public string Func { get; set; }
        public object Data { get; set; }
    }
}
