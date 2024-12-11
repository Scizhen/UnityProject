using System;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System.Linq.Expressions;

namespace RDTS.NodeEditor
{
    public delegate void CustomPortIODelegate(BaseNode node, List<SerializableEdge> edges, NodePort outputPort = null);

    /// <summary>
    /// �Զ���˿����������������
    /// </summary>
	public static class CustomPortIO
    {
        class PortIOPerField : Dictionary<string, CustomPortIODelegate> { }//<�˿��ֶ��������ж˿����/�������Եķ�����ί��>
        class PortIOPerNode : Dictionary<Type, PortIOPerField> { }//�ֵ�<�ڵ����ͣ�<�˿��ֶ��������ж˿����/�������Եķ�����ί��> >

        /// <summary>�������͵��ֵ䣺��ԭ�������ͣ�Ҫת���ɵ����ͣ� </summary>
		static Dictionary<Type, List<Type>> assignableTypes = new Dictionary<Type, List<Type>>();
        /// <summary>��¼�Զ������/����˿ڷ�����ί��</summary>
		static PortIOPerNode customIOPortMethods = new PortIOPerNode();

        static CustomPortIO()
        {
            LoadCustomPortMethods();
        }

        /// <summary>
        /// �����Զ���˿ڵķ���������BaseNode���������о��С��Զ���˿����/�������ԡ��ķ�����ͨ��Lambda���ʽ���ķ�ʽ�����������ί�У������뵽�ֵ��У��ڷ��������ֵ������Ԫ��
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

                foreach (var method in methods)//����BaseNode���������еķ���
                {
                    var portInputAttr = method.GetCustomAttribute<CustomPortInputAttribute>();
                    var portOutputAttr = method.GetCustomAttribute<CustomPortOutputAttribute>();

                    //ֻ�о���CustomPortInputAttribute ��CustomPortOutputAttribute���Եķ������Ż�������д���
                    if (portInputAttr == null && portOutputAttr == null)
                        continue;

                    var p = method.GetParameters();
                    bool nodePortSignature = false;

                    // Check if the function can take a NodePort in optional param ��麯���Ƿ�����ڿ�ѡ������ʹ�� NodePort
                    if (p.Length == 2 && p[1].ParameterType == typeof(NodePort))//�Ƿ������Ϊ2���ҵڶ�������������NodePort
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
                    var p1 = Expression.Parameter(typeof(BaseNode), "node");//����һ��BaseNode���͵Ĳ���node
                    var p2 = Expression.Parameter(typeof(List<SerializableEdge>), "edges");//����һ��List< SerializableEdge >���͵Ĳ���edges
                    var p3 = Expression.Parameter(typeof(NodePort), "port");//����һ��NodePort���͵Ĳ���port

                    MethodCallExpression ex;//����һ���Ծ�̬������ʵ�������ĵ���
                    ///�˴���Call���Բ���һ�������������ķ����ĵ���
                    ///    ����1��ָ��һ��ʵ�����õ�ʵ������BaseNodeת���ɲ�ѯ����type���ͣ�һ��ΪBaseNode�����ࣩ
                    ///    ����2����ʾĿ�귽��������CustomPortInputAttribute��CustomPortOutputAttribute���Եķ�����һ��һ������ֻ��������һ�����ԣ�
                    ///    ����3����һ��������List< SerializableEdge >��
                    ///    ����4���ڶ���������NodePort��
                    if (nodePortSignature)
                        ex = Expression.Call(Expression.Convert(p1, type), method, p2, p3);//NodePort������һ������
                    else
                        ex = Expression.Call(Expression.Convert(p1, type), method, p2);//��BaseNode�����ʵ�������е��ô������ΪList< SerializableEdge >��method���� ��������CustomPortsNode.cs�е�PullInputs������

                    //lambda���ʽ�������Funcί�У� �÷���deleg(p1,p2,p3) ��
                    deleg = Expression.Lambda<CustomPortIODelegate>(ex, p1, p2, p3).Compile();//Lambda���ʽΪ��(p1,p2,p3) => ex
#endif

                    if (deleg == null)
                    {
                        Debug.LogWarning("Can't use custom IO port function " + method + ": The method have to respect this format: " + typeof(CustomPortIODelegate));
                        continue;
                    }

                    string fieldName = (portInputAttr == null) ? portOutputAttr.fieldName : portInputAttr.fieldName;//���������õ��ֶ���
                    Type customType = (portInputAttr == null) ? portOutputAttr.outputType : portInputAttr.inputType;//���������õ�����
                    Type fieldType = type.GetField(fieldName, bindingFlags).FieldType;//��(�ڵ�)���л�ȡ��Ӧ���ֶ���������

                    AddCustomIOMethod(type, fieldName, deleg);//type���ڵ����ͣ�fieldName���Զ���˿����/���������ж�����ֶ�����deleg��������Lambda���ʽ������ί��

                    AddAssignableTypes(customType, fieldType);
                    AddAssignableTypes(fieldType, customType);
                }
            }
        }

        /// <summary>
        /// ����ָ���Ľڵ����ͺ�(�˿�)�ֶ�����ȡ��Ӧ�ľ��С��˿����/�������ԡ��ķ�����ί��
        /// </summary>
        /// <param name="nodeType">�ڵ�����</param>
        /// <param name="fieldName">�˿��ֶ���</param>
        /// <returns></returns>
		public static CustomPortIODelegate GetCustomPortMethod(Type nodeType, string fieldName)
        {
            PortIOPerField portIOPerField;
            CustomPortIODelegate deleg;

            customIOPortMethods.TryGetValue(nodeType, out portIOPerField);//���ֵ��л�ȡ�Բ����ڵ�����Ϊ����ֵ���ֶ���Ϊ�����䷽��ί��Ϊֵ���ֵ䣩������ֵ��portIOPerField

            if (portIOPerField == null)
                return null;

            portIOPerField.TryGetValue(fieldName, out deleg);//��ȡ���ֶ���Ϊ����ֵ����Ӧ�䷽��ί�У�

            return deleg;
        }

        /// <summary>
        /// ����ָ���Ĳ��������á��Զ���˿ڷ������ֵ��ж�ӦԪ�صļ���ֵ
        /// </summary>
        /// <param name="nodeType">�Զ���˿����ڵĽڵ�����</param>
        /// <param name="fieldName">�˿����/���������ж�����ֶ���</param>
        /// <param name="deleg">���ھ��ж˿����/�������Եķ�����ί��</param>
		static void AddCustomIOMethod(Type nodeType, string fieldName, CustomPortIODelegate deleg)
        {
            if (!customIOPortMethods.ContainsKey(nodeType))//���������˽ڵ����͵ļ��������ֵ������һ���µ�Ԫ��
                customIOPortMethods[nodeType] = new PortIOPerField();

            customIOPortMethods[nodeType][fieldName] = deleg;//���ݲ������ô�Ԫ��
        }

        /// <summary>
        /// ����������ֵ�assignableTypes������Ԫ��
        /// </summary>
        /// <param name="fromType">ԭ����</param>
        /// <param name="toType">Ҫת���ɵ�����</param>
		static void AddAssignableTypes(Type fromType, Type toType)
        {
            if (!assignableTypes.ContainsKey(fromType))
                assignableTypes[fromType] = new List<Type>();

            assignableTypes[fromType].Add(toType);
        }

        /// <summary>
        /// �ж�����1�Ƿ��ܷ��䵽����2
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