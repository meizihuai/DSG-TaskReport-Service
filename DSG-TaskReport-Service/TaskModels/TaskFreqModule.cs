using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSG_TaskReport_Service
{
    public class TaskFreqModule
    {
        /// <summary>
        /// 频率开始 MHz
        /// </summary>
        public decimal FreqStart { get; set; }
        /// <summary>
        /// 频率结束 MHz
        /// </summary>
        public decimal FreqStop { get; set; }
        /// <summary>
        /// 频谱步进 KHz
        /// </summary>
        public decimal FreqStep { get; set; }
    }
}
