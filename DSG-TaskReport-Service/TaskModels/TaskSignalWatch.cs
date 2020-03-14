using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSGServerCore
{
    /// <summary>
    /// 台站监督
    /// </summary>
    class TaskSignalWatch
    {
        public decimal FreqStart { get; set; }
        public decimal FreqStop { get; set; }
        public decimal FreqStep { get; set; }
        /// <summary>
        /// 关注频点
        /// </summary>
        public List<double> WatchPoints { get; set; }
        /// <summary>
        /// 幅差
        /// </summary>
        public int Range { get; set; }
        /// <summary>
        /// 带宽
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// 门限电平
        /// </summary>
        public int Threshold { get; set; }
        /// <summary>
        /// 持续时间
        /// </summary>
        public int HoldSecond { get; set; }

    }
}
