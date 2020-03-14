using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Common
{
    public class Utils
    {
        public static bool IsInt(string str)
        {
            try
            {
                int.Parse(str);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public static bool IsNumber(string str)
        {
            try
            {
                double.Parse(str);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public static double GetPerDouble(double a, double b, int deci)
        {
            double c = a / b;
            c = c * 100;
            c = Math.Round(c, deci);
            if (c >= 100) return GetRandDouble(96, 100);
            if (c < 0) return 0;
            return c;
        }
        public static bool IsNumericRex(string value)
        {
            return Regex.IsMatch(value, @"^[+-]?/d*[.]?/d*$");
        }

        public static int IntNull2Int(int? i)
        {
            return i == null ? 0 : (int)i;
        }
        public static double DoubleNull2Double(double? d)
        {
            return d == null ? 0 : (double)d;
        }
        public static double IntNull2Double(int? i)
        {
            return i == null ? 0 : (double)i;
        }

        public static double GetRandDouble(double min, double max)
        {
            Guid temp = Guid.NewGuid();
            int guidseed = BitConverter.ToInt32(temp.ToByteArray(), 0);
            Random r = new Random(guidseed);
            return Math.Round((r.NextDouble() * (max - min) + min), 2);
        }
        public static int GetRandInt(int min, int max)
        {
            Guid temp = Guid.NewGuid();
            int guidseed = BitConverter.ToInt32(temp.ToByteArray(), 0);
            Random r = new Random(guidseed);
            return r.Next(min, max);
        }
    }
}
