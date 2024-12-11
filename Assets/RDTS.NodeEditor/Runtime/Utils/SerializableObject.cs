using System;
using UnityEngine;
using System.Globalization;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RDTS.NodeEditor
{
    // Warning: this class only support the serialization of UnityObject and primitive
    [System.Serializable]
    public class SerializableObject
    {
        [System.Serializable]
        class ObjectWrapper//对象包装器
        {
            public UnityEngine.Object value;
        }

        public string serializedType;
        public string serializedName;
        public string serializedValue;

        public object value;

        public SerializableObject(object value, Type type, string name = null)
        {
            this.value = value;
            this.serializedName = name;
            this.serializedType = type.AssemblyQualifiedName;//获取类型的程序集限定名
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        public void Deserialize()
        {
            if (String.IsNullOrEmpty(serializedType))
            {
                Debug.LogError("Can't deserialize the object from null type");
                return;
            }

            Type type = Type.GetType(serializedType);

            if (type.IsPrimitive)//若是基元类型（Boolen，SByte，Int，Uint，IntPtr，UIntPtr，Char，Double，Single）
            {
                ///CultureInfo：提供有关特定区域性（对于非托管代码开发，则称为“区域设置”）的信息。 这些信息包括区域性的名称、书写系统、使用的日历、字符串的排序顺序以及对日期和数字的格式化设置
                ///InvariantCulture：不依赖于区域性（固定）的对象

                if (string.IsNullOrEmpty(serializedValue))
                    value = Activator.CreateInstance(type);
                else
                    value = Convert.ChangeType(serializedValue, type, CultureInfo.InvariantCulture);//返回：类型为type、值为serializedValue的对象
            }
            else if (typeof(UnityEngine.Object).IsAssignableFrom(type))//判断指定类型(type)的实例是否能分配给当前类型(UnityEngine.Object)的变量
            {
                ///FromJsonOverwrite:通过读取对象的JSON表示形式覆盖其数据。它将JSON数据加载到现有对象中。
                ///    参数1：对象的JSON表示形式
                ///    参数2：应覆盖的对象
                ObjectWrapper obj = new ObjectWrapper();
                JsonUtility.FromJsonOverwrite(serializedValue, obj);
                value = obj.value;
            }
            else if (type == typeof(string))//若是字符串类型
                value = serializedValue.Length > 1 ? serializedValue.Substring(1, serializedValue.Length - 2).Replace("\\\"", "\"") : "";
            else
            {
                try
                {
                    value = Activator.CreateInstance(type);
                    JsonUtility.FromJsonOverwrite(serializedValue, value);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    Debug.LogError("Can't serialize type " + serializedType);
                }
            }
        }

        /// <summary>
        /// 序列化
        /// </summary>
        public void Serialize()
        {
            if (value == null)
                return;

            serializedType = value.GetType().AssemblyQualifiedName;

            if (value.GetType().IsPrimitive)//若是基元类型（Boolen，SByte，Int，Uint，IntPtr，UIntPtr，Char，Double，Single）
                serializedValue = Convert.ToString(value, CultureInfo.InvariantCulture);//使用指定的区域性特定格式设置信息，将指定对象的值转换为其等效的字符串表示形式
            else if (value is UnityEngine.Object) //type is a unity object
            {
                if ((value as UnityEngine.Object) == null)
                    return;

                ObjectWrapper wrapper = new ObjectWrapper { value = value as UnityEngine.Object };
                serializedValue = JsonUtility.ToJson(wrapper);//生成对象的公共字段的 JSON 表示形式
            }
            else if (value is string)
                serializedValue = "\"" + ((string)value).Replace("\"", "\\\"") + "\"";
            else
            {
                try
                {
                    serializedValue = JsonUtility.ToJson(value);
                    if (String.IsNullOrEmpty(serializedValue))
                        throw new Exception();
                }
                catch
                {
                    Debug.LogError("Can't serialize type " + serializedType);
                }
            }
        }
    }
}