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
    // 这是一个解决方法类，用于在 [SerializeReference] 上应用的 PropertyFields 周围添加一个包装器。
    // 因为属性字段绑定非常慢，并且 AppliedModifiedProperties() 在调用时重新创建 ScriptableObject（这在 NGP 中会导致重新构建graph）。
    // 我们不能直接使用 PropertyFields，该类提供了一组函数来为 Exposed Parameters 创建 PropertyFields 但不附加到图形上，因此当我们调用
    // AppliedModifiedProperties 时，graph不会重新构建。缺点是我们必须检查自己的值变化，然后将它们应用于图形参数，但这比每次更改参数或
    // 设置时都必须重新创建graph要好得多。
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
            serializedObject.Update();//更新序列化对象的表示形式
            ///FindProperty：按名称查找序列化属性
            serializedParameters = serializedObject.FindProperty(nameof(ExposedParameterWorkaround.parameters));//
        }

        public VisualElement GetParameterValueField(ExposedParameter parameter, Action<object> valueChangedCallback)
        {
            serializedObject.Update();
            int propIndex = FindPropertyIndex(parameter);//获取参数在exposedParameterObject.parameters中的索引值
            ///GetArrayElementAtIndex：返回数组中指定索引处的元素
            var field = new PropertyField(serializedParameters.GetArrayElementAtIndex(propIndex));
            field.Bind(serializedObject);//将 SerializedObject 绑定到元素层级视图中的字段

            VisualElement view = new VisualElement();
            view.Add(field);

            oldParameterValues[parameter] = parameter.value;
            view.Add(new IMGUIContainer(() =>
            {
                ///调用来渲染和处理 IMGUI 事件的函数
                if (oldParameterValues.TryGetValue(parameter, out var value))
                {
                    if (parameter.value != null && !parameter.value.Equals(value))
                        valueChangedCallback(parameter.value);
                }
                oldParameterValues[parameter] = parameter.value;
            }));

            // Disallow picking scene objects when the graph is not linked to a scene
            // 当graph没有链接到scene时，不允许选取scene对象
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
            serializedObject.Update();//如果在多个帧中保留对 SerializedObject 实例的引用，则必须确保首先手动调用其 SerializedObject.Update 方法，然后再从此实例中读取任何数据，因为一个或多个目标对象可能已在其他位置修改过
            int propIndex = FindPropertyIndex(parameter);//参数在列表中的索引值
            var serializedParameter = serializedParameters.GetArrayElementAtIndex(propIndex);
            ///managedReferenceValue：分配给具有 SerializeReference 属性的字段的对象
            serializedParameter.managedReferenceValue = exposedParameterObject.parameters[propIndex];
            ///FindPropertyRelative：从当前属性的相关路径检索 SerializedProperty
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
                ///调用来渲染和处理 IMGUI 事件的函数
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

        /// <summary>查找指定参数的索引值</summary>
        int FindPropertyIndex(ExposedParameter param) => exposedParameterObject.parameters.FindIndex(p => p == param);

        public void Dispose()
        {
            GameObject.DestroyImmediate(exposedParameterObject);
        }
    }
}