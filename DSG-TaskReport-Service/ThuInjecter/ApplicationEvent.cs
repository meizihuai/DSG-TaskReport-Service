using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThuInjecter
{
    class ApplicationEventPublisher
    {
        public static void PublishEvent<T>(T t)
        {
            List<IApplicationEventListener<T>> applicationEvents = ServiceFactory.GetServiceList<IApplicationEventListener<T>>();
            if (applicationEvents == null || applicationEvents.Count == 0) return;
            foreach (var itm in applicationEvents)
            {
                if (itm.GetEventType() == t.GetType())
                {
                    itm.OnApplicationEvent(t);
                }
            }
        }
    }
}
