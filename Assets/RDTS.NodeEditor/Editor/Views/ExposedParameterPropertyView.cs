using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor;
using UnityEngine;

namespace RDTS.NodeEditor
{
    /// <summary>
    /// ExposedParameter的属性视图（BlackboardRow展开后的内容）
    /// </summary>
	public class ExposedParameterPropertyView : VisualElement
    {
        protected BaseGraphView baseGraphView;

        public ExposedParameter parameter { get; private set; }

        public Toggle hideInInspector { get; private set; }

        public ExposedParameterPropertyView(BaseGraphView graphView, ExposedParameter param)
        {
            baseGraphView = graphView;
            parameter = param;

            var field = graphView.exposedParameterFactory.GetParameterSettingsField(param, (newValue) => {
                param.settings = newValue as ExposedParameter.Settings;
            });

            Add(field);
        }
    }
}