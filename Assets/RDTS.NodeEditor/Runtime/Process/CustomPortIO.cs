using System;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System.Linq.Expressions;

namespace RDTS.NodeEditor
{
    public delegate void CustomPortIODelegate(BaseNode node, List<SerializableEdge> edges, NodePort outputPort = null);

    /// <summary>
    /// 自定义端口输入输出方法的类
    /// </summary>
	public static class CustomPortIO
    {
        class PortIOPerField : Dictionary<string, CustomPortIODelegate> { }//<端口字段名，具有端口输出/输入特性的方法的委托>
        class PortIOPerNode : Dictionary<Type, PortIOPerField> { }//字典<节点类型，<端口字段名，具有端口输出/输入特性的方法的委托> >

        /// <summary>分配类型的字典：（原本的类型，要转换成的类型） </summary>
		static Dictionary<Type, List<Type>> assignableTypes = new Dictionary<Type, List<Type>>();
        /// <summary>记录自定义输出/输入端口方法的委托</summary>
		static PortIOPerNode customIOPortMethods = new PortIOPerNode();

        static CustomPortIO()
        {
            LoadCustomPortMethods();
        }

        /// <summary>
        /// 加载自定义端口的方法：查找BaseNode及其子类中具有“自定义端口输出/输入特性”的方法，通过Lambda表达式树的方式将方法编译成委托，并加入到字典中；在分配类型字典中添加元素
        /// </summary>
		static void LoadCustomPortMethods()
        {
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            foreach (var type in AppDomain.CurrentDomain.GetAllTypes())
            {
                if (type.IsAbstract || type.ContainsGenericParameters)
                    continue;
                if (!(type.IsSubclassOf(typeof(BaseNode))))
                    continue;

                var methods = type.GetMethods(bindingFlags);

                foreach (var method in methods)//遍历BaseNode及其子类中的方法
                {
                    var portInputAttr = method.GetCustomAttribute<CustomPortInputAttribute>();
                    var portOutputAttr = method.GetCustomAttribute<CustomPortOutputAttribute>();

                    //只有具有CustomPortInputAttribute 或CustomPortOutputAttribute特性的方法，才会继续进行处理
                    if (portInputAttr == null && portOutputAttr == null)
                        continue;

                    var p = method.GetParameters();
                    bool nodePortSignature = false;

                    // Check if the function can take a NodePort in optional param 检查函数是否可以在可选参数中使用 NodePort
                    if (p.Length == 2 && p[1].ParameterType == typeof(NodePort))//是否参数个为2，且第二个参数类型是NodePort
                        nodePortSignature = true;

                    CustomPortIODelegate deleg;
#if ENABLE_IL2CPP
					// IL2CPP doesn't support expression builders
					if (nodePortSignature)
					{
						deleg = new CustomPortIODelegate((node, edges, port) => {
							Debug.Log(port);
							method.Invoke(node, new object[]{ edges, port});
						});
					}
					else
					{
						deleg = new CustomPortIODelegate((node, edges, port) => {
							method.Invoke(node, new object[]{ edges });
						});
					}
#else
                    var p1 = Expression.Parameter(typeof(BaseNode), "node");//声明一个BaseNode类型的参数node
                    var p2 = Expression.Parameter(typeof(List<SerializableEdge>), "edges");//声明一个List< SerializableEdge >类型的参数edges
                    var p3 = Expression.Parameter(typeof(NodePort), "port");//声明一个NodePort类型的参数port

                    MethodCallExpression ex;//声明一个对静态方法或实例方法的调用
                    ///此处的Call：对采用一个或两个参数的方法的调用
                    ///    参数1：指定一个实例调用的实例（将BaseNode转换成查询到的type类型，一般为BaseNode的子类）
                    ///    参数2：表示目标方法（具有CustomPortInputAttribute或CustomPortOutputAttribute特性的方法，一般一个方法只具有其中一个特性）
                    ///    参数3：第一个参数（List< SerializableEdge >）
                    ///    参数4：第二个参数（NodePort）
                    if (nodePortSignature)
                        ex = Expression.Call(Expression.Convert(p1, type), method, p2, p3);//NodePort是其中一个参数
                    else
                        ex = Expression.Call(Expression.Convert(p1, type), method, p2);//在BaseNode子类的实例对象中调用传入参数为List< SerializableEdge >的method方法 （例如在CustomPortsNode.cs中的PullInputs方法）

                    //lambda表达式树编译成Func委托（ 用法：deleg(p1,p2,p3) ）
                    deleg = Expression.Lambda<CustomPortIODelegate>(ex, p1, p2, p3).Compile();//Lambda表达式为：(p1,p2,p3) => ex
#endif

                    if (deleg == null)
                    {
                        Debug.LogWarning("Can't use custom IO port function " + method + ": The method have to respect this format: " + typeof(CustomPortIODelegate));
                        continue;
                    }

                    string fieldName = (portInputAttr == null) ? portOutputAttr.fieldName : portInputAttr.fieldName;//特性中设置的字段名
                    Type customType = (portInputAttr == null) ? portOutputAttr.outputType : portInputAttr.inputType;//特性中设置的类型
                    Type fieldType = type.GetField(fieldName, bindingFlags).FieldType;//在(节点)类中获取对应的字段名的类型

                    AddCustomIOMethod(type, fieldName, deleg);//type：节点类型，fieldName：自定义端口输出/输入特性中定义的字段名，deleg：上面右Lambda表达式编译后的委托

                    AddAssignableTypes(customType, fieldType);
                    AddAssignableTypes(fieldType, customType);
                }
            }
        }

        /// <summary>
        /// 根据指定的节点类型和(端口)字段名获取对应的具有“端口输出/输入特性”的方法的委托
        /// </summary>
        /// <param name="nodeType">节点类型</param>
        /// <param name="fieldName">端口字段名</param>
        /// <returns></returns>
		public static CustomPortIODelegate GetCustomPortMethod(Type nodeType, string fieldName)
        {
            PortIOPerField portIOPerField;
            CustomPortIODelegate deleg;

            customIOPortMethods.TryGetValue(nodeType, out portIOPerField);//从字典中获取以参数节点类型为键的值（字段名为键、其方法委托为值的字典），并赋值给portIOPerField

            if (portIOPerField == null)
                return null;

            portIOPerField.TryGetValue(fieldName, out deleg);//获取以字段名为键的值（对应其方法委托）

            return deleg;
        }

        /// <summary>
        /// 根据指定的参数来设置“自定义端口方法”字典中对应元素的键与值
        /// </summary>
        /// <param name="nodeType">自定义端口所在的节点类型</param>
        /// <param name="fieldName">端口输出/输入特性中定义的字段名</param>
        /// <param name="deleg">对于具有端口输出/输入特性的方法的委托</param>
		static void AddCustomIOMethod(Type nodeType, string fieldName, CustomPortIODelegate deleg)
        {
            if (!customIOPortMethods.ContainsKey(nodeType))//若不包含此节点类型的键，就向字典中添加一个新的元素
                customIOPortMethods[nodeType] = new PortIOPerField();

            customIOPortMethods[nodeType][fieldName] = deleg;//根据参数设置此元素
        }

        /// <summary>
        /// 向分配类型字典assignableTypes中设置元素
        /// </summary>
        /// <param name="fromType">原类型</param>
        /// <param name="toType">要转换成的类型</param>
		static void AddAssignableTypes(Type fromType, Type toType)
        {
            if (!assignableTypes.ContainsKey(fromType))
                assignableTypes[fromType] = new List<Type>();

            assignableTypes[fromType].Add(toType);
        }

        /// <summary>
        /// 判断类型1是否能分配到类型2
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <returns></returns>
		public static bool IsAssignable(Type input, Type output)
        {
            if (assignableTypes.ContainsKey(input))
                return assignableTypes[input].Contains(output);
            return false;
        }
    }
}