using System.Collections.Generic;
using System.Collections;
using System;

namespace RDTS.NodeEditor
{
    public static class AppDomainExtension
    {
        /// <summary>
        /// ��ȡ��ǰӦ�ó������еĳ��������е�ǰʵ��׼ȷ����ʱ������
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
		public static IEnumerable<Type> GetAllTypes(this AppDomain domain)
        {
            foreach (var assembly in domain.GetAssemblies())//GetAssemblies����ȡ�Ѽ��ص���Ӧ�ó������ִ���������еĳ���
            {
                Type[] types = { };

                try
                {
                    types = assembly.GetTypes();//���ص�ǰʵ����׼ȷ����ʱ���ͣ�һ��Ϊʵ����������
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
