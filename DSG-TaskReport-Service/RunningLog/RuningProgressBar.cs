using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DSG_TaskReport_Service.RunningLog
{
    public class RuningProgressBar
    {
        // public int Value { get => Value; set => Value = (value > 100 ? 100 : (value < 0 ? 0 : value)); }
        private int value;
        private int logIndex;
        public int width = 50;
        public RuningProgressBar()
        {
            logIndex = RuningLog.GetNewProgressBarLogIndex();
        }
        public void SetValue(int value, string msg = "")
        {
            value = (value > 100 ? 100 : (value < 0 ? 0 : value));
            this.value = value;
            int pvWidth = (int)(width * ((double)value / 100));

            string pb = "";
            string space = "-";
            for (int i = 0; i < width; i++)
            {
                if (i < pvWidth - 1) pb = pb + "=";
                if (i == pvWidth - 1) pb = pb + (pvWidth < width ? ">" : "=");
                if (i > pvWidth - 1) pb = pb + space;
            }

            string str = $"[{pb}]{value}%{space}{msg}";
            RuningLog.Log(str, "", 0, logIndex);
        }
    }
}
