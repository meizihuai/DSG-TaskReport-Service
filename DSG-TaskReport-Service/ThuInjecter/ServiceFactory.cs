using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ThuInjecter
{
    class ServiceFactory
    {
        private static List<ServiceBean> serviceBeans = new List<ServiceBean>();
        public static void Init()
        {
            Type[] components = GetAttributeTypes<ComponentAttribute>();
            Type[] primarys = GetAttributeTypes<PrimaryAttribute>();
            if (components == null || components.Length == 0) return;
            foreach (var itm in components)
            {
                if (!primarys.Contains(itm))
                {
                    var interfaces = itm.GetInterfaces();
                    if (interfaces != null && interfaces.Length > 0)
                    {
                        Type type = interfaces[0];
                        ComponentAttribute componentAttr = (ComponentAttribute)itm.GetCustomAttributes(typeof(ComponentAttribute), false).FirstOrDefault();
                        bool flagAdded = false;
                        if (componentAttr != null)
                        {
                            bool isSington = componentAttr.Singleton;
                            if (!isSington)
                            {
                                AddService(type, itm);
                                flagAdded = true;
                            }
                        }
                        if (!flagAdded) AddSingleton(type, itm);
                    }
                }

            }
            if (primarys != null && primarys.Length != 0)
            {
                foreach (var itm in primarys)
                {
                    if (components.Contains(itm))
                    {
                        var interfaces = itm.GetInterfaces();
                        if (interfaces != null && interfaces.Length > 0)
                        {
                            Type type = interfaces[0];
                            ComponentAttribute componentAttr = (ComponentAttribute)itm.GetCustomAttributes(typeof(ComponentAttribute), false).FirstOrDefault();
                            bool flagAdded = false;
                            if (componentAttr != null)
                            {
                                bool isSington = componentAttr.Singleton;
                                if (!isSington)
                                {
                                    AddService(type, itm);
                                    flagAdded = true;
                                }
                            }
                            if (!flagAdded) AddSingleton(type, itm);
                        }
                    }
                }
            }

        }
        private static Type[] GetAttributeTypes<T>()
        {
            //System.Reflection.Assembly asm = Assembly.GetExecutingAssembly().GetTypes();
            System.Type[] types = Assembly.GetExecutingAssembly().GetTypes();

            Func<Attribute[], bool> IsMyAttribute = o =>
            {
                foreach (Attribute a in o)
                {
                    if (a is T)
                        return true;
                }
                return false;
            };

            System.Type[] cosType = types.Where(o =>
            {
                return IsMyAttribute(System.Attribute.GetCustomAttributes(o, true));
            }
            ).ToArray();
            return cosType;
        }

        public static void AddService<K, V>()
        {
            ServiceBean bean = new ServiceBean(typeof(K), typeof(V));
            serviceBeans.Add(bean);
        }
        public static void AddSingleton<K, V>()
        {
            ServiceBean bean = new ServiceBean(typeof(K), typeof(V), true);
            serviceBeans.Add(bean);
        }
        public static void AddSingleton(Type k, Type v)
        {
            ServiceBean bean = new ServiceBean(k, v, true);
            serviceBeans.Add(bean);
        }
        public static void AddService(Type k, Type v)
        {
            ServiceBean bean = new ServiceBean(k, v);
            serviceBeans.Add(bean);
        }
        public static T GetService<T>()
        {
            ServiceBean[] beans = serviceBeans.Where(a => a.IType == typeof(T)).ToArray();
            if (beans == null || beans.Length == 0) throw new Exception("没有找到此服务");
            ServiceBean b = beans[beans.Length - 1];
            if (b.IsSingleton)
            {
                return (T)b.SingletonInstance;
            }
            else
            {
                var instance = Activator.CreateInstance(b.Type);
                return (T)instance;
            }
        }
        public static List<T> GetServiceList<T>()
        {
            List<T> list = new List<T>();
            ServiceBean[] beans = serviceBeans.Where(a => a.IType == typeof(T)).ToArray();
            if (beans == null || beans.Length == 0) return null;
            foreach (var b in beans)
            {
                if (b.IsSingleton)
                {
                    list.Add((T)b.SingletonInstance);
                }
                else
                {
                    var instance = Activator.CreateInstance(b.Type);
                    list.Add((T)instance);
                }
            }
            return list;
        }
    }
}
