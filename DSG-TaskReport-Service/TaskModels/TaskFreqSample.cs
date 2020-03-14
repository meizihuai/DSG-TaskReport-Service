using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSGServerCore
{
    /// <summary>
    /// 频谱取样
    /// </summary>
    class TaskFreqSample
    {
        public decimal FreqStart { get; set; }
        public decimal FreqStop { get; set; }
        public decimal FreqStep { get; set; }
        /// <summary>
        /// 取样时间间隔，单位秒
        /// </summary>
        public int Interval { get; set; }
    }
}
