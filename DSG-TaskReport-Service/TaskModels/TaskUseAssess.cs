using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DSG_TaskReport_Service.TaskModels
{
    public class TaskUseAssess
    {
        /// <summary>
        /// 建模时间
        /// </summary>
        public int ModuleSecond { get; set; }
        /// <summary>
        /// 超标门限 
        /// </summary>
        public int MaxPercent { get; set; }
        /// <summary>
        /// 故障门限
        /// </summary>
        public int MinPercent { get; set; }
        /// <summary>
        /// 持续时间
        /// </summary>
        public int HoldSecond { get; set; }
        /// <summary>
        /// 关注频点
        /// </summary>
        public List<double> WatchPoints { get; set; }


    }
}
