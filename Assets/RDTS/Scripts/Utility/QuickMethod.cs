///********************************************************************************************
///this is part of Robot Digital Twin System.                                                 *
///@Copyright by Shaw.S                                                                       *
///Do not distribute without authorization,thanks!                                            *
///********************************************************************************************
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using RDTS.Data;


namespace RDTS.Utility
{
    /// <summary>
    /// �����⣺��װһЩ��ݷ���
    /// </summary>
    public static class QM ///QuickMethod
    {

        #region ��Ϣ��ӡ

        public static void Log(string info = "")
        {
            UnityEngine.Debug.Log(info);
        }
        public static void Log(GameObject go)
        {
            string goName = go.name;
            UnityEngine.Debug.Log(goName);
        }
        public static void Log<T>(List<T> list)
        {
            list?.ForEach(x => Log(x.ToString()));
        }
        public static void Log<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
        {
            foreach (var key in dictionary.Keys)
            {
                Log($"{key.ToString()}:{dictionary[key].ToString()}");
            }
        }
        public static void Log(Vector2 v2)
        {
            Log($"Vector2��({v2.x},{v2.y})");
        }
        public static void Log(Vector3 v3)
        {
            Log($"Vector3��({v3.x},{v3.y},{v3.z})");
        }

        public static void Log(Rect rect)
        {
            Log($"Rect��({rect.x},{rect.y},{rect.width},{rect.height})");
        }

        public static void Warn(string info = "")
        {
            UnityEngine.Debug.LogWarning(info);
        }
        public static void Warn<T>(List<T> list)
        {
            list?.ForEach(x => Warn(x.ToString()));
        }
        public static void Warn<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
        {
            foreach (var key in dictionary.Keys)
            {
                Warn($"{key.ToString()}:{dictionary[key].ToString()}");
            }
        }
        public static void Warn(Vector2 v2)
        {
            Warn($"Vector2��({v2.x},{v2.y})");
        }
        public static void Warn(Vector3 v3)
        {
            Warn($"Vector3��({v3.x},{v3.y},{v3.z})");
        }

        public static void Error(string info = "")
        {
            UnityEngine.Debug.LogError(info);
        }
        public static void Error<T>(List<T> list)
        {
            list?.ForEach(x => Error(x.ToString()));
        }
        public static void Error<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
        {
            foreach (var key in dictionary.Keys)
            {
                Error($"{key.ToString()}:{dictionary[key].ToString()}");
            }
        }
        public static void Error(Vector2 v2)
        {
            Error($"Vector2��({v2.x},{v2.y})");
        }
        public static void Error(Vector3 v3)
        {
            Error($"Vector3��({v3.x},{v3.y},{v3.z})");
        }

        #endregion

        #region ��ǩTag

        public static string QueryTag(GameObject go)//����Tag����
        {
            return go.tag;
        }

        public static bool QueryTag(GameObject go, RDTSTag tag)//�Ƿ���ָ��Tag��ͬ
        {
            return go.CompareTag(tag.ToString());
        }

        public static void SetTag(GameObject go, RDTSTag tag)//����Tag
        {
            go.tag = tag.ToString();
        }

        #endregion

        #region �㼶Layer

        public static int QueryLayerIndex(GameObject go)//���ض���Ĳ�����
        {
            return go.layer;
        }

        public static int QueryLayerIndex(RDTSLayer layer)//����ָ��Layer�Ĳ�����
        {
            return LayerMask.NameToLayer(layer.ToString());
        }

        public static int QueryLayerIndex(string layerName)//����ָ��Layer���ƵĲ�����
        {
            return LayerMask.NameToLayer(layerName);
        }

        public static string QueryLayerName(int layer)//����ָ���������µ�Layer����
        {
            return LayerMask.LayerToName(layer);
        }

        public static int QueryLayerMask(RDTSLayer layer)//����ָ��Layer�Ĳ�����
        {
            return LayerMask.GetMask(layer.ToString());///���أ�2^index
        }

        public static int QueryLayerMask(string layerName)//����ָ��Layer���ƵĲ�����
        {
            return LayerMask.GetMask(layerName);///���أ�2^index
        }

        public static int GetLayerMaskValue(LayerMask layerMask)//��������ֵת��Ϊ����ֵ����������
        {
            return layerMask.value;
        }

        public static int GetLayerMask(List<string> layerList)//��һ��layer�б�ת���ɶ�Ӧ��layermask
        {
            return LayerMask.GetMask(layerList.ToArray());
        }


        #endregion


#if UNITY_EDITOR
        #region ��ӵķ���

        /// <summary>
        /// ��ӻ򴴽�һ��ָ���Ķ�������ǰ��ѡ�еĶ�����Ϊ�˶�����ӣ�
        /// </summary>
        /// <param name="gameobject"></param>
        public static GameObject AddComponent(GameObject gameobject, string name = "")
        {
            if (gameobject == null)
                return null;

            GameObject component = Selection.activeGameObject;
            //GameObject gameobj = PrefabUtility.InstantiatePrefab(gameobject) as GameObject;//��Ԥ���崴���������������еĸ���Ԥ�Ƽ�ʵ����
            GameObject gameobj = UnityEngine.Object.Instantiate(gameobject);//��gameobject�����������������еĸ���Ԥ�Ƽ�ʵ����

            if (gameobj != null)
            {
                gameobj.transform.position = new Vector3(0, 0, 0);
                if (component != null)
                {
                    gameobj.transform.parent = component.transform;
                }

                if (name == "") name = gameobject.name;
                gameobj.name = name;

                Undo.RegisterCreatedObjectUndo(gameobj, "Add " + gameobj.name);//����´����Ķ���ע�᳷������
            }

            return gameobj;
        }



        /// <summary>
        /// ��ӻ򴴽�һ��ָ��·���Ķ�������ǰ��ѡ�еĶ�����Ϊ�˶�����ӣ�
        /// </summary>
        /// <param name="assetpath"></param>
        /// <returns></returns>
        public static GameObject AddComponentByPath(string assetpath)
        {
            GameObject component = Selection.activeGameObject;
            UnityEngine.Object prefab = AssetDatabase.LoadAssetAtPath(assetpath, typeof(GameObject));
            //GameObject gameobj = PrefabUtility.InstantiatePrefab(prefab) as GameObject;//�����������еĸ���Ԥ�Ƽ�ʵ����
            GameObject gameobj = UnityEngine.Object.Instantiate(prefab) as GameObject;//�����������еĸ���Ԥ�Ƽ�ʵ����

            if (gameobj != null)
            {
                gameobj.transform.position = new Vector3(0, 0, 0);
                if (component != null)
                {
                    gameobj.transform.parent = component.transform;
                }

                Undo.RegisterCreatedObjectUndo(gameobj, "Add " + gameobj.name);//����´����Ķ���ע�᳷������


            }
            return gameobj;
        }

        /// <summary>
        /// Ϊ��ǰѡ�е��Ķ����������Ϊtype�����
        /// </summary>
        /// <param name="type"></param>
        public static void AddScript(System.Type type)
        {
            GameObject component = Selection.activeGameObject;//��ǰѡ�еĶ���

            if (component != null)
            {
                Undo.AddComponent(component, type);//��component�������Ϊtype�����
            }
            else
            {
                EditorUtility.DisplayDialog("ȱ��Ŀ�����", "����ѡ������Ӵ˽ű��Ķ���!", "OK");
            }
        }


        /// <summary>
        /// ��ָ���Ķ����´���һ��"ֵ����"
        /// </summary>
        /// <param name="gameObj">������ֵ���ĸ�����</param>
        /// <param name="name">��ֵ������</param>
        /// <param name="type">��ֵ����������</param>
        /// <param name="direction">��ֵ�����ݷ���</param>
        /// <returns></returns>
        public static Value CreateOneValue(GameObject gameObj, string name, VALUETYPE type, VALUEDIRECTION direction)
        {
            GameObject valueObj;
            Value newValue = null;
            Value valueScript = null;

            valueObj = GetChildByName(gameObj, name);
            if (valueObj == null)
            {
                valueObj = new GameObject();
                valueObj.transform.parent = gameObj.transform;//ָ��������
                valueObj.name = name;//��������
            }

            if (direction == VALUEDIRECTION.INPUT)
            {
                // Byte and  Input
                switch (type)
                {
                    case VALUETYPE.BOOL:
                        if (valueObj.GetComponent<ValueInputBool>() == null)
                        {
                            newValue = valueObj.AddComponent<ValueInputBool>();
                        }

                        break;
                    case VALUETYPE.INT:
                        if (valueObj.GetComponent<ValueInputInt>() == null)
                        {
                            newValue = valueObj.AddComponent<ValueInputInt>();
                        }

                        break;
                    case VALUETYPE.DINT:
                        if (valueObj.GetComponent<ValueInputInt>() == null)
                        {
                            newValue = valueObj.AddComponent<ValueInputInt>();
                        }

                        break;
                    case VALUETYPE.BYTE:
                        if (valueObj.GetComponent<ValueInputInt>() == null)
                        {
                            newValue = valueObj.AddComponent<ValueInputInt>();
                        }

                        break;
                    case VALUETYPE.WORD:
                        if (valueObj.GetComponent<ValueInputInt>() == null)
                        {
                            newValue = valueObj.AddComponent<ValueInputInt>();
                        }

                        break;
                    case VALUETYPE.DWORD:
                        if (valueObj.GetComponent<ValueInputInt>() == null)
                        {
                            newValue = valueObj.AddComponent<ValueInputInt>();
                        }

                        break;
                    case VALUETYPE.REAL:
                        if (valueObj.GetComponent<ValueInputFloat>() == null)
                        {
                            newValue = valueObj.AddComponent<ValueInputFloat>();
                        }
                        break;
                }
            }

            if (direction == VALUEDIRECTION.OUTPUT)
            {
                switch (type)
                {
                    case VALUETYPE.BOOL:
                        if (valueObj.GetComponent<ValueOutputBool>() == null)
                        {
                            newValue = valueObj.AddComponent<ValueOutputBool>();
                        }

                        break;
                    case VALUETYPE.INT:
                        if (valueObj.GetComponent<ValueOutputInt>() == null)
                        {
                            newValue = valueObj.AddComponent<ValueOutputInt>();
                        }

                        break;
                    case VALUETYPE.DINT:
                        if (valueObj.GetComponent<ValueOutputInt>() == null)
                        {
                            newValue = valueObj.AddComponent<ValueOutputInt>();
                        }

                        break;
                    case VALUETYPE.BYTE:
                        if (valueObj.GetComponent<ValueOutputInt>() == null)
                        {
                            newValue = valueObj.AddComponent<ValueOutputInt>();
                        }

                        break;
                    case VALUETYPE.WORD:
                        if (valueObj.GetComponent<ValueOutputInt>() == null)
                        {
                            newValue = valueObj.AddComponent<ValueOutputInt>();
                        }

                        break;
                    case VALUETYPE.DWORD:
                        if (valueObj.GetComponent<ValueOutputInt>() == null)
                        {
                            newValue = valueObj.AddComponent<ValueOutputInt>();
                        }

                        break;
                    case VALUETYPE.REAL:
                        if (valueObj.GetComponent<ValueOutputFloat>() == null)
                        {
                            newValue = valueObj.AddComponent<ValueOutputFloat>();
                        }

                        break;
                }
            }

            if (newValue != null)
                valueScript = newValue;
            else
                valueScript = valueObj.gameObject.GetComponent<Value>();

            var allvaluescripts = valueObj.GetComponents<Value>();
            foreach (var vals in allvaluescripts)
            {
                if (vals != valueScript)
                    UnityEngine.Object.DestroyImmediate(vals);
            }

            if (valueScript != null)
            {
                valueScript.Settings.Active = true;
                valueScript.SetStatusConnected(true);
            }

            return valueScript;
        }


        /// <summary>
        /// ��ָ���Ķ����´���һ��"ֵ����"��������������
        /// </summary>
        /// <param name="gameObj">������ֵ���ĸ�����</param>
        /// <param name="name">��ֵ������</param>
        /// <param name="type">��ֵ����������</param>
        /// <param name="direction">��ֵ�����ݷ���</param>
        /// <returns>������gameobject</returns>
        public static GameObject CreateOneValueObj(GameObject parentobj, string name, string address, VALUETYPE type, VALUEDIRECTION direction)
        {
            GameObject valueObj;
            Value newValue = null;
            Value valueScript = null;

            if (parentobj == null)
            {
                valueObj = new GameObject();
                valueObj.name = name;//��������
                Undo.RegisterCreatedObjectUndo(valueObj, "Add " + valueObj.name);//����´����Ķ���ע�᳷������

            }
            else
            {
                valueObj = new GameObject();
                valueObj.transform.parent = parentobj.transform;//ָ��������
                valueObj.name = name;//��������
                Undo.RegisterCreatedObjectUndo(valueObj, "Add " + valueObj.name);//����´����Ķ���ע�᳷������
            }


            //���Input����
            if (direction == VALUEDIRECTION.INPUT)
            {
                switch (type)
                {
                    case VALUETYPE.BOOL:
                        if (valueObj.GetComponent<ValueInputBool>() == null)
                        {
                            newValue = valueObj.AddComponent<ValueInputBool>();
                        }

                        break;
                    case VALUETYPE.INT:
                        if (valueObj.GetComponent<ValueInputInt>() == null)
                        {
                            newValue = valueObj.AddComponent<ValueInputInt>();
                        }

                        break;
                    case VALUETYPE.REAL:
                        if (valueObj.GetComponent<ValueInputFloat>() == null)
                        {
                            newValue = valueObj.AddComponent<ValueInputFloat>();
                        }

                        break;
                }
            }
            //���Output����
            if (direction == VALUEDIRECTION.OUTPUT)
            {
                switch (type)
                {
                    case VALUETYPE.BOOL:
                        if (valueObj.GetComponent<ValueOutputBool>() == null)
                        {
                            newValue = valueObj.AddComponent<ValueOutputBool>();
                        }

                        break;
                    case VALUETYPE.INT:
                        if (valueObj.GetComponent<ValueOutputInt>() == null)
                        {
                            newValue = valueObj.AddComponent<ValueOutputInt>();
                        }

                        break;
                    case VALUETYPE.REAL:
                        if (valueObj.GetComponent<ValueOutputFloat>() == null)
                        {
                            newValue = valueObj.AddComponent<ValueOutputFloat>();
                        }

                        break;
                }
            }
            //���Middle����
            if (direction == VALUEDIRECTION.INPUTOUTPUT)
            {
                switch (type)
                {
                    case VALUETYPE.BOOL:
                        if (valueObj.GetComponent<ValueMiddleBool>() == null)
                        {
                            newValue = valueObj.AddComponent<ValueMiddleBool>();
                        }

                        break;
                    case VALUETYPE.INT:
                        if (valueObj.GetComponent<ValueMiddleInt>() == null)
                        {
                            newValue = valueObj.AddComponent<ValueMiddleInt>();
                        }

                        break;
                    case VALUETYPE.REAL:
                        if (valueObj.GetComponent<ValueMiddleFloat>() == null)
                        {
                            newValue = valueObj.AddComponent<ValueMiddleFloat>();
                        }

                        break;
                }
            }


            if (newValue != null)//������˷���Ҫ��Ľű�
                valueScript = newValue;
            else//��û����ӷ���Ҫ��Ľű����ͻ�ȡ�˶�����ԭ���Ľű�
                valueScript = valueObj.gameObject.GetComponent<Value>();

            var allvaluescripts = valueObj.GetComponents<Value>();//��ȡ���е�Value�ű�
            foreach (var vals in allvaluescripts)//���ٶ��಻ƥ���Value�ű�
            {
                if (vals != valueScript)
                    UnityEngine.Object.DestroyImmediate(vals);
            }

            if (valueScript != null)
            {
                valueScript.Settings.Active = true;
                valueScript.SetStatusConnected(true);
                valueScript.Name = name;
                valueScript.Address = address;
            }

            return valueObj;
        }


        /// <summary>
        /// �޸�ָ��ֵ����Ľű�
        /// </summary>
        /// <param name="gameObj">������ֵ���ĸ�����</param>
        /// <param name="name">��ֵ������</param>
        /// <param name="type">��ֵ����������</param>
        /// <param name="direction">��ֵ�����ݷ���</param>
        /// <returns>������gameobject</returns>
        public static GameObject EditOneValueObj(GameObject valueObj, string name, string address, VALUETYPE type, VALUEDIRECTION direction)
        {
            Value newValue = null;
            Value valueScript = null;
            QM.Log($"�༭�޸ģ�{name}, {address}, {type.ToString()}, {direction.ToString()}");

            if (valueObj == null)
            {
                valueObj = new GameObject();
                Undo.RegisterCreatedObjectUndo(valueObj, "Add " + valueObj.name);//����´����Ķ���ע�᳷������ 
            }

            valueObj.name = name;//��������

            //���Input����
            if (direction == VALUEDIRECTION.INPUT)
            {
                switch (type)
                {
                    case VALUETYPE.BOOL:
                        if (valueObj.GetComponent<ValueInputBool>() == null)
                        {
                            newValue = valueObj.AddComponent<ValueInputBool>();
                        }

                        break;
                    case VALUETYPE.INT:
                        if (valueObj.GetComponent<ValueInputInt>() == null)
                        {
                            newValue = valueObj.AddComponent<ValueInputInt>();
                        }

                        break;
                    case VALUETYPE.REAL:
                        if (valueObj.GetComponent<ValueInputFloat>() == null)
                        {
                            newValue = valueObj.AddComponent<ValueInputFloat>();
                        }

                        break;
                }
            }
            //���Output����
            if (direction == VALUEDIRECTION.OUTPUT)
            {
                switch (type)
                {
                    case VALUETYPE.BOOL:
                        if (valueObj.GetComponent<ValueOutputBool>() == null)
                        {
                            newValue = valueObj.AddComponent<ValueOutputBool>();
                        }

                        break;
                    case VALUETYPE.INT:
                        if (valueObj.GetComponent<ValueOutputInt>() == null)
                        {
                            newValue = valueObj.AddComponent<ValueOutputInt>();
                        }

                        break;
                    case VALUETYPE.REAL:
                        if (valueObj.GetComponent<ValueOutputFloat>() == null)
                        {
                            newValue = valueObj.AddComponent<ValueOutputFloat>();
                        }

                        break;
                }
            }
            //���Middle����
            if (direction == VALUEDIRECTION.INPUTOUTPUT)
            {
                switch (type)
                {
                    case VALUETYPE.BOOL:
                        if (valueObj.GetComponent<ValueMiddleBool>() == null)
                        {
                            newValue = valueObj.AddComponent<ValueMiddleBool>();
                        }

                        break;
                    case VALUETYPE.INT:
                        if (valueObj.GetComponent<ValueMiddleInt>() == null)
                        {
                            newValue = valueObj.AddComponent<ValueMiddleInt>();
                        }

                        break;
                    case VALUETYPE.REAL:
                        if (valueObj.GetComponent<ValueMiddleFloat>() == null)
                        {
                            newValue = valueObj.AddComponent<ValueMiddleFloat>();
                        }

                        break;
                }
            }


            if (newValue != null)//������˷���Ҫ��Ľű�
            {
                Undo.RegisterCreatedObjectUndo(newValue, "Add " + newValue.name);//����´����Ķ���ע�᳷������ 
                valueScript = newValue;
            }
            else//��û����ӷ���Ҫ��Ľű����ͻ�ȡ�˶�����ԭ���Ľű�
                valueScript = valueObj.gameObject.GetComponent<Value>();

            var allvaluescripts = valueObj.GetComponents<Value>();//��ȡ���е�Value�ű�
            foreach (var vals in allvaluescripts)//���ٶ��಻ƥ���Value�ű�
            {
                if (vals != valueScript)
                {
                    Undo.DestroyObjectImmediate(vals);//�������ٶ��������*Ҫ�����������ǰ��
                    UnityEngine.Object.DestroyImmediate(vals);
                }

            }

            if (valueScript != null)
            {
                valueScript.Settings.Active = true;
                valueScript.SetStatusConnected(true);
                valueScript.Name = name;
                valueScript.Address = address;
            }

            return valueObj;
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

        /// <summary>
        /// Ϊָ���������ָ������
        /// </summary>
        /// <param name="go"></param>
        /// <param name="material"></param>
        /// <returns></returns>
        public static bool AddMaterialToGameobject(GameObject go, Material material)
        {
            if (go == null)//δѡ��ĳ������
                return false;

            MeshRenderer meshRenderer = go.GetComponent<MeshRenderer>();
            if (meshRenderer == null)//������MeshRenderer����򷵻�false
                return false;

            Undo.RecordObject(meshRenderer, "Add Material to Gameobject");//��������
            meshRenderer.sharedMaterial = material;
            return true;//����ȷ��Ӳ��ʾͷ���true
        }


        #endregion
#endif

        #region �����ķ���

        /// <summary>
        /// �жϵ�ǰ�������Ƿ����ָ��type���͵ĸ�����
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool SearchComponentInCurrentScene(Type type)
        {
            ///QM.Log(type.ToString());
            ///QM.Log(Type.GetType(type.ToString()).ToString());
            Type searchType = Type.GetType(type.ToString());

            Scene currentScene = SceneManager.GetActiveScene();
            var gameObjs = currentScene.GetRootGameObjects();
            try
            {
                GameObject gameObj = gameObjs.First(go => go.GetComponentInChildren(searchType));
                if (gameObj != null)
                    return true;
            }
            catch
            {

            }

            return false;
        }

        /// <summary>
        /// ��ȡ��ǰ�����о���ָ��type���͵ĸ�����
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static GameObject GetComponentInCurrentScene(Type type)
        {
            ///QM.Log(type.ToString());
            ///QM.Log(Type.GetType(type.ToString()).ToString());
            Type searchType = Type.GetType(type.ToString());

            Scene currentScene = SceneManager.GetActiveScene();
            var gameObjs = currentScene.GetRootGameObjects();
            try
            {
                GameObject gameObj = gameObjs.First(go => go.GetComponentInChildren(searchType));
                if (gameObj != null)
                    return gameObj;
            }
            catch
            {

            }

            return null;
        }


        /// <summary>
        /// ��ȡ��ǰ���������й�����ָ��type��������Ķ���
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<GameObject> GetGameObjectsInCurrentScene(Type type)
        {
            //Type searchType = Type.GetType(type.ToString());//����Ҫ��תһ��

            Scene currentScene = SceneManager.GetActiveScene();
            List<GameObject> rootGameObjs = currentScene.GetRootGameObjects().ToList();//��ȡ��ǰ�����µ����и������б�
            List<GameObject> needGameObjs = new List<GameObject>();
            rootGameObjs.ForEach(rgos =>
            {
                List<Component> comps = rgos.GetComponentsInChildren(type).ToList();//��ȡ�˸������¾���type�����������
                comps.ForEach(comp => { needGameObjs.Add(comp.gameObject); });//�������������Ӧ��gameobject���뵽�б���
            });

            return needGameObjs;
        }


        /// <summary>
        /// ��ȡָ�������¹�����type��������Ķ���
        /// </summary>
        /// <param name="parentObj"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<GameObject> GetGameObjectsUnderGivenGameobject(GameObject parentObj, Type type)
        {
            List<GameObject> needGameObjs = new List<GameObject>();
            List<Component> comps = parentObj.GetComponentsInChildren(type).ToList();//��ȡ�˸������¾���type�����������
            comps.ForEach(comp => { needGameObjs.Add(comp.gameObject); });//�������������Ӧ��gameobject���뵽�б���
            return needGameObjs;
        }



        /// <summary>
        /// ͨ�����ƻ�ȡָ�������ϵ��Ӷ���
        /// </summary>
        /// <param name="go"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static GameObject GetChildByName(GameObject go, string name)
        {
            Transform[] children = go.GetComponentsInChildren<Transform>();
            foreach (var child in children)
            {
                if (child.name == name)
                {
                    return child.gameObject;
                }
            }

            return null;
        }




        #endregion

        #region �����ж���ѡ��ķ���


        /// <summary>
        /// �ڳ�����ѡ�и����Ķ���
        /// </summary>
        /// <param name="gameObject"></param>
        public static void SelectGameObjectInScene(GameObject gameObject)
        {
            if (gameObject == null)
                return;

            List<UnityEngine.Object> select = new List<UnityEngine.Object>();
            select.Add(gameObject);
            Selection.objects = select.ToArray();
        }


        /// <summary>
        /// ����ѡ�����Ϊ��
        /// </summary>
        public static void SelectObjectIsNull()
        {
            Selection.objects = null;
        }

        #endregion

        #region Excel��ط���





        #endregion




    }

}
