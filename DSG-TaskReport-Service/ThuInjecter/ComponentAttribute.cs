using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThuInjecter
{
    class ComponentAttribute : Attribute
    {
        public bool Singleton { get; set; }
        public ComponentAttribute(bool singleton = true)
        {
            this.Singleton = singleton;
        }
    }
}
