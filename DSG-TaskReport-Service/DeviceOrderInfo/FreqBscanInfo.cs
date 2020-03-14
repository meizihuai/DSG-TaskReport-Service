using DSG_TaskReport_Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DSGServerCore
{
    public class FreqBscanInfo
    {
        //设备ID
        public string DeviceID { get; set; }
        //频谱时间，仅供本地制作报告使用
        public string DateTime { get; set; }
        //增益
        public int DbValue { get; set; }
        public string DeviceDHKind { get; set; }
        //起始频率 MHz
        public decimal FreqStart { get; set; }
        //结束频率 MHz
        public decimal FreqStop { get; set; }
        //频率步进 KHz
        public decimal FreqStep { get; set; }
        //横坐标，频谱点
        public double[] Freqs { get; set; }
        //计算后的真实场强值
        public double[] FreqValues { get; set; }
        //频谱点数
        public int FreqDataCount { get; set; }
        //频谱数据，Base64格式的GZIP
        public string FreqData { get; set; }
        public short[] Value { get; set; }
        //频谱地图经纬度信息
        public GPSInfo GpsInfo { get; set; }
        //频谱类型，0为实时普通频谱，1为最大值保持，2为差分谱
        public int FreqType { get; set; }
        public FreqBscanInfo()
        {

        }
        public FreqBscanInfo(string deviceId, decimal freqStart, decimal freqStop, decimal freqStep,short[] value)
        {
            this.DeviceID = deviceId;
            this.FreqStart = freqStart;
            this.FreqStop = freqStop;
            this.FreqStep = freqStep;
            if (value != null)
            {
                this.FreqDataCount = value.Length;
                this.FreqData = CompressFreqData(value);
            }
            this.Value = null;
        }
        public FreqBscanInfo(string deviceId, decimal freqStart, decimal freqStop, decimal freqStep, string base64)
        {
            this.DeviceID = deviceId;
            this.FreqStart = freqStart;
            this.FreqStop = freqStop;
            this.FreqStep = freqStep;
            if (base64 != null)
            {
                this.FreqData = "";
                this.Value = DeCompressFreqData(base64);
                if(Value!=null) this.FreqDataCount = this.Value.Length;
            }
        }
        //将接收到服务器的base64压缩的频谱，还原
        public void DeValue()
        {
            short[] value = this.DeCompressFreqData(this.FreqData);
            this.FreqData = "";
            double[] freqValue = new double[value.Length];
            double[] xx = new double[freqValue.Length];
            for (int i = 0; i < value.Length; i++)
            {
                double f = value[i];
                freqValue[i] = f * 0.1;
                xx[i] = (double)FreqStart + (double)FreqStep * i;
            }
            xx[xx.Length - 1] = (double)FreqStop;
            this.Freqs = xx;
            this.FreqValues = freqValue;
        }
        public short[] DeCompressFreqData(string base64)
        {
            if (base64 == "") return null;
            try
            {
                byte[] zipBuffer = Convert.FromBase64String(base64);
                byte[] unzipBuffer = GZIPHelper.Decompress(zipBuffer);
                short[] value = new short[unzipBuffer.Length/2];
                int index = 0;
                for (int i = 0; i < unzipBuffer.Length; i+=2)
                {
                    value[index] = BitConverter.ToInt16(unzipBuffer, i);
                    index++;
                }
                return value;
            }
            catch (Exception)
            {
                return null;
            }          
        }
        public string CompressFreqData(short[] value)
        {
            if (value == null) return "";
            byte[] by = new byte[value.Length*2];
            for(int i = 0; i < value.Length; i++)
            {
                short val = value[i];
                byte[] tmp = BitConverter.GetBytes(val);
                by[i * 2] = tmp[0];
                by[i * 2+1] = tmp[1];
            }
            byte[] zipbuffer = GZIPHelper.Compress(by);
            string base64 = Convert.ToBase64String(zipbuffer);
            return base64;
        }
        public string CompressFreqData(double[] value)
        {
            if (value == null) return "";
            byte[] by = new byte[value.Length * 2];
            for (int i = 0; i < value.Length; i++)
            {
                short val = (short)(value[i] * 10);
                byte[] tmp = BitConverter.GetBytes(val);
                by[i * 2] = tmp[0];
                by[i * 2 + 1] = tmp[1];
            }
            byte[] zipbuffer = GZIPHelper.Compress(by);
            string base64 = Convert.ToBase64String(zipbuffer);
            return base64;
        }

        public FreqBscanInfo Copy()
        {
            if (this == null) return null;
            FreqBscanInfo cp = (FreqBscanInfo)this.MemberwiseClone();
            if (GpsInfo != null) cp.GpsInfo = this.GpsInfo.Copy();
            return cp;
        }
        //减法，差分用
        public static FreqBscanInfo operator -(FreqBscanInfo newFreq, FreqBscanInfo moduleFreq)
        {
            if (newFreq.FreqStart != moduleFreq.FreqStart || newFreq.FreqStop != moduleFreq.FreqStop || newFreq.FreqStep != moduleFreq.FreqStep) return null;
            if (newFreq.Value == null || newFreq.Freqs == null) return null;
            FreqBscanInfo info = new FreqBscanInfo();
            info.FreqStart = newFreq.FreqStart;
            info.FreqStop = newFreq.FreqStop;
            info.FreqStep = newFreq.FreqStep;
            info.FreqValues = newFreq.FreqValues.ToArray();
            info.Freqs = newFreq.Freqs.ToArray();
            for (int i = 0; i < info.Value.Length; i++)
            {
                info.FreqValues[i] = info.FreqValues[i] - moduleFreq.FreqValues[i];
            }
            return info;
        }

        public double GetFreqPointValue(double freq, double selfValue)
        {
            if (this.Freqs == null || this.FreqValues == null || this.FreqStart > (decimal)freq || this.FreqStop < (decimal)freq) return selfValue;
            int index = 0;
            double freqToLeft = freq - (double)FreqStart;
            index = (int)Math.Floor(freqToLeft / (double)FreqStep);
            if (index < Freqs.Length - 1)
            {
                double index_freq_left_abs = Math.Abs(Freqs[index] - freq);
                double index_freq_right_abs = Math.Abs(Freqs[index + 1] - freq);
                index = (index_freq_left_abs > index_freq_right_abs ? index + 1 : index);
            }
            if (index >= FreqValues.Length) index = FreqValues.Length - 1;
            if (index < 0) index = 0;
            return FreqValues[index];
        }


        //public double GetFreqPointValueBak(double freq, double selfValue)
        //{
        //    if (this.Freqs == null || this.FreqValues == null || this.FreqStart > (decimal)freq || this.FreqStop < (decimal)freq) return selfValue;
        //    double abs = double.MaxValue;
        //    double resultValue = selfValue;
        //    int index = 0;
        //    if (freq <= (double)this.FreqStart) index = 0;
        //    if (freq >= (double)this.FreqStop) index = Freqs.Length-1;
        //    double dindex = (freq - (double)this.FreqStart) /(double) this.FreqStep;
        //    index = (int)dindex;
        //    {
        //        double freqBefore = Freqs[index - 1];
        //        double freqIndex = Freqs[index];
        //        double freqAfter = Freqs[index+1];
        //    }
        //    for (int i =index-1; i <=index+1; i++)
        //    {
        //        if(i>=0 && i < Freqs.Length)
        //        {
        //            double pointFreq = Freqs[i];
        //            double newAbs = Math.Abs(pointFreq - freq);
        //            if (newAbs == 0)
        //            {
        //                return FreqValues[i];
        //            }
        //            if (abs >= newAbs)
        //            {
        //                abs = newAbs;
        //                resultValue = FreqValues[i];
        //            }
        //            else
        //            {
        //                return resultValue;
        //            }
        //        }            
        //    }
        //    return resultValue;
        //}

        //public double GetFreqPointValueBak(double freq, double selfValue)
        //{
        //    if (this.Freqs == null || this.FreqValues == null || this.FreqStart > (decimal)freq || this.FreqStop < (decimal)freq) return selfValue;
        //    double abs = double.MaxValue;
        //    double resultValue = selfValue;
        //    for (int i = 0; i < Freqs.Length; i++)
        //    {
        //        double pointFreq = Freqs[i];
        //        double newAbs = Math.Abs(pointFreq - freq);
        //        if (newAbs == 0)
        //        {
        //            return FreqValues[i];
        //        }
        //        if (abs >= newAbs)
        //        {
        //            abs = newAbs;
        //            resultValue = FreqValues[i];
        //        }
        //        else
        //        {
        //            return resultValue;
        //        }
        //    }
        //    return resultValue;
        //}

    }
}
