using System;
using System.Collections.Generic;
using UnityEngine;

namespace RDTS.NodeEditor
{
    /// <summary>
    ///  �ܹ���¶������е�Ԫ�أ�����һ��Settings�࣬guid��name��type�Ȳ���
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

            /// <summary>�ж��Ƿ��ǲ�Ϊnull��Settings���Ͷ���</summary>
            public override bool Equals(object obj)
            {
                if (obj is Settings s && s != null)
                    return Equals(s);
                else
                    return false;
            }

            /// <summary>�ж��Ƿ�Settings���Ͷ����isHidden��expanded�����Ƿ���ֵͬ</summary>
            public virtual bool Equals(Settings param)
                => isHidden == param.isHidden && expanded == param.expanded;

            public override int GetHashCode() => base.GetHashCode();
        }

        public string guid; // unique id to keep track of the parameter ���ڸ��ٲ�����Ψһ ID
        public string name;
        [Obsolete("Use GetValueType()")]//Obsolete���ԣ���ǲ���ʹ�õĳ���Ԫ�ء� ��Ԫ�ر��Ϊ�ѹ�ʱ֪ͨ�û����ڲ�Ʒ��δ���汾�п��ܻ�ɾ����Ԫ�ء�
        public string type;
        [Obsolete("Use value instead")]
        public SerializableObject serializedValue;
        public bool input = true;
        [SerializeReference]
        public Settings settings;
        public string shortType => GetValueType()?.Name;

        public void Initialize(string name, object value)
        {
            guid = Guid.NewGuid().ToString(); // Generated once and unique per parameter  ����һ����ÿ������Ψһ
            settings = CreateSettings();//����һ����Settings
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

        /// <summary>ExposedParameter�����������ֵ</summary>
        public virtual object value { get; set; }
        /// <summary>��ȡ�ֶ�value�����ͣ�valueΪnull�򷵻�object</summary>
        public virtual Type GetValueType() => value == null ? typeof(object) : value.GetType();

        /// <summary>ExposedParameter���ͻ��� ��ExposedParameter.GetValueType(), ��ǰʵ��׼ȷ����ʱ�����ͣ�</summary>
        static Dictionary<Type, Type> exposedParameterTypeCache = new Dictionary<Type, Type>();
        internal ExposedParameter Migrate()
        {
            if (exposedParameterTypeCache.Count == 0)
            {
                ///AppDomain����ʾӦ�ó���������һ��Ӧ�ó���������ִ�еĶ�������
                ///CurrentDomain����ȡ��ǰ Thread �ĵ�ǰӦ�ó�����
                foreach (var type in AppDomain.CurrentDomain.GetAllTypes())//������ǰӦ�ó������еĳ��������е�ǰʵ��׼ȷ����ʱ������
                {
                    ///IsAbstract����� Type �ǳ���ģ���Ϊ true������Ϊ false
                    if (type.IsSubclassOf(typeof(ExposedParameter)) && !type.IsAbstract)//��ExposedParameter������ �� ���ǳ����
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
        /// �ж�����ExposedParameter���Ͷ����Ƿ�Ϊ��ͬ��ʵ����
        /// ������ͬ��guid�Ƿ���ͬ
        /// </summary>
        /// <param name="param1"></param>
        /// <param name="param2"></param>
        /// <returns></returns>
        public static bool operator ==(ExposedParameter param1, ExposedParameter param2)
        {
            //�жϲ����Ƿ�Ϊ��ͬ��ʵ��
            ///ReferenceEquals��ȷ��ָ����Objectʵ���Ƿ�����ͬ��ʵ���� ��ͬΪtrue������Ϊfalse
            if (ReferenceEquals(param1, null) && ReferenceEquals(param2, null))//����null
                return true;
            if (ReferenceEquals(param1, param2))//��ͬ�Ҳ�Ϊnull
                return true;
            if (ReferenceEquals(param1, null))//����1Ϊnull
                return false;
            if (ReferenceEquals(param2, null))//����2Ϊnull
                return false;

            //����������Ϊnull���Ҳ���ͬ�����ж�guid�Ƿ����
            return param1.Equals(param2);
        }

        public static bool operator !=(ExposedParameter param1, ExposedParameter param2) => !(param1 == param2);

        /// <summary>�ж�����ExposedParameter��guid�Ƿ����</summary>
        public bool Equals(ExposedParameter parameter) => guid == parameter.guid;

        public override bool Equals(object obj)
        {
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))//ָ������Ϊnull �� ��ǰ���������(ExposedParameter������)��ָ����������Ͳ����
                return false;
            else
                return Equals((ExposedParameter)obj);//�Ƚ����������guid�Ƿ����
        }

        public override int GetHashCode() => guid.GetHashCode();

        /// <summary>��¡��ExposedParameter����</summary>
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
    //���� [SerializeReference] �Ķ�̬Լ����������ҪΪͼ�п��õ�ÿ������������ʽ����һ���ࣨ��ģ�岻�����ã�
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