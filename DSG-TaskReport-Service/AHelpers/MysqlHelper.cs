using DSG_TaskReport_Service.Model;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;


namespace Common
{
    public class MysqlHelper
    {
        private string connstr;
        private string dbName;
        public MysqlHelper(string connstr)
        {
            this.connstr = connstr;
            string[] sh = connstr.Split(';');
            foreach (var itm in sh)
            {
                if (!string.IsNullOrEmpty(itm))
                {
                    string[] ss = itm.Split('=');
                    if (ss[0].ToLower() == "database")
                    {
                        dbName = ss[1];
                    }
                }
            }
        }
        public MysqlHelper(string ip, int port, string usr, string pwd, string dbName)
        {
            connstr = $"server={ip};port={port};database={dbName};uid={usr};password={pwd};sslmode=none";
            this.dbName = dbName;
        }
        public int GetCount(string sql)
        {
            DataTable dt = GetDt(sql);
            if (dt == null || dt.Rows.Count == 0) return 0;
            string str = dt.Rows[0][0].ToString();
            int.TryParse(str, out int result);
            return result;
        }
        public List<TableFieldInfo> GetTableFields(string tableName)
        {
            string sql = $"select column_name as ColumnName,data_type as Type,COLUMN_TYPE as ColumnType,COLUMN_KEY as ColumnKey, EXTRA as ColumnExtra, ORDINAL_POSITION as Position,CHARACTER_MAXIMUM_LENGTH as MaxLength,NUMERIC_PRECISION as NumLength,NUMERIC_SCALE as NumScale from information_schema.columns  where table_schema='{dbName}' and table_name='{tableName}'";
            List<TableFieldInfo> list = Getlist<TableFieldInfo>(sql);
            if (list == null) return null;
            list = list.OrderBy(a => a.Position).ToList();
            return list;
        }
        public List<T> Getlist<T>(string sql)
        {
            DataTable dt = GetDt(sql);
            if (dt == null || dt.Rows.Count == 0) return null;
            if (typeof(T) == typeof(string) || typeof(T) == typeof(int) || typeof(T) == typeof(long) || typeof(T) == typeof(double) || typeof(T) == typeof(float))
            {
                List<T> strlist = new List<T>();
                foreach (DataRow row in dt.Rows)
                {
                    if (row[0] != null)
                        strlist.Add((T)row[0]);
                }
                return strlist;
            }
            string json = JsonConvert.SerializeObject(dt);
            try
            {
                List<T> list = JsonConvert.DeserializeObject<List<T>>(json);
                return list;
            }
            catch (Exception e)
            {
                return null;
            }
        }
        public DataTable GetDt(string sql)
        {
            try
            {
                DataTable dt = new DataTable();
                using (var conn = new MySqlConnection(connstr))
                {
                    conn.Open();
                    using (var command = new MySqlCommand(sql, conn))
                    {
                        command.CommandTimeout = 600 * 1000;
                        using (var adapter = new MySqlDataAdapter(command))
                        {
                            adapter.Fill(dt);
                        }
                    }
                }
                if (dt == null) dt = new DataTable();
                return dt;
            }
            catch (Exception e)
            {
                return null;
            }
        }
        public int Insert(DataTable dt, string tableName, bool useInsertTogether = true)
        {
            return useInsertTogether ? InsertDataTableTogether(dt, tableName) : InsertDataTableRowByRow(dt, tableName);
        }
        private int InsertDataTableTogether(DataTable dt, string tableName)
        {
            string sql = $"insert into {tableName}";
            string tableField = "";
            List<Type> colTypeList = new List<Type>();
            foreach (DataColumn col in dt.Columns)
            {
                colTypeList.Add(col.DataType);
                tableField = tableField + $"{col.ColumnName}" + ",";
            }
            tableField = tableField.Substring(0, tableField.Length - 1);
            int colCount = dt.Columns.Count;
            List<string> insertList = new List<string>();
            foreach (DataRow row in dt.Rows)
            {
                string str = "";
                for (int i = 0; i < colCount; i++)
                {
                    Type t = colTypeList[i];
                    if (t == typeof(int) || t == typeof(long) || t == typeof(float) || t == typeof(double) || t == typeof(decimal))
                    {
                        var obj = row[i];
                        string tmp = $"{(obj == null || obj is DBNull ? "null" : obj)}";
                        str = str + tmp + ",";
                    }
                    else
                    {
                        string value = row[i].ToString().Replace("'", @"\'");
                        str = str + $"'{value}'" + ",";
                    }
                }
                str = str.Substring(0, str.Length - 1);
                insertList.Add($"({str})");
            }
            if (insertList == null || insertList.Count == 0) return 0;
            sql = $"{sql} ({tableField}) values {string.Join(",", insertList)}";
            return Cmd(sql);
        }
        private int InsertDataTableRowByRow(DataTable dt, string tableName)
        {

            string tableField = "";
            List<Type> colTypeList = new List<Type>();
            foreach (DataColumn col in dt.Columns)
            {
                colTypeList.Add(col.DataType);
                tableField = tableField + $"{col.ColumnName}" + ",";
            }
            tableField = tableField.Substring(0, tableField.Length - 1);
            int colCount = dt.Columns.Count;
            List<string> insertList = new List<string>();
            string sql = $"insert into {tableName}";
            foreach (DataRow row in dt.Rows)
            {
                string str = "";
                for (int i = 0; i < colCount; i++)
                {
                    Type t = colTypeList[i];
                    if (t == typeof(int) || t == typeof(long) || t == typeof(float) || t == typeof(double) || t == typeof(decimal))
                    {
                        var obj = row[i];
                        string tmp = $"{(obj == null || obj is DBNull ? "null" : obj)}";
                        str = str + tmp + ",";
                    }
                    else
                    {
                        string value = row[i].ToString().Replace("'", @"\'");
                        str = str + $"'{value}'" + ",";
                    }
                }
                str = str.Substring(0, str.Length - 1);
                str = $"{sql} ({tableField}) values ({str})";
                insertList.Add(str);
            }
            int count = 0;
            using (var conn = new MySqlConnection(connstr))
            {
                conn.Open();
                foreach (var itm in insertList)
                {
                    using (var command = new MySqlCommand(itm, conn))
                    {
                        command.CommandTimeout = 600 * 1000;
                        try
                        {
                            count += command.ExecuteNonQuery();
                        }
                        catch (Exception e)
                        {
                            string str = e.ToString();
                            return 0;
                        }

                    }
                }
            }
            return count;
        }
        public int Cmd(string sql)
        {
            try
            {
                using (var conn = new MySqlConnection(connstr))
                {
                    conn.Open();
                    using (var command = new MySqlCommand(sql, conn))
                    {
                        command.CommandTimeout = 600 * 1000;
                        return command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception e)
            {
                string str = e.ToString();
                return 0;
            }
        }

    }
}
