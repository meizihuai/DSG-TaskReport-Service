using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSGServerCore
{
    class TaskKind
    {
        public static string FreqSample = "频谱取样";
        public static string SignalWatch = "台站监督";
        public static string SignalStatus = "状态预警";
        public static string UseAssess = "可用评估";
        public static string IllegalSignalCatch = "非法信号捕获";
        public static string RemoteFreqModule = "远端频谱建模";

        public static string DeviceIllegalWarn = "TSS设备的非法信号捕获命令";
        public static string DeviceFreqPointsWarn = "TZBQ设备WARN上报";
        public static int GetLevel(string taskKind)
        {
            if (string.IsNullOrEmpty(taskKind)) return 0;
            if (taskKind == FreqSample) return 9;
            if (taskKind == SignalWatch) return 9;
            if (taskKind == SignalStatus) return 9;
            if (taskKind == IllegalSignalCatch) return 9;
            if (taskKind == RemoteFreqModule) return 9;

            if (taskKind == DeviceIllegalWarn) return 1;
            if (taskKind == DeviceFreqPointsWarn) return 1;



            return 0;
        }
    }
}
