using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DSGServerCore
{
   public class FreqModule
    {
        public string ID { get; set; }
        public string DeviceID { get; set; }
        public string DateTime { get; set; }
        public string StopTime { get; set; }
        public int HoldSecond { get; set; }
        public bool IsOver { get; set; }
        public string Status { get; set; }
        public FreqBscanInfo Freq { get; set; }
        public FreqModule()
        {

        }
        public FreqModule(FreqBscanInfo freq, int second, string moduleDeviceId)
        {
            DateTime now = System.DateTime.Now;
            double freqStart =(double)freq.FreqStart;
            double freqStop = (double)freq.FreqStop;
            double freqStep = (double)freq.FreqStep;
            string deviceId = freq.DeviceID;
            if (string.IsNullOrEmpty(deviceId)) deviceId = moduleDeviceId;
            this.ID = now.ToString("yyyyMMdd_HHmmss_") + $"{freqStart}_{freqStop}_{freqStep}_{deviceId}";
            this.DeviceID = deviceId;
            this.DateTime = now.ToString("yyyy-MM-dd HH:mm:ss");
            if (second <= 0) second = 10;
            this.StopTime = now.AddSeconds(second).ToString("yyyy-MM-dd HH:mm:ss");
            this.HoldSecond = second;
            FreqBscanInfo info = new FreqBscanInfo();
            info.DeviceID = deviceId;
            info.FreqStart =(decimal) freqStart;
            info.FreqStop = (decimal)freqStop;
            info.FreqStep = (decimal)freqStep;
            info.FreqType = 2;
            this.Freq = info;
        }
        public FreqModule(double freqStart, double freqStop, double freqStep, string deviceId, int second)
        {
            DateTime now = System.DateTime.Now;
            this.ID = now.ToString($"yyyyMMdd_HHmmss_{freqStart}_{freqStop}_{freqStep}_{deviceId}");
            this.DeviceID = deviceId;
            this.DateTime = now.ToString("yyyy-MM-dd HH:mm:ss");
            if (second <= 0) second = 10;
            this.StopTime = now.AddSeconds(second).ToString("yyyy-MM-dd HH:mm:ss");
            FreqBscanInfo info = new FreqBscanInfo();
            info.DeviceID = deviceId;
            info.FreqStart =(decimal) freqStart;
            info.FreqStop = (decimal)freqStop;
            info.FreqStep = (decimal)freqStep;
            info.FreqType = 2;
            this.Freq = info;
        }
        public void Update(FreqBscanInfo info)
        {
            if (this.IsOver) return;
            DateTime now = System.DateTime.Now;
            DateTime stopTime = System.DateTime.Parse(this.StopTime);
            if (now >= stopTime) this.IsOver = true;
            if (info.FreqStart != this.Freq.FreqStart || info.FreqStop != this.Freq.FreqStop || info.FreqStep != this.Freq.FreqStep) return;
            if (info.FreqValues == null || info.Freqs == null) return;

            if (this.Freq.FreqValues == null)
            {
                this.Freq.Freqs = info.Freqs;
                this.Freq.FreqValues = info.FreqValues;
                return;
            }
            if (info.FreqValues.Length != this.Freq.FreqValues.Length || info.Freqs.Length != this.Freq.Freqs.Length) return;
            for (int i = 0; i < info.FreqValues.Length; i++)
            {
                double oldValue = this.Freq.FreqValues[i];
                double newValue = info.FreqValues[i];
                if (newValue > oldValue)
                    this.Freq.FreqValues[i] = newValue;
            }
        }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
        public static FreqModule Parse(string str)
        {
            try
            {
                FreqModule f = JsonConvert.DeserializeObject<FreqModule>(str);
                return f;
            }
            catch (Exception e)
            {
                string txt = e.ToString();
                return null;
            }
        }
    }
}
