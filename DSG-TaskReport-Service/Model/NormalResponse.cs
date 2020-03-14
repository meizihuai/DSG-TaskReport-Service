using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace Common
{
    /// <summary>
    /// 一般返回格式,JSON格式
    /// </summary>
    public class NormalResponse
    {
        /// <summary>
        /// 处理结果，true:成功，false:失败
        /// </summary>
        public bool result { get; set; }

        public int statusCode { get; set; }
        /// <summary>
        /// 处理消息或处理过程
        /// </summary>
        public string msg { get; set; }
        /// <summary>
        /// 错误信息
        /// </summary>
        public string errmsg { get; set; }
        /// <summary>
        /// 数据，可能是string或者json或者json数组
        /// </summary>
        public object data { get; set; }
        public NormalResponse()
        {

        }

        public NormalResponse(object data)
        {
            this.result = true;
            NPCodeMsg status = NPCode.OK;
            this.statusCode = status.Code;
            this.msg = msg;
            this.data = data;
        }
        public NormalResponse(string msg = "", string errmsg = "", object data = null)
        {
            this.result = true;
            NPCodeMsg status = NPCode.OK;
            this.statusCode = status.Code;
            if (!string.IsNullOrEmpty(msg)) this.msg = msg;
            if (!string.IsNullOrEmpty(errmsg)) this.errmsg = errmsg;
            this.data = data;
        }
        public NormalResponse(bool result, string msg = "", string errmsg = "", object data = null)
        {
            this.result = result;
            NPCodeMsg status = result ? NPCode.OK : NPCode.Fail;
            this.statusCode = status.Code;
            if (!string.IsNullOrEmpty(msg)) this.msg = msg;
            if (!string.IsNullOrEmpty(errmsg)) this.errmsg = errmsg;
            this.data = data;
        }

        public NormalResponse(NPCodeMsg status, string msg = "", string errmsg = "", object data = null)
        {
            this.statusCode = status.Code;
            if (status.Code == NPCode.OK.Code)
            {
                this.msg = msg;
                this.result = true;
            }
            else
            {
                this.result = false;
                this.errmsg = status.Msg;
            }
            if (!string.IsNullOrEmpty(msg)) this.msg = msg;
            if (!string.IsNullOrEmpty(errmsg)) this.errmsg = errmsg;
            this.data = data;
        }
        public NormalResponse(string msg)
        {
            this.result = false;
            if (msg == "success")
            {
                this.result = true;
            }
            this.msg = msg;
            this.errmsg = "";
            this.data = "";
        }
        public T Parse<T>()
        {
            try
            {
                if (data == null) return default(T);
                string json = JsonConvert.SerializeObject(data);
                T t = JsonConvert.DeserializeObject<T>(json);
                return t;
            }
            catch (Exception)
            {
                return default(T);
            }
        }
    }
}
