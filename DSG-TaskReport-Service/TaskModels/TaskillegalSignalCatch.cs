using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSGServerCore 
{
    /// <summary>
    /// 非法信号捕获(包括黑广播捕获)
    /// </summary>
    class TaskillegalSignalCatch
    {
        public decimal FreqStart { get; set; }
        public decimal FreqStop { get; set; }
        public decimal FreqStep { get; set; }
        /// <summary>
        /// 幅差
        /// </summary>
        public int Range { get; set; }
        /// <summary>
        /// 带宽
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// 预警门限
        /// </summary>
        public int Threshold { get; set; }
        /// <summary>
        /// 出现次数
        /// </summary>
        public int IllegalCount { get; set; }
    }
}
