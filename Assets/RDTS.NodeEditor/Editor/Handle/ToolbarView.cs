using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEditor;
using System.Linq;
using System;

using Status = UnityEngine.UIElements.DropdownMenuAction.Status;

namespace RDTS.NodeEditor
{
    public class ToolbarView : VisualElement
    {
        /// <summary>控件类型</summary>
		protected enum ElementType
        {
            Button,
            ButtonIcon,
            Toggle,
            ToggleIcon,
            DropDownButton,
            Separator,
            Custom,
            FlexibleSpace,
        }

        protected class ToolbarButtonData
        {
            public GUIContent content;
            public Texture2D icon;
            public ElementType type;
            public bool value;
            public bool visible = true;
            public Action buttonCallback;
            public Action<bool> toggleCallback;
            public int size;
            public Action customDrawFunction;
        }

        List<ToolbarButtonData> leftButtonDatas = new List<ToolbarButtonData>();//要绘制的左侧菜单栏
        List<ToolbarButtonData> rightButtonDatas = new List<ToolbarButtonData>();//要绘制的右恻菜单栏
        protected BaseGraphView graphView;

        ToolbarButtonData showProcessor;
        ToolbarButtonData showParameters;

        public ToolbarView(BaseGraphView graphView)
        {
            name = "ToolbarView";
            this.graphView = graphView;
            Initialize();

            graphView.initialized += () => {
                leftButtonDatas.Clear();
                rightButtonDatas.Clear();
                AddButtons();//基础菜单控件
            };

            Add(new IMGUIContainer(DrawImGUIToolbar));//绘制的菜单栏
        }

        //初始化操作
        void Initialize()
        {
            LoadTexture();
        }


        /// <summary>添加按钮</summary>
		protected ToolbarButtonData AddButton(string name, Action callback, bool left = true)
            => AddButton(new GUIContent(name), callback, left);
        /// <summary>添加按钮</summary>
		protected ToolbarButtonData AddButtonIcon(Texture2D icon, Action callback, bool left = true)
            => AddButton(icon, callback, left);

        /// <summary>添加按钮（文字）</summary>
		protected ToolbarButtonData AddButton(GUIContent content, Action callback, bool left = true)
        {
            var data = new ToolbarButtonData
            {
                content = content,
                type = ElementType.Button,
                buttonCallback = callback
            };
            ((left) ? leftButtonDatas : rightButtonDatas).Add(data);
            return data;
        }

        /// <summary>添加按钮（图标）</summary>
        protected ToolbarButtonData AddButton(Texture2D icon, Action callback, bool left = true)
        {
            var data = new ToolbarButtonData
            {
                icon = icon,
                type = ElementType.ButtonIcon,
                buttonCallback = callback
            };
            ((left) ? leftButtonDatas : rightButtonDatas).Add(data);
            return data;
        }


        /// <summary>添加分隔空间</summary>
		protected void AddSeparator(int sizeInPixels = 10, bool left = true)
        {
            var data = new ToolbarButtonData
            {
                type = ElementType.Separator,
                size = sizeInPixels,
            };
            ((left) ? leftButtonDatas : rightButtonDatas).Add(data);
        }

        protected void AddCustom(Action imguiDrawFunction, bool left = true)
        {
            if (imguiDrawFunction == null)
                throw new ArgumentException("imguiDrawFunction can't be null");

            var data = new ToolbarButtonData
            {
                type = ElementType.Custom,
                customDrawFunction = imguiDrawFunction,
            };
            ((left) ? leftButtonDatas : rightButtonDatas).Add(data);
        }
        /// <summary>添加自动填充的空间</summary>
		protected void AddFlexibleSpace(bool left = true)
        {
            ((left) ? leftButtonDatas : rightButtonDatas).Add(new ToolbarButtonData { type = ElementType.FlexibleSpace });
        }
        /// <summary>添加开关</summary>
		protected ToolbarButtonData AddToggle(string name, bool defaultValue, Action<bool> callback, bool left = true)
            => AddToggle(new GUIContent(name), defaultValue, callback, left);
        /// <summary>添加开关</summary>
		protected ToolbarButtonData AddToggle(GUIContent content, bool defaultValue, Action<bool> callback, bool left = true)
        {
            var data = new ToolbarButtonData
            {
                content = content,
                type = ElementType.Toggle,
                value = defaultValue,
                toggleCallback = callback
            };
            ((left) ? leftButtonDatas : rightButtonDatas).Add(data);
            return data;
        }
        /// <summary>添加下拉菜单按钮</summary>
		protected ToolbarButtonData AddDropDownButton(string name, Action callback, bool left = true)
            => AddDropDownButton(new GUIContent(name), callback, left);
        /// <summary>添加下拉菜单按钮</summary>
		protected ToolbarButtonData AddDropDownButton(GUIContent content, Action callback, bool left = true)
        {
            var data = new ToolbarButtonData
            {
                content = content,
                type = ElementType.DropDownButton,
                buttonCallback = callback
            };
            ((left) ? leftButtonDatas : rightButtonDatas).Add(data);
            return data;
        }

        /// <summary>
        /// Also works for toggles 移除按钮/开关
        /// </summary>
        /// <param name="name"></param>
        /// <param name="left"></param>
        protected void RemoveButton(string name, bool left)
        {
            ((left) ? leftButtonDatas : rightButtonDatas).RemoveAll(b => b.content.text == name);
        }

        /// <summary>
        /// Hide the button 隐藏按钮
        /// </summary>
        /// <param name="name">Display name of the button</param>
        protected void HideButton(string name)
        {
            leftButtonDatas.Concat(rightButtonDatas).All(b => {
                if (b?.content?.text == name)
                    b.visible = false;
                return true;
            });
        }

        /// <summary>
        /// Show the button 显示按钮
        /// </summary>
        /// <param name="name">Display name of the button</param>
        protected void ShowButton(string name)
        {
            leftButtonDatas.Concat(rightButtonDatas).All(b => {
                if (b?.content?.text == name)
                    b.visible = true;
                return true;
            });
        }

        /// <summary>添加基础的开关/按钮</summary>
		protected virtual void AddButtons()
        {
            //添加在Project文件夹中显示graph的asset文件的按钮功能 [靠左侧]
            AddButton("Ping", () => EditorGUIUtility.PingObject(graphView.graph));
            //重置图表graph的位置的缩放大小(重归中央) [靠左侧]
            AddButton("Center", graphView.ResetPositionAndZoom);
            //运行图表
            AddButtonIcon(icon_play, 
                () =>
                {
                    ProcessGraphProcessor processor = new ProcessGraphProcessor(graphView.graph);
                    graphView.computeOrderUpdated += processor.UpdateComputeOrder;//更新计算顺序
                    processor.Run();
                    Debug.Log("play once !");
                }
            );
            //小地图
           // bool miniMapVisible = 

            //Help提示 [靠右侧]
            AddButtonIcon(icon_help, () => graphView.OpenHelpTipsWindow(new GraphTipsWindow()), false);
            //关闭窗口 [靠右侧]
            AddButtonIcon(icon_close, graphView.CloseWindow, false);

            ////添加对ProcessorView的开关（打开/关闭）功能 [靠左侧]
            //bool processorVisible = graphView.GetPinnedElementStatus<ProcessorView>() != Status.Hidden;
            //showProcessor = AddToggle("Show Processor", processorVisible, (v) => graphView.ToggleView<ProcessorView>());
            ////添加对ExposedParameterView的开关（打开/关闭）功能 [靠左侧]
            //bool exposedParamsVisible = graphView.GetPinnedElementStatus<ExposedParameterView>() != Status.Hidden;
            //showParameters = AddToggle("Show Parameters", exposedParamsVisible, (v) => graphView.ToggleView<ExposedParameterView>());

        }

        public virtual void UpdateButtonStatus()//no use
        {
            if (showProcessor != null)
                showProcessor.value = graphView.GetPinnedElementStatus<ProcessorView>() != Status.Hidden;
            if (showParameters != null)
                showParameters.value = graphView.GetPinnedElementStatus<ExposedParameterView>() != Status.Hidden;
        }

        void DrawImGUIButtonList(List<ToolbarButtonData> buttons)
        {
            foreach (var button in buttons.ToList())
            {
                if (!button.visible)
                    continue;

                switch (button.type)
                {
                    case ElementType.Button:
                        if (GUILayout.Button(button.content, EditorStyles.toolbarButton) && button.buttonCallback != null)
                            button.buttonCallback();
                        break;
                    case ElementType.ButtonIcon:
                        if (GUILayout.Button(button.icon, EditorStyles.toolbarButton) && button.buttonCallback != null)
                            button.buttonCallback();
                        break;
                    case ElementType.Toggle:
                        EditorGUI.BeginChangeCheck();
                        button.value = GUILayout.Toggle(button.value, button.content, EditorStyles.toolbarButton);
                        if (EditorGUI.EndChangeCheck() && button.toggleCallback != null)
                            button.toggleCallback(button.value);
                        break;
                    case ElementType.DropDownButton://创建一个能够对鼠标按下做出反应的按钮，用于显示您自己的下拉菜单内容
                        if (EditorGUILayout.DropdownButton(button.content, FocusType.Passive, EditorStyles.toolbarDropDown))
                            button.buttonCallback();
                        break;
                    case ElementType.Separator://分隔
                        EditorGUILayout.Separator();
                        EditorGUILayout.Space(button.size);//在上一个控件和下一个控件之间留出一个小空间
                        break;
                    case ElementType.Custom://自定义
                        button.customDrawFunction();
                        break;
                    case ElementType.FlexibleSpace://灵活的空白元素，将占用布局中的任何剩余空间
                        GUILayout.FlexibleSpace();
                        break;
                }
            }
        }

        protected virtual void DrawImGUIToolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            DrawImGUIButtonList(leftButtonDatas);

            GUILayout.FlexibleSpace();

            DrawImGUIButtonList(rightButtonDatas);

            GUILayout.EndHorizontal();
        }

        private Texture2D icon_center;
        private Texture2D icon_play;
        private Texture2D icon_help;
        private Texture2D icon_close;
        /// <summary>
        /// 获取unity内置或自定义的素材
        /// </summary>
        void LoadTexture()
        {
            icon_center = EditorGUIUtility.FindTexture("d_scenevis_visible_hover@2x");
            icon_play = EditorGUIUtility.FindTexture("d_PlayButton On");
            icon_help = EditorGUIUtility.FindTexture("d__Help@2x");
            icon_close = EditorGUIUtility.FindTexture("d_winbtn_win_close");
        }
    }
}
