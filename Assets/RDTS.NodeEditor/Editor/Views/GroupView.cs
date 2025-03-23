using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RDTS.NodeEditor
{
    /// <summary>
    /// Group元素如何绘制、处理
    /// </summary>
    public class GroupView : UnityEditor.Experimental.GraphView.Group
    {
        public BaseGraphView owner;//关联的GrapgView
        public Group group;//对应的group

        Label titleLabel;//标题
        ColorField colorField;//颜色字段

        readonly string groupStyle = "GraphStyles/GroupView";

        public GroupView()
        {
            styleSheets.Add(Resources.Load<StyleSheet>(groupStyle));//样式
        }

        ///ContextualMenuPopulateEvent：当上下文菜单需要菜单项时发送的事件
        private static void BuildContextualMenu(ContextualMenuPopulateEvent evt) { }

        /// <summary>
        /// 初始化GroupView：关联Group、BaseGraphView，设置标题、位置，添加颜色字段并注册回调，初始内部节点
        /// </summary>
        /// <param name="graphView"></param>
        /// <param name="block"></param>
        public void Initialize(BaseGraphView graphView, Group block)
        {
            group = block;
            owner = graphView;

            title = block.title;
            SetPosition(block.position);//设置位置

            //ContextualMenuManipulator：用于在用户单击鼠标右键或按下键盘上的菜单键时显示上下文菜单的操控器
            this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));

            headerContainer.Q<TextField>().RegisterCallback<ChangeEvent<string>>(TitleChangedCallback);
            titleLabel = headerContainer.Q<Label>();

            colorField = new ColorField { value = group.color, name = "headerColorPicker" };
            colorField.RegisterValueChangedCallback(e =>//注册回调，以在改变color字段颜色时设置group的颜色
            {
                UpdateGroupColor(e.newValue);
            });
            UpdateGroupColor(group.color);

            ///Scope：此类可用于将节点分组到公共区域，或称为作用域 (Scope)。此类包括自动对作用域进行大小调整和定位以容纳节点组的方法
            ///headerContainer：返回作用域的标头容器。使用此标头容器可以自定义作用域的标头
            headerContainer.Add(colorField);//在标头中添加颜色字段

            InitializeInnerNodes();//初始化内部节点
        }

        /// <summary>
        /// 初始化group内部节点：移除不存在的节点，添加存在的节点
        /// </summary>
        void InitializeInnerNodes()
        {
            foreach (var nodeGUID in group.innerNodeGUIDs.ToList())//遍历在group中储存的节点
            {
                if (!owner.graph.nodesPerGUID.ContainsKey(nodeGUID))
                {
                    Debug.LogWarning("Node GUID not found: " + nodeGUID);
                    group.innerNodeGUIDs.Remove(nodeGUID);
                    continue;
                }
                var node = owner.graph.nodesPerGUID[nodeGUID];
                var nodeView = owner.nodeViewsPerNode[node];

                AddElement(nodeView);//添加此节点到group中
            }
        }

        /// <summary>
        /// 将元素添加到此group时调用：将BaseNodeView类型的元素添加到group中
        /// </summary>
        /// <param name="elements">添加的元素</param>
        protected override void OnElementsAdded(IEnumerable<GraphElement> elements)
        {
            foreach (var element in elements)
            {
                var node = element as BaseNodeView;

                // Adding an element that is not a node currently supported 添加不是当前支持的节点的元素（group中没有的BaseNodeView元素）
                if (node == null)
                    continue;

                if (!group.innerNodeGUIDs.Contains(node.nodeTarget.GUID))
                    group.innerNodeGUIDs.Add(node.nodeTarget.GUID);
            }
            base.OnElementsAdded(elements);
        }

        /// <summary>
        /// 将从此组中删除元素时调用：删除group中的节点
        /// </summary>
        /// <param name="elements">删除的元素</param>
        protected override void OnElementsRemoved(IEnumerable<GraphElement> elements)
        {
            // Only remove the nodes when the group exists in the hierarchy 
            // 仅当group存在于层次结构中时才删除节点
            if (parent != null)
            {
                foreach (var elem in elements)
                {
                    if (elem is BaseNodeView nodeView)
                    {
                        group.innerNodeGUIDs.Remove(nodeView.nodeTarget.GUID);
                    }
                }
            }

            base.OnElementsRemoved(elements);
        }

        /// <summary>设置group的颜色</summary>
        public void UpdateGroupColor(Color newColor)
        {
            group.color = newColor;
            style.backgroundColor = newColor;
        }

        /// <summary>设置group的标题</summary>
        void TitleChangedCallback(ChangeEvent<string> e)
        {
            group.title = e.newValue;
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);

            group.position = newPos;
        }
    }
}