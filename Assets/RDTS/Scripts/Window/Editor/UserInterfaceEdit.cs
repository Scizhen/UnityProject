///********************************************************************************************
///this is part of Robot Digital Twin System.                                                 *
///@Copyright by Shaw.S                                                                       *
///Do not distribute without authorization,thanks!                                            *
///********************************************************************************************

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
//using RDTS.NodeEditor;
using System.Linq;
using System;

namespace RDTS.Method
{
#if UNITY_EDITOR
    /// <summary>
    /// 用户界面编辑的方法类
    /// </summary>
    public static class UserInterfaceEdit
    {
        /* 
        *用法：
        *string path;
        *Rect rect;
        *void OnGUI()
        *{
        * rect = EditorGUILayout.GetControlRect(GUILayout.Width(300));
        *  path = UserInterfaceEdit.DragInto(rect, path);
        *}
        */
        /// <summary>
        /// 拖拽获取对象路径(矩形框形式)
        /// </summary>
        /// <param name="rect">被拖入对象的字段</param>
        /// <param name="path">所要获取的路径</param>
        public static string DragInto(Rect rect, string path)
        {
            path = EditorGUI.TextField(rect, path);

            if ((Event.current.type == EventType.DragUpdated || Event.current.type == EventType.MouseUp || Event.current.type == EventType.DragExited)
              && rect.Contains(Event.current.mousePosition))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;//改变鼠标的外表
                if (DragAndDrop.paths != null && DragAndDrop.paths.Length > 0 && (Event.current.type == EventType.MouseUp || Event.current.type == EventType.DragExited))
                {
                    path = DragAndDrop.paths[0];
                }
            }

            return path;

        }

        /* 用法：
        *GameObject obj;
        *Rect rect;
        *void OnGUI()
        *{
        * rect = EditorGUILayout.GetControlRect(GUILayout.Width(300));
        * obj = UserInterfaceEdit.DragInto(rect, obj);
        *}
        */
        /// <summary>
        /// 拖拽获取gameobject对象(矩形框形式)
        /// </summary>
        /// <param name="rect">被拖入对象的字段</param>
        /// <param name="path">所要获取的gameobject对象</param>
        public static GameObject DragInto(Rect rect, GameObject obj)
        {
            obj = (GameObject)EditorGUI.ObjectField(rect, obj, typeof(UnityEngine.Object), true);

            if ((Event.current.type == EventType.DragUpdated || Event.current.type == EventType.MouseUp || Event.current.type == EventType.DragExited)
              && rect.Contains(Event.current.mousePosition))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;//改变鼠标的外表
                if (DragAndDrop.paths != null && DragAndDrop.paths.Length > 0 && (Event.current.type == EventType.MouseUp || Event.current.type == EventType.DragExited))
                {
                    obj = (GameObject)DragAndDrop.objectReferences[0];
                }
            }

            return obj;

        }

        public static Value DragInto(Rect rect, Value obj)
        {
            obj = (Value)EditorGUI.ObjectField(rect, obj, typeof(Value), true);

            if ((Event.current.type == EventType.DragUpdated || Event.current.type == EventType.MouseUp || Event.current.type == EventType.DragExited)
              && rect.Contains(Event.current.mousePosition))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;//改变鼠标的外表
                if (DragAndDrop.paths != null && DragAndDrop.paths.Length > 0 && (Event.current.type == EventType.MouseUp || Event.current.type == EventType.DragExited))
                {
                    obj = (Value)DragAndDrop.objectReferences[0];
                }
            }

            return obj;

        }

        public static ScriptableObject DragInto(Rect rect, ScriptableObject obj)
        {
            obj = (ScriptableObject)EditorGUI.ObjectField(rect, obj, typeof(ScriptableObject), true);

            if ((Event.current.type == EventType.DragUpdated || Event.current.type == EventType.MouseUp || Event.current.type == EventType.DragExited)
              && rect.Contains(Event.current.mousePosition))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;//改变鼠标的外表
                if (DragAndDrop.paths != null && DragAndDrop.paths.Length > 0 && (Event.current.type == EventType.MouseUp || Event.current.type == EventType.DragExited))
                {
                    obj = (ScriptableObject)DragAndDrop.objectReferences[0];
                }
            }

            return obj;

        }

        //public static ProjectViewInterface DragInto(Rect rect, ProjectViewInterface obj)
        //{
        //    obj = (ProjectViewInterface)EditorGUI.ObjectField(rect, obj, typeof(ProjectViewInterface), true);

        //    if ((Event.current.type == EventType.DragUpdated || Event.current.type == EventType.MouseUp || Event.current.type == EventType.DragExited)
        //      && rect.Contains(Event.current.mousePosition))
        //    {
        //        DragAndDrop.visualMode = DragAndDropVisualMode.Generic;//改变鼠标的外表
        //        if (DragAndDrop.paths != null && DragAndDrop.paths.Length > 0 && (Event.current.type == EventType.MouseUp || Event.current.type == EventType.DragExited))
        //        {
        //            obj = (ProjectViewInterface)DragAndDrop.objectReferences[0];
        //        }
        //    }

        //    return obj;

        //}

        //public static T<Rect,Type> DragInto(Rect rect, T obj)
        //{
        //    obj = (T)EditorGUI.ObjectField(rect, obj, typeof(T), true);

        //    if ((Event.current.type == EventType.DragUpdated || Event.current.type == EventType.MouseUp || Event.current.type == EventType.DragExited)
        //      && rect.Contains(Event.current.mousePosition))
        //    {
        //        DragAndDrop.visualMode = DragAndDropVisualMode.Generic;//改变鼠标的外表
        //        if (DragAndDrop.paths != null && DragAndDrop.paths.Length > 0 && (Event.current.type == EventType.MouseUp || Event.current.type == EventType.DragExited))
        //        {
        //            obj = (T)DragAndDrop.objectReferences[0];
        //        }
        //    }

        //    return obj;

        //}


        /// <summary>
        /// 鼠标按下，开始一次拖拽
        /// </summary>
        /// <param name="dragObjName">要拖拽的对象名称</param>
        /// <param name="isDrag">为true时进行拖拽</param>
        public static void StartDrag(string dragObjName, bool isDrag)
        {
            if (Event.current.type == EventType.MouseDown && isDrag)
            {
                ///Debug.Log(_dragObjName);
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.StartDrag(dragObjName);
                DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                Event.current.Use();

            }
        }

        /// <summary>
        /// 获取Scene窗口下的鼠标位置的世界坐标
        /// [不能直接用Event.current.mousePosition]
        /// </summary>
        /// <returns></returns>
        public static Vector3 GetMousePosToScene()
        {
            SceneView sceneView = SceneView.currentDrawingSceneView;
            //当前屏幕坐标,左上角(0,0)右下角(camera.pixelWidth,camera.pixelHeight)
            Vector2 mousePos_V2 = Event.current.mousePosition;
            //retina 屏幕需要拉伸值
            float mult = 1;
#if UNITY_5_4_OR_NEWER
            mult = EditorGUIUtility.pixelsPerPoint;
#endif
            //转换成摄像机可接受的屏幕坐标,左下角是(0,0,0);右上角是(camera.pixelWidth,camera.pixelHeight,0)
            mousePos_V2.y = sceneView.camera.pixelHeight - mousePos_V2.y * mult;
            mousePos_V2.x *= mult;
            //近平面往里一些,才能看到摄像机里的位置
            Vector3 mousePos_V3 = mousePos_V2;
            mousePos_V3.z = 30;
            Vector3 mousePos = sceneView.camera.ScreenToWorldPoint(mousePos_V3);
            return mousePos;
        }


        public static void CreateAsset(UnityEngine.Object obj, string assetName, string assetPath)
        {
            AssetDatabase.CreateAsset(obj, assetPath);
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 查找挂载T类型脚本
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<T> FindObjects<T>()
        {
            List<T> objs = new List<T>();
            var gameobjects = UnityEngine.Object.FindObjectsOfType(typeof(GameObject)) as GameObject[];
            foreach (GameObject obj in gameobjects)
            {
                if (obj.GetComponent<T>() != null)
                    objs.Add(obj.GetComponent<T>());
            }

            return objs;
        }


        /// <summary>
        /// 为当前选中的对象添加给定材质
        /// </summary>
        /// <param name="material"></param>
        /// <returns></returns>
        public static bool AddMaterialToGameobject(Material material)
        {
            GameObject obj = Selection.activeGameObject;
            if (obj == null)//未选中某个对象
                return false;

            MeshRenderer meshRenderer = obj.GetComponent<MeshRenderer>();
            if (meshRenderer == null)//不存在MeshRenderer组件则返回false
                return false;

            Undo.RecordObject(meshRenderer, "Add Material to Gameobject");//撤销操作
            meshRenderer.sharedMaterial = material;
            return true;//能正确添加材质就返回true
        }



        public static bool AddAddMaterialToGameobjects(Material material)
        {
            GameObject obj = Selection.activeGameObject;
            if (obj == null)//未选中某个对象
                return false;

            List<MeshRenderer> meshRenderers = obj.GetComponentsInChildren<MeshRenderer>().ToList();
            meshRenderers.ForEach(mr => {
                Undo.RecordObject(mr, "Add Material to Gameobject");//撤销操作
                mr.sharedMaterial = material;
            });


            return true;
        }



    }


   


#endif
}