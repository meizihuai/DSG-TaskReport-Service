using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSG_TaskReport_Service.Classes
{
    public static class WhiteRadioUtils
    {
        private static List<double> whiteFreqList = null;
        private static object listlock = new object();
        private static List<double> GetWhiteFreqList()
        {
            if (whiteFreqList == null)
            {
                lock (listlock)
                {
                    if (whiteFreqList == null)
                    {
                        using (var db = new DSGDbContext())
                        {
                            var rt = db.WhiteRadioTable.Select(a => new { a.Freq }).ToList();
                            whiteFreqList = new List<double>();
                            rt.ForEach(a => whiteFreqList.Add(a.Freq));
                        }
                    }

                }
              
            }
            return whiteFreqList;
        }
        public static bool IsWhite(double freq)
        {
            List<double> whitelist = GetWhiteFreqList();
            if (whitelist == null) return false;
            return whitelist.Where(a => a == freq).Count() > 0;
        }
    }
}
