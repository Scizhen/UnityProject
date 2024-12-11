using System.Collections.Generic;
using System.Collections;
using System;

namespace RDTS.NodeEditor
{
    public static class AppDomainExtension
    {
        /// <summary>
        /// 获取当前应用程序域中的程序集中所有当前实例准确运行时的类型
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
		public static IEnumerable<Type> GetAllTypes(this AppDomain domain)
        {
            foreach (var assembly in domain.GetAssemblies())//GetAssemblies：获取已加载到此应用程序域的执行上下文中的程序集
            {
                Type[] types = { };

                try
                {
                    types = assembly.GetTypes();//返回当前实例的准确运行时类型（一般为实例的类名）
                }
                catch
                {
                    //just ignore it ...
                }

                foreach (var type in types)
                    yield return type;
            }
        }
    }
}
