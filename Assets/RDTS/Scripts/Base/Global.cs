//*************************************************************************
//Thanks for the code reference game4automation provides.                 *
//                                                                        *
//*************************************************************************
using UnityEngine;
using System;
using System.Collections;
using System.IO;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
#endif
using System.Collections.Generic;
using System.Linq;
using RDTS.Utility;


namespace RDTS.Method
{
    /// <summary>
    /// 静态类，提供方法
    /// </summary>
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    public static class Global
    {
        // Global Variables
        public static bool RuntimeInspectorEnabled = true;

        public static string Version = "";
        public static string Release = "";
        public static string Build = "";
        public static RDTSController rdtsController; //  Controller of last Scene playing
        public static bool rdtsControllernotnull = false;
        public static bool QuickEditDisplay = true;

        #region Selection Tools

        public static List<String> GetGroups()
        {

            var groups = GetAllSceneComponents<Group>();
            var Groups = new List<string>();
#if UNITY_EDITOR
            foreach (Group group in groups)
            {
                if (EditorUtility.IsPersistent(group.transform.root.gameObject))
                    continue;
                if (!Groups.Contains(group.GroupName))
                    Groups.Add(group.GroupName);
            }
            Groups.Sort();//使用默认比较器对元素进行排列
#endif
            return Groups;
        }

        public static List<GameObject> GetAllWithGroup(string group)
        {
            List<GameObject> list = new List<GameObject>();
            var groupcomps = UnityEngine.Resources.FindObjectsOfTypeAll(typeof(Group));
#if UNITY_EDITOR
            foreach (var comp in groupcomps)
            {
                var gr = (Group)comp;
                if (EditorUtility.IsPersistent(gr.transform.root.gameObject))
                    continue;
                if (gr.GroupName == group)
                    list.Add(gr.gameObject);
            }
#endif
            return list;
        }

        /// <summary>
        /// 从根游戏对象获取给定类型的组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetRootComponent<T>() where T : Component
        {
            List<GameObject> rootObjects = new List<GameObject>();
            Scene scene = SceneManager.GetActiveScene();//获取当前活动的场景。
            scene.GetRootGameObjects(rootObjects);//返回场景中的所有根游戏对象。
            foreach (var obj in rootObjects)
            {
                var com = obj.GetComponent<T>();
                if (com != null)
                {
                    return com;
                }
            }

            return null;
        }

        public static List<GameObject> GetAllPrefabsAtPathWithComponent<T>(string Folder) where T : Component
        {
            List<GameObject> results = new List<GameObject>();
#if UNITY_EDITOR
            var res = AssetDatabase.FindAssets("t: Prefab", new[] { Folder });//使用搜索筛选器字符串搜索资源数据库：在文件夹Folder中查找Prefab类型或类名的asset

            foreach (var guid in res)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);//获取asset路径
                var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);//加载asset
                if (asset.GetComponent<T>() != null)
                    results.Add(asset);
            }

#endif
            return results;
        }


        //找到所有指定类型的咋Scene中的组件
        public static List<T> GetAllSceneComponents<T>() where T : Component
        {
            object[] objs = UnityEngine.Resources.FindObjectsOfTypeAll(typeof(T));
            var comps = new List<T>();
#if UNITY_EDITOR
            foreach (T obj in objs)
            {
                if (EditorUtility.IsPersistent(obj.transform.root.gameObject))//确定对象是否存储在磁盘中。
                    continue;//如果是存在磁盘中的（不是Scene的），就继续查找下一个

                comps.Add(obj);
            }
#endif
            return comps;
        }

        //销毁给定对象下的指定类型的组件
        public static bool DestroyObjectsByComponent<T>(GameObject parent) where T : Component
        {
            var firstList = parent.GetComponentsInChildren<T>();
            bool deleted = false;
            for (var i = 0; i < firstList.Length; i++)
            {
                try
                {
                    Object.DestroyImmediate(firstList[i].gameObject);
                    deleted = true;
                }
                catch
                {

                }
            }

            return deleted;
        }

        //按照名字寻找对象，未找到则创建一个
        public static GameObject AddGameObjectIfNotExisting(string name, GameObject parent = null)
        {
            GameObject go;

            if (parent != null)
            {
                var transform = parent.transform.Find(name);
                if (transform != null)
                {
                    go = transform.gameObject;
                    return go;
                }
            }
            else
            {
                go = GameObject.Find("/" + name);//遍历层级视图来寻找
                if (go != null)
                {
                    return go;
                }
            }

            go = new GameObject();
            go.name = name;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            return go;
        }

        //判断给定对象是否有指定类型的组件，没有就添加
        public static T AddComponentIfNotExisting<T>(GameObject gameobject) where T : Component
        {
            T comp = null;
            comp = gameobject.GetComponent<T>();
            if (comp == null)
            {
                comp = gameobject.AddComponent<T>();
            }

            return comp;
        }

        //销毁给定对象上的指定类型的组件
        public static T DestroyComponents<T>(GameObject gameobject) where T : Component
        {
            var firstList = gameobject.GetComponents<T>();

            for (var i = 0; i < firstList.Length; i++)
            {
                try
                {
                    Object.DestroyImmediate(firstList[i]);
                }
                catch
                {

                }
            }

            return null;
        }

        //获取给定对象上的指定类型和名称的组件：1个或null
        public static T GetComponentByName<T>(GameObject parent, string name) where T : Component
        {
            var firstList = parent.GetComponentsInChildren<T>(true);//包含非活动的gameobject

            for (var i = 0; i < firstList.Length; i++)
            {
                if (firstList[i].gameObject.name == name)
                //   if (firstList[i].gameObject.name == name && parent != this)
                {
                    return firstList[i];
                }
            }

            return null;
        }
        //获取给定对象上所有的指定类型和名称的组件
        public static List<T> GetComponentsByName<T>(GameObject parent, string name) where T : Component
        {
            var firstList = parent.GetComponentsInChildren<T>(true);//包含非活动的gameobject
            List<T> reslist = new List<T>();
            for (var i = 0; i < firstList.Length; i++)
            {
                if (firstList[i].gameObject.name == name)
                {
                    reslist.Add(firstList[i]);
                }
            }

            return reslist;
        }
        //获取给定对象上所有的指定类型的组件
        public static List<T> GetComponentsAlsoInactive<T>(GameObject parent) where T : Component
        {
            var firstList = parent.GetComponentsInChildren<T>(true);//包含非活动的gameobject
            List<T> reslist = new List<T>();
            for (var i = 0; i < firstList.Length; i++)
                reslist.Add(firstList[i]);
            return reslist;
        }

        /// <summary>
        /// 设置控制器组件，或清空控制器组件
        /// </summary>
        /// <param name="controller"></param>
        public static void SetRDTSController(RDTSController controller)
        {
            if (controller != null)
            {
                rdtsController = controller;
                rdtsControllernotnull = true;
            }
            else
            {
                rdtsController = null;
                rdtsControllernotnull = false;
            }
        }

        ///应用程序域（由 AppDomain 对象表示）有助于提供用于执行托管代码的隔离、卸载和安全边界
        //获取所有派生类型
        public static System.Type[] GetAllDerivedTypes(this System.AppDomain aAppDomain, System.Type aType)
        {
            var result = new List<System.Type>();
            var assemblies = aAppDomain.GetAssemblies();//获取已加载到此应用程序域的执行上下文中的程序集
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (type.IsSubclassOf(aType))
                        result.Add(type);
                }
            }

            return result.ToArray();
        }

        public static Rect GetEditorMainWindowPos()
        {
            var containerWinType = System.AppDomain.CurrentDomain.GetAllDerivedTypes(typeof(ScriptableObject))
                .Where(t => t.Name == "ContainerWindow").FirstOrDefault();
            if (containerWinType == null)
                throw new System.MissingMemberException(
                    "Can't find internal type ContainerWindow. Maybe something has changed inside Unity");
            var showModeField = containerWinType.GetField("m_ShowMode",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var positionProperty = containerWinType.GetProperty("position",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (showModeField == null || positionProperty == null)
                throw new System.MissingFieldException(
                    "Can't find internal fields 'm_ShowMode' or 'position'. Maybe something has changed inside Unity");
            var windows = UnityEngine.Resources.FindObjectsOfTypeAll(containerWinType);
            foreach (var win in windows)
            {
                var showmode = (int)showModeField.GetValue(win);
                if (showmode == 4) // main window
                {
                    var pos = (Rect)positionProperty.GetValue(win, null);
                    return pos;
                }
            }

            throw new System.NotSupportedException(
                "Can't find internal main window. Maybe something has changed inside Unity");
        }

#if UNITY_EDITOR
        public static void CenterOnMainWin(this UnityEditor.EditorWindow aWin)
        {
            var main = GetEditorMainWindowPos();
            var pos = aWin.position;
            float w = (main.width - pos.width) * 0.5f;
            float h = (main.height - pos.height) * 0.5f;
            pos.x = main.x + w;
            pos.y = main.y + h;
            aWin.position = pos;
        }
#endif

        //Bounds：表示一个轴对齐的包围盒。
        //获取一个能包围给定对象的包围盒
        public static Bounds GetTotalBounds(GameObject root)
        {
            Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                bounds = renderers[0].bounds;
                foreach (Renderer renderer in renderers)
                {
                    bounds.Encapsulate(renderer.bounds);//Encapsulate：增大 Bounds，以便封装这些边界。  
                    //renderer.bounds：渲染器的包围体积。使用 bounds 可以方便地描述对象位置及其范围的近似形状
                }
            }

            return bounds;
        }

        //移动给定(根)对象的位置，但保持其子对象位置不变
        public static void MovePositionKeepChildren(GameObject root, Vector3 deltalocalposition)
        {
            List<GameObject> childs = new List<GameObject>();
            // save all children
            foreach (Transform child in root.transform)
            {
                childs.Add(child.gameObject);
            }

            // temp unparent children
            root.transform.DetachChildren();// 清除所有子项的父级
            root.transform.localPosition = root.transform.localPosition + deltalocalposition;//移动根对象位置
            foreach (var child in childs)
            {
                child.transform.parent = root.transform;//重新将子对象的父级设为此根对象
            }
        }
        //移动给定(根)对象的旋转，但保持其子对象旋转不变
        public static void MoveRotationKeepChildren(GameObject root, Quaternion deltarotation)
        {
            List<GameObject> childs = new List<GameObject>();
            // save all children
            foreach (Transform child in root.transform)
            {
                childs.Add(child.gameObject);
            }

            // temp unparent children
            root.transform.DetachChildren();
            root.transform.localRotation = root.transform.localRotation * deltarotation;
            foreach (var child in childs)
            {
                child.transform.parent = root.transform;
            }
        }
        //设置给定(根)对象的位置，但保持其子对象位置不变
        public static void SetPositionKeepChildren(GameObject root, Vector3 globalposition)
        {
            List<GameObject> childs = new List<GameObject>();
            // save all children
            foreach (Transform child in root.transform)
            {
                childs.Add(child.gameObject);
            }

            // temp unparent children
            root.transform.DetachChildren();
            root.transform.position = globalposition;
            foreach (var child in childs)
            {
                child.transform.parent = root.transform;
            }
        }
        //设置给定(根)对象的旋转，但保持其子对象旋转不变
        public static void SetRotationKeepChildren(GameObject root, Quaternion rotation)
        {
            List<GameObject> childs = new List<GameObject>();
            // save all children
            foreach (Transform child in root.transform)
            {
                childs.Add(child.gameObject);
            }

            // temp unparent children
            root.transform.DetachChildren();
            root.transform.rotation = rotation;
            foreach (var child in childs)
            {
                child.transform.parent = root.transform;
            }
        }

        /// <summary>
        /// 返回给定对象的中心
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        public static Vector3 GetTotalCenter(GameObject root)
        {
            var bounds = GetTotalBounds(root);
            return bounds.center;//在描述对象中心时，center属性通常比Transform.position更精确，特别是当对象不对称时。
        }

        //收集给定对象中的对象(返回数组)
        public static Object[] GatherObjects(GameObject root)
        {
            List<UnityEngine.Object> objects = new List<UnityEngine.Object>();
            Stack<GameObject> recurseStack = new Stack<GameObject>(new GameObject[] { root });//此年间一个堆栈，并且复制root到堆栈

            while (recurseStack.Count > 0)
            {
                GameObject obj = recurseStack.Pop();//删除并返回顶部的对象
                objects.Add(obj);
                if (obj != null)
                    foreach (Transform childT in obj.transform)
                        recurseStack.Push(childT.gameObject);
            }

            return objects.ToArray();
        }

        //判断给定的对象是否包含RDTSBehavior组件
        public static bool IsRDTSTypeIncluded(GameObject target)
        {
            RDTSBehavior[] behavior = target.GetComponentsInChildren<RDTSBehavior>();
            var length = behavior.Length;
            if (length == 0)
            {
                return false;
            }

            return true;
        }

        public static bool IsRDTSViewTypeIncluded(GameObject target)
        {
            try
            {
                RDTSBehavior[] behavior = target.GetComponentsInChildren<RDTSBehavior>();

                if (behavior.Length == 0)
                {
                    return false;
                }
            }
            catch
            {
            }

            return true;
        }


#if UNITY_EDITOR
        //设置用户给定的脚本编译符号
        public static void SetDefine(string mydefine)
        {
            var currtarget = EditorUserBuildSettings.selectedBuildTargetGroup;//EditorUserBuildSettings：编辑器的用户构建设置。selectedBuildTargetGroup：当前选择的构建目标组
            string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(currtarget);//为给定构建目标组获取用户指定的脚本编译符号
            if (!symbols.Contains(mydefine))
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(currtarget, symbols + ";" + mydefine);//为给定构建目标组设置用户指定的脚本编译符号
            }
        }
        //清楚用户给定的脚本编译符号
        public static void DeleteDefine(string mydefine)
        {
            var currtarget = EditorUserBuildSettings.selectedBuildTargetGroup;
            string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(currtarget);
            if (symbols.Contains(";" + mydefine))
            {
                symbols = symbols.Replace(";" + mydefine, "");
                PlayerSettings.SetScriptingDefineSymbolsForGroup(currtarget, symbols);
            }

            if (symbols.Contains(mydefine))
            {
                symbols = symbols.Replace(mydefine, "");
                PlayerSettings.SetScriptingDefineSymbolsForGroup(currtarget, symbols);
            }
        }

        public static void SetAssemblyDefReference(string assemblydef, string reference, bool set)
        {
            var path = Path.Combine(Application.dataPath, assemblydef);
            string assydef = File.ReadAllText(path);//打开一个文本文件，读取文件中的所有文本，然后关闭此文件
            if (set)
            {
                // already there
                if (assydef.Contains(reference))
                    return;
                var search = "\"references\": [";
                var pos = assydef.IndexOf(search) + search.Length;

                var insertvalue = "\n        \"" + reference + "\",";
                assydef = assydef.Insert(pos, insertvalue);
            }

            if (!set)
            {
                if (!assydef.Contains(reference))
                    return;
                var start = assydef.IndexOf(reference);
                var posend = assydef.IndexOf(",", start);
                var posstart = assydef.LastIndexOf("\n", posend);
                assydef = assydef.Remove(posstart, posend - posstart + 1);
            }

            File.WriteAllText(path, assydef);
        }

        /// <summary>
        /// 为当前选中的对象添加指定路径的gameobject
        /// </summary>
        /// <param name="assetpath"></param>
        public static void AddComponent(string assetpath)
        {
            GameObject component = Selection.activeGameObject;//当前选中的对象
            Object prefab = AssetDatabase.LoadAssetAtPath(assetpath, typeof(GameObject));//获取要加载的资源
            GameObject go = PrefabUtility.InstantiatePrefab(prefab) as GameObject;//创建一个新对象
            go.transform.position = new Vector3(0, 0, 0);
            if (component != null)
            {
                go.transform.parent = component.transform;
            }

            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);//针对新创建的对象注册撤销操作：Ctrl+Z 或者 Edit — Undo
        }

        //未给定的对象添加指定路径的gameobject
        public static GameObject AddComponentTo(Transform transform, string assetpath)
        {
            Object prefab = AssetDatabase.LoadAssetAtPath(assetpath, typeof(GameObject));
            GameObject go = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            go.transform.position = new Vector3(0, 0, 0);
            if (transform != null)
            {
                go.transform.parent = transform;
            }

            return go;
        }

        /// <summary>
        /// 对给定的目标对象进行激活/隐藏的操作；
        /// 若选了多个对象。则对所有选择的对象进行激活/隐藏的操作
        /// </summary>
        /// <param name="target"></param>
        /// <param name="isActive"></param>
        public static void SetVisible(GameObject target, bool isActive)
        {
            if (target.activeSelf == isActive) return;

            target.SetActive(isActive);
            EditorUtility.SetDirty(target);//想要修改对象而不创建撤销条目时可使用。 若确实想支持撤销，应使用Undo.RecordObject

            Object[] objects = GatherObjects(target);//获取该对象下的所有对象
            foreach (Object obj in objects)
            {
                GameObject go = (GameObject)obj;
                go.SetActive(isActive);
                EditorUtility.SetDirty(go);
            }

            if (Selection.objects.Length > 1)//若选择了多个对象
                foreach (var obj in Selection.objects)
                {
                    if (obj.GetType() == typeof(GameObject))
                    {
                        if (obj != target)
                            SetVisible((GameObject)obj, isActive);//嵌套
                    }
                }
        }

        public static void HideSubObjects(GameObject target, bool hide)
        {
            if (ReferenceEquals(rdtsController, null))
                return;

            EditorUtility.SetDirty(rdtsController);
            if (!hide)//不隐藏
            {
                if (rdtsController.ObjectsWithHiddenSubobjects.Contains(target))
                    rdtsController.ObjectsWithHiddenSubobjects.Remove(target);

                Object[] objects = GatherObjects(target);

                foreach (Object obj in objects)
                {
                    if (rdtsController.ObjectsWithHiddenSubobjects.Contains((GameObject)obj))
                        rdtsController.ObjectsWithHiddenSubobjects.Remove((GameObject)obj);
                    obj.hideFlags = HideFlags.None;
                    if (rdtsController.HiddenSubobjects.Contains((GameObject)obj))
                        rdtsController.HiddenSubobjects.Remove((GameObject)obj);
                    if (rdtsController.InObjectWithHiddenSubobjects.Contains((GameObject)obj))
                        rdtsController.InObjectWithHiddenSubobjects.Remove((GameObject)obj);
                }

                SetExpandedRecursive(target, true);
                EditorApplication.DirtyHierarchyWindowSorting();
            }
            else//隐藏
            {
                rdtsController.ObjectsWithHiddenSubobjects.Add(target);
                SetExpandedRecursive(target, true);
                Object[] objects = GatherObjects(target);
                foreach (Object obj in objects)
                {
                    if (IsRDTSViewTypeIncluded((GameObject)obj) == false && obj != target)
                    {
                        obj.hideFlags = HideFlags.HideInHierarchy;
                        if (!rdtsController.HiddenSubobjects.Contains((GameObject)obj))
                            rdtsController.HiddenSubobjects.Add((GameObject)obj);
                    }
                    else
                    {
                        if (obj != target)
                        {
                            if (!rdtsController.InObjectWithHiddenSubobjects.Contains((GameObject)obj))
                                rdtsController.InObjectWithHiddenSubobjects.Add((GameObject)obj);
                        }
                    }
                }

                EditorApplication.DirtyHierarchyWindowSorting();
            }
        }

        public static void SetLockObject(GameObject target, bool isLocked)
        {
            bool objectLockState = (target.hideFlags & HideFlags.NotEditable) > 0;
            if (objectLockState == isLocked)
                return;

            Object[] objects = GatherObjects(target);

            if (isLocked && rdtsController != null)
            {
                if (!rdtsController.LockedObjects.Contains(target))
                    rdtsController.LockedObjects.Add(target);
            }
            else
                rdtsController.LockedObjects.Remove(target);

            foreach (Object obj in objects)
            {
                GameObject go = (GameObject)obj;
                string undoString = string.Format("{0} {1}", isLocked ? "Lock" : "Unlock", go.name);
                Undo.RecordObject(go, undoString);

                // Set state according to isLocked
                if (isLocked)
                {
                    go.hideFlags |= HideFlags.NotEditable;
                }
                else
                {
                    if (Global.rdtsControllernotnull)
                        rdtsController.LockedObjects.Remove(go);
                    go.hideFlags &= ~HideFlags.NotEditable;
                }

                // Set hideflags of components
                foreach (Component comp in go.GetComponents<Component>())
                {
                    if (comp is Transform)
                        continue;

                    Undo.RecordObject(comp, undoString);

                    if (isLocked)
                    {
                        comp.hideFlags |= HideFlags.NotEditable;
                        comp.hideFlags |= HideFlags.HideInHierarchy;
                    }
                    else
                    {
                        comp.hideFlags &= ~HideFlags.NotEditable;
                        comp.hideFlags &= ~HideFlags.HideInHierarchy;
                    }

                    EditorUtility.SetDirty(comp);
                }

                EditorUtility.SetDirty(go);
                if (rdtsController != null)
                    EditorUtility.SetDirty(rdtsController);
            }
        }

        //设置扩展递归
        public static void SetExpandedRecursive(GameObject go, bool expand)
        {
            System.Type type = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
            System.Reflection.MethodInfo methodInfo = type.GetMethod("SetExpandedRecursive");//GetMethod：搜索具有指定名称的公共方法
            EditorApplication.ExecuteMenuItem("Window/General/Hierarchy");
            EditorWindow editorWindow = EditorWindow.focusedWindow;
            methodInfo.Invoke(editorWindow, new object[] { go.GetInstanceID(), expand });
        }

#endif

        #endregion


        #region DebugTools

        public static void DebugDrawArrow(Vector3 position, Vector3 direction, Color color, float duration = 0,
            bool depthTest = true)
        {
            Debug.DrawRay(position, direction, color, duration, depthTest);//在世界坐标中绘制一条从 position 到 position + direction 的直线
            DebugCone(position + direction, -direction * 0.333f, color, 15, duration, depthTest);//锥体
        }

        private static void DebugCircle(Vector3 position, Vector3 up, Color color, float radius = 1.0f,
            float duration = 0, bool depthTest = true)
        {
            Vector3 _up = up.normalized * radius;
            Vector3 _forward = Vector3.Slerp(_up, -_up, 0.5f);//球形插值
            Vector3 _right = Vector3.Cross(_up, _forward).normalized * radius;//叉积

            Matrix4x4 matrix = new Matrix4x4();

            matrix[0] = _right.x;
            matrix[1] = _right.y;
            matrix[2] = _right.z;

            matrix[4] = _up.x;
            matrix[5] = _up.y;
            matrix[6] = _up.z;

            matrix[8] = _forward.x;
            matrix[9] = _forward.y;
            matrix[10] = _forward.z;

            Vector3 _lastPoint = position + matrix.MultiplyPoint3x4(new Vector3(Mathf.Cos(0), 0, Mathf.Sin(0)));
            Vector3 _nextPoint = Vector3.zero;

            color = (color == default(Color)) ? Color.white : color;

            for (var i = 0; i < 91; i++)
            {
                _nextPoint.x = Mathf.Cos((i * 4) * Mathf.Deg2Rad);
                _nextPoint.z = Mathf.Sin((i * 4) * Mathf.Deg2Rad);
                _nextPoint.y = 0;

                _nextPoint = position + matrix.MultiplyPoint3x4(_nextPoint);

                Debug.DrawLine(_lastPoint, _nextPoint, color, duration, depthTest);
                _lastPoint = _nextPoint;
            }
        }

        public static void DebugArrow(Vector3 position, Vector3 direction, float duration = 0)
        {
            DebugDrawArrow(position, direction, Color.yellow, duration);
        }

        public static void DebugArrow(Vector3 position, Vector3 direction, Color color, float duration = 0)
        {
            DebugDrawArrow(position, direction, color, duration);
        }

        //绘制锥体
        private static void DebugCone(Vector3 position, Vector3 direction, Color color, float angle = 45,
            float duration = 0, bool depthTest = true)
        {
            float length = direction.magnitude;

            Vector3 _forward = direction;
            Vector3 _up = Vector3.Slerp(_forward, -_forward, 0.5f);
            Vector3 _right = Vector3.Cross(_forward, _up).normalized * length;

            direction = direction.normalized;

            Vector3 slerpedVector = Vector3.Slerp(_forward, _up, angle / 90.0f);

            float dist;
            var farPlane = new Plane(-direction, position + _forward);
            var distRay = new Ray(position, slerpedVector);

            farPlane.Raycast(distRay, out dist);

            Debug.DrawRay(position, slerpedVector.normalized * dist, color);
            Debug.DrawRay(position, Vector3.Slerp(_forward, -_up, angle / 90.0f).normalized * dist, color, duration,
                depthTest);
            Debug.DrawRay(position, Vector3.Slerp(_forward, _right, angle / 90.0f).normalized * dist, color, duration,
                depthTest);
            Debug.DrawRay(position, Vector3.Slerp(_forward, -_right, angle / 90.0f).normalized * dist, color, duration,
                depthTest);

            DebugCircle(position + _forward, direction, color, (_forward - (slerpedVector.normalized * dist)).magnitude,
                duration, depthTest);
            DebugCircle(position + (_forward * 0.5f), direction, color,
                ((_forward * 0.5f) - (slerpedVector.normalized * (dist * 0.5f))).magnitude, duration, depthTest);
        }

        //绘制全局坐标系
        public static void DebugGlobalAxis(Vector3 position, float duration = 0)
        {
            Debug.DrawRay(position, new Vector3(0.5f, 0, 0), Color.red, duration, false);
            Debug.DrawRay(position, new Vector3(0, 0.5f, 0), Color.green, duration, false);
            Debug.DrawRay(position, new Vector3(0, 0, 0.5f), Color.blue, duration, false);
        }
        //绘制局部坐标系
        public static void DebugLocalAxis(Vector3 position, float duration = 0, GameObject gameObject = null)
        {
            if (gameObject == null)
            {
                DebugGlobalAxis(position, duration);
                return;
            }

            var xaxis = gameObject.transform.right;
            var yaxis = gameObject.transform.up;
            var zaxis = gameObject.transform.forward;
            var globalpos = gameObject.transform.TransformPoint(position);
            Debug.DrawRay(globalpos, xaxis * 0.5f, Color.red, duration, false);
            Debug.DrawRay(globalpos, yaxis * 0.5f, Color.green, duration, false);
            Debug.DrawRay(globalpos, zaxis * 0.5f, Color.blue, duration, false);
        }

        #endregion

        #region VERSION

//        // Get Version
//        public static void IncrementVersion()
//        {
//#if UNITY_EDITOR
//            Game4AutomationVersion scriptableversion =
//                UnityEngine.Resources.Load<Game4AutomationVersion>("Game4AutomationVersion");
//            scriptableversion.Build = scriptableversion.Build + 1;
//#endif
//        }

//        public static void SetVersion()
//        {
//            Game4AutomationVersion scriptableversion =
//                UnityEngine.Resources.Load<Game4AutomationVersion>("Game4AutomationVersion");
//            if (scriptableversion != null)
//            {
//                Build = scriptableversion.Build.ToString();
//                Build = Build.Replace("\n", "");


//                Release = scriptableversion.Release;
//                scriptableversion.Build = int.Parse(Build);
//                Version = Release + "." + Build + " (Unity " + Application.unityVersion + ")";
//            }
//        }

        static Global()
        {
            Initialize();
        }

        #endregion


        #region EVENTS

#if UNITY_EDITOR
        // Global Events
        public static void OnSceneLoaded(Scene scene, OpenSceneMode mode)
        {
            Debug.Log("RDTS Scene " + scene.name + " loaded");
            try
            {
                var rootobjs = scene.GetRootGameObjects();

            }
            catch
            {

            }
        }

        // Global Events
        public static void OnSceneClosing(Scene scene, bool removing)
        {
            QuickToggle.SetRDTSController(null);
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            Debug.Log(state);
            if (rdtsControllernotnull)
                if (state == PlayModeStateChange.EnteredEditMode)
                {
                    rdtsController.OnPlayModeFinished();
                }
        }

#endif

        // When Unity Is Loaded
#if !UNITY_EDITOR
      [RuntimeInitializeOnLoadMethod]
#endif
        public static void Initialize()
        {
          //  SetVersion();

#if UNITY_EDITOR
            EditorSceneManager.sceneOpened += OnSceneLoaded;
            EditorSceneManager.sceneClosing += OnSceneClosing;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
        }

        #endregion
    }
}

// Started Before Build
#if UNITY_EDITOR
class RDTSBuildProcessor : IPreprocessBuildWithReport
{
    public int callbackOrder
    {
        get { return 0; }
    }

    public void OnPreprocessBuild(BuildReport target)
    {
        RDTS.Method.Global.Initialize();
    }
}

#endif