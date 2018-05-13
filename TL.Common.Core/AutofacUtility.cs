using Autofac;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TL.Common.Core
{
    public class AutofacUtility
    {
        private static IContainer _container;
        private static readonly ContainerBuilder _builder = new ContainerBuilder();
        public static T Find<T>() where T : class
        {
            return _container.ResolveOptional<T>();
        }

        public static T FindByKey<T>(object serviceKey) where T : class
        {
            return _container.ResolveOptionalKeyed<T>(serviceKey);
        }

        public static void RegisterAssembly(Assembly assembly)
        {
            var types = assembly.GetTypes();
            foreach (var type in types)
            {
                if (!string.IsNullOrWhiteSpace(type.Namespace))
                {
                    var attributes = type.GetCustomAttributes<IocKeyAttribute>();
                    if (attributes != null && attributes.Any())
                    {
                        foreach (var attribute in attributes)
                        {
                            RegisterType(type, attribute);
                        }
                    }
                    else
                    {
                        RegisterType(type, null);
                    }
                }
            }
        }

        public static void Build()
        {
            _container = _builder.Build();
        }

        private static void RegisterType(Type instanceType, IocKeyAttribute keyAttribute)
        {
            var baseInterfaces = instanceType.GetInterfaces();
            if (baseInterfaces != null && baseInterfaces.Any())
            {
                if (keyAttribute != null)
                {
                    _builder.RegisterType(instanceType).Keyed(keyAttribute.Key, baseInterfaces[0]).SingleInstance();
                }
                else
                {
                    _builder.RegisterType(instanceType).As(baseInterfaces[0]).SingleInstance();
                }
            }
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class IocKeyAttribute : Attribute
    {
        public Object Key { get; private set; }
        public IocKeyAttribute(Object key)
        {
            Key = key;
        }
    }
}
