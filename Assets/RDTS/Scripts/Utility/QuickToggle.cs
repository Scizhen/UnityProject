///*************************************************************************
///Thanks for the code reference Jeiel Aranal provides.                    *
///                                                                        *
///*************************************************************************  
/*
Copyright 2017, Jeiel Aranal

Permission is hereby granted, free of charge, to any person obtaining a copy of this software
and associated documentation files (the "Software"), to deal in the Software without restriction,
including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial
portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT
LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH
THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using RDTS.Interface;
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;


#if UNITY_EDITOR
namespace RDTS.Utility
{
    /// <summary>
    /// 
    /// </summary>
    [InitializeOnLoad]
    public static class QuickToggle
    {
        #region Constants
        //ͼ��·��
        private static string iconPath = RDTSPath.path_HierarchyIcos;

        //������unity��Ӧ��windowsע�����
        private const string PrefKeyShowToggle = "UnityToolbag.QuickToggle.Visible";
        private const string PrefKeyShowDividers = "UnityToolbag.QuickToggle.Dividers";
        private const string PrefKeyShowIcons = "UnityToolbag.QuickToggle.Icons";
        private const string PrefKeyGutterLevel = "UnityToolbag.QuickToggle.Gutter";

        private const string MENU_NAME = "Parallel-RDTS/Hierarchy Window/Show Icons";
        private const string MENU_DIVIDER = "Parallel-RDTS/Hierarchy Window/Dividers";

        private const string
            MENU_ICONS = "Parallel-RDTS/Hierarchy Window/Object Icons";

        private const string
            MENU_GUTTER_0 = "Parallel-RDTS/Hierarchy Window/Right Gutter/0";

        private const string
            MENU_GUTTER_1 = "Parallel-RDTS/Hierarchy Window/Right Gutter/1";

        private const string
            MENU_GUTTER_2 = "Parallel-RDTS/Hierarchy Window/Right Gutter/2";

        #endregion

        private static readonly Type HierarchyWindowType;
        private static readonly MethodInfo getObjectIcon;
        private static RDTSController rdts;

        private static bool stylesBuilt;//�Ƿ���ʽ��������
        private static bool rdtsNotNull;//Controller�Ƿ�Ϊnull

        private static GUIStyle styleLock,
            styleUnlocked,
            styleVisOn,
            styleVisOff,
            styleDivider,
            stylevalue;

        private static bool showDivider, showIcons;

        private static RDTSBehavior[] behaviors = new RDTSBehavior[0];
        private static BaseNodeEditorInterface[] nodeEditors = new BaseNodeEditorInterface[0];
        private static BaseEffector[] effectors = new BaseEffector[0];

        private static Texture icondrive,
            iconsensor,
            iconbehaviour,
            iconcontroller,
            iconinterface,
            iconveride,
            icondriveinactive,
            iconsensorinactive,
            iconbehaviourinactive,
            iconinterfaceinactive,
            iconcontrollerinactive,
            iconhide,
            iconshow,
            iconsource,
            iconsourceinactive,
            iconsink,
            iconsinkinactive,
            icongrip,
            icongripinactive,
            icontransport,
            icontransportinactive,
            icondisplayon,
            icondisplayoff,
            iconfilteron,
            iconfilteroff,
            iconmu,
            iconmuinactive,
            iconnodeeditor,//�ڵ�༭��
            iconnodeeditorinactive,
            iconobjectpool,//�����
            iconobjectpoolinactive,
            iconeffector,//ЧӦ��
            iconeffectorinactive
            ;
        
             


        #region Menu stuff

        [MenuItem(MENU_NAME, false, 500)]//ͼ��
        private static void QuickToggleMenu()
        {
            bool toggle = EditorPrefs.GetBool(PrefKeyShowToggle);
            ShowQuickToggle(!toggle);
            Menu.SetChecked(MENU_NAME, !toggle);
        }

        [MenuItem(MENU_NAME, true, 501)]
        private static bool SetupMenuCheckMarks()
        {
            Menu.SetChecked(MENU_NAME, EditorPrefs.GetBool(PrefKeyShowToggle));
            Menu.SetChecked(MENU_DIVIDER, EditorPrefs.GetBool(PrefKeyShowDividers));
            Menu.SetChecked(MENU_ICONS, EditorPrefs.GetBool(PrefKeyShowIcons));

            int gutterLevel = EditorPrefs.GetInt(PrefKeyGutterLevel, 0);
            gutterCount = gutterLevel;
            UpdateGutterMenu(gutterCount);
            return true;
        }

        [MenuItem(MENU_DIVIDER, false, 502)]//�ָ���
        private static void ToggleDivider()
        {
            ToggleSettings(PrefKeyShowDividers, MENU_DIVIDER, out showDivider);
        }

        [MenuItem(MENU_ICONS, false, 503)]//Object icons
        private static void ToggleIcons()
        {
            ToggleSettings(PrefKeyShowIcons, MENU_ICONS, out showIcons);
        }

        private static void ToggleSettings(string prefKey, string menuString, out bool valueBool)
        {
            valueBool = !EditorPrefs.GetBool(prefKey);
            EditorPrefs.SetBool(prefKey, valueBool);
            Menu.SetChecked(menuString, valueBool);
            EditorApplication.RepaintHierarchyWindow();
        }

        [MenuItem(MENU_GUTTER_0, false, 540)]
        private static void SetGutter0()
        {
            SetGutterLevel(0);
        }

        [MenuItem(MENU_GUTTER_1, false, 541)]
        private static void SetGutter1()
        {
            SetGutterLevel(1);
        }

        [MenuItem(MENU_GUTTER_2, false, 542)]
        private static void SetGutter2()
        {
            SetGutterLevel(2);
        }

        private static void SetGutterLevel(int gutterLevel)
        {
            gutterLevel = Mathf.Clamp(gutterLevel, 0, 2);
            EditorPrefs.SetInt(PrefKeyGutterLevel, gutterLevel);
            gutterCount = gutterLevel;
            UpdateGutterMenu(gutterCount);
            EditorApplication.RepaintHierarchyWindow();
        }

        private static void UpdateGutterMenu(int gutterLevel)
        {
            string[] gutterKeys = new[] { MENU_GUTTER_0, MENU_GUTTER_1, MENU_GUTTER_2 };
            bool[] gutterValues = null;
            switch (gutterLevel)
            {
                case 1:
                    gutterValues = new[] { false, true, false };
                    break;
                case 2:
                    gutterValues = new[] { false, false, true };
                    break;
                default:
                    gutterValues = new[] { true, false, false };
                    break;
            }

            for (int i = 0; i < gutterKeys.Length; i++)
            {
                string key = gutterKeys[i];
                bool isChecked = gutterValues[i];
                Menu.SetChecked(key, isChecked);
            }
        }

        #endregion

        static QuickToggle()
        {
            // Setup initial state of editor prefs if there are no prefs keys yet
            string[] resetPrefs = new string[] { PrefKeyShowToggle, PrefKeyShowDividers, PrefKeyShowIcons };
            foreach (string prefKey in resetPrefs)
            {
                if (EditorPrefs.HasKey(prefKey) == false)
                    EditorPrefs.SetBool(prefKey, false);
            }

            // Fetch some reflection/type stuff for use later on
            Assembly editorAssembly = typeof(EditorWindow).Assembly;
            HierarchyWindowType = editorAssembly.GetType("UnityEditor.SceneHierarchyWindow");

            var flags = BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic;
            Type editorGuiUtil = typeof(EditorGUIUtility);
            getObjectIcon = editorGuiUtil.GetMethod("GetIconForObject", flags, null,
                new Type[] { typeof(UnityEngine.Object) }, null);//GetMethod����ʾ����ָ��Ҫ��ķ�����GetIconForObject(UnityEngine.Object object)���Ķ�������ҵ��Ļ���

            // Not calling BuildStyles() in constructor because script gets loaded
            // on Unity initialization, styles might not be loaded yet

            // Reset mouse state
            ResetVars();
            // Setup quick toggle
            ShowQuickToggle(EditorPrefs.GetBool(PrefKeyShowToggle));
        }

        //��ȡע����е�ֵ�����ж��Ժ�����ʽˢ��
        public static void Refresh()
        {
            ShowQuickToggle(EditorPrefs.GetBool(PrefKeyShowToggle));
        }

        //����QuickToggle��Ӧ��Controller�����ж�Controller�Ƿ����
        public static void SetRDTSController(RDTSController controller)
        {
            rdts = controller;
            if (controller != null)
                rdtsNotNull = true;
            else
                rdtsNotNull = false;
            Refresh();
        }

        public static void ShowQuickToggle(bool show)
        {
            stylesBuilt = false;
            EditorPrefs.SetBool(PrefKeyShowToggle, show);
            //��ȡע���������������ֵ
            showDivider = EditorPrefs.GetBool(PrefKeyShowDividers, false);
            showIcons = EditorPrefs.GetBool(PrefKeyShowIcons, false);
            gutterCount = EditorPrefs.GetInt(PrefKeyGutterLevel);

            if (show)
            {
                EditorApplication.update -= HandleEditorUpdate;// EditorApplication.update��ͨ�ø��µ�ί�У������ĺ�����ӵ���ί���Ի�ȡ����

                ResetVars();
                EditorApplication.update += HandleEditorUpdate;
                EditorApplication.hierarchyWindowItemOnGUI += DrawHierarchyItem;// EditorApplication.hierarchyWindowItemOnGUI��Hierarchy ������ÿ���ɼ��б���� OnGUI �¼���ί��
            }
            else
            {
                EditorApplication.update -= HandleEditorUpdate;
                EditorApplication.hierarchyWindowItemOnGUI -= DrawHierarchyItem;
            }

            EditorApplication.RepaintHierarchyWindow();//������ȷ���ػ� Hierarchy ���ڡ�
        }

        private struct PropagateState
        {
            public bool isVisibility;
            public bool propagateValue;

            public PropagateState(bool isVisibility, bool propagateValue)
            {
                this.isVisibility = isVisibility;
                this.propagateValue = propagateValue;
            }
        }

        private static PropagateState propagateState;

        // Because we can't hook into OnGUI of HierarchyWindow, doing a hack
        // button that involves the editor update loop and the hierarchy item draw event
        //��Ϊ�����޷��ҹ��� HierarchyWindow �� OnGUI��������һ���漰�༭������ѭ���Ͳ��������¼���hack��ť
        private static bool isFrameFresh;
        private static bool isMousePressed;

        private static int gutterCount = 0;

        private static void ResetVars()
        {
            isFrameFresh = false;
            isMousePressed = false;
        }

        private static void HandleEditorUpdate()
        {
            // Debug.Log("HandleEditorUpdate");
            EditorWindow window = EditorWindow.mouseOverWindow;//��ǰ��������ڵĴ���
            if (window == null)
            {
                ResetVars();
                return;
            }

            if (window.GetType() == HierarchyWindowType)
            {
                if (window.wantsMouseMove == false)//����Ƿ����ڴ˱༭�����ڵ� GUI ���յ� MouseMove �¼����������Ϊ true����ÿ������ƶ�������֮��ʱ���ô��ڶ����յ�һ�� OnGUI ����
                    window.wantsMouseMove = true;

                isFrameFresh = true;
            }
        }

        private static Rect CreateRect(Rect selrect, float posmin, float posmax, int padding)
        {
            float gutterX = selrect.height * gutterCount;
            if (gutterX > 0)
                gutterX += selrect.height * 0.1f;
            float xMax = selrect.xMax - gutterX;
            Rect rect = new Rect(selrect)
            {
                xMin = xMax - (selrect.height * posmin),//��ȥ�ϴ�ֵ��Сֵ
                xMax = xMax - selrect.height * posmax//��ȥ��Сֵ�Ĵ�ֵ
            };
            rect.xMax -= padding;
            rect.xMin += padding;
            rect.yMax -= padding;
            rect.yMin += padding;

            return rect;
        }

        /// <summary>
        /// ��ȡ�����ϸ��ӵ�����RDTSBehavior���/�ű�
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        private static RDTSBehavior[] GetRDTSComponents(GameObject target)
        {
            RDTSBehavior[] behav;
            behav = target.GetComponents<RDTSBehavior>();
            return behav;
        }

        /// <summary>
        /// ��ȡ��type������ͬ��type�����RDTSBehavior�����/�ű�
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static RDTSBehavior GetRDTSComponent(Type type)
        {
            if (behaviors == null)
                return null;
            foreach (RDTSBehavior behavior in behaviors)
            {
                Type mytype = behavior.GetType();
                if (mytype.IsSubclassOf(type))
                    return behavior;
                if (mytype == type)
                    return behavior;
            }
            return null;
        }


        //��ȡ�ڵ�༭����Ľű�
        private static BaseNodeEditorInterface[] GetRDTSNodeEditorComponent(GameObject target)
        {
            BaseNodeEditorInterface[] nodeEditor;
            nodeEditor = target.GetComponents<BaseNodeEditorInterface>();
            return nodeEditor;
        }


        private static BaseNodeEditorInterface GetRDTSNodeEditorComponent(Type type)
        {
            if (nodeEditors == null)
                return null;
            foreach (BaseNodeEditorInterface nodeEditor in nodeEditors)
            {
                Type mytype = nodeEditor.GetType();
                if (mytype.IsSubclassOf(type))
                    return nodeEditor;
                if (mytype == type)
                    return nodeEditor;
            }
            return null;
        }


        //��ȡЧӦ����Ľű�
        private static BaseEffector[] GetRDTSEffectorComponent(GameObject target)
        {
            BaseEffector[] effector;
            effector = target.GetComponents<BaseEffector>();
            return effector;
        }


        private static BaseEffector GetRDTSEffectorComponent(Type type)
        {
            if (effectors == null)
                return null;
            foreach (BaseEffector effector in effectors)
            {
                Type mytype = effector.GetType();
                if (mytype.IsSubclassOf(type))
                    return effector;
                if (mytype == type)
                    return effector;
            }
            return null;
        }


        /// <summary>
        /// ����Hierarchy����item��ͼ�ꡢ��ǩ
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="selectionRect"></param>
        private static void DrawHierarchyItem(int instanceId, Rect selectionRect)
        {
            Color fontColor = Color.blue;
            Color backgroundColor = new Color(.76f, .76f, .76f);

            Value _value = null;
            BaseDrive _drive = null;
            Behaviour _behaviour = null;
            //InterfaceBaseClass _interface2 = null;
            BaseInterface _interface = null;

            BaseSource _source = null;
            BaseSink _sink = null;
            BaseSensor _sensor = null;
            ControlLogic _controllogic = null;
            BaseGrip _grip = null; ;
            BaseTransportSurface _transport = null;
            Group group = null; ;
            MU _mu = null;
            BaseNodeEditorInterface _nodeEditor = null;
            BaseObjectPool _objectPool = null;
            BaseEffector _effector = null;




            if (!rdtsNotNull)//��ControllerΪ����ֱ�ӷ���
                return;

            if (!rdts.ShowHierarchyIcons)//����Controller������ShowHierarchyIconsΪfalse������ʾͼ��
                return;

            BuildStyles();//GUI����ʽ����

            if (iconsensor == null)
                return;

            if (icondrive == null)
                return;

            GameObject target = EditorUtility.InstanceIDToObject(instanceId) as GameObject;//InstanceIDToObject����ʵ�� ID ת��Ϊ�Զ�������á�GameObject�и�InstanceID��������ΪΨһ��ʶ


            if (target == null)
                return;

            if (!ReferenceEquals(target.GetComponent<RDTSController>(), null))//ControllerΪ��
                return;

            // RDTS types
            behaviors = GetRDTSComponents(target);
            if (behaviors.Length > 0)
            {
                _value = (Value)GetRDTSComponent(typeof(Value));//�һ��ࣺ��̳е�������ᱻ�ҵ�
                _drive = (BaseDrive)GetRDTSComponent(typeof(BaseDrive));
                _behaviour = (BehaviorInterface)GetRDTSComponent(typeof(BehaviorInterface));
                //_interface2 = (InterfaceBaseClass)GetRDTSComponent(typeof(InterfaceBaseClass));
                _interface = (BaseInterface)GetRDTSComponent(typeof(BaseInterface));

                _sensor = (BaseSensor)GetRDTSComponent(typeof(BaseSensor));
                _controllogic = (ControlLogic)GetRDTSComponent(typeof(ControlLogic));
                _grip = (BaseGrip)GetRDTSComponent(typeof(BaseGrip));
                _transport = (BaseTransportSurface)GetRDTSComponent(typeof(BaseTransportSurface));
                group = (Group)GetRDTSComponent(typeof(Group));


                _source = (BaseSource)GetRDTSComponent(typeof(BaseSource));
                _sink = (BaseSink)GetRDTSComponent(typeof(BaseSink));
                _mu = (MU)GetRDTSComponent(typeof(MU));

                _objectPool = (BaseObjectPool)GetRDTSComponent(typeof(BaseObjectPool));


            }

            nodeEditors = GetRDTSNodeEditorComponent(target);
            if(nodeEditors.Length > 0)
            {
                _nodeEditor = (BaseNodeEditorInterface)GetRDTSNodeEditorComponent(typeof(BaseNodeEditorInterface));
            }

            effectors = GetRDTSEffectorComponent(target);
            if(effectors.Length > 0)
            {
                _effector = (BaseEffector)GetRDTSEffectorComponent(typeof(BaseEffector));
            }



            /* selectionRect��
             *     height���Ʋ���Hierarchy�����һ��gameobject�ĸ߶ȣ���ֵ��
             *     width���Ʋ���Hierarchy�����һ��gameobject�Ŀ�ȣ����Ŵ��ڵ�������仯��
             *   ���ӣ�https://docs.unity.cn/cn/2021.1/ScriptReference/Rect.html
             */
            // Reserve the draw rects
            float gutterX = selectionRect.height * gutterCount; //gutterCount�Ƕ�ȡע������ֵ
            if (gutterX > 0)
                gutterX += selectionRect.height * 0.1f;
            float xMax = selectionRect.xMax - gutterX;
            float curpos = 1.1f;//ʹͼ����ǩ�����������
            float size = 0;
            Rect filterrect = CreateRect(selectionRect, 0, 0, 0);
            Rect hiddenRect = CreateRect(selectionRect, 0, 0, 0);

            if (rdts.ShowFilter)///ɸѡ
            {
                size = 1;
                filterrect = CreateRect(selectionRect, curpos + size, curpos, 0);
                GUI.DrawTexture(filterrect, iconfilteroff, ScaleMode.ScaleToFit);
                curpos += size;
            }

            bool isHidden = false;
            if (rdts.ShowHide)///��ʾ�Ҳ���ϡ�������ͼ��
            {
                size = 1;

                if (rdts.AreSubObjectsHidden(target))
                {
                    isHidden = true;
                }

                hiddenRect = CreateRect(selectionRect, curpos + size, curpos, 3);
                if (target.transform.childCount > 0 && (rdts.IsHiddenSubObject(target) == false &&
                                                        rdts.IsInObjectWithHiddenSubobjects(target) ==
                                                        false))
                {
                    if (!isHidden)
                    {
                        GUI.DrawTexture(hiddenRect, iconhide, ScaleMode.ScaleToFit);
                    }
                    else
                    {
                        GUI.DrawTexture(hiddenRect, iconshow, ScaleMode.ScaleToFit);
                    }
                }
            }

            curpos += size;


            Rect sensorrect = new Rect();
            sensorrect.center = new Vector2(0, 0);
            Rect unforcrect = new Rect();
            sensorrect.center = new Vector2(0, 0);
            if (!ReferenceEquals(_value, null))///��Selecion�����Ƿ����Value���(Ĭ�ϻ���size=5)
            {


                if (_value.Settings.Override)//����ź�ΪOverride״̬������ʾһ����̾��ͼ�꣨size=1,1��selectionRect.height��
                {
                    size = 1; unforcrect = CreateRect(selectionRect, curpos + size, curpos, 1);
                    GUI.DrawTexture(unforcrect, iconveride, ScaleMode.ScaleAndCrop);
                    curpos += size;
                }

                size = 5;
                sensorrect = CreateRect(selectionRect, curpos + size, curpos, 0);//ռsize=5��5��selectionRect.height��

                if (_value.GetValueDirection() == 1)//Input������ʾΪ��ɫ
                {
                    stylevalue.normal.textColor = new Color(1, 0.4f, 0.016f, 1);//��ɫ
                }
                else if(_value.GetValueDirection() == 2)//Output������ʾΪ��ɫ
                {
                    stylevalue.normal.textColor = Color.green;
                }
                else if(_value.GetValueDirection() == 3)//Middle������ʾΪ��ɫ
                {
                    stylevalue.normal.textColor = new Color(1, 0.5952f, 0 ,1);
                }
                  

                if (_value.Settings.Override)
                {
                    stylevalue.fontStyle = FontStyle.Italic;//б��
                }
                else
                {
                    stylevalue.fontStyle = FontStyle.Normal;
                }

                if (_value.Settings.Active == false || _value.GetStatusConnected() == false)
                {
                    stylevalue.normal.textColor = Color.gray;
                }

                /// Highlight ValueObjs if connected to current gameobject ������ӵ���ǰ��Ϸ������ͻ����ʾ�ź�
                var sel = Selection.activeGameObject;//Selection�����ش��ڻ״̬����Ϸ���󡣣���ʾ�ڼ�������еĶ���
                if (!ReferenceEquals(sel, null))
                {
                    var valueobjs = sel.GetComponents<IValueInterface>();
                    foreach (var valueobj in valueobjs)
                    {
                        if (!ReferenceEquals(valueobj, null))
                        {
                            var conns = valueobj.GetValues();
                            if (conns.Contains(_value))
                                stylevalue.normal.textColor = Color.yellow;
                        }
                    }

                }

                if (_value.IsConnectedToBehavior())
                    GUI.Label(sensorrect, _value.GetVisuText(), stylevalue);//����signal�����ӵ��κ�behavior script���򲻴�������ʾlabel
                else
                    GUI.Label(sensorrect, "(" + _value.GetVisuText() + ")", stylevalue);//����signalδ���ӵ��κ�behavior script�����������ʾlabel
                curpos += size;
            }

            ///��Selection�����Ƿ����InterfaceBaseClass��������������Controller�й�ѡ�ˡ�ShowComponents��
            //if (!ReferenceEquals(_interface2, null) && rdts.ShowComponents)//�ӿ��������ͼ��
            //{
            //    size = 1;
            //    Rect interfacerect = CreateRect(selectionRect, curpos + size, curpos, 1);

            //    if (iconinterface != null)
            //    {
            //        if (_interface2.IsConnected && _interface2.enabled)
            //        {
            //            GUI.DrawTexture(interfacerect, iconinterface, ScaleMode.ScaleToFit);
            //        }
            //        else
            //        {
            //            GUI.DrawTexture(interfacerect, iconinterfaceinactive, ScaleMode.ScaleToFit);
            //        }
            //    }

            //    curpos += size;
            //}

            ///��Selection�����Ƿ����InterfaceBaseClass��������������Controller�й�ѡ�ˡ�ShowComponents��
            if (!ReferenceEquals(_interface, null) && rdts.ShowComponents)//�ӿ��������ͼ��
            {
                size = 1;
                Rect interfacerect = CreateRect(selectionRect, curpos + size, curpos, 1);

                if (iconinterface != null)
                {
                    if (_interface.ConnectionStatus && _interface.enabled)
                    {
                        GUI.DrawTexture(interfacerect, iconinterface, ScaleMode.ScaleToFit);
                    }
                    else
                    {
                        GUI.DrawTexture(interfacerect, iconinterfaceinactive, ScaleMode.ScaleToFit);
                    }
                }

                curpos += size;
            }

            //�����߼��Ļ��ࡣ �˻��������� RDTS ��νṹ��������ʾ���͡�
            if (!ReferenceEquals(_controllogic, null) && rdts.ShowComponents)
            {
                size = 1;
                Rect controlrect = CreateRect(selectionRect, curpos + size, curpos, 1);

                if (iconcontroller != null)
                {
                    if (_controllogic.enabled)
                    {
                        GUI.DrawTexture(controlrect, iconcontroller, ScaleMode.ScaleAndCrop);
                    }
                    else
                    {
                        GUI.DrawTexture(controlrect, iconcontrollerinactive, ScaleMode.ScaleAndCrop);
                    }
                }

                curpos += size;
            }

            //TransportSurface��������
            if (!ReferenceEquals(_transport, null) && rdts.ShowComponents)
            {
                size = 1;
                Rect transportrect = CreateRect(selectionRect, curpos + size, curpos, 1);

                if (icontransport != null)
                {
                    if (_transport.enabled)
                    {
                        GUI.DrawTexture(transportrect, icontransport, ScaleMode.ScaleToFit);
                    }
                    else
                    {
                        GUI.DrawTexture(transportrect, icontransportinactive, ScaleMode.ScaleToFit);
                    }
                }

                curpos += size;
            }

            //Behaviour��Ϊ��
            if (!ReferenceEquals(_behaviour, null) && rdts.ShowComponents)
            {
                size = 1;
                Rect behaviourrect = CreateRect(selectionRect, curpos + size, curpos, 1);

                if (iconbehaviour != null)
                {
                    if (_behaviour.enabled)
                    {
                        GUI.DrawTexture(behaviourrect, iconbehaviour, ScaleMode.ScaleAndCrop);
                    }
                    else
                    {
                        GUI.DrawTexture(behaviourrect, iconbehaviourinactive, ScaleMode.ScaleToFit);
                    }
                }

                curpos += size;
            }



            //Grip��
            if (!ReferenceEquals(_grip, null) && rdts.ShowComponents)
            {
                size = 1;
                Rect griprect = CreateRect(selectionRect, curpos + size, curpos, 1);

                if (icongrip != null)
                {
                    if (_grip.enabled)
                    {
                        GUI.DrawTexture(griprect, icongrip, ScaleMode.ScaleAndCrop);
                    }
                    else
                    {
                        GUI.DrawTexture(griprect, icongripinactive, ScaleMode.ScaleAndCrop);
                    }
                }

                curpos += size;
            }

            //Source��
            if (!ReferenceEquals(_source, null) && rdts.ShowComponents)
            {
                size = 1;
                Rect sourcerect = CreateRect(selectionRect, curpos + size, curpos, 1);

                if (iconsource != null)
                {
                    if (_source.enabled)
                    {
                        GUI.DrawTexture(sourcerect, iconsource, ScaleMode.ScaleAndCrop);
                    }
                    else
                    {
                        GUI.DrawTexture(sourcerect, iconsourceinactive, ScaleMode.ScaleAndCrop);
                    }
                }

                curpos += size;
            }

            //Sink��
            if (!ReferenceEquals(_sink, null) && rdts.ShowComponents)
            {
                size = 1;
                Rect sinkrect = CreateRect(selectionRect, curpos + size, curpos, 1);

                if (iconsink != null)
                {
                    if (_sink.enabled)
                    {
                        GUI.DrawTexture(sinkrect, iconsink, ScaleMode.ScaleAndCrop);
                    }
                    else
                    {
                        GUI.DrawTexture(sinkrect, iconsinkinactive, ScaleMode.ScaleAndCrop);
                    }
                }

                curpos += size;
            }

            //Sensor��
            if (!ReferenceEquals(_sensor, null) && rdts.ShowComponents)
            {
                size = 1;
                Rect rect = CreateRect(selectionRect, curpos + size, curpos, 1);

                if (iconsensor != null)
                {
                    if (_sensor.enabled)
                    {
                        GUI.DrawTexture(rect, iconsensor, ScaleMode.ScaleToFit);
                    }
                    else
                    {
                        GUI.DrawTexture(rect, iconsensorinactive, ScaleMode.ScaleToFit);
                    }
                }

                curpos += size;
            }

            //Drive��
            if (!ReferenceEquals(_drive, null) && rdts.ShowComponents)
            {
                size = 1;
                Rect driverect = CreateRect(selectionRect, curpos + size, curpos, 1);

                if (icondrive != null)
                {
                    if (_drive.enabled)
                    {
                        GUI.DrawTexture(driverect, icondrive, ScaleMode.ScaleAndCrop);
                    }
                    else
                    {
                        GUI.DrawTexture(driverect, icondriveinactive, ScaleMode.ScaleAndCrop);
                    }
                }

                curpos += size;
            }

            //MU��
            if (!ReferenceEquals(_mu, null) && rdts.ShowComponents)
            {
                size = 1;
                Rect murect = CreateRect(selectionRect, curpos + size, curpos, 1);

                if (iconmu != null)
                {
                    if (_mu.enabled)
                    {
                        GUI.DrawTexture(murect, iconmu, ScaleMode.ScaleAndCrop);
                    }
                    else
                    {
                        GUI.DrawTexture(murect, iconmuinactive, ScaleMode.ScaleAndCrop);
                    }
                }

                curpos += size;
            }

            //NodeEditor��
            if (!ReferenceEquals(_nodeEditor, null) && rdts.ShowComponents)
            {
                size = 1;
                Rect noderect = CreateRect(selectionRect, curpos + size, curpos, 1);

                if (iconnodeeditor != null)
                {
                    if (_nodeEditor.enabled)
                    {
                        GUI.DrawTexture(noderect, iconnodeeditor, ScaleMode.ScaleAndCrop);
                    }
                    else
                    {
                        GUI.DrawTexture(noderect, iconnodeeditorinactive, ScaleMode.ScaleAndCrop);
                    }
                }

                curpos += size;
            }

            
            //�������
            if (!ReferenceEquals(_objectPool, null) && rdts.ShowComponents)
            {
                size = 1;
                Rect objectpoolrect = CreateRect(selectionRect, curpos + size, curpos, 1);

                if (iconobjectpool != null)
                {
                    if (_objectPool.enabled)
                    {
                        GUI.DrawTexture(objectpoolrect, iconobjectpool, ScaleMode.ScaleAndCrop);
                    }
                    else
                    {
                        GUI.DrawTexture(objectpoolrect, iconobjectpoolinactive, ScaleMode.ScaleAndCrop);
                    }
                }

                curpos += size;
            }

            //Effector��
            if (!ReferenceEquals(_effector, null) && rdts.ShowComponents)
            {
                size = 1;
                Rect effectorrect = CreateRect(selectionRect, curpos + size, curpos, 1);

                if (iconeffector != null)
                {
                    if (_effector.enabled)
                    {
                        GUI.DrawTexture(effectorrect, iconeffector, ScaleMode.ScaleAndCrop);
                    }
                    else
                    {
                        GUI.DrawTexture(effectorrect, iconeffectorinactive, ScaleMode.ScaleAndCrop);
                    }
                }

                curpos += size;
            }
            




            if (!ReferenceEquals(group, null))
            {
                size = rdts.WidthGroupName;
                Rect grouprect = CreateRect(selectionRect, curpos + size, curpos, 0);

                stylevalue.normal.textColor = Color.yellow;

                GUI.Label(grouprect, group.GetVisuText(), stylevalue);
            }


            curpos += size;
            // Get states
            bool isVisible = target.activeSelf;//activeSelf�������Է��ش� GameObject �ı��ػ״̬������ʹ�� GameObject.SetActive ���õġ� ע�⣬GameObject ������Ϊ����δ���ڻ״̬�����ڷǻ״̬����ʹ�䷵�� true Ҳ�����
            bool isLocked = (target.hideFlags & HideFlags.NotEditable) > 0;//HideFlags��λ���룬���ڿ��ƶ�������١�������� Inspector �еĿɼ���


            // Draw the visibility toggle ���ƿɼ����л������ƴ�gameobject�Ƿ�ɼ���
            Rect visRect = new Rect(selectionRect)//���
            {
                xMin = xMax - (selectionRect.height * 1.05f),
                xMax = xMax - selectionRect.height * 0.05f
            };

            var iconshowhide = (isVisible) ? icondisplayon : icondisplayoff;
            GUI.DrawTexture(visRect, iconshowhide, ScaleMode.ScaleToFit);


            // Draw optional divider ����(��ѡ)�ָ���
            if (showDivider)
            {
                Rect lineRect = new Rect(selectionRect)
                {
                    yMin = selectionRect.yMax - 1f,
                    yMax = selectionRect.yMax + 2f
                };
                GUI.Label(lineRect, GUIContent.none, styleDivider);
            }

            // Draw optional object icons ���ƿ�ѡ����ͼ��
            if (showIcons && getObjectIcon != null)
            {
                Texture2D iconImg = getObjectIcon.Invoke(null, new object[] { target }) as Texture2D;
                if (iconImg != null)
                {
                    Rect iconRect = new Rect(selectionRect)
                    {
                        xMin = visRect.xMin - 30,
                        xMax = visRect.xMin - 5
                    };
                    GUI.DrawTexture(iconRect, iconImg, ScaleMode.ScaleToFit);
                }
            }
            

            if (Event.current == null)
                return;
            HandleMouse(target, isVisible, isLocked, isHidden, visRect, hiddenRect, sensorrect, unforcrect);
        }


        /// <summary>
        /// �����Hierarchy����еĲ���
        /// </summary>
        /// <param name="target"></param>
        /// <param name="isVisible"></param>
        /// <param name="isLocked"></param>
        /// <param name="isHidden"></param>
        /// <param name="visRect"></param>
        /// <param name="hiddenrect"></param>
        /// <param name="valuerect"></param>
        /// <param name="unforerect"></param>
        private static void HandleMouse(GameObject target, bool isVisible, bool isLocked, bool isHidden, Rect visRect,
            Rect hiddenrect, Rect valuerect, Rect unforerect)
        {
            Event evt = Event.current;//��ǰ��Event

            bool toggleActive = visRect.Contains(evt.mousePosition);//������λ����visRect�ľ����� ���Ƿ�ɼ���ͼ�꣩

            bool toggleHide = hiddenrect.Contains(evt.mousePosition);//������λ����hiddenrect�ľ����� ���Ҳ��ϡ�������ͼ�꣩
            bool toggleValue = valuerect.Contains(evt.mousePosition);//������λ����signalrect�ľ����� ��Value��ǩ��
            bool toogleUnforce = unforerect.Contains(evt.mousePosition);//������λ����unforerect�ľ����� ����̾�Ż����ͼ�꣩
            bool stateChanged = (toggleActive || toggleHide || toggleValue || toogleUnforce);

            bool doMouse = false;
            switch (evt.type)
            {
                case EventType.MouseDown:
                    // Checking is frame fresh so mouse state is only tested once per frame 
                    // instead of every time a hierarchy item is drawn
                    //�����֡ˢ�µģ����������״̬ÿֻ֡����һ�Σ�������ÿ�λ���hierarchy itemʱ
                    bool isMouseDown = false;
                    if (isFrameFresh && stateChanged)//�����Hierarchy�����ϣ��ҵ����ͼ����ǩ
                    {
                        isMouseDown = !isMousePressed;//false
                        isMousePressed = true;
                        isFrameFresh = false;
                    }

                    if (stateChanged && isMouseDown)
                    {
                        doMouse = true;
                        if (toggleActive) isVisible = !isVisible;
                        if (toggleHide)
                        {
                            if (rdts != null)
                                rdts.AlterHideObjects(target);
                        }

                        if (toogleUnforce)//�����̾�Ż����ͼ�꣬���ǿ��״̬(Override)
                        {
                            Value value = target.GetComponent<Value>();
                            value.Unforce();
                            value.Settings.Override = false;
                        }
                        if (toggleValue)//���value��label��Bool���͵�Value����ֵ��ת����
                        {
                            Value value = target.GetComponent<Value>();
                            value.OnToggleHierarchy();

                        }
                        propagateState = new PropagateState(toggleActive, (toggleActive) ? isVisible : isLocked);//��¼�ɼ����״̬���ɼ�or���ɼ���
                        evt.Use();
                    }

                    break;
                case EventType.MouseDrag://MouseDragʱ������ѱ����£�����MouseDown״̬��isMousePressed=true
                    doMouse = isMousePressed;
                    break;
                case EventType.DragPerform:
                case EventType.DragExited:
                case EventType.DragUpdated:
                case EventType.MouseUp://���̧��ʱ������״̬
                    ResetVars();
                    break;
            }

            if (doMouse && stateChanged)
            {
                if (propagateState.isVisibility)
                    rdts.SetVisible(target, propagateState.propagateValue);//���ô˶���ļ���/ͣ��״̬

                EditorApplication.RepaintHierarchyWindow();//�ػ����
            }
        }

        /// <summary>
        /// ���� GUI ��ʽ
        /// </summary>
        private static void BuildStyles()
        {
            // All of the styles have been built, don't do anything ���е���ʽ���Ѿ������ˣ�ʲô������
            if (stylesBuilt)
                return;

            // Now build the GUI styles
            // Using icons different from regular lock button so that
            // it would look darker
            //ʹ�ò�ͬ�ڳ���������ť��ͼ�꣬�Ա��������������
            var tempStyle = GUI.skin.FindStyle("IN LockButton");//unity����
            styleLock = new GUIStyle(tempStyle)
            {
                normal = tempStyle.onNormal,
                active = tempStyle.onActive,
                hover = tempStyle.onHover,
                focused = tempStyle.onFocused,
            };


            // Unselected just makes the normal states have no lock images δѡ��ֻ��ʹ����״̬û������ͼ��
            tempStyle = GUI.skin.FindStyle("OL Toggle");//unity����
            styleUnlocked = new GUIStyle(tempStyle);
#if UNITY_2018_3_OR_NEWER
            tempStyle = new GUIStyle()
            {
                normal = new GUIStyleState()
                { background = EditorGUIUtility.Load("Icons/animationvisibilitytoggleoff.png") as Texture2D },//unity����
                onNormal = new GUIStyleState()
                { background = EditorGUIUtility.Load("Icons/animationvisibilitytoggleon.png") as Texture2D },//unity����
                fixedHeight = 11,
                fixedWidth = 13,
                border = new RectOffset(2, 2, 2, 2),
                overflow = new RectOffset(-1, 1, -2, 2),
                padding = new RectOffset(3, 3, 3, 3),
                richText = false,
                stretchHeight = false,
                stretchWidth = false,
            };
#else
            tempStyle = GUI.skin.FindStyle("VisibilityToggle");
#endif

            styleVisOff = new GUIStyle(tempStyle);
            styleVisOn = new GUIStyle(tempStyle)
            {
                normal = new GUIStyleState() { background = tempStyle.onNormal.background }
            };

            styleDivider = GUI.skin.FindStyle("EyeDropperHorizontalLine");


            // Styles RDTS
            //Valueֵ������ʽ
            tempStyle = GUI.skin.FindStyle("WhiteLabel");
            stylevalue = new GUIStyle(tempStyle);
            stylevalue.fontSize = 10;
            stylevalue.alignment = TextAnchor.MiddleRight;
            //ͼ����ʽ
            icondrive = UnityEngine.Resources.Load(iconPath + "Drive") as Texture;
            icondriveinactive = UnityEngine.Resources.Load(iconPath + "Driveinactive") as Texture;
            iconsensor = UnityEngine.Resources.Load(iconPath + "Sensor") as Texture;
            iconsensorinactive = UnityEngine.Resources.Load(iconPath + "Sensorinactive") as Texture;
            iconbehaviour = UnityEngine.Resources.Load(iconPath + "Behaviour") as Texture;
            iconbehaviourinactive = UnityEngine.Resources.Load(iconPath + "Behaviourinactive") as Texture;
            iconcontroller = UnityEngine.Resources.Load(iconPath + "ControlLogic") as Texture;
            iconcontrollerinactive = UnityEngine.Resources.Load(iconPath + "ControlLogicinactive") as Texture;
            iconinterface = UnityEngine.Resources.Load(iconPath + "Interface") as Texture;
            iconinterfaceinactive = UnityEngine.Resources.Load(iconPath + "Interfaceinactive") as Texture;
            iconsource = UnityEngine.Resources.Load(iconPath + "Source") as Texture;
            iconsourceinactive = UnityEngine.Resources.Load(iconPath + "Sourceinactive") as Texture;
            iconsink = UnityEngine.Resources.Load(iconPath + "Sink") as Texture;
            iconsinkinactive = UnityEngine.Resources.Load(iconPath + "Sinkinactive") as Texture;
            icongrip = UnityEngine.Resources.Load(iconPath + "Grip") as Texture;
            icongripinactive = UnityEngine.Resources.Load(iconPath + "Gripinactive") as Texture;
            icontransport = UnityEngine.Resources.Load(iconPath + "Transportsurface") as Texture;
            icontransportinactive = UnityEngine.Resources.Load(iconPath + "Transportsurfaceinactive") as Texture;
            iconmu = UnityEngine.Resources.Load(iconPath + "MU") as Texture;
            iconmuinactive = UnityEngine.Resources.Load(iconPath + "MUinactive") as Texture;
            iconnodeeditor = UnityEngine.Resources.Load(iconPath + "NodeEditor") as Texture;//�ڵ�༭��
            iconnodeeditorinactive = UnityEngine.Resources.Load(iconPath + "NodeEditorinactive") as Texture;
            iconobjectpool = UnityEngine.Resources.Load(iconPath + "ObjectPool") as Texture;//�����
            iconobjectpoolinactive = UnityEngine.Resources.Load(iconPath + "ObjectPoolinactive") as Texture;
            iconeffector = UnityEngine.Resources.Load(iconPath + "Effector") as Texture;//ЧӦ��
            iconeffectorinactive = UnityEngine.Resources.Load(iconPath + "Effectorinactive") as Texture;

            iconveride = UnityEngine.Resources.Load(iconPath + "Overide") as Texture;
            iconhide = UnityEngine.Resources.Load(iconPath + "Hide") as Texture;
            iconshow = UnityEngine.Resources.Load(iconPath + "Show") as Texture;
            icondisplayon = UnityEngine.Resources.Load(iconPath + "displayon") as Texture;
            icondisplayoff = UnityEngine.Resources.Load(iconPath + "displayoff") as Texture;


            stylesBuilt = (styleLock != null && styleUnlocked != null &&
                           styleVisOn != null && styleVisOff != null &&
                           styleDivider != null);
        }
    }
}
#endif