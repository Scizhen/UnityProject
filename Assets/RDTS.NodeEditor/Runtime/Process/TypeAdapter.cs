using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace RDTS.NodeEditor
{
    /// <summary>
    /// Implement this interface to use the inside your class to define type convertions to use inside the graph.
    /// ʵ�ִ˽ӿ���ʹ���������ڲ�������Ҫ��ͼ���ڲ�ʹ�õ�����ת��
    /// Example:
    /// <code>
    /// public class CustomConvertions : ITypeAdapter
    /// {
    ///     public static Vector4 ConvertFloatToVector(float from) => new Vector4(from, from, from, from);
    ///     ...
    /// }
    /// </code>
    /// </summary>
    public abstract class ITypeAdapter // TODO: turn this back into an interface when we have C# 8
    {
        /// <summary>
        /// yield break��ֱ����ֹ������������ֻ����ֹѭ��������ֹ����������������ִ��yield break�������䡣
        /// yield break����һ����Ϊnull������countΪ0��IEnumerable����
        /// </summary>
        public virtual IEnumerable<(Type, Type)> GetIncompatibleTypes() { yield break; }
    }

    /// <summary>
    /// ������������������������֮���ת����������ת�������Ĳ��ҡ�����ί�ж��壬���������͵Ļ�ȡ���¼
    /// </summary>
    public static class TypeAdapter//����������
    {
        /// <summary>���ͬһ�������ֵ䣺�� �������������ͣ������������ͣ���ָ��������ί�У�</summary>
        static Dictionary<(Type from, Type to), Func<object, object>> adapters = new Dictionary<(Type, Type), Func<object, object>>();
        /// <summary>���ͬһ�������ֵ䣺�� �������������ͣ������������ͣ���������</summary>
        static Dictionary<(Type from, Type to), MethodInfo> adapterMethods = new Dictionary<(Type, Type), MethodInfo>();
        static List<(Type from, Type to)> incompatibleTypes = new List<(Type from, Type to)>();//�����ݵ�����

        [System.NonSerialized]
        static bool adaptersLoaded = false;//

#if !ENABLE_IL2CPP
        /// <summary>
        /// ���ͷ��������һ���ɵ���ǿ����ί�е������͵�ί�У�ָ���ķ���Ϊmethod
        /// </summary>
        /// <typeparam name="TParam"></typeparam>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="method">��̬��ʵ�������� MethodInfo</param>
        /// <returns></returns>
        static Func<object, object> ConvertTypeMethodHelper<TParam, TReturn>(MethodInfo method)
        {
            // Convert the slow MethodInfo into a fast, strongly typed, open delegate
            // ������ MethodInfo ת��Ϊ���١�ǿ���͡����ŵ�ί��
            ///CreateDelegate������ָ������(����1)��ί���Ա�ʾָ���ľ�̬����(����2)�� ���ر�ʾָ����̬������ָ�����͵�ί��
            Func<TParam, TReturn> func = (Func<TParam, TReturn>)Delegate.CreateDelegate
                (typeof(Func<TParam, TReturn>), method);

            // Now create a more weakly typed delegate which will call the strongly typed one
            // ���ڴ���һ���������͵�ί�У���������ǿ���͵�ί��
            Func<object, object> ret = (object param) => func((TParam)param);
            return ret;
        }
#endif
        /// <summary>
        /// ���ش�ITypeAdapter�ӿ��������еġ�����ת���������������������ͷ���������ǰ�߽�������ת��ί�С������Ĵ��������Ӧ�ֵ��У��Ժ��߻�ȡ�����ݵ����Ͳ����뵽��Ӧ�ֵ���
        /// </summary>
        static void LoadAllAdapters()
        {
            ///AppDomain.CurrentDomain����ȡ��ǰ Thread �ĵ�ǰӦ�ó�����
            foreach (Type type in AppDomain.CurrentDomain.GetAllTypes())//��ȡ��ǰӦ�ó������еĳ��������е�ǰʵ��׼ȷ����ʱ������
            {
                ///IsAssignableFrom��ȷ��ָ������type��ʵ���Ƿ��ܷ������ǰ���͵ı���
                ///�����κ�һ������Ϊtrue��
                ///     ��type�͵�ǰʵ��(ITypeAdapter)��ʾ��ͬ����
                ///     ��type�Ǵӵ�ǰʵ��ֱ�ӻ���������[һ�����������]
                ///     �۵�ǰʵ����typeʵ�ֵ�һ���ӿ�
                ///     ��type��һ���������Ͳ��������ҵ�ǰʵ����ʾtype��Լ��֮һ
                ///     ��type��ʾһ��ֵ���ͣ����ҵ�ǰʵ����ʾ Nullable<type>
                if (typeof(ITypeAdapter).IsAssignableFrom(type))
                {
                    if (type.IsAbstract)//���Type�ǳ����(����ʵ����,��ֻ������������Ļ���)����Ϊ true������Ϊ false
                        continue;

                    var adapter = Activator.CreateInstance(type) as ITypeAdapter;
                    if (adapter != null)
                    {
                        foreach (var types in adapter.GetIncompatibleTypes())//���������ص�GetIncompatibleTypes()����
                        {
                            //��Ӳ����ݵ�����
                            incompatibleTypes.Add((types.Item1, types.Item2));
                            incompatibleTypes.Add((types.Item2, types.Item1));
                        }
                    }

                    //���������еķ���
                    foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                    {
                        if (method.GetParameters().Length != 1)//��������һ����ֱ�ӷ���
                        {
                            Debug.LogError($"Ignoring convertion method {method} because it does not have exactly one parameter");
                            continue;
                        }
                        if (method.ReturnType == typeof(void))//����Ϊ�գ�ֱ�ӷ���
                        {
                            Debug.LogError($"Ignoring convertion method {method} because it does not returns anything");
                            continue;
                        }
                        ///GetParameters()������ParameterInfo ���͵����飬�������ʵ��������ķ��������캯������ǩ��ƥ�����Ϣ
                        ///ParameterType����ȡ�ò����� Type
                        Type from = method.GetParameters()[0].ParameterType;//��һ����������
                        Type to = method.ReturnType;///��ȡ�˷����ķ�������

                        //���� ����ת������
                        try
                        {

#if ENABLE_IL2CPP
                            // IL2CPP doesn't suport calling generic functions via reflection (AOT can't generate templated code)
                            Func<object, object> r = (object param) => { return (object)method.Invoke(null, new object[]{ param }); };
#else
                            //��ȡ��ΪConvertTypeMethodHelper�ķ���
                            MethodInfo genericHelper = typeof(TypeAdapter).GetMethod("ConvertTypeMethodHelper",
                                BindingFlags.Static | BindingFlags.NonPublic);

                            // Now supply the type arguments �����ṩ���Ͳ���
                            ///MakeGenericMethod�������������Ԫ�������ǰ���ͷ�����������Ͳ����������ر�ʾ������췽���� MethodInfo ����
                            //��from,to��=> (TParam, TReturn)
                            MethodInfo constructedHelper = genericHelper.MakeGenericMethod(from, to);

                            object ret = constructedHelper.Invoke(null, new object[] { method });
                            var r = (Func<object, object>)ret;//���ز���Ϊfrom����������Ϊto��ָ������Ϊmethod��������ί��
#endif
                            //�����ֵ�
                            adapters.Add((method.GetParameters()[0].ParameterType, method.ReturnType), r);
                            adapterMethods.Add((method.GetParameters()[0].ParameterType, method.ReturnType), method);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"Failed to load the type convertion method: {method}\n{e}");
                        }
                    }
                }
            }

            // Ensure that the dictionary contains all the convertions in both ways ȷ���ֵ�������ַ�ʽ������ת�� ��from��to���ͣ�to��from��
            // ex: float to vector but no vector to float ����
            foreach (var kp in adapters)
            {
                if (!adapters.ContainsKey((kp.Key.to, kp.Key.from)))//����������to�� from���ļ�
                    Debug.LogError($"Missing convertion method. There is one for {kp.Key.from} to {kp.Key.to} but not for {kp.Key.to} to {kp.Key.from}");
            }

            adaptersLoaded = true;
        }

        /// <summary>�ж��Ƿ��ǲ���������</summary>
        public static bool AreIncompatible(Type from, Type to)
        {
            if (incompatibleTypes.Any((k) => k.from == from && k.to == to))
                return true;
            return false;
        }
        /// <summary>�ж��Ƿ��ǿɷ�������(���ͼ���)</summary>
        public static bool AreAssignable(Type from, Type to)
        {
            if (!adaptersLoaded)//��δ������adapters����ȥ��������
                LoadAllAdapters();

            if (AreIncompatible(from, to))//���ǲ��������ͣ���ֱ�ӷ���
                return false;

            return adapters.ContainsKey((from, to));
        }

        /// <summary>��ȡת������</summary>
        public static MethodInfo GetConvertionMethod(Type from, Type to) => adapterMethods[(from, to)];

        /// <summary>
        /// ������ָ��������ƥ���ί�з�����ת�����ͣ���from���������ת����targetType
        /// </summary>
        /// <param name="from">Ҫ��ת���Ķ���</param>
        /// <param name="targetType">ת����Ŀ������</param>
        /// <returns></returns>
        public static object Convert(object from, Type targetType)
        {
            if (!adaptersLoaded)
                LoadAllAdapters();

            Func<object, object> convertionFunction;//��װһ���������÷�������[һ������]����[������TResult����]ָ��������object��ֵ
            //TryGetValue����ȡ��ָ����������ֵ
            if (adapters.TryGetValue((from.GetType(), targetType), out convertionFunction))
                return convertionFunction?.Invoke(from);

            return null;
        }
    }
}