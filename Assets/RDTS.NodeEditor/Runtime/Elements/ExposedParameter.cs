using System;
using System.Collections.Generic;
using UnityEngine;

namespace RDTS.NodeEditor
{
    /// <summary>
    ///  能够暴露在面板中的元素，包含一个Settings类，guid、name、type等参数
    /// </summary>
	[Serializable]
    public class ExposedParameter : ISerializationCallbackReceiver
    {
        [Serializable]
        public class Settings
        {
            public bool isHidden = false;
            public bool expanded = false;

            [SerializeField]
            internal string guid = null;

            /// <summary>判断是否是不为null的Settings类型对象</summary>
            public override bool Equals(object obj)
            {
                if (obj is Settings s && s != null)
                    return Equals(s);
                else
                    return false;
            }

            /// <summary>判断是否Settings类型对象的isHidden、expanded参数是否相同值</summary>
            public virtual bool Equals(Settings param)
                => isHidden == param.isHidden && expanded == param.expanded;

            public override int GetHashCode() => base.GetHashCode();
        }

        public string guid; // unique id to keep track of the parameter 用于跟踪参数的唯一 ID
        public string name;
        [Obsolete("Use GetValueType()")]//Obsolete特性：标记不再使用的程序元素。 将元素标记为已过时通知用户，在产品的未来版本中可能会删除该元素。
        public string type;
        [Obsolete("Use value instead")]
        public SerializableObject serializedValue;
        public bool input = true;
        [SerializeReference]
        public Settings settings;
        public string shortType => GetValueType()?.Name;

        public void Initialize(string name, object value)
        {
            guid = Guid.NewGuid().ToString(); // Generated once and unique per parameter  生成一次且每个参数唯一
            settings = CreateSettings();//创建一个新Settings
            settings.guid = guid;
            this.name = name;
            this.value = value;
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            // SerializeReference migration step:
#pragma warning disable CS0618
            if (serializedValue?.value != null) // old serialization system can't serialize null values
            {
                value = serializedValue.value;
                Debug.Log("Migrated: " + serializedValue.value + " | " + serializedValue.serializedName);
                serializedValue.value = null;
            }
#pragma warning restore CS0618
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize() { }

        protected virtual Settings CreateSettings() => new Settings();

        /// <summary>ExposedParameter及其派生类的值</summary>
        public virtual object value { get; set; }
        /// <summary>获取字段value的类型，value为null则返回object</summary>
        public virtual Type GetValueType() => value == null ? typeof(object) : value.GetType();

        /// <summary>ExposedParameter类型缓存 （ExposedParameter.GetValueType(), 当前实例准确运行时的类型）</summary>
        static Dictionary<Type, Type> exposedParameterTypeCache = new Dictionary<Type, Type>();
        internal ExposedParameter Migrate()
        {
            if (exposedParameterTypeCache.Count == 0)
            {
                ///AppDomain：表示应用程序域，它是一个应用程序在其中执行的独立环境
                ///CurrentDomain：获取当前 Thread 的当前应用程序域
                foreach (var type in AppDomain.CurrentDomain.GetAllTypes())//遍历当前应用程序域中的程序集中所有当前实例准确运行时的类型
                {
                    ///IsAbstract：如果 Type 是抽象的，则为 true；否则为 false
                    if (type.IsSubclassOf(typeof(ExposedParameter)) && !type.IsAbstract)//是ExposedParameter的子类 且 不是抽象的
                    {
                        var paramType = Activator.CreateInstance(type) as ExposedParameter;
                        exposedParameterTypeCache[paramType.GetValueType()] = type;
                    }
                }
            }
#pragma warning disable CS0618 // Use of obsolete fields
            var oldType = Type.GetType(type);
#pragma warning restore CS0618
            if (oldType == null || !exposedParameterTypeCache.TryGetValue(oldType, out var newParamType))
                return null;

            var newParam = Activator.CreateInstance(newParamType) as ExposedParameter;

            newParam.guid = guid;
            newParam.name = name;
            newParam.input = input;
            newParam.settings = newParam.CreateSettings();
            newParam.settings.guid = guid;

            return newParam;

        }

        /// <summary>
        /// 判断两个ExposedParameter类型对象是否为相同的实例，
        /// 若不相同则guid是否相同
        /// </summary>
        /// <param name="param1"></param>
        /// <param name="param2"></param>
        /// <returns></returns>
        public static bool operator ==(ExposedParameter param1, ExposedParameter param2)
        {
            //判断参数是否为相同的实例
            ///ReferenceEquals：确定指定的Object实例是否是相同的实例。 相同为true，否则为false
            if (ReferenceEquals(param1, null) && ReferenceEquals(param2, null))//都是null
                return true;
            if (ReferenceEquals(param1, param2))//相同且不为null
                return true;
            if (ReferenceEquals(param1, null))//参数1为null
                return false;
            if (ReferenceEquals(param2, null))//参数2为null
                return false;

            //若参数均不为null，且不相同，则判断guid是否相等
            return param1.Equals(param2);
        }

        public static bool operator !=(ExposedParameter param1, ExposedParameter param2) => !(param1 == param2);

        /// <summary>判断两个ExposedParameter的guid是否相等</summary>
        public bool Equals(ExposedParameter parameter) => guid == parameter.guid;

        public override bool Equals(object obj)
        {
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))//指定对象为null 或 当前对象的类型(ExposedParameter或子类)与指定对象的类型不相等
                return false;
            else
                return Equals((ExposedParameter)obj);//比较两个对象的guid是否相等
        }

        public override int GetHashCode() => guid.GetHashCode();

        /// <summary>克隆此ExposedParameter对象</summary>
        public ExposedParameter Clone()
        {
            var clonedParam = Activator.CreateInstance(GetType()) as ExposedParameter;

            clonedParam.guid = guid;
            clonedParam.name = name;
            clonedParam.input = input;
            clonedParam.settings = settings;
            clonedParam.value = value;

            return clonedParam;
        }
    }

    // Due to polymorphic constraints with [SerializeReference] we need to explicitly create a class for
    // every parameter type available in the graph (i.e. templating doesn't work)
    //由于 [SerializeReference] 的多态约束，我们需要为图中可用的每个参数类型显式创建一个类（即模板不起作用）
    [System.Serializable]
    public class ColorParameter : ExposedParameter
    {
        public enum ColorMode
        {
            Default,
            HDR
        }

        [Serializable]
        public class ColorSettings : Settings
        {
            public ColorMode mode;

            public override bool Equals(Settings param)
                => base.Equals(param) && mode == ((ColorSettings)param).mode;
        }

        [SerializeField] Color val;

        public override object value { get => val; set => val = (Color)value; }
        protected override Settings CreateSettings() => new ColorSettings();
    }

    [System.Serializable]
    public class FloatParameter : ExposedParameter
    {
        public enum FloatMode
        {
            Default,
            Slider,
        }

        [Serializable]
        public class FloatSettings : Settings
        {
            public FloatMode mode;
            public float min = 0;
            public float max = 1;

            public override bool Equals(Settings param)
                => base.Equals(param) && mode == ((FloatSettings)param).mode && min == ((FloatSettings)param).min && max == ((FloatSettings)param).max;
        }

        [SerializeField] float val;

        public override object value { get => val; set => val = (float)value; }
        protected override Settings CreateSettings() => new FloatSettings();
    }

    [System.Serializable]
    public class Vector2Parameter : ExposedParameter
    {
        public enum Vector2Mode
        {
            Default,
            MinMaxSlider,
        }

        [Serializable]
        public class Vector2Settings : Settings
        {
            public Vector2Mode mode;
            public float min = 0;
            public float max = 1;

            public override bool Equals(Settings param)
                => base.Equals(param) && mode == ((Vector2Settings)param).mode && min == ((Vector2Settings)param).min && max == ((Vector2Settings)param).max;
        }

        [SerializeField] Vector2 val;

        public override object value { get => val; set => val = (Vector2)value; }
        protected override Settings CreateSettings() => new Vector2Settings();
    }

    [System.Serializable]
    public class Vector3Parameter : ExposedParameter
    {
        [SerializeField] Vector3 val;

        public override object value { get => val; set => val = (Vector3)value; }
    }

    [System.Serializable]
    public class Vector4Parameter : ExposedParameter
    {
        [SerializeField] Vector4 val;

        public override object value { get => val; set => val = (Vector4)value; }
    }

    [System.Serializable]
    public class IntParameter : ExposedParameter
    {
        public enum IntMode
        {
            Default,
            Slider,
        }

        [Serializable]
        public class IntSettings : Settings
        {
            public IntMode mode;
            public int min = 0;
            public int max = 10;

            public override bool Equals(Settings param)
                => base.Equals(param) && mode == ((IntSettings)param).mode && min == ((IntSettings)param).min && max == ((IntSettings)param).max;
        }

        [SerializeField] int val;

        public override object value { get => val; set => val = (int)value; }
        protected override Settings CreateSettings() => new IntSettings();
    }

    [System.Serializable]
    public class Vector2IntParameter : ExposedParameter
    {
        [SerializeField] Vector2Int val;

        public override object value { get => val; set => val = (Vector2Int)value; }
    }

    [System.Serializable]
    public class Vector3IntParameter : ExposedParameter
    {
        [SerializeField] Vector3Int val;

        public override object value { get => val; set => val = (Vector3Int)value; }
    }

    [System.Serializable]
    public class DoubleParameter : ExposedParameter
    {
        [SerializeField] Double val;

        public override object value { get => val; set => val = (Double)value; }
    }

    [System.Serializable]
    public class LongParameter : ExposedParameter
    {
        [SerializeField] long val;

        public override object value { get => val; set => val = (long)value; }
    }

    [System.Serializable]
    public class StringParameter : ExposedParameter
    {
        [SerializeField] string val;

        public override object value { get => val; set => val = (string)value; }
        public override Type GetValueType() => typeof(String);
    }

    [System.Serializable]
    public class RectParameter : ExposedParameter
    {
        [SerializeField] Rect val;

        public override object value { get => val; set => val = (Rect)value; }
    }

    [System.Serializable]
    public class RectIntParameter : ExposedParameter
    {
        [SerializeField] RectInt val;

        public override object value { get => val; set => val = (RectInt)value; }
    }

    [System.Serializable]
    public class BoundsParameter : ExposedParameter
    {
        [SerializeField] Bounds val;

        public override object value { get => val; set => val = (Bounds)value; }
    }

    [System.Serializable]
    public class BoundsIntParameter : ExposedParameter
    {
        [SerializeField] BoundsInt val;

        public override object value { get => val; set => val = (BoundsInt)value; }
    }

    [System.Serializable]
    public class AnimationCurveParameter : ExposedParameter
    {
        [SerializeField] AnimationCurve val;

        public override object value { get => val; set => val = (AnimationCurve)value; }
        public override Type GetValueType() => typeof(AnimationCurve);
    }

    [System.Serializable]
    public class GradientParameter : ExposedParameter
    {
        public enum GradientColorMode
        {
            Default,
            HDR,
        }

        [Serializable]
        public class GradientSettings : Settings
        {
            public GradientColorMode mode;

            public override bool Equals(Settings param)
                => base.Equals(param) && mode == ((GradientSettings)param).mode;
        }

        [SerializeField] Gradient val;
        [SerializeField, GradientUsage(true)] Gradient hdrVal;

        public override object value { get => val; set => val = (Gradient)value; }
        public override Type GetValueType() => typeof(Gradient);
        protected override Settings CreateSettings() => new GradientSettings();
    }

    [System.Serializable]
    public class GameObjectParameter : ExposedParameter
    {
        [SerializeField] GameObject val;

        public override object value { get => val; set => val = (GameObject)value; }
        public override Type GetValueType() => typeof(GameObject);
    }

    [System.Serializable]
    public class BoolParameter : ExposedParameter
    {
        [SerializeField] bool val;

        public override object value { get => val; set => val = (bool)value; }
    }

    [System.Serializable]
    public class Texture2DParameter : ExposedParameter
    {
        [SerializeField] Texture2D val;

        public override object value { get => val; set => val = (Texture2D)value; }
        public override Type GetValueType() => typeof(Texture2D);
    }

    [System.Serializable]
    public class RenderTextureParameter : ExposedParameter
    {
        [SerializeField] RenderTexture val;

        public override object value { get => val; set => val = (RenderTexture)value; }
        public override Type GetValueType() => typeof(RenderTexture);
    }

    [System.Serializable]
    public class MeshParameter : ExposedParameter
    {
        [SerializeField] Mesh val;

        public override object value { get => val; set => val = (Mesh)value; }
        public override Type GetValueType() => typeof(Mesh);
    }

    [System.Serializable]
    public class MaterialParameter : ExposedParameter
    {
        [SerializeField] Material val;

        public override object value { get => val; set => val = (Material)value; }
        public override Type GetValueType() => typeof(Material);
    }
}