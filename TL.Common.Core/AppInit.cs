using AutoMapper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TL.Common.Core;

namespace TL.Common.Core
{
    public class AppInit

    {
        private const string BIZ_DLL = "biz.dll";
        private const string INTEGRATION_DLL = "integration.dll";
        private const string DAL_DLL = "dal.dll";
        private const string DSF_DLL = "dsf.core";
        public static void Init()
        {
            var assemblys = GetCurrentDomainAssembies();
            AutoMapperInit(assemblys);
            AutofacInit(assemblys);
        }

        private static void AutoMapperInit(List<Assembly> assemblys)
        {
            Mapper.Initialize(cfg =>
            {
                var profiles = assemblys.SelectMany(p => p.GetTypes()).Where(p => p.IsSubclassOf(typeof(Profile)) && (!p.IsGenericType) && (!p.IsAbstract) && (p.GetConstructors().Any(c => c.GetParameters().Length == 0))).ToList();
                cfg.AddProfiles(profiles);
            });
        }

        private static void AutofacInit(List<Assembly> assemblys)
        {
            RegisterDsf(assemblys);
            RegisterBiz(assemblys);
            RegisterIntegration(assemblys);
            RegisterDal(assemblys);
            
            AutofacUtility.Build();
        }

        private static void RegisterDsf(List<Assembly> assemblys)
        {
            var assembly = assemblys.First(u => u.CodeBase.ToLower().Contains(DSF_DLL));
            AutofacUtility.RegisterAssembly(assembly);
        }

        private static void RegisterBiz(List<Assembly> assemblys)
        {
            var assembly = assemblys.First(u => u.CodeBase.ToLower().Contains(BIZ_DLL));
            AutofacUtility.RegisterAssembly(assembly);
        }

        private static void RegisterIntegration(List<Assembly> assemblys)
        {
            var assembly = assemblys.First(u => u.CodeBase.ToLower().Contains(INTEGRATION_DLL));
            AutofacUtility.RegisterAssembly(assembly);
        }

        private static void RegisterDal(List<Assembly> assemblys)
        {
            var assembly = assemblys.First(u => u.CodeBase.ToLower().Contains(DAL_DLL));
            AutofacUtility.RegisterAssembly(assembly);
        }

        private static List<Assembly> GetCurrentDomainAssembies()
        {
            var assemblys = new List<Assembly>();
            var path = AppDomain.CurrentDomain.BaseDirectory;
            var files = Directory.GetFiles(path);
            files = files.Where(u => u.Contains("Tc.Flight.Recommend") && u.EndsWith(".dll")).ToArray();
            foreach (var file in files)
            {
                assemblys.Add(Assembly.LoadFile(file));
            }
            return assemblys;
        }
    }
}
