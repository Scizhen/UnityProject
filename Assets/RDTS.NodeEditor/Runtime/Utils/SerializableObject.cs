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
        class ObjectWrapper//�����װ��
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
            this.serializedType = type.AssemblyQualifiedName;//��ȡ���͵ĳ����޶���
        }

        /// <summary>
        /// �����л�
        /// </summary>
        public void Deserialize()
        {
            if (String.IsNullOrEmpty(serializedType))
            {
                Debug.LogError("Can't deserialize the object from null type");
                return;
            }

            Type type = Type.GetType(serializedType);

            if (type.IsPrimitive)//���ǻ�Ԫ���ͣ�Boolen��SByte��Int��Uint��IntPtr��UIntPtr��Char��Double��Single��
            {
                ///CultureInfo���ṩ�й��ض������ԣ����ڷ��йܴ��뿪�������Ϊ���������á�������Ϣ�� ��Щ��Ϣ���������Ե����ơ���дϵͳ��ʹ�õ��������ַ���������˳���Լ������ں����ֵĸ�ʽ������
                ///InvariantCulture���������������ԣ��̶����Ķ���

                if (string.IsNullOrEmpty(serializedValue))
                    value = Activator.CreateInstance(type);
                else
                    value = Convert.ChangeType(serializedValue, type, CultureInfo.InvariantCulture);//���أ�����Ϊtype��ֵΪserializedValue�Ķ���
            }
            else if (typeof(UnityEngine.Object).IsAssignableFrom(type))//�ж�ָ������(type)��ʵ���Ƿ��ܷ������ǰ����(UnityEngine.Object)�ı���
            {
                ///FromJsonOverwrite:ͨ����ȡ�����JSON��ʾ��ʽ���������ݡ�����JSON���ݼ��ص����ж����С�
                ///    ����1�������JSON��ʾ��ʽ
                ///    ����2��Ӧ���ǵĶ���
                ObjectWrapper obj = new ObjectWrapper();
                JsonUtility.FromJsonOverwrite(serializedValue, obj);
                value = obj.value;
            }
            else if (type == typeof(string))//�����ַ�������
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
        /// ���л�
        /// </summary>
        public void Serialize()
        {
            if (value == null)
                return;

            serializedType = value.GetType().AssemblyQualifiedName;

            if (value.GetType().IsPrimitive)//���ǻ�Ԫ���ͣ�Boolen��SByte��Int��Uint��IntPtr��UIntPtr��Char��Double��Single��
                serializedValue = Convert.ToString(value, CultureInfo.InvariantCulture);//ʹ��ָ�����������ض���ʽ������Ϣ����ָ�������ֵת��Ϊ���Ч���ַ�����ʾ��ʽ
            else if (value is UnityEngine.Object) //type is a unity object
            {
                if ((value as UnityEngine.Object) == null)
                    return;

                ObjectWrapper wrapper = new ObjectWrapper { value = value as UnityEngine.Object };
                serializedValue = JsonUtility.ToJson(wrapper);//���ɶ���Ĺ����ֶε� JSON ��ʾ��ʽ
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