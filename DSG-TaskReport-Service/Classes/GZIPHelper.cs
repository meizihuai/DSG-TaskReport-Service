using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSG_TaskReport_Service
{
    class GZIPHelper
    {
        public static byte[] Compress(byte[] data)
        {
            MemoryStream stream = new MemoryStream();
            GZipStream gZip = new GZipStream(stream, CompressionMode.Compress);
            gZip.Write(data, 0, data.Length);
            gZip.Close();
            return stream.ToArray();
        }
        public static byte[] Decompress(byte[] data)
        {
            MemoryStream stream = new MemoryStream();
            GZipStream gZip = new GZipStream(new MemoryStream(data), CompressionMode.Decompress);
            int n = 0;
            while (true)
            {
                byte[] by = new byte[409601];
                n = gZip.Read(by, 0, by.Length);
                if (n == 0)
                    break;
                stream.Write(by, 0, n);
            }
            gZip.Close();
            return stream.ToArray();
        }
    }
}
