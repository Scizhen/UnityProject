using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System.Linq;
using System;

namespace RDTS.NodeEditor
{
    /// <summary>
    /// Parameters面板的视图脚本：关联graphView，设置标题，添加委托和回调，更新参数列表，添加按钮(右上角的“+”按钮)
    /// </summary>
	public class ExposedParameterView : PinnedElementView
    {
        protected BaseGraphView graphView;//关联的图表

        new const string title = "Parameters";// ExposedParameter 面板的标题

        readonly string exposedParameterViewStyle = "GraphProcessorStyles/ExposedParameterView";

        List<Rect> blackboardLayouts = new List<Rect>();//储存内容容器中子元素的布局（位置，尺寸）的列表

        public ExposedParameterView()
        {
            var style = Resources.Load<StyleSheet>(exposedParameterViewStyle);
            if (style != null)
                styleSheets.Add(style);
        }

        protected virtual void OnAddClicked()
        {
            ///GenericMenu：允许创建自定义上下文菜单和下拉菜单
            var parameterType = new GenericMenu();

            foreach (var paramType in GetExposedParameterTypes())//遍历ExposedParameter派生的类型，添加菜单项
                parameterType.AddItem(new GUIContent(GetNiceNameFromType(paramType)), false, () =>
                {
                    string uniqueName = "New " + GetNiceNameFromType(paramType);

                    uniqueName = GetUniqueExposedPropertyName(uniqueName);
                    graphView.graph.AddExposedParameter(uniqueName, paramType);
                });

            parameterType.ShowAsContext();///右键单击时在鼠标下显示菜单
        }

        protected string GetNiceNameFromType(Type type)
        {
            string name = type.Name;

            // Remove parameter in the name of the type if it exists
            name = name.Replace("Parameter", "");

            return ObjectNames.NicifyVariableName(name);
        }

        /// <summary>获取一个唯一的ExposedParameter的名称</summary>
        protected string GetUniqueExposedPropertyName(string name)
        {
            // Generate unique name  生成唯一名称
            string uniqueName = name;
            int i = 0;
            while (graphView.graph.exposedParameters.Any(e => e.name == name))//有名称重复的，就加递增的数字
                name = uniqueName + " " + i++;
            return name;
        }

        /// <summary>（一个一个）获取ExposedParameter派生的类型</summary>
        protected virtual IEnumerable<Type> GetExposedParameterTypes()
        {
            foreach (var type in TypeCache.GetTypesDerivedFrom<ExposedParameter>())//遍历ExposedParameter派生的类型的集合
            {
                ///IsGenericType：获取一个值，该值指示当前类型是否是泛型类型
                if (type.IsGenericType)
                    continue;

                yield return type;
            }
        }

        /// <summary>更新ExposedParameter面板(窗口)中的参数列表：参数的个数，是否展开 </summary>
        protected virtual void UpdateParameterList()
        {
            content.Clear();//情况内容容器

            foreach (var param in graphView.graph.exposedParameters)//遍历graph中的exposedParameters
            {
                ///BlackboardRow：一个可折叠的 GraphElement，用于表示 BlackboardSection 中的一行
                ///           参数1：用于BlackboardRow的绘制
                ///           参数2：用于BlackboardRow展开后的内容的绘制
                var row = new BlackboardRow(new ExposedParameterFieldView(graphView, param), new ExposedParameterPropertyView(graphView, param));
                row.expanded = param.settings.expanded;//是否已展开 BlackboardRow
                row.RegisterCallback<GeometryChangedEvent>(e => {///注册回调：进行布局计算后当元素的位置或尺寸发生变化时发送的事件
                    param.settings.expanded = row.expanded;//记录参数是否展开的状态
                });

                content.Add(row);//添加一个参数元素
            }
        }

        /// <summary>
        /// [重载]对于ExposedParameter面板(窗口)的初始化：关联graphView，设置标题，添加委托和回调，更新参数列表，添加按钮
        /// </summary>
        /// <param name="graphView"></param>
        protected override void Initialize(BaseGraphView graphView)
        {
            this.graphView = graphView;
            base.title = title;
            scrollable = true;

            graphView.onExposedParameterListChanged += UpdateParameterList;
            graphView.initialized += UpdateParameterList;
            Undo.undoRedoPerformed += UpdateParameterList;

            RegisterCallback<DragUpdatedEvent>(OnDragUpdatedEvent);//拖动的元素进入可能的放置目标时发送的事件：在ExposedParameter面板(窗口)中进行元素的拖动以移动其在列表中的位置
            RegisterCallback<DragPerformEvent>(OnDragPerformEvent);//在一个元素上拖放另一个元素时，发送到该元素的事件：更新graph中的相关储存信息
            RegisterCallback<MouseDownEvent>(OnMouseDownEvent, TrickleDown.TrickleDown);//鼠标按下回调：将内容容器中子元素的布局（位置，尺寸）存储为列表
            RegisterCallback<DetachFromPanelEvent>(OnViewClosed);//从父级分离元素前发送的事件

            UpdateParameterList();

            // Add exposed parameter button 添加一个 ExposedParameter的按钮（即右上角的“+”按钮）
            header.Add(new Button(OnAddClicked)
            {
                text = "+"
            });
        }

        void OnViewClosed(DetachFromPanelEvent evt)
            => Undo.undoRedoPerformed -= UpdateParameterList;

        void OnMouseDownEvent(MouseDownEvent evt)
        {
            blackboardLayouts = content.Children().Select(c => c.layout).ToList();//将内容容器中子元素的布局（位置，尺寸）存储为列表
        }

        /// <summary>
        /// 从鼠标位置获取插入索引
        /// </summary>
        /// <param name="pos">鼠标位置</param>
        /// <returns></returns>
        int GetInsertIndexFromMousePosition(Vector2 pos)
        {
            pos = content.WorldToLocal(pos);//转换到内容容器的位置
            // We only need to look for y axis; 只需要找y轴的数值
            float mousePos = pos.y;

            if (mousePos < 0)
                return 0;

            int index = 0;
            foreach (var layout in blackboardLayouts)//遍历内容容器中子元素的布局（位置，尺寸）
            {
                if (mousePos > layout.yMin && mousePos < layout.yMax)//找出当前鼠标光标在哪个子元素上，若找到就返回其在列表中的索引值
                    return index + 1;
                index++;
            }

            return content.childCount;//若不在子元素上，就返回列表最后的索引值
        }

        void OnDragUpdatedEvent(DragUpdatedEvent evt)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Move;//鼠标光标的指示模式：移动拖动对象
            int newIndex = GetInsertIndexFromMousePosition(evt.mousePosition);//从鼠标位置获取插入索引
            var graphSelectionDragData = DragAndDrop.GetGenericData("DragSelection");//获取当前拖动的对象

            if (graphSelectionDragData == null)//没有拖动的对象就直接返回
                return;

            foreach (var obj in graphSelectionDragData as List<ISelectable>)//遍历拖动的对象
            {
                if (obj is ExposedParameterFieldView view)//若拖动的是ExposedParameterFieldView类型
                {
                    var blackBoardRow = view.parent.parent.parent.parent.parent.parent;
                    int oldIndex = content.Children().ToList().FindIndex(c => c == blackBoardRow);
                    // Try to find the blackboard row
                    content.Remove(blackBoardRow);

                    if (newIndex > oldIndex)
                        newIndex--;

                    content.Insert(newIndex, blackBoardRow);
                }
            }
        }

        void OnDragPerformEvent(DragPerformEvent evt)
        {
            bool updateList = false;

            int newIndex = GetInsertIndexFromMousePosition(evt.mousePosition);
            foreach (var obj in DragAndDrop.GetGenericData("DragSelection") as List<ISelectable>)
            {
                if (obj is ExposedParameterFieldView view)
                {
                    if (!updateList)
                        graphView.RegisterCompleteObjectUndo("Moved parameters");

                    int oldIndex = graphView.graph.exposedParameters.FindIndex(e => e == view.parameter);
                    var parameter = graphView.graph.exposedParameters[oldIndex];
                    graphView.graph.exposedParameters.RemoveAt(oldIndex);

                    // Patch new index after the remove operation: 删除操作后修补新索引
                    if (newIndex > oldIndex)
                        newIndex--;

                    graphView.graph.exposedParameters.Insert(newIndex, parameter);

                    updateList = true;
                }
            }

            if (updateList)
            {
                graphView.graph.NotifyExposedParameterListChanged();
                evt.StopImmediatePropagation();
                UpdateParameterList();
            }
        }
    }
}