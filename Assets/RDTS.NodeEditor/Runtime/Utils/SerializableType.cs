using System;
using System.Collections.Generic;
using UnityEngine;

namespace RDTS.NodeEditor
{
    /// <summary>
    /// Ҫ���л�����(��)����¼���������� �����һ������
    /// </summary>
	[Serializable]
    public class SerializableType : ISerializationCallbackReceiver//���л��ͷ����л�
    {
        /// <summary>�ֵ䣨serializedType��type��</summary>
		static Dictionary<string, Type> typeCache = new Dictionary<string, Type>();
        /// <summary>�ֵ䣨type��serializedType��</summary>
		static Dictionary<Type, string> typeNameCache = new Dictionary<Type, string>();

        [SerializeField]
        public string serializedType;//Ҫ���л������������

        [NonSerialized]
        public Type type;//serializedType��Ӧ���ࣿһ���Ǵ�View���ࣿ

        public SerializableType(Type t)
        {
            type = t;
        }

        /// <summary>
        /// �ڷ����л�����󣬼�¼����serializedType��type�����ֵ�typeCache�У���¼����
        /// </summary>
        public void OnAfterDeserialize()//ʵ�ָ÷������Ա��� Unity �����л�����[��]���ջص���
        {
            if (!String.IsNullOrEmpty(serializedType))//��serializedType����
            {
                if (!typeCache.TryGetValue(serializedType, out type))
                {
                    type = Type.GetType(serializedType);//��ȡ����ָ�����Ƶ� Type��ִ�����ִ�Сд��������
                    typeCache[serializedType] = type;
                }
            }
        }
        /// <summary>
        /// ���л�����ǰ����¼��type��serializedType�����ֵ�typeNameCache�У���¼����
        /// </summary>
        public void OnBeforeSerialize()//ʵ�ָ÷������Ա��� Unity ���л�����[ǰ]���ջص���
        {
            if (type != null)
            {
                if (!typeNameCache.TryGetValue(type, out serializedType))//��ȡ��ָ����������ֵ
                {
                    serializedType = type.AssemblyQualifiedName;//��ȡ���͵ĳ����޶��������а������м��� Type �ĳ��򼯵����ơ�
                    typeNameCache[type] = serializedType;
                }
            }
        }
    }
}