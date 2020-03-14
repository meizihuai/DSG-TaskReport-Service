using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThuInjecter
{
    class ServiceBean
    {
        /// <summary>
        /// 类型，0表示接口-impl;1表示单类
        /// </summary>
        public int BeanType { get; set; }
        public Type Type { get; set; }
        public Type IType { get; set; }
        public bool IsSingleton { get; set; }
        public object SingletonInstance { get; set; }
        public ServiceBean(Type iType, Type type, bool isSingleton = false)
        {
            this.BeanType = 0;
            this.Type = type;
            this.IType = iType;
            this.IsSingleton = isSingleton;
            if (isSingleton)
            {
                this.SingletonInstance = Activator.CreateInstance(type);
            }
        }
    }
}
