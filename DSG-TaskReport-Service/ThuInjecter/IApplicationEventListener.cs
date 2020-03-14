using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThuInjecter
{
    interface IApplicationEventListener<T>
    {
        Type GetEventType();
        void OnApplicationEvent(T t);
    }
}
