using DSGServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSG_TaskReport_Service
{
    /// <summary>
    /// 信号结构体，主要用于信号分析，包含信号属性，多为频谱Parse信号
    /// </summary>
    public class SignalInfo
    {
        public DateTime Time { get; set; }
        public double Freq { get; set; }
        public string Location { get; set; }
        public string SignalMark { get; set; }
        public string StatusMark { get; set; }
        public bool IsCanUse { get; set; }
        public double Value { get; set; }
        public List<double> ValueList { get; set; }
        public double ModuleValue { get; set; }
        public double ModuleMinValue { get; set; }
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
        public double AvgValue { get; set; }
        public double SumValue { get; set; }
        public string WatchTimespan { get; set; }
        public double Occupy { get; set; }
        public int WatchCount { get; set; }
        public int OverCount { get; set; }
        public int LowerCount { get; set; }
        //是否正在建模
        public bool IsBuildingModule { get; set; }
        //是否是建模过程中产生的信号
        public bool IsBuildingModuleSignal { get; set; }
        //是否需要建模
        public bool IsNeedBuiledModule { get; set; }
        //超标门限
        private double LevelValue { get; set; }
        //可用评估占用度门限
        private double LevelOccupy { get; set; }
        //是否在白名单之列
        public bool IsWhite { get; set; }
        //信号分离后，信号梯形 x轴值
        public double[] SigNalChartPointX { get; set; }
        //信号分离后，信号梯形 y轴值
        public double[] SigNalChartPointY { get; set; }

        public SignalInfo(double freq, double value, bool isNeedBuiledModule = false, string location = "")
        {
            this.Time = DateTime.Now;
            this.Freq = freq;
            if (string.IsNullOrEmpty(location)) location = "当地";
            this.Location = location;
            this.IsCanUse = true;
            this.Value = value;
            this.MinValue = value;
            this.MaxValue = value;
            this.AvgValue = value;
            this.SumValue = value;
            this.ModuleValue = value;
            this.ModuleMinValue = value;
            this.WatchTimespan = "00:00:00";
            this.Occupy = 0;
            this.WatchCount = 0;
            this.OverCount = 0;
            //设置超标门限以及可用评估占用度门限
            this.LevelValue = -85;
            this.LevelOccupy = 30;

            this.IsBuildingModule = true;
            this.IsNeedBuiledModule = isNeedBuiledModule;
            if (!isNeedBuiledModule)
            {
                this.IsBuildingModule = false;
            }
        }
        public void Update(double value, double withModuleValue = 0, bool isInDifferSigList = true)
        {
            if (value >= 0) return;

            this.Value = value;

            if (value > this.MaxValue) this.MaxValue = value;
            if (value < this.MinValue) this.MinValue = value;

            this.SumValue += this.Value;
            if (this.WatchCount == 0) this.WatchCount = 1;
            this.AvgValue = Math.Round(this.SumValue / this.WatchCount, 1);

            int maxAbsValue = 5;

            if (this.MaxValue >= 0) this.MaxValue = value;
            if (this.MinValue >= 0) this.MinValue = value;
            if (this.ModuleValue >= 0) this.ModuleValue = (this.AvgValue + maxAbsValue);
            if (this.ModuleMinValue >= 0) this.ModuleMinValue = this.AvgValue - maxAbsValue;

            this.SignalMark = "不明信号";
            if (this.IsBuildingModuleSignal)
            {
                this.SignalMark = "建模中信号";
            }
            if (!IsNeedBuiledModule)
            {
                //将超标模板设置为平均值+maxAbsValue
                this.ModuleValue = this.AvgValue + maxAbsValue;
                //如果超标模板比门限还小，则取门限值
                if (this.ModuleValue < LevelValue) this.ModuleValue = this.LevelValue;
                //将故障模板设置为平均值-maxAbsValue
                this.ModuleMinValue = this.AvgValue - maxAbsValue;
            }
            //如果超标模板比门限还小，则取门限值
            if (this.ModuleValue < LevelValue) this.ModuleValue = this.LevelValue;
            if (withModuleValue < 0)
            {
                this.ModuleValue = withModuleValue;
            }

            bool isThisTimeOver = false;
            if (!IsBuildingModule)
            {
                bool isNormal = true;
                if (value > (this.ModuleValue + maxAbsValue))
                {
                    this.StatusMark = "超标";
                    isNormal = false;
                }
                //if (value < (this.ModuleMinValue - maxAbsValue))
                //{
                //    this.StatusMark = "故障";
                //    isNormal = false;
                //}
                if (value > ModuleValue)
                {
                    isThisTimeOver = true;
                    this.StatusMark = "超标";
                    isNormal = false;
                }
                if (!isInDifferSigList)
                {
                    isNormal = true;
                    isThisTimeOver = false;
                }
                if (isNormal) this.StatusMark = "正常";
            }
            else
            {
                this.StatusMark = "建模中";
                this.ModuleValue = this.MaxValue;
                this.ModuleMinValue = this.MinValue;
            }
            TimeSpan ts = DateTime.Now - this.Time;
            if (IsBuildingModule && this.WatchCount >= 10)
            {
                IsBuildingModule = false;
            }
            this.WatchTimespan = $"{ts.Hours.ToString("00")}:{ts.Minutes.ToString("00")}:{ts.Seconds.ToString("00")}";
            this.WatchCount++;
            if (isThisTimeOver) this.OverCount++;
            if (!this.IsWhite)
            {
                if (this.OverCount == 0) this.OverCount = 1;
            }
            if (this.WatchCount > 0)
            {
                this.Occupy = Math.Round((double)this.OverCount * 100 / (double)this.WatchCount, 1);
            }
            this.IsCanUse = (this.Occupy < this.LevelOccupy);
        }
        public List<object> ToLV()
        {
            List<object> list = new List<object>();
            list.Add(Time.ToString("yyyy-MM-dd HH:mm:ss"));
            list.Add(Freq);
            list.Add(Location);
            list.Add(IsWhite ? "白名单" : "普通");
            list.Add(SignalMark);
            list.Add(StatusMark);
            list.Add(IsCanUse ? "是" : "否");
            list.Add(Value);
            list.Add(MinValue);
            list.Add(MaxValue);
            list.Add(AvgValue);
            list.Add(ModuleValue);
            list.Add(ModuleMinValue);
            list.Add(WatchTimespan);
            list.Add(Occupy.ToString("0.0"));
            list.Add(WatchCount);
            list.Add(OverCount);
            return list;
        }
        /// <summary>
        /// 普通信号分离   从一副频谱中分离出信号列表
        /// </summary>
        /// <param name="info">频谱数据</param>
        /// <param name="width">信号分离带宽</param>
        /// <param name="range">信号分离幅度</param>
        /// <param name="minValue">分离门限</param>
        /// <returns></returns>
        public static List<SignalInfo> Parse(FreqBscanInfo info, int width = 2, int range = 5, int minValue = -85)
        {
            if (info == null) return null;
            if (info.FreqValues == null || info.FreqValues.Length == 0) return null;
            double[] xx = info.Freqs;
            double[] yy = info.FreqValues;
            int count = xx.Length;
            if (count != yy.Length) return null;
            if (count <= width) return null;
            List<SignalInfo> list = new List<SignalInfo>();
            for (int i = 0; i < count; i++)
            {
                int left = i - width;
                int right = i + width;
                if (left < 0) left = 0;
                if (right > count - 1) right = count - 1;
                bool isContinue = false;
                if (minValue < 0)
                {
                    if (yy[i] < minValue) continue;
                }
                //向左比较
                for (int j = left; j < i; j++)
                {
                    if (yy[j] >= yy[j + 1]) { isContinue = true; continue; }
                }
                if (isContinue) continue;
                //向右比较
                for (int j = i + 1; j <= right; j++)
                {
                    if (yy[j - 1] <= yy[j]) { isContinue = true; continue; }
                }
                if (isContinue) continue;
                if (left == i - width)
                {
                    if (yy[i] - yy[left] < range) continue;
                }
                if (right == i + width)
                {
                    if (yy[i] - yy[right] < range) continue;
                }
                if (isContinue) continue;
                SignalInfo sig = new SignalInfo(xx[i], yy[i]);
                sig.SigNalChartPointX = new double[right - left + 1];
                sig.SigNalChartPointY = new double[sig.SigNalChartPointX.Length];
                for (int j = left; j <= right; j++)
                {
                    sig.SigNalChartPointX[j - left] = xx[j];
                    sig.SigNalChartPointY[j - left] = yy[j];
                }
                list.Add(sig);
            }
            return list;
        }
        /// <summary>
        /// 差分谱信号分离
        /// </summary>
        /// <param name="realFreq">真实频谱</param>
        /// <param name="differRealFreq">差分谱</param>
        /// <param name="width">信号分离带宽</param>
        /// <param name="range">信号分离幅度</param>
        /// <param name="minValue">分离门限</param>
        /// <param name="minDiffer">最小差值</param>
        /// <returns></returns>
        public static List<SignalInfo> ParseByDiffer(FreqBscanInfo realFreq, FreqBscanInfo differRealFreq, int width = 2, int range = 5, int minValue = -85, int minDiffer = 5)
        {
            if (realFreq == null) return null;
            if (realFreq.Value == null || realFreq.Value.Length == 0) return null;
            if (differRealFreq == null) return null;
            if (differRealFreq.Value == null || differRealFreq.Value.Length == 0) return null;

            if (realFreq.FreqStart != differRealFreq.FreqStart || realFreq.FreqStop != differRealFreq.FreqStop || realFreq.FreqStep != differRealFreq.FreqStep) return null;
            if (realFreq.Value == null || realFreq.Freqs == null) return null;

            double[] xx = realFreq.Freqs;
            double[] yy = realFreq.FreqValues;
            double[] dYY = differRealFreq.FreqValues;

            int count = xx.Length;
            if (count != yy.Length) return null;
            if (count <= width) return null;
            List<SignalInfo> list = new List<SignalInfo>();
            for (int i = 0; i < count; i++)
            {
                int left = i - width;
                int right = i + width;
                if (left < 0) left = 0;
                if (right > count - 1) right = count - 1;
                bool isContinue = false;
                if (minValue < 0)
                {
                    if (yy[i] < minValue) continue;
                }
                //向左比较
                for (int j = left; j < i; j++)
                {
                    if (yy[j] >= yy[j + 1]) { isContinue = true; continue; }
                }
                if (isContinue) continue;
                //向右比较
                for (int j = i + 1; j <= right; j++)
                {
                    if (yy[j - 1] <= yy[j]) { isContinue = true; continue; }
                }
                if (isContinue) continue;
                if (left == i - width)
                {
                    if (yy[i] - yy[left] < range) continue;
                }
                if (right == i + width)
                {
                    if (yy[i] - yy[right] < range) continue;
                }
                if (dYY[i] < minDiffer) { continue; }
                if (isContinue) continue;
                SignalInfo sig = new SignalInfo(xx[i], yy[i]);
                sig.SigNalChartPointX = new double[right - left + 1];
                sig.SigNalChartPointY = new double[sig.SigNalChartPointX.Length];
                for (int j = left; j <= right; j++)
                {
                    sig.SigNalChartPointX[j - left] = xx[j];
                    sig.SigNalChartPointY[j - left] = yy[j];
                }
                list.Add(sig);
            }
            return list;
        }
    }
}
