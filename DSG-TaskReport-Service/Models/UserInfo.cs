using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace DSGServerCore
{
    [Table("usertable")]
    public class UserInfo
    {
        public int ID { get; set; }
        public string Usr { get; set; }
        public string Pwd { get; set; }
        public string Kind { get; set; }
        public string Email { get; set; }
        public int Power { get; set; }
        public string Status { get; set; }
        public string UI { get; set; }
        public string Device { get; set; }
        public string Token { get; set; }
    }
}
