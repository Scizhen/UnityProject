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
    /// ExposedParameter元素的节点的视图，实现ExposedParameter类型在graph中的绘制视图
    /// </summary>
    [NodeCustomEditor(typeof(ParameterNode))]
    public class ParameterNodeView : BaseNodeView
    {
        ParameterNode parameterNode;

        /// <summary>
        /// 关联对应的ParameterNode，添加一个ParameterAccessor的枚举元素，查找并删除展开/折叠按钮，更新parameterNode的标题
        /// </summary>
        /// <param name="fromInspector"></param>
        public override void Enable(bool fromInspector = false)
        {
            parameterNode = nodeTarget as ParameterNode;

            ///EnumField:生成一个用于在枚举值之间切换的下拉列表
            EnumField accessorSelector = new EnumField(parameterNode.accessor);
            accessorSelector.SetValueWithoutNotify(parameterNode.accessor);
            //注册回调：更新枚举值，刷新端口，重绘元素
            accessorSelector.RegisterValueChangedCallback(evt =>
            {
                parameterNode.accessor = (ParameterAccessor)evt.newValue;
                UpdatePort();
                controlsContainer.MarkDirtyRepaint();
                ForceUpdatePorts();
            });

            UpdatePort();
            //添加枚举类型元素
            controlsContainer.Add(accessorSelector);

            //    Find and remove expand/collapse button 查找并删除展开/折叠按钮
            titleContainer.Remove(titleContainer.Q("title-button-container"));
            //    Remove Port from the #content 移除顶部内容容器
            // topContainer.parent.Remove(topContainer);
            //    Add Port to the #title  将端口添加到标题容器中
            /// topContainer：包含输入和输出容器的整个顶部区域
            //titleContainer.Add(topContainer);

            parameterNode.onParameterChanged += UpdateView;
            UpdateView();
        }

        /// <summary>更新parameterNode的标题：exposed parameter名称来作为节点标题</summary>
        void UpdateView()
        {
            title = parameterNode.parameter?.name;
        }


        void UpdatePort()
        {
            if (parameterNode.accessor == ParameterAccessor.Set)
            {
                titleContainer.AddToClassList("input");
            }
            else
            {
                titleContainer.RemoveFromClassList("input");
            }
        }
    }

}