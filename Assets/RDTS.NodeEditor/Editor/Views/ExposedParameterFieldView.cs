using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System.Linq;

namespace RDTS.NodeEditor
{
    /// <summary>
    /// ExposedParameter字段的视图：表示可折叠的那一行元素
    /// </summary>
	public class ExposedParameterFieldView : BlackboardField//BlackboardField：一个表示图形字段的 GraphElement
    {
        protected BaseGraphView graphView;

        public ExposedParameter parameter { get; private set; }

        public ExposedParameterFieldView(BaseGraphView graphView, ExposedParameter param) : base(null, param.name, param.shortType)
        {
            this.graphView = graphView;//关联 graph
            parameter = param;//关联ExposedParameter，便于后续删除
            this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));//添加菜单
            this.Q("icon").AddToClassList("parameter-" + param.shortType);
            this.Q("icon").visible = true;//显示字段的图标（小圆点）

            (this.Q("textField") as TextField).RegisterValueChangedCallback((e) => {
                param.name = e.newValue;
                text = e.newValue;
                graphView.graph.UpdateExposedParameterName(param, e.newValue);//更新名称
            });
        }

        /// <summary>字段菜单</summary>
		void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            ///OpenTextEditor：打开 TextField 以编辑 BlackboardField 中的文本
            ///AlwaysEnabled：始终返回 Status.Enabled 的状态回调
            //字段重命名
            evt.menu.AppendAction("Rename", (a) => OpenTextEditor(), DropdownMenuAction.AlwaysEnabled);
            //移除一个ExposedParameter
            evt.menu.AppendAction("Delete", (a) => graphView.graph.RemoveExposedParameter(parameter), DropdownMenuAction.AlwaysEnabled);

            evt.StopPropagation();
        }
    }
}