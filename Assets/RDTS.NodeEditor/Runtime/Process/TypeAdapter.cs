using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace RDTS.NodeEditor
{
    /// <summary>
    /// Implement this interface to use the inside your class to define type convertions to use inside the graph.
    /// 实现此接口以使用您的类内部来定义要在图形内部使用的类型转换
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
        /// yield break：直接终止方法（不仅仅只是终止循环，是终止整个方法），而不执行yield break后面的语句。
        /// yield break返回一个不为null，但是count为0的IEnumerable集合
        /// </summary>
        public virtual IEnumerable<(Type, Type)> GetIncompatibleTypes() { yield break; }
    }

    /// <summary>
    /// 类型适配器，处理两个类型之间的转换：对类型转换方法的查找、处理、委托定义，不兼容类型的获取与记录
    /// </summary>
    public static class TypeAdapter//类型适配器
    {
        /// <summary>针对同一方法的字典：（ （方法参数类型，方法返回类型），指定方法的委托）</summary>
        static Dictionary<(Type from, Type to), Func<object, object>> adapters = new Dictionary<(Type, Type), Func<object, object>>();
        /// <summary>针对同一方法的字典：（ （方法参数类型，方法返回类型），方法）</summary>
        static Dictionary<(Type from, Type to), MethodInfo> adapterMethods = new Dictionary<(Type, Type), MethodInfo>();
        static List<(Type from, Type to)> incompatibleTypes = new List<(Type from, Type to)>();//不兼容的类型

        [System.NonSerialized]
        static bool adaptersLoaded = false;//

#if !ENABLE_IL2CPP
        /// <summary>
        /// 泛型方法：获得一个可调用强类型委托的弱类型的委托，指定的方法为method
        /// </summary>
        /// <typeparam name="TParam"></typeparam>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="method">静态或实例方法的 MethodInfo</param>
        /// <returns></returns>
        static Func<object, object> ConvertTypeMethodHelper<TParam, TReturn>(MethodInfo method)
        {
            // Convert the slow MethodInfo into a fast, strongly typed, open delegate
            // 将慢速 MethodInfo 转换为快速、强类型、开放的委托
            ///CreateDelegate：创建指定类型(参数1)的委托以表示指定的静态方法(参数2)。 返回表示指定静态方法的指定类型的委托
            Func<TParam, TReturn> func = (Func<TParam, TReturn>)Delegate.CreateDelegate
                (typeof(Func<TParam, TReturn>), method);

            // Now create a more weakly typed delegate which will call the strongly typed one
            // 现在创建一个更弱类型的委托，它将调用强类型的委托
            Func<object, object> ret = (object param) => func((TParam)param);
            return ret;
        }
#endif
        /// <summary>
        /// 加载从ITypeAdapter接口派生类中的“类型转换方法”、“不兼容类型方法”。对前者进行类型转换委托、方法的处理并加入对应字典中，对后者获取不兼容的类型并加入到对应字典中
        /// </summary>
        static void LoadAllAdapters()
        {
            ///AppDomain.CurrentDomain：获取当前 Thread 的当前应用程序域
            foreach (Type type in AppDomain.CurrentDomain.GetAllTypes())//获取当前应用程序域中的程序集中所有当前实例准确运行时的类型
            {
                ///IsAssignableFrom：确定指定类型type的实例是否能分配给当前类型的变量
                ///满足任何一个条件为true：
                ///     ①type和当前实例(ITypeAdapter)表示相同类型
                ///     ②type是从当前实例直接或间接派生的[一般是这种情况]
                ///     ③当前实例是type实现的一个接口
                ///     ④type是一个泛型类型参数，并且当前实例表示type的约束之一
                ///     ⑤type表示一个值类型，并且当前实例表示 Nullable<type>
                if (typeof(ITypeAdapter).IsAssignableFrom(type))
                {
                    if (type.IsAbstract)//如果Type是抽象的(不能实例化,但只能用作派生类的基类)，则为 true；否则为 false
                        continue;

                    var adapter = Activator.CreateInstance(type) as ITypeAdapter;
                    if (adapter != null)
                    {
                        foreach (var types in adapter.GetIncompatibleTypes())//子类中重载的GetIncompatibleTypes()方法
                        {
                            //添加不兼容的类型
                            incompatibleTypes.Add((types.Item1, types.Item2));
                            incompatibleTypes.Add((types.Item2, types.Item1));
                        }
                    }

                    //遍历此类中的方法
                    foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                    {
                        if (method.GetParameters().Length != 1)//参数不是一个，直接返回
                        {
                            Debug.LogError($"Ignoring convertion method {method} because it does not have exactly one parameter");
                            continue;
                        }
                        if (method.ReturnType == typeof(void))//返回为空，直接返回
                        {
                            Debug.LogError($"Ignoring convertion method {method} because it does not returns anything");
                            continue;
                        }
                        ///GetParameters()；返回ParameterInfo 类型的数组，包含与此实例所反射的方法（或构造函数）的签名匹配的信息
                        ///ParameterType：获取该参数的 Type
                        Type from = method.GetParameters()[0].ParameterType;//第一个参数类型
                        Type to = method.ReturnType;///获取此方法的返回类型

                        //加载 类型转换方法
                        try
                        {

#if ENABLE_IL2CPP
                            // IL2CPP doesn't suport calling generic functions via reflection (AOT can't generate templated code)
                            Func<object, object> r = (object param) => { return (object)method.Invoke(null, new object[]{ param }); };
#else
                            //获取名为ConvertTypeMethodHelper的方法
                            MethodInfo genericHelper = typeof(TypeAdapter).GetMethod("ConvertTypeMethodHelper",
                                BindingFlags.Static | BindingFlags.NonPublic);

                            // Now supply the type arguments 现在提供类型参数
                            ///MakeGenericMethod：用类型数组的元素替代当前泛型方法定义的类型参数，并返回表示结果构造方法的 MethodInfo 对象
                            //（from,to）=> (TParam, TReturn)
                            MethodInfo constructedHelper = genericHelper.MakeGenericMethod(from, to);

                            object ret = constructedHelper.Invoke(null, new object[] { method });
                            var r = (Func<object, object>)ret;//返回参数为from，返回类型为to，指定方法为method的弱类型委托
#endif
                            //加入字典
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

            // Ensure that the dictionary contains all the convertions in both ways 确保字典包含两种方式的所有转换 （from，to）和（to，from）
            // ex: float to vector but no vector to float 例如
            foreach (var kp in adapters)
            {
                if (!adapters.ContainsKey((kp.Key.to, kp.Key.from)))//若不包含（to， from）的键
                    Debug.LogError($"Missing convertion method. There is one for {kp.Key.from} to {kp.Key.to} but not for {kp.Key.to} to {kp.Key.from}");
            }

            adaptersLoaded = true;
        }

        /// <summary>判断是否是不兼容类型</summary>
        public static bool AreIncompatible(Type from, Type to)
        {
            if (incompatibleTypes.Any((k) => k.from == from && k.to == to))
                return true;
            return false;
        }
        /// <summary>判断是否是可分配类型(类型兼容)</summary>
        public static bool AreAssignable(Type from, Type to)
        {
            if (!adaptersLoaded)//若未加载完adapters，就去搜索加载
                LoadAllAdapters();

            if (AreIncompatible(from, to))//若是不兼容类型，则直接返回
                return false;

            return adapters.ContainsKey((from, to));
        }

        /// <summary>获取转换方法</summary>
        public static MethodInfo GetConvertionMethod(Type from, Type to) => adapterMethods[(from, to)];

        /// <summary>
        /// 调用与指定参数相匹配的委托方法来转换类型：将from对象的类型转换成targetType
        /// </summary>
        /// <param name="from">要被转换的对象</param>
        /// <param name="targetType">转换的目标类型</param>
        /// <returns></returns>
        public static object Convert(object from, Type targetType)
        {
            if (!adaptersLoaded)
                LoadAllAdapters();

            Func<object, object> convertionFunction;//封装一个方法，该方法具有[一个参数]，且[返回由TResult参数]指定的类型object的值
            //TryGetValue：获取与指定键关联的值
            if (adapters.TryGetValue((from.GetType(), targetType), out convertionFunction))
                return convertionFunction?.Invoke(from);

            return null;
        }
    }
}