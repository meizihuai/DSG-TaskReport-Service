using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft;
using Newtonsoft.Json;

namespace DSGServerCore
{
    public class DiscreteScanInfo
    {

        //设备ID
        public string DeviceID { get; set; }
        public decimal FreqStart { get; set; }
        public decimal FreqStop { get; set; }
        //频点数
        public int PointCount { get; set; }
        //频点列表
        public decimal[] FreqPoints { get; set; }
        //场强列表  与频点列表一 一对应
        public double[] Values { get; set; }
        public static DiscreteScanInfo Parse(DiscreteScanInfo info, FreqBscanInfo freq)
        {

            if (freq == null) return null;
            if (info == null || info.FreqPoints == null || info.FreqPoints.Length == 0 || info.FreqStart != freq.FreqStart)
            {
                return null;
            }
            if (Math.Abs(info.FreqStop - freq.FreqStop) > (decimal)0.025)
            {
                Console.WriteLine(Math.Abs(info.FreqStop - freq.FreqStop).ToString());
                return null;
            }
            short[] value = freq.DeCompressFreqData(freq.FreqData);
            if (value == null)
            {
                Console.WriteLine("freq.DeCompressFreqData is null");
                return null;
            }
            double[] freqValue = new double[value.Length];
            decimal[] xx = new decimal[freqValue.Length];
            for (int i = 0; i < value.Length; i++)
            {
                double f = value[i];
                freqValue[i] = f * 0.1;
                xx[i] = freq.FreqStart + freq.FreqStep * i;
            }
            xx[xx.Length - 1] = freq.FreqStop;
            value = null;
            DiscreteScanInfo result = new DiscreteScanInfo();
            result.DeviceID = info.DeviceID;
            result.FreqStart = info.FreqStart;
            result.FreqStop = info.FreqStop;
            result.PointCount = info.PointCount;
            result.FreqPoints = info.FreqPoints;
            result.Values = new double[result.PointCount];
            int recordj = 0;
            for (int i = 0; i < result.FreqPoints.Length; i++)
            {
                decimal pointFreq = result.FreqPoints[i];
                decimal abs = decimal.MaxValue;
                for (int j = recordj; j < xx.Length; j++)
                {
                    recordj = j;
                    decimal newAbs = Math.Abs(pointFreq - xx[j]);
                    if (newAbs == 0)
                    {
                        result.Values[i] = freqValue[j];
                        break;
                    }
                    if (abs >= newAbs)
                    {
                        abs = newAbs;
                        result.Values[i] = freqValue[j];
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return result;
        }
      
    }
}
