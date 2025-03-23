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
    /// �û�����༭�ķ�����
    /// </summary>
    public static class UserInterfaceEdit
    {
        /* 
        *�÷���
        *string path;
        *Rect rect;
        *void OnGUI()
        *{
        * rect = EditorGUILayout.GetControlRect(GUILayout.Width(300));
        *  path = UserInterfaceEdit.DragInto(rect, path);
        *}
        */
        /// <summary>
        /// ��ק��ȡ����·��(���ο���ʽ)
        /// </summary>
        /// <param name="rect">�����������ֶ�</param>
        /// <param name="path">��Ҫ��ȡ��·��</param>
        public static string DragInto(Rect rect, string path)
        {
            path = EditorGUI.TextField(rect, path);

            if ((Event.current.type == EventType.DragUpdated || Event.current.type == EventType.MouseUp || Event.current.type == EventType.DragExited)
              && rect.Contains(Event.current.mousePosition))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;//�ı��������
                if (DragAndDrop.paths != null && DragAndDrop.paths.Length > 0 && (Event.current.type == EventType.MouseUp || Event.current.type == EventType.DragExited))
                {
                    path = DragAndDrop.paths[0];
                }
            }

            return path;

        }

        /* �÷���
        *GameObject obj;
        *Rect rect;
        *void OnGUI()
        *{
        * rect = EditorGUILayout.GetControlRect(GUILayout.Width(300));
        * obj = UserInterfaceEdit.DragInto(rect, obj);
        *}
        */
        /// <summary>
        /// ��ק��ȡgameobject����(���ο���ʽ)
        /// </summary>
        /// <param name="rect">�����������ֶ�</param>
        /// <param name="path">��Ҫ��ȡ��gameobject����</param>
        public static GameObject DragInto(Rect rect, GameObject obj)
        {
            obj = (GameObject)EditorGUI.ObjectField(rect, obj, typeof(UnityEngine.Object), true);

            if ((Event.current.type == EventType.DragUpdated || Event.current.type == EventType.MouseUp || Event.current.type == EventType.DragExited)
              && rect.Contains(Event.current.mousePosition))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;//�ı��������
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
                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;//�ı��������
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
                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;//�ı��������
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
        //        DragAndDrop.visualMode = DragAndDropVisualMode.Generic;//�ı��������
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
        //        DragAndDrop.visualMode = DragAndDropVisualMode.Generic;//�ı��������
        //        if (DragAndDrop.paths != null && DragAndDrop.paths.Length > 0 && (Event.current.type == EventType.MouseUp || Event.current.type == EventType.DragExited))
        //        {
        //            obj = (T)DragAndDrop.objectReferences[0];
        //        }
        //    }

        //    return obj;

        //}


        /// <summary>
        /// ��갴�£���ʼһ����ק
        /// </summary>
        /// <param name="dragObjName">Ҫ��ק�Ķ�������</param>
        /// <param name="isDrag">Ϊtrueʱ������ק</param>
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
        /// ��ȡScene�����µ����λ�õ���������
        /// [����ֱ����Event.current.mousePosition]
        /// </summary>
        /// <returns></returns>
        public static Vector3 GetMousePosToScene()
        {
            SceneView sceneView = SceneView.currentDrawingSceneView;
            //��ǰ��Ļ����,���Ͻ�(0,0)���½�(camera.pixelWidth,camera.pixelHeight)
            Vector2 mousePos_V2 = Event.current.mousePosition;
            //retina ��Ļ��Ҫ����ֵ
            float mult = 1;
#if UNITY_5_4_OR_NEWER
            mult = EditorGUIUtility.pixelsPerPoint;
#endif
            //ת����������ɽ��ܵ���Ļ����,���½���(0,0,0);���Ͻ���(camera.pixelWidth,camera.pixelHeight,0)
            mousePos_V2.y = sceneView.camera.pixelHeight - mousePos_V2.y * mult;
            mousePos_V2.x *= mult;
            //��ƽ������һЩ,���ܿ�����������λ��
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
        /// ���ҹ���T���ͽű�
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
        /// Ϊ��ǰѡ�еĶ�����Ӹ�������
        /// </summary>
        /// <param name="material"></param>
        /// <returns></returns>
        public static bool AddMaterialToGameobject(Material material)
        {
            GameObject obj = Selection.activeGameObject;
            if (obj == null)//δѡ��ĳ������
                return false;

            MeshRenderer meshRenderer = obj.GetComponent<MeshRenderer>();
            if (meshRenderer == null)//������MeshRenderer����򷵻�false
                return false;

            Undo.RecordObject(meshRenderer, "Add Material to Gameobject");//��������
            meshRenderer.sharedMaterial = material;
            return true;//����ȷ��Ӳ��ʾͷ���true
        }



        public static bool AddAddMaterialToGameobjects(Material material)
        {
            GameObject obj = Selection.activeGameObject;
            if (obj == null)//δѡ��ĳ������
                return false;

            List<MeshRenderer> meshRenderers = obj.GetComponentsInChildren<MeshRenderer>().ToList();
            meshRenderers.ForEach(mr => {
                Undo.RecordObject(mr, "Add Material to Gameobject");//��������
                mr.sharedMaterial = material;
            });


            return true;
        }



    }


   


#endif
}