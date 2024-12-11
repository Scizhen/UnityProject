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
        /// <summary>�ؼ�����</summary>
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

        List<ToolbarButtonData> leftButtonDatas = new List<ToolbarButtonData>();//Ҫ���Ƶ����˵���
        List<ToolbarButtonData> rightButtonDatas = new List<ToolbarButtonData>();//Ҫ���Ƶ������˵���
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
                AddButtons();//�����˵��ؼ�
            };

            Add(new IMGUIContainer(DrawImGUIToolbar));//���ƵĲ˵���
        }

        //��ʼ������
        void Initialize()
        {
            LoadTexture();
        }


        /// <summary>��Ӱ�ť</summary>
		protected ToolbarButtonData AddButton(string name, Action callback, bool left = true)
            => AddButton(new GUIContent(name), callback, left);
        /// <summary>��Ӱ�ť</summary>
		protected ToolbarButtonData AddButtonIcon(Texture2D icon, Action callback, bool left = true)
            => AddButton(icon, callback, left);

        /// <summary>��Ӱ�ť�����֣�</summary>
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

        /// <summary>��Ӱ�ť��ͼ�꣩</summary>
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


        /// <summary>��ӷָ��ռ�</summary>
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
        /// <summary>����Զ����Ŀռ�</summary>
		protected void AddFlexibleSpace(bool left = true)
        {
            ((left) ? leftButtonDatas : rightButtonDatas).Add(new ToolbarButtonData { type = ElementType.FlexibleSpace });
        }
        /// <summary>��ӿ���</summary>
		protected ToolbarButtonData AddToggle(string name, bool defaultValue, Action<bool> callback, bool left = true)
            => AddToggle(new GUIContent(name), defaultValue, callback, left);
        /// <summary>��ӿ���</summary>
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
        /// <summary>��������˵���ť</summary>
		protected ToolbarButtonData AddDropDownButton(string name, Action callback, bool left = true)
            => AddDropDownButton(new GUIContent(name), callback, left);
        /// <summary>��������˵���ť</summary>
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
        /// Also works for toggles �Ƴ���ť/����
        /// </summary>
        /// <param name="name"></param>
        /// <param name="left"></param>
        protected void RemoveButton(string name, bool left)
        {
            ((left) ? leftButtonDatas : rightButtonDatas).RemoveAll(b => b.content.text == name);
        }

        /// <summary>
        /// Hide the button ���ذ�ť
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
        /// Show the button ��ʾ��ť
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

        /// <summary>��ӻ����Ŀ���/��ť</summary>
		protected virtual void AddButtons()
        {
            //�����Project�ļ�������ʾgraph��asset�ļ��İ�ť���� [�����]
            AddButton("Ping", () => EditorGUIUtility.PingObject(graphView.graph));
            //����ͼ��graph��λ�õ����Ŵ�С(�ع�����) [�����]
            AddButton("Center", graphView.ResetPositionAndZoom);
            //����ͼ��
            AddButtonIcon(icon_play, 
                () =>
                {
                    ProcessGraphProcessor processor = new ProcessGraphProcessor(graphView.graph);
                    graphView.computeOrderUpdated += processor.UpdateComputeOrder;//���¼���˳��
                    processor.Run();
                    Debug.Log("play once !");
                }
            );
            //С��ͼ
           // bool miniMapVisible = 

            //Help��ʾ [���Ҳ�]
            AddButtonIcon(icon_help, () => graphView.OpenHelpTipsWindow(new GraphTipsWindow()), false);
            //�رմ��� [���Ҳ�]
            AddButtonIcon(icon_close, graphView.CloseWindow, false);

            ////��Ӷ�ProcessorView�Ŀ��أ���/�رգ����� [�����]
            //bool processorVisible = graphView.GetPinnedElementStatus<ProcessorView>() != Status.Hidden;
            //showProcessor = AddToggle("Show Processor", processorVisible, (v) => graphView.ToggleView<ProcessorView>());
            ////��Ӷ�ExposedParameterView�Ŀ��أ���/�رգ����� [�����]
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
                    case ElementType.DropDownButton://����һ���ܹ�����갴��������Ӧ�İ�ť��������ʾ���Լ��������˵�����
                        if (EditorGUILayout.DropdownButton(button.content, FocusType.Passive, EditorStyles.toolbarDropDown))
                            button.buttonCallback();
                        break;
                    case ElementType.Separator://�ָ�
                        EditorGUILayout.Separator();
                        EditorGUILayout.Space(button.size);//����һ���ؼ�����һ���ؼ�֮������һ��С�ռ�
                        break;
                    case ElementType.Custom://�Զ���
                        button.customDrawFunction();
                        break;
                    case ElementType.FlexibleSpace://���Ŀհ�Ԫ�أ���ռ�ò����е��κ�ʣ��ռ�
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
        /// ��ȡunity���û��Զ�����ز�
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
