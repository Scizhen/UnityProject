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
        //图标路径
        private static string iconPath = RDTSPath.path_HierarchyIcos;

        //储存在unity对应的windows注册表中
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

        private static bool stylesBuilt;//是否样式都建好了
        private static bool rdtsNotNull;//Controller是否为null

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
            iconnodeeditor,//节点编辑器
            iconnodeeditorinactive,
            iconobjectpool,//对象池
            iconobjectpoolinactive,
            iconeffector,//效应器
            iconeffectorinactive
            ;
        
             


        #region Menu stuff

        [MenuItem(MENU_NAME, false, 500)]//图标
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

        [MenuItem(MENU_DIVIDER, false, 502)]//分隔线
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
                new Type[] { typeof(UnityEngine.Object) }, null);//GetMethod：表示符合指定要求的方法“GetIconForObject(UnityEngine.Object object)”的对象（如果找到的话）

            // Not calling BuildStyles() in constructor because script gets loaded
            // on Unity initialization, styles might not be loaded yet

            // Reset mouse state
            ResetVars();
            // Setup quick toggle
            ShowQuickToggle(EditorPrefs.GetBool(PrefKeyShowToggle));
        }

        //读取注册表中的值，来判断以何种形式刷新
        public static void Refresh()
        {
            ShowQuickToggle(EditorPrefs.GetBool(PrefKeyShowToggle));
        }

        //设置QuickToggle对应的Controller，并判断Controller是否存在
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
            //获取注册表中这三个键的值
            showDivider = EditorPrefs.GetBool(PrefKeyShowDividers, false);
            showIcons = EditorPrefs.GetBool(PrefKeyShowIcons, false);
            gutterCount = EditorPrefs.GetInt(PrefKeyGutterLevel);

            if (show)
            {
                EditorApplication.update -= HandleEditorUpdate;// EditorApplication.update：通用更新的委托，将您的函数添加到此委托以获取更新

                ResetVars();
                EditorApplication.update += HandleEditorUpdate;
                EditorApplication.hierarchyWindowItemOnGUI += DrawHierarchyItem;// EditorApplication.hierarchyWindowItemOnGUI：Hierarchy 窗口中每个可见列表项的 OnGUI 事件的委托
            }
            else
            {
                EditorApplication.update -= HandleEditorUpdate;
                EditorApplication.hierarchyWindowItemOnGUI -= DrawHierarchyItem;
            }

            EditorApplication.RepaintHierarchyWindow();//可用于确保重绘 Hierarchy 窗口。
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
        //因为我们无法挂钩到 HierarchyWindow 的 OnGUI，所以做一个涉及编辑器更新循环和层次项绘制事件的hack按钮
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
            EditorWindow window = EditorWindow.mouseOverWindow;//当前鼠标光标所在的窗口
            if (window == null)
            {
                ResetVars();
                return;
            }

            if (window.GetType() == HierarchyWindowType)
            {
                if (window.wantsMouseMove == false)//检查是否已在此编辑器窗口的 GUI 中收到 MouseMove 事件，如果设置为 true，则每当鼠标移动到窗口之上时，该窗口都会收到一次 OnGUI 调用
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
                xMin = xMax - (selrect.height * posmin),//减去较大值的小值
                xMax = xMax - selrect.height * posmax//减去较小值的大值
            };
            rect.xMax -= padding;
            rect.xMin += padding;
            rect.yMax -= padding;
            rect.yMin += padding;

            return rect;
        }

        /// <summary>
        /// 获取对象上附加的所有RDTSBehavior组件/脚本
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
        /// 获取与type类型相同、type子类的RDTSBehavior类组件/脚本
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


        //获取节点编辑器类的脚本
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


        //获取效应器类的脚本
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
        /// 绘制Hierarchy面板的item：图标、标签
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




            if (!rdtsNotNull)//若Controller为空则直接返回
                return;

            if (!rdts.ShowHierarchyIcons)//若在Controller中设置ShowHierarchyIcons为false，则不显示图标
                return;

            BuildStyles();//GUI的样式构建

            if (iconsensor == null)
                return;

            if (icondrive == null)
                return;

            GameObject target = EditorUtility.InstanceIDToObject(instanceId) as GameObject;//InstanceIDToObject：将实例 ID 转换为对对象的引用。GameObject有个InstanceID，可以作为唯一标识


            if (target == null)
                return;

            if (!ReferenceEquals(target.GetComponent<RDTSController>(), null))//Controller为空
                return;

            // RDTS types
            behaviors = GetRDTSComponents(target);
            if (behaviors.Length > 0)
            {
                _value = (Value)GetRDTSComponent(typeof(Value));//找基类：其继承的子类均会被找到
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



            /* selectionRect：
             *     height：推测是Hierarchy面板中一个gameobject的高度（定值）
             *     width：推测是Hierarchy面板中一个gameobject的宽度（随着窗口的拉伸而变化）
             *   链接：https://docs.unity.cn/cn/2021.1/ScriptReference/Rect.html
             */
            // Reserve the draw rects
            float gutterX = selectionRect.height * gutterCount; //gutterCount是读取注册表里的值
            if (gutterX > 0)
                gutterX += selectionRect.height * 0.1f;
            float xMax = selectionRect.xMax - gutterX;
            float curpos = 1.1f;//使图标或标签从右向左绘制
            float size = 0;
            Rect filterrect = CreateRect(selectionRect, 0, 0, 0);
            Rect hiddenRect = CreateRect(selectionRect, 0, 0, 0);

            if (rdts.ShowFilter)///筛选
            {
                size = 1;
                filterrect = CreateRect(selectionRect, curpos + size, curpos, 0);
                GUI.DrawTexture(filterrect, iconfilteroff, ScaleMode.ScaleToFit);
                curpos += size;
            }

            bool isHidden = false;
            if (rdts.ShowHide)///显示右侧的上、下三角图标
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
            if (!ReferenceEquals(_value, null))///此Selecion对象是否带有Value组件(默认基础size=5)
            {


                if (_value.Settings.Override)//如果信号为Override状态，则显示一个感叹号图标（size=1,1个selectionRect.height）
                {
                    size = 1; unforcrect = CreateRect(selectionRect, curpos + size, curpos, 1);
                    GUI.DrawTexture(unforcrect, iconveride, ScaleMode.ScaleAndCrop);
                    curpos += size;
                }

                size = 5;
                sensorrect = CreateRect(selectionRect, curpos + size, curpos, 0);//占size=5（5个selectionRect.height）

                if (_value.GetValueDirection() == 1)//Input类型显示为红色
                {
                    stylevalue.normal.textColor = new Color(1, 0.4f, 0.016f, 1);//红色
                }
                else if(_value.GetValueDirection() == 2)//Output类型显示为绿色
                {
                    stylevalue.normal.textColor = Color.green;
                }
                else if(_value.GetValueDirection() == 3)//Middle类型显示为橙色
                {
                    stylevalue.normal.textColor = new Color(1, 0.5952f, 0 ,1);
                }
                  

                if (_value.Settings.Override)
                {
                    stylevalue.fontStyle = FontStyle.Italic;//斜体
                }
                else
                {
                    stylevalue.fontStyle = FontStyle.Normal;
                }

                if (_value.Settings.Active == false || _value.GetStatusConnected() == false)
                {
                    stylevalue.normal.textColor = Color.gray;
                }

                /// Highlight ValueObjs if connected to current gameobject 如果连接到当前游戏对象，则突出显示信号
                var sel = Selection.activeGameObject;//Selection：返回处于活动状态的游戏对象。（显示在检视面板中的对象）
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
                    GUI.Label(sensorrect, _value.GetVisuText(), stylevalue);//若此signal有连接到任何behavior script，则不带括号显示label
                else
                    GUI.Label(sensorrect, "(" + _value.GetVisuText() + ")", stylevalue);//若此signal未连接到任何behavior script，则带括号显示label
                curpos += size;
            }

            ///此Selection对象是否带有InterfaceBaseClass类的组件，并且在Controller中勾选了“ShowComponents”
            //if (!ReferenceEquals(_interface2, null) && rdts.ShowComponents)//接口类组件的图标
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

            ///此Selection对象是否带有InterfaceBaseClass类的组件，并且在Controller中勾选了“ShowComponents”
            if (!ReferenceEquals(_interface, null) && rdts.ShowComponents)//接口类组件的图标
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

            //控制逻辑的基类。 此基类用于在 RDTS 层次结构窗口中显示类型。
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

            //TransportSurface传输面类
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

            //Behaviour行为类
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



            //Grip类
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

            //Source类
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

            //Sink类
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

            //Sensor类
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

            //Drive类
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

            //MU类
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

            //NodeEditor类
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

            
            //对象池类
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

            //Effector类
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
            bool isVisible = target.activeSelf;//activeSelf：该属性返回此 GameObject 的本地活动状态，这是使用 GameObject.SetActive 设置的。 注意，GameObject 可能因为父项未处于活动状态而处于非活动状态，即使其返回 true 也是如此
            bool isLocked = (target.hideFlags & HideFlags.NotEditable) > 0;//HideFlags：位掩码，用于控制对象的销毁、保存和在 Inspector 中的可见性


            // Draw the visibility toggle 绘制可见性切换（控制此gameobject是否可见）
            Rect visRect = new Rect(selectionRect)//最靠右
            {
                xMin = xMax - (selectionRect.height * 1.05f),
                xMax = xMax - selectionRect.height * 0.05f
            };

            var iconshowhide = (isVisible) ? icondisplayon : icondisplayoff;
            GUI.DrawTexture(visRect, iconshowhide, ScaleMode.ScaleToFit);


            // Draw optional divider 绘制(可选)分隔线
            if (showDivider)
            {
                Rect lineRect = new Rect(selectionRect)
                {
                    yMin = selectionRect.yMax - 1f,
                    yMax = selectionRect.yMax + 2f
                };
                GUI.Label(lineRect, GUIContent.none, styleDivider);
            }

            // Draw optional object icons 绘制可选对象图标
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
        /// 鼠标在Hierarchy面板中的操作
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
            Event evt = Event.current;//当前的Event

            bool toggleActive = visRect.Contains(evt.mousePosition);//如果鼠标位置在visRect的矩形内 （是否可见的图标）

            bool toggleHide = hiddenrect.Contains(evt.mousePosition);//如果鼠标位置在hiddenrect的矩形内 （右侧上、下三角图标）
            bool toggleValue = valuerect.Contains(evt.mousePosition);//如果鼠标位置在signalrect的矩形内 （Value标签）
            bool toogleUnforce = unforerect.Contains(evt.mousePosition);//如果鼠标位置在unforerect的矩形内 （感叹号或扳手图标）
            bool stateChanged = (toggleActive || toggleHide || toggleValue || toogleUnforce);

            bool doMouse = false;
            switch (evt.type)
            {
                case EventType.MouseDown:
                    // Checking is frame fresh so mouse state is only tested once per frame 
                    // instead of every time a hierarchy item is drawn
                    //检查是帧刷新的，以致于鼠标状态每帧只测试一次，而不是每次绘制hierarchy item时
                    bool isMouseDown = false;
                    if (isFrameFresh && stateChanged)//鼠标在Hierarchy窗口上，且点击了图标或标签
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

                        if (toogleUnforce)//点击感叹号或扳手图标，解除强制状态(Override)
                        {
                            Value value = target.GetComponent<Value>();
                            value.Unforce();
                            value.Settings.Override = false;
                        }
                        if (toggleValue)//点击value的label对Bool类型的Value进行值反转操作
                        {
                            Value value = target.GetComponent<Value>();
                            value.OnToggleHierarchy();

                        }
                        propagateState = new PropagateState(toggleActive, (toggleActive) ? isVisible : isLocked);//记录可见框的状态（可见or不可见）
                        evt.Use();
                    }

                    break;
                case EventType.MouseDrag://MouseDrag时，鼠标已被按下，故在MouseDown状态中isMousePressed=true
                    doMouse = isMousePressed;
                    break;
                case EventType.DragPerform:
                case EventType.DragExited:
                case EventType.DragUpdated:
                case EventType.MouseUp://鼠标抬起时，重置状态
                    ResetVars();
                    break;
            }

            if (doMouse && stateChanged)
            {
                if (propagateState.isVisibility)
                    rdts.SetVisible(target, propagateState.propagateValue);//设置此对象的激活/停用状态

                EditorApplication.RepaintHierarchyWindow();//重绘面板
            }
        }

        /// <summary>
        /// 构建 GUI 样式
        /// </summary>
        private static void BuildStyles()
        {
            // All of the styles have been built, don't do anything 所有的样式都已经建好了，什么都不做
            if (stylesBuilt)
                return;

            // Now build the GUI styles
            // Using icons different from regular lock button so that
            // it would look darker
            //使用不同于常规锁定按钮的图标，以便它看起来会更暗
            var tempStyle = GUI.skin.FindStyle("IN LockButton");//unity内置
            styleLock = new GUIStyle(tempStyle)
            {
                normal = tempStyle.onNormal,
                active = tempStyle.onActive,
                hover = tempStyle.onHover,
                focused = tempStyle.onFocused,
            };


            // Unselected just makes the normal states have no lock images 未选中只是使正常状态没有锁定图像
            tempStyle = GUI.skin.FindStyle("OL Toggle");//unity内置
            styleUnlocked = new GUIStyle(tempStyle);
#if UNITY_2018_3_OR_NEWER
            tempStyle = new GUIStyle()
            {
                normal = new GUIStyleState()
                { background = EditorGUIUtility.Load("Icons/animationvisibilitytoggleoff.png") as Texture2D },//unity内置
                onNormal = new GUIStyleState()
                { background = EditorGUIUtility.Load("Icons/animationvisibilitytoggleon.png") as Texture2D },//unity内置
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
            //Value值对象样式
            tempStyle = GUI.skin.FindStyle("WhiteLabel");
            stylevalue = new GUIStyle(tempStyle);
            stylevalue.fontSize = 10;
            stylevalue.alignment = TextAnchor.MiddleRight;
            //图标样式
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
            iconnodeeditor = UnityEngine.Resources.Load(iconPath + "NodeEditor") as Texture;//节点编辑器
            iconnodeeditorinactive = UnityEngine.Resources.Load(iconPath + "NodeEditorinactive") as Texture;
            iconobjectpool = UnityEngine.Resources.Load(iconPath + "ObjectPool") as Texture;//对象池
            iconobjectpoolinactive = UnityEngine.Resources.Load(iconPath + "ObjectPoolinactive") as Texture;
            iconeffector = UnityEngine.Resources.Load(iconPath + "Effector") as Texture;//效应器
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