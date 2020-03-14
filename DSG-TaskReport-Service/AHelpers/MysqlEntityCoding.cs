using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
namespace Common
{
    public class MysqlEntityCoding
    {
        private string namespaceName = "DSG_TaskReport_Service";
        /// <summary>
        /// 生成文件的路径
        /// </summary>
        private string filePath = "Entity";
        /// <summary>
        /// 数据库名称
        /// </summary>
        private string dbName = "QoEDataDB";
        /// <summary>
        /// 是否允许覆盖写入
        /// </summary>
        private bool isAllowOverwrite = false;
        //下划线转驼峰
        private bool isChangeLowerLine = true;
        //增加Column标识
        private bool isAddColumnAttribute = true;
        //首字母大写
        private bool isChangeFirst = true;
        //_t转Info
        private bool isT2Info = true;

        private MysqlHelper mysql = new MysqlHelper("39.100.133.230;port=3306;database=QoEDataDB;uid=root;password=Smart9080;sslmode=none");


        public MysqlEntityCoding(string namespaceName, string filePath, string dbName, string msyqlConnstr)
        {
            this.namespaceName = namespaceName;
            this.filePath = filePath;
            this.dbName = dbName;
            this.mysql = new MysqlHelper(msyqlConnstr);
        }

        /// <summary>
        /// 数据库字段类型对应c#数据类型
        /// </summary>
        private Dictionary<string, string> typeDik = new Dictionary<string, string>()
        {
            {"varchar","string"},
            {"text","string"},
            {"longtext","string"},
            {"int","int?"},
            {"double","double?"}
        };
        class TableInfo
        {
            public string column_name { get; set; }
            public string column_comment { get; set; }
            public string data_type { get; set; }
            public int position { get; set; }

        }


        public List<string> GetAllTable(string dbName)
        {
            string sql = $"select table_name from information_schema.tables where table_schema='{dbName}'";
            List<string> list = mysql.Getlist<string>(sql);
            return list;
        }
        public void WriteTableEntityToClassFile(params string[] tableNames)
        {
            tableNames.ToList().ForEach(a => WriteTableEntityToClassFile(a));
        }
        private void WriteTableEntityToClassFile(string tableName)
        {
            Console.WriteLine($"========生成表 {tableName}========");

            string sql = $"select column_name,column_comment,data_type,ORDINAL_POSITION as position from information_schema.columns where table_schema='{dbName}' and table_name='{tableName}'";
            List<TableInfo> list = mysql.Getlist<TableInfo>(sql);
            if (list == null)
            {
                Console.WriteLine("查询结果为空，文件生成失败");
                return;
            }
            list = list.OrderBy(a => a.position).ToList();
            string space = "    ";
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using System.ComponentModel.DataAnnotations.Schema;");
            sb.AppendLine();
            sb.AppendLine($"namespace {namespaceName}");
            sb.AppendLine("{");
            sb.AppendLine($"{space}[Table(\"{tableName}\")]");
            if (isT2Info) tableName = tableName.Replace("_t", "Info");
            if (isChangeFirst) tableName = UpperFirst(tableName);
            if (isChangeLowerLine) tableName = ChangeLowerline(tableName);
            string fileName = Path.Combine(filePath, tableName + ".cs");
            sb.AppendLine($"{space}public class {tableName}");
            sb.AppendLine($"{space}{{");
            list.ForEach(a =>
            {
                string typeTmp = GetType(a.data_type);
                string colName = a.column_name;
                if (colName.ToLower() == "id")
                {
                    typeTmp = "int";
                }
                if (isChangeFirst) colName = UpperFirst(colName);
                if (isChangeLowerLine) colName = ChangeLowerline(colName);
                if (isAddColumnAttribute)
                {
                    sb.AppendLine($"{space}{space}[Column(\"{a.column_name}\")]");
                }
                sb.AppendLine($"{space}{space}public {typeTmp} {colName} " + "{get;set;}");
                sb.AppendLine();
            });
            sb.AppendLine($"{space}}}");
            sb.AppendLine("}");

            if (File.Exists(fileName))
            {
                Console.WriteLine("监测到文件已存在");
                if (!isAllowOverwrite)
                {
                    Console.WriteLine("文件已存在，但不允许重复写入文件，退出生成");
                    return;
                }
                else
                {
                    Console.WriteLine("监测到文件已存在,将删除旧文件");
                    File.Delete(fileName);
                }
            }
            File.WriteAllText(fileName, sb.ToString());
            Console.WriteLine($"========生成文件 {tableName} 成功！！！========");
        }
        private string UpperFirst(string str)
        {
            string first = str.Substring(0, 1);
            first = first.ToUpper();
            str = first + str.Substring(1, str.Length - 1);
            return str;
        }
        private string ChangeLowerline(string str)
        {
            Match mt = Regex.Match(str, @"_(\w*)*");
            while (mt.Success)
            {
                var item = mt.Value;
                while (item.IndexOf('_') >= 0)
                {
                    string newUpper = item.Substring(item.IndexOf('_'), 2);
                    item = item.Replace(newUpper, newUpper.Trim('_').ToUpper());
                    str = str.Replace(newUpper, newUpper.Trim('_').ToUpper());
                }
                mt = mt.NextMatch();
            }
            return str;
        }
        private string GetType(string type)
        {
            if (typeDik.Keys.Contains(type))
            {
                return typeDik[type];
            }
            else
            {
                throw new Exception($"没有对应的db数据类型 {type}");
            }
        }
    }
}
