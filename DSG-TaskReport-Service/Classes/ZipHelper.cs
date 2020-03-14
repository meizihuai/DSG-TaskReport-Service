using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;

namespace DSG_TaskReport_Service
{
    class ZipHelper
    {
        public static void Zip(string dirPath, string outputFilePath)
        {
            File.Delete(outputFilePath);
            DirectoryInfo df = new DirectoryInfo(dirPath);
            if (!df.Exists) return;
            List<string> fileNames = new List<string>();
            if (df.GetFiles().Length == 0) return;
            foreach (var item in df.GetFiles())
            {
                if (item.Extension == ".zip" || item.Extension == ".rar") continue;
                fileNames.Add(item.FullName);
            }
            // 创建并添加被压缩文件
            using (FileStream zipFileToOpen = new FileStream(outputFilePath, FileMode.Create))
            using (ZipArchive archive = new ZipArchive(zipFileToOpen, ZipArchiveMode.Create))
            {
                System.Reflection.Assembly assemble = System.Reflection.Assembly.GetExecutingAssembly();
                foreach (var f in fileNames)
                {
                    string filename = System.IO.Path.GetFileName(f);
                    ZipArchiveEntry readMeEntry = archive.CreateEntry(filename);
                    using (System.IO.Stream stream = readMeEntry.Open())
                    {
                        byte[] bytes = System.IO.File.ReadAllBytes(f);
                        stream.Write(bytes, 0, bytes.Length);
                    }
                }
            }
        }
    }
}
