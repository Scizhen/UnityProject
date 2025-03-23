using System;
using System.Collections.Generic;
using UnityEngine;

namespace RDTS.NodeEditor
{
    /// <summary>
    /// 要序列化的类(型)，记录类名和类型 （针对一个对象）
    /// </summary>
	[Serializable]
    public class SerializableType : ISerializationCallbackReceiver//序列化和反序列化
    {
        /// <summary>字典（serializedType，type）</summary>
		static Dictionary<string, Type> typeCache = new Dictionary<string, Type>();
        /// <summary>字典（type，serializedType）</summary>
		static Dictionary<Type, string> typeNameCache = new Dictionary<Type, string>();

        [SerializeField]
        public string serializedType;//要序列化的类的类名？

        [NonSerialized]
        public Type type;//serializedType对应的类？一般是带View的类？

        public SerializableType(Type t)
        {
            type = t;
        }

        /// <summary>
        /// 在反序列化对象后，记录到（serializedType，type）的字典typeCache中，记录类型
        /// </summary>
        public void OnAfterDeserialize()//实现该方法，以便在 Unity 反序列化对象[后]接收回调。
        {
            if (!String.IsNullOrEmpty(serializedType))//若serializedType存在
            {
                if (!typeCache.TryGetValue(serializedType, out type))
                {
                    type = Type.GetType(serializedType);//获取具有指定名称的 Type，执行区分大小写的搜索。
                    typeCache[serializedType] = type;
                }
            }
        }
        /// <summary>
        /// 序列化对象前，记录（type，serializedType）的字典typeNameCache中，记录类名
        /// </summary>
        public void OnBeforeSerialize()//实现该方法，以便在 Unity 序列化对象[前]接收回调。
        {
            if (type != null)
            {
                if (!typeNameCache.TryGetValue(type, out serializedType))//获取与指定键关联的值
                {
                    serializedType = type.AssemblyQualifiedName;//获取类型的程序集限定名，其中包括从中加载 Type 的程序集的名称。
                    typeNameCache[type] = serializedType;
                }
            }
        }
    }
}