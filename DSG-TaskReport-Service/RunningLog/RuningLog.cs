using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DSG_TaskReport_Service.RunningLog
{
    public static class RuningLog
    {
        private static List<LogInfo> mLogList = new List<LogInfo>();
        private static object logListLock = new object();
        private static int maxListLength = 10000;
        public static int GetNewProgressBarLogIndex()
        {
            LogInfo info = new LogInfo()
            {
                ServiceId = "",
                Content = "",
                Time = TimeUtil.Now().ToString("yyyy-MM-dd HH:mm:ss"),
                Level = 0,
                Type = "Info"
            };
            lock (logListLock)
            {
                mLogList.Add(info);
                if (mLogList.Count >= maxListLength) mLogList.RemoveAt(0);
                return mLogList.Count - 1;
            }
        }
        public static void Log(string str, string serviceId = "", int level = 0, int logIndex = -1)
        {
            LogInfo info = new LogInfo()
            {
                ServiceId = serviceId,
                Content = str,
                Time = TimeUtil.Now().ToString("yyyy-MM-dd HH:mm:ss"),
                Level = level,
                Type = "Info"
            };
            lock (logListLock)
            {
                if (logIndex == -1)
                {
                    mLogList.Add(info);
                }
                else
                {
                    if (logIndex < mLogList.Count)
                    {
                        mLogList[logIndex] = info;
                    }
                }
                if (mLogList.Count >= maxListLength) mLogList.RemoveAt(0);
            }
        }

        public static List<LogInfo> GetLogs(int getCount = 0)
        {
            lock (logListLock)
            {
                if (getCount == 0) return mLogList.ToList();
                return mLogList.Skip(mLogList.Count - getCount).Take(getCount).ToList();
            }
        }


    }
}
