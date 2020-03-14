using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Reflection;
using System.ComponentModel;
using System.Security.Cryptography;

namespace DBUtility
{
    public class DButility
    {
        private static Random ran = new Random();
        private static readonly int TIMEOUT = 5000;
        #region DBNull类型转换
        public static long ToInt64(object value)
        {
            return (Convert.IsDBNull(value)) ? 0 : Convert.ToInt64(value);
        }

        public static int ToInt32(object value)
        {
            return (Convert.IsDBNull(value)) ? 0 : Convert.ToInt32(value);
        }

        public static short ToInt16(object value)
        {
            return (Convert.IsDBNull(value)) ? (short)0 : Convert.ToInt16(value);
        }

        public static string ToString(object value)
        {
            return value.ToString();
        }

        public static decimal ToDecimal(object value)
        {
            return (Convert.IsDBNull(value)) ? 0 : Convert.ToDecimal(value);
        }

        public static DateTime ToDateTime(object value)
        {
            return (Convert.IsDBNull(value)) ? DateTime.MinValue : Convert.ToDateTime(value);
        }
        #endregion

        #region AES 加密/解密
        /// <summary>
        ///  AES 加密
        /// </summary>
        /// <param name="str">明文（待加密）</param>
        /// <param name="key">密文</param>
        /// <returns></returns>
        public static string AesEncryptToHex(string str, string key)
        {
            if (string.IsNullOrEmpty(str)) return null;
            Byte[] toEncryptArray = Encoding.UTF8.GetBytes(str);   //命名空间： using System.Text;

            System.Security.Cryptography.RijndaelManaged rm = new System.Security.Cryptography.RijndaelManaged
            {
                Key = Encoding.UTF8.GetBytes(key),
                Mode = System.Security.Cryptography.CipherMode.ECB,
                Padding = System.Security.Cryptography.PaddingMode.PKCS7
            };

            System.Security.Cryptography.ICryptoTransform cTransform = rm.CreateEncryptor();
            Byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            var hex = BitConverter.ToString(resultArray, 0).Replace("-", string.Empty).ToLower();
            return hex;
        }

        /// <summary>
        ///  AES 解密
        /// </summary>
        /// <param name="str">明文（待解密）</param>
        /// <param name="key">密文</param>
        /// <returns></returns>
        public static string AesDecryptFromHex(string str, string key)
        {
            if (string.IsNullOrEmpty(str)) return null;
            var toEncryptArray = new byte[str.Length / 2];
            for (var x = 0; x < toEncryptArray.Length; x++)
            {
                var i = Convert.ToInt32(str.Substring(x * 2, 2), 16);
                toEncryptArray[x] = (byte)i;
            }

            //Byte[] toEncryptArray = Convert.FromBase64String(str);

            System.Security.Cryptography.RijndaelManaged rm = new System.Security.Cryptography.RijndaelManaged
            {
                Key = Encoding.UTF8.GetBytes(key),
                Mode = System.Security.Cryptography.CipherMode.ECB,
                Padding = System.Security.Cryptography.PaddingMode.PKCS7
            };

            System.Security.Cryptography.ICryptoTransform cTransform = rm.CreateDecryptor();
            Byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return Encoding.UTF8.GetString(resultArray);
        }
        #endregion

        #region 获取时间戳，取随机数
        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        public static long GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds);
        }

        /// <summary>
        /// 取随机数
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int GetRandom(int min, int max)
        {
            return ran.Next(min, max);
        }

        /// <summary>
        /// 获取当前本地时间戳
        /// </summary>
        /// <returns></returns>
        public static long GetCurrentTimeUnix()
        {
            TimeSpan cha = (DateTime.Now - TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)));
            long t = (long)cha.TotalSeconds;
            return t;
        }

        /// <summary>
        /// 时间戳转换为本地时间对象
        /// </summary>
        /// <returns></returns>
        public static DateTime GetUnixDateTime(long unix)
        {
            //long unix = 1500863191;
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            DateTime newTime = dtStart.AddSeconds(unix);
            return newTime;
        }

        /// <summary>
        /// Unicode转字符串
        /// </summary>
        /// <param name="source">经过Unicode编码的字符串</param>
        /// <returns>正常字符串</returns>
        public static string UnicodeToString(string source)
        {
            return new Regex(@"\\u([0-9A-F]{4})", RegexOptions.IgnoreCase | RegexOptions.Compiled).Replace(
                         source, x => string.Empty + Convert.ToChar(Convert.ToUInt16(x.Result("$1"), 16)));
        }

        /// <summary>
        /// Stream流转化为字符串
        /// </summary>
        /// <returns></returns>
        public static string StreamToString(Stream stream)
        {
            if (stream == null || stream.Length == 0)
            {
                return string.Empty;
            }
            StreamReader sr = new StreamReader(stream);
            return sr.ReadToEnd();
        }

        /// <summary>
        /// RequestForm转换成String, key=value格式
        /// </summary>
        /// <returns></returns>
        public static string RequestFormToString(NameValueCollection form)
        {
            if (form == null)
            {
                return null;
            }

            string strTemp = string.Empty;

            String[] requestItem = form.AllKeys;
            for (int i = 0; i < requestItem.Length; i++)
            {
                strTemp += requestItem[i] + "=" + form[requestItem[i]] + "&";
            }

            strTemp = strTemp.TrimEnd('&');
            return strTemp;
        }
        #endregion

        #region MD5加密

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sDataIn"></param>
        /// <param name="move">给空即可</param>
        /// <returns></returns>
        public static string GetMD532(string sDataIn, string move)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] bytValue, bytHash;
            bytValue = System.Text.Encoding.UTF8.GetBytes(move + sDataIn);
            bytHash = md5.ComputeHash(bytValue);
            md5.Clear();
            string sTemp = "";
            for (int i = 0; i < bytHash.Length; i++)
            {
                sTemp += bytHash[i].ToString("x").PadLeft(2, '0');
            }
            return sTemp;
        }

        public static string GetMD516(string ConvertString)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            string t2 = BitConverter.ToString(md5.ComputeHash(UTF8Encoding.Default.GetBytes(ConvertString)), 4, 8);
            t2 = t2.Replace("-", "");

            t2 = t2.ToLower();

            return t2;
        }
        #endregion


        /// <summary>
        /// 判断字符串全部是否是中文。返回true 则表示含有非中文
        /// </summary>
        /// <param name="str_chinese"></param>
        /// <returns></returns>
        public static bool IsChinese(string str_chinese)
        {
            if (string.IsNullOrEmpty(str_chinese)) return false;
            for (int i = 0; i < str_chinese.Length; i++)
            {
                Regex reg = new Regex(@"[\u4e00-\u9fa5]");
                if (!reg.IsMatch(str_chinese[i].ToString()))
                {
                    return false;
                }
            }

            return true;

        }
    }


    #region DataTable,DataSet,list集合 互转
    public class ModelHandler<T> where T : new()
    {
        #region DataSet=>List
        /// <summary>
        /// 填充对象列表：用DataSet的第一个表填充实体类
        /// </summary>
        /// <param name="ds">DataSet</param>
        /// <returns></returns>
        public List<T> FillModelByDataSet(DataSet ds)
        {
            if (ds == null || ds.Tables[0] == null || ds.Tables[0].Rows.Count == 0)
            {
                return null;
            }
            else
            {
                return FillModelByDataTable(ds.Tables[0]);
            }
        }
        #endregion

        #region DataTable=>List


        #region

        /// <summary>
        /// 类型枚举
        /// </summary>
        private enum ModelType
        {
            //值类型
            Struct,

            Enum,

            //引用类型
            String,

            Object,
            Else
        }

        private static ModelType GetModelType(Type modelType)
        {
            //值类型
            if (modelType.IsEnum)
            {
                return ModelType.Enum;
            }
            //值类型
            if (modelType.IsValueType)
            {
                return ModelType.Struct;
            }
            //引用类型 特殊类型处理
            if (modelType == typeof(string))
            {
                return ModelType.String;
            }
            //引用类型 特殊类型处理
            return modelType == typeof(object) ? ModelType.Object : ModelType.Else;
        }

        #endregion

        /// <summary>  
        /// 填充对象列表：用DataTable填充实体类
        /// </summary>  
        public List<T> FillModelByDataTable(DataTable dt)
        {
            if (dt == null || dt.Rows.Count == 0)
            {
                return null;
            }
            List<T> modelList = new List<T>();
            foreach (DataRow dr in dt.Rows)
            {
                //T model = (T)Activator.CreateInstance(typeof(T));  
                T model = new T();
                for (int i = 0; i < dr.Table.Columns.Count; i++)
                {
                    PropertyInfo propertyInfo = model.GetType().GetProperty(dr.Table.Columns[i].ColumnName);
                    if (propertyInfo != null && dr[i] != DBNull.Value)
                        propertyInfo.SetValue(model, dr[i], null);
                }

                modelList.Add(model);
            }
            return modelList;
        }



        /// <summary>
        /// datatable转换为List<T>集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <returns></returns>
        public static List<T> DataTableToList<T>(DataTable table)
        {
            var list = new List<T>();
            foreach (DataRow item in table.Rows)
            {
                list.Add(DataRowToModel<T>(item));
            }
            return list;
        }
        #endregion




        #region DataRow=>Model
        /// <summary>  
        /// 填充对象：用DataRow填充实体类
        /// </summary>  
        public T FillModelByDataRow(DataRow dr)
        {
            if (dr == null)
            {
                return default(T);
            }

            //T model = (T)Activator.CreateInstance(typeof(T));  
            T model = new T();

            for (int i = 0; i < dr.Table.Columns.Count; i++)
            {
                PropertyInfo propertyInfo = model.GetType().GetProperty(dr.Table.Columns[i].ColumnName);
                if (propertyInfo != null && dr[i] != DBNull.Value)
                    propertyInfo.SetValue(model, dr[i], null);
            }
            return model;
        }


        public static T DataRowToModel<T>(DataRow row)
        {
            T model;
            var type = typeof(T);
            var modelType = GetModelType(type);
            switch (modelType)
            {
                //值类型
                case ModelType.Struct:
                    {
                        model = default(T);
                        if (row[0] != null)
                            model = (T)row[0];
                    }
                    break;
                //值类型
                case ModelType.Enum:
                    {
                        model = default(T);
                        if (row[0] != null)
                        {
                            var fiType = row[0].GetType();
                            if (fiType == typeof(int))
                            {
                                model = (T)row[0];
                            }
                            else if (fiType == typeof(string))
                            {
                                model = (T)Enum.Parse(typeof(T), row[0].ToString());
                            }
                        }
                    }
                    break;
                //引用类型 c#对string也当做值类型处理
                case ModelType.String:
                    {
                        model = default(T);
                        if (row[0] != null)
                            model = (T)row[0];
                    }
                    break;
                //引用类型 直接返回第一行第一列的值
                case ModelType.Object:
                    {
                        model = default(T);
                        if (row[0] != null)
                            model = (T)row[0];
                    }
                    break;
                //引用类型
                case ModelType.Else:
                    {
                        //引用类型 必须对泛型实例化
                        model = Activator.CreateInstance<T>();
                        //获取model中的属性
                        var modelPropertyInfos = type.GetProperties();
                        //遍历model每一个属性并赋值DataRow对应的列
                        foreach (var pi in modelPropertyInfos)
                        {
                            //获取属性名称
                            var name = pi.Name;
                            if (!row.Table.Columns.Contains(name) || row[name] == null || row[name] == DBNull.Value) continue;
                            var piType = GetModelType(pi.PropertyType);
                            switch (piType)
                            {
                                case ModelType.Struct:
                                    {
                                        object value;
                                        if (!pi.PropertyType.Name.ToLower().Contains("nullable"))
                                            value = Convert.ChangeType(row[name], pi.PropertyType);
                                        else
                                            value = new NullableConverter(pi.PropertyType).ConvertFromString(row[name].ToString());
                                        pi.SetValue(model, value, null);
                                    }
                                    break;

                                case ModelType.Enum:
                                    {
                                        var fiType = row[0].GetType();
                                        if (fiType == typeof(int))
                                        {
                                            pi.SetValue(model, row[name], null);
                                        }
                                        else if (fiType == typeof(string))
                                        {
                                            var value = (T)Enum.Parse(typeof(T), row[name].ToString());
                                            if (value != null)
                                                pi.SetValue(model, value, null);
                                        }
                                    }
                                    break;

                                case ModelType.String:
                                    {
                                        var value = Convert.ChangeType(row[name], pi.PropertyType);
                                        pi.SetValue(model, value, null);
                                    }
                                    break;

                                case ModelType.Object:
                                    {
                                        pi.SetValue(model, row[name], null);
                                    }
                                    break;

                                case ModelType.Else:
                                    throw new Exception("不支持该类型转换");
                                default:
                                    throw new Exception("未知类型");
                            }
                        }
                    }
                    break;

                default:
                    model = default(T);
                    break;
            }
            return model;
        }

        #endregion
    }
    #endregion


}