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
// 警告，当前的序列化代码在编辑器之外的播放模式下不处理统一对象（因为 JsonUtility）

namespace RDTS.NodeEditor
{
    [Serializable]
    public struct JsonElement
    {
        public string type;//类名
        public string jsonDatas;//类中被序列化的JSON字段数据（类似于 "playerName":"Dr Charles" ）

        public override string ToString()
        {
            return "type: " + type + " | JSON: " + jsonDatas;
        }
    }

    /// <summary>
    /// 序列化和反序列化方法，对于节点的序列化和反序列化方法
    /// </summary>
	public static class JsonSerializer
    {
        public static JsonElement Serialize(object obj)
        {
            JsonElement elem = new JsonElement();

            elem.type = obj.GetType().AssemblyQualifiedName;
#if UNITY_EDITOR
            elem.jsonDatas = EditorJsonUtility.ToJson(obj);//ToJson：生成对象的JSON表示形式
#else
			elem.jsonDatas = JsonUtility.ToJson(obj);
#endif

            return elem;
        }

        /// <summary>
        /// [泛型]反序列化指定的JsonElement参数，将其中的数据覆盖到新的T类型对象上返回
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
            ///FromJsonOverwrite：通过读取对象的JSON表示形式覆盖其数据
            EditorJsonUtility.FromJsonOverwrite(e.jsonDatas, obj);//将参数e的JSON数据覆盖到obj上（即将JsonElement中的数据赋值到类型为T的obj上）
#else
			JsonUtility.FromJsonOverwrite(e.jsonDatas, obj);
#endif

            return obj;
        }

        /// <summary>序列化节点成Json形式</summary>
		public static JsonElement SerializeNode(BaseNode node)
        {
            return Serialize(node);
        }
        /// <summary>反序列化成节点</summary>
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