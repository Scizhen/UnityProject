using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif

// Warning, the current serialization code does not handle unity objects
// in play mode outside of the editor (because of JsonUtility)
// ���棬��ǰ�����л������ڱ༭��֮��Ĳ���ģʽ�²�����ͳһ������Ϊ JsonUtility��

namespace RDTS.NodeEditor
{
    [Serializable]
    public struct JsonElement
    {
        public string type;//����
        public string jsonDatas;//���б����л���JSON�ֶ����ݣ������� "playerName":"Dr Charles" ��

        public override string ToString()
        {
            return "type: " + type + " | JSON: " + jsonDatas;
        }
    }

    /// <summary>
    /// ���л��ͷ����л����������ڽڵ�����л��ͷ����л�����
    /// </summary>
	public static class JsonSerializer
    {
        public static JsonElement Serialize(object obj)
        {
            JsonElement elem = new JsonElement();

            elem.type = obj.GetType().AssemblyQualifiedName;
#if UNITY_EDITOR
            elem.jsonDatas = EditorJsonUtility.ToJson(obj);//ToJson�����ɶ����JSON��ʾ��ʽ
#else
			elem.jsonDatas = JsonUtility.ToJson(obj);
#endif

            return elem;
        }

        /// <summary>
        /// [����]�����л�ָ����JsonElement�����������е����ݸ��ǵ��µ�T���Ͷ����Ϸ���
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="e"></param>
        /// <returns></returns>
		public static T Deserialize<T>(JsonElement e)
        {
            if (typeof(T) != Type.GetType(e.type))
                throw new ArgumentException("Deserializing type is not the same than Json element type");

            var obj = Activator.CreateInstance<T>();
#if UNITY_EDITOR
            ///FromJsonOverwrite��ͨ����ȡ�����JSON��ʾ��ʽ����������
            EditorJsonUtility.FromJsonOverwrite(e.jsonDatas, obj);//������e��JSON���ݸ��ǵ�obj�ϣ�����JsonElement�е����ݸ�ֵ������ΪT��obj�ϣ�
#else
			JsonUtility.FromJsonOverwrite(e.jsonDatas, obj);
#endif

            return obj;
        }

        /// <summary>���л��ڵ��Json��ʽ</summary>
		public static JsonElement SerializeNode(BaseNode node)
        {
            return Serialize(node);
        }
        /// <summary>�����л��ɽڵ�</summary>
		public static BaseNode DeserializeNode(JsonElement e)
        {
            try
            {
                var baseNodeType = Type.GetType(e.type);

                if (e.jsonDatas == null)
                    return null;

                var node = Activator.CreateInstance(baseNodeType) as BaseNode;
#if UNITY_EDITOR
                EditorJsonUtility.FromJsonOverwrite(e.jsonDatas, node);
#else
				JsonUtility.FromJsonOverwrite(e.jsonDatas, node);
#endif
                return node;
            }
            catch
            {
                return null;
            }
        }
    }
}