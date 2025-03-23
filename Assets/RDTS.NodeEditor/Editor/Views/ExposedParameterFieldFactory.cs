using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;

namespace RDTS.NodeEditor
{
    // So, this is a workaround class to add a wrapper around PropertyFields applied on [SerializeReference].
    // Because Property Fields binding being extremely slow (https://forum.unity.com/threads/propertyfield-extremely-slow.966191/)
    // and AppliedModifiedProperties() re-creating the ScriptableObject when called (which in NGP causes the graph to be re-built)
    // we can't use PropertyFields directly. This class provides a set of function to create PropertyFields for Exposed Parameters
    // but without being attached to the graph, so when we call AppliedModifiedProperties, the graph is not re-built.
    // The drawback is that we have to check ourselves for value changes and then apply them on the graph parameters,
    // but it's far better than having to re-create the graph every time a parameter or a setting is changed.
    // ����һ����������࣬������ [SerializeReference] ��Ӧ�õ� PropertyFields ��Χ���һ����װ����
    // ��Ϊ�����ֶΰ󶨷ǳ��������� AppliedModifiedProperties() �ڵ���ʱ���´��� ScriptableObject������ NGP �лᵼ�����¹���graph����
    // ���ǲ���ֱ��ʹ�� PropertyFields�������ṩ��һ�麯����Ϊ Exposed Parameters ���� PropertyFields �������ӵ�ͼ���ϣ���˵����ǵ���
    // AppliedModifiedProperties ʱ��graph�������¹�����ȱ�������Ǳ������Լ���ֵ�仯��Ȼ������Ӧ����ͼ�β����������ÿ�θ��Ĳ�����
    // ����ʱ���������´���graphҪ�õöࡣ
    public class ExposedParameterFieldFactory : IDisposable
    {
        BaseGraph graph;
        [SerializeField]
        ExposedParameterWorkaround exposedParameterObject;
        SerializedObject serializedObject;
        SerializedProperty serializedParameters;

        Dictionary<ExposedParameter, object> oldParameterValues = new Dictionary<ExposedParameter, object>();
        Dictionary<ExposedParameter, ExposedParameter.Settings> oldParameterSettings = new Dictionary<ExposedParameter, ExposedParameter.Settings>();

        public ExposedParameterFieldFactory(BaseGraph graph, List<ExposedParameter> customParameters = null)
        {
            this.graph = graph;

            exposedParameterObject = ScriptableObject.CreateInstance<ExposedParameterWorkaround>();
            exposedParameterObject.graph = graph;
            exposedParameterObject.hideFlags = HideFlags.HideAndDontSave ^ HideFlags.NotEditable;
            serializedObject = new SerializedObject(exposedParameterObject);
            UpdateSerializedProperties(customParameters);
        }

        public void UpdateSerializedProperties(List<ExposedParameter> parameters = null)
        {
            if (parameters != null)
                exposedParameterObject.parameters = parameters;
            else
                exposedParameterObject.parameters = graph.exposedParameters;
            serializedObject.Update();//�������л�����ı�ʾ��ʽ
            ///FindProperty�������Ʋ������л�����
            serializedParameters = serializedObject.FindProperty(nameof(ExposedParameterWorkaround.parameters));//
        }

        public VisualElement GetParameterValueField(ExposedParameter parameter, Action<object> valueChangedCallback)
        {
            serializedObject.Update();
            int propIndex = FindPropertyIndex(parameter);//��ȡ������exposedParameterObject.parameters�е�����ֵ
            ///GetArrayElementAtIndex������������ָ����������Ԫ��
            var field = new PropertyField(serializedParameters.GetArrayElementAtIndex(propIndex));
            field.Bind(serializedObject);//�� SerializedObject �󶨵�Ԫ�ز㼶��ͼ�е��ֶ�

            VisualElement view = new VisualElement();
            view.Add(field);

            oldParameterValues[parameter] = parameter.value;
            view.Add(new IMGUIContainer(() =>
            {
                ///��������Ⱦ�ʹ��� IMGUI �¼��ĺ���
                if (oldParameterValues.TryGetValue(parameter, out var value))
                {
                    if (parameter.value != null && !parameter.value.Equals(value))
                        valueChangedCallback(parameter.value);
                }
                oldParameterValues[parameter] = parameter.value;
            }));

            // Disallow picking scene objects when the graph is not linked to a scene
            // ��graphû�����ӵ�sceneʱ��������ѡȡscene����
            if (!this.graph.IsLinkedToScene())
            {
                var objectField = view.Q<ObjectField>();
                if (objectField != null)
                    objectField.allowSceneObjects = false;
            }
            return view;
        }

        public VisualElement GetParameterSettingsField(ExposedParameter parameter, Action<object> valueChangedCallback)
        {
            serializedObject.Update();//����ڶ��֡�б����� SerializedObject ʵ�������ã������ȷ�������ֶ������� SerializedObject.Update ������Ȼ���ٴӴ�ʵ���ж�ȡ�κ����ݣ���Ϊһ������Ŀ����������������λ���޸Ĺ�
            int propIndex = FindPropertyIndex(parameter);//�������б��е�����ֵ
            var serializedParameter = serializedParameters.GetArrayElementAtIndex(propIndex);
            ///managedReferenceValue����������� SerializeReference ���Ե��ֶεĶ���
            serializedParameter.managedReferenceValue = exposedParameterObject.parameters[propIndex];
            ///FindPropertyRelative���ӵ�ǰ���Ե����·������ SerializedProperty
            var serializedSettings = serializedParameter.FindPropertyRelative(nameof(ExposedParameter.settings));
            serializedSettings.managedReferenceValue = exposedParameterObject.parameters[propIndex].settings;
            var settingsField = new PropertyField(serializedSettings);
            settingsField.Bind(serializedObject);

            VisualElement view = new VisualElement();
            view.Add(settingsField);

            // TODO: see if we can replace this with an event
            oldParameterSettings[parameter] = parameter.settings;
            view.Add(new IMGUIContainer(() =>
            {
                ///��������Ⱦ�ʹ��� IMGUI �¼��ĺ���
                if (oldParameterSettings.TryGetValue(parameter, out var settings))
                {
                    if (!settings.Equals(parameter.settings))
                        valueChangedCallback(parameter.settings);
                }
                oldParameterSettings[parameter] = parameter.settings;
            }));

            return view;
        }

        public void ResetOldParameter(ExposedParameter parameter)
        {
            oldParameterValues.Remove(parameter);
            oldParameterSettings.Remove(parameter);
        }

        /// <summary>����ָ������������ֵ</summary>
        int FindPropertyIndex(ExposedParameter param) => exposedParameterObject.parameters.FindIndex(p => p == param);

        public void Dispose()
        {
            GameObject.DestroyImmediate(exposedParameterObject);
        }
    }
}