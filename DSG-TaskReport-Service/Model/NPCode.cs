using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Common
{
    public static class NPCode
    {
        public static NPCodeMsg OK = new NPCodeMsg(0, "Success");
        public static NPCodeMsg Fail = new NPCodeMsg(-1, "执行失败");
        public static NPCodeMsg ModelError = new NPCodeMsg(401, "请求数据格式非法");
        public static NPCodeMsg PowerError = new NPCodeMsg(402, "权限不足");

        public static NPCodeMsg ResourceNone = new NPCodeMsg(404, "资源不存在");
        public static NPCodeMsg LoginFail = new NPCodeMsg(405, "登陆认证失败，用户名或密码错误");
        public static NPCodeMsg TokenError = new NPCodeMsg(406, "token无效");
        public static NPCodeMsg ServerError = new NPCodeMsg(500, "服务器处理错误");
        public static NPCodeMsg DbError = new NPCodeMsg(501, "数据库处理错误");
        public static NPCodeMsg RedisError = new NPCodeMsg(502, "缓存服务处理错误");
        public static NPCodeMsg RestError = new NPCodeMsg(600, "内部接口通信错误");
    }
    public class NPCodeMsg
    {
        public string Msg { get; set; }
        public int Code { get; set; }

        public NPCodeMsg(int code, string msg)
        {
            this.Code = code;
            this.Msg = msg;
        }
    }
}
