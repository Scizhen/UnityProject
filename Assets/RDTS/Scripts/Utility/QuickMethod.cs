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
    /// 方法库：封装一些快捷方法
    /// </summary>
    public static class QM ///QuickMethod
    {

        #region 消息打印

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
            Log($"Vector2：({v2.x},{v2.y})");
        }
        public static void Log(Vector3 v3)
        {
            Log($"Vector3：({v3.x},{v3.y},{v3.z})");
        }

        public static void Log(Rect rect)
        {
            Log($"Rect：({rect.x},{rect.y},{rect.width},{rect.height})");
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
            Warn($"Vector2：({v2.x},{v2.y})");
        }
        public static void Warn(Vector3 v3)
        {
            Warn($"Vector3：({v3.x},{v3.y},{v3.z})");
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
            Error($"Vector2：({v2.x},{v2.y})");
        }
        public static void Error(Vector3 v3)
        {
            Error($"Vector3：({v3.x},{v3.y},{v3.z})");
        }

        #endregion

        #region 标签Tag

        public static string QueryTag(GameObject go)//返回Tag名称
        {
            return go.tag;
        }

        public static bool QueryTag(GameObject go, RDTSTag tag)//是否与指定Tag相同
        {
            return go.CompareTag(tag.ToString());
        }

        public static void SetTag(GameObject go, RDTSTag tag)//设置Tag
        {
            go.tag = tag.ToString();
        }

        #endregion

        #region 层级Layer

        public static int QueryLayerIndex(GameObject go)//返回对象的层索引
        {
            return go.layer;
        }

        public static int QueryLayerIndex(RDTSLayer layer)//返回指定Layer的层索引
        {
            return LayerMask.NameToLayer(layer.ToString());
        }

        public static int QueryLayerIndex(string layerName)//返回指定Layer名称的层索引
        {
            return LayerMask.NameToLayer(layerName);
        }

        public static string QueryLayerName(int layer)//返回指定层索引下的Layer名称
        {
            return LayerMask.LayerToName(layer);
        }

        public static int QueryLayerMask(RDTSLayer layer)//返回指定Layer的层遮罩
        {
            return LayerMask.GetMask(layer.ToString());///返回：2^index
        }

        public static int QueryLayerMask(string layerName)//返回指定Layer名称的层遮罩
        {
            return LayerMask.GetMask(layerName);///返回：2^index
        }

        public static int GetLayerMaskValue(LayerMask layerMask)//将层遮罩值转换为整数值（层索引）
        {
            return layerMask.value;
        }

        public static int GetLayerMask(List<string> layerList)//将一个layer列表转换成对应的layermask
        {
            return LayerMask.GetMask(layerList.ToArray());
        }


        #endregion


#if UNITY_EDITOR
        #region 添加的方法

        /// <summary>
        /// 添加或创建一个指定的对象（若当前有选中的对象，则为此对象添加）
        /// </summary>
        /// <param name="gameobject"></param>
        public static GameObject AddComponent(GameObject gameobject, string name = "")
        {
            if (gameobject == null)
                return null;

            GameObject component = Selection.activeGameObject;
            //GameObject gameobj = PrefabUtility.InstantiatePrefab(gameobject) as GameObject;//【预制体创建】将给定场景中的给定预制件实例化
            GameObject gameobj = UnityEngine.Object.Instantiate(gameobject);//【gameobject创建】将给定场景中的给定预制件实例化

            if (gameobj != null)
            {
                gameobj.transform.position = new Vector3(0, 0, 0);
                if (component != null)
                {
                    gameobj.transform.parent = component.transform;
                }

                if (name == "") name = gameobject.name;
                gameobj.name = name;

                Undo.RegisterCreatedObjectUndo(gameobj, "Add " + gameobj.name);//针对新创建的对象注册撤销操作
            }

            return gameobj;
        }



        /// <summary>
        /// 添加或创建一个指定路径的对象（若当前有选中的对象，则为此对象添加）
        /// </summary>
        /// <param name="assetpath"></param>
        /// <returns></returns>
        public static GameObject AddComponentByPath(string assetpath)
        {
            GameObject component = Selection.activeGameObject;
            UnityEngine.Object prefab = AssetDatabase.LoadAssetAtPath(assetpath, typeof(GameObject));
            //GameObject gameobj = PrefabUtility.InstantiatePrefab(prefab) as GameObject;//将给定场景中的给定预制件实例化
            GameObject gameobj = UnityEngine.Object.Instantiate(prefab) as GameObject;//将给定场景中的给定预制件实例化

            if (gameobj != null)
            {
                gameobj.transform.position = new Vector3(0, 0, 0);
                if (component != null)
                {
                    gameobj.transform.parent = component.transform;
                }

                Undo.RegisterCreatedObjectUndo(gameobj, "Add " + gameobj.name);//针对新创建的对象注册撤销操作


            }
            return gameobj;
        }

        /// <summary>
        /// 为当前选中到的对象添加类型为type的组件
        /// </summary>
        /// <param name="type"></param>
        public static void AddScript(System.Type type)
        {
            GameObject component = Selection.activeGameObject;//当前选中的对象

            if (component != null)
            {
                Undo.AddComponent(component, type);//向component添加类型为type的组件
            }
            else
            {
                EditorUtility.DisplayDialog("缺少目标对象", "请先选择需添加此脚本的对象!", "OK");
            }
        }


        /// <summary>
        /// 在指定的对象下创建一个"值对象"
        /// </summary>
        /// <param name="gameObj">创建“值”的父对象</param>
        /// <param name="name">“值”名称</param>
        /// <param name="type">“值”数据类型</param>
        /// <param name="direction">“值”数据方向</param>
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
                valueObj.transform.parent = gameObj.transform;//指定父对象
                valueObj.name = name;//设置名称
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
        /// 在指定的对象下创建一个"值对象"（带撤销操作）
        /// </summary>
        /// <param name="gameObj">创建“值”的父对象</param>
        /// <param name="name">“值”名称</param>
        /// <param name="type">“值”数据类型</param>
        /// <param name="direction">“值”数据方向</param>
        /// <returns>创建的gameobject</returns>
        public static GameObject CreateOneValueObj(GameObject parentobj, string name, string address, VALUETYPE type, VALUEDIRECTION direction)
        {
            GameObject valueObj;
            Value newValue = null;
            Value valueScript = null;

            if (parentobj == null)
            {
                valueObj = new GameObject();
                valueObj.name = name;//设置名称
                Undo.RegisterCreatedObjectUndo(valueObj, "Add " + valueObj.name);//针对新创建的对象注册撤销操作

            }
            else
            {
                valueObj = new GameObject();
                valueObj.transform.parent = parentobj.transform;//指定父对象
                valueObj.name = name;//设置名称
                Undo.RegisterCreatedObjectUndo(valueObj, "Add " + valueObj.name);//针对新创建的对象注册撤销操作
            }


            //添加Input类型
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
            //添加Output类型
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
            //添加Middle类型
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


            if (newValue != null)//若添加了符合要求的脚本
                valueScript = newValue;
            else//若没有添加符合要求的脚本，就获取此对象上原来的脚本
                valueScript = valueObj.gameObject.GetComponent<Value>();

            var allvaluescripts = valueObj.GetComponents<Value>();//获取所有的Value脚本
            foreach (var vals in allvaluescripts)//销毁多余不匹配的Value脚本
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
        /// 修改指定值对象的脚本
        /// </summary>
        /// <param name="gameObj">创建“值”的父对象</param>
        /// <param name="name">“值”名称</param>
        /// <param name="type">“值”数据类型</param>
        /// <param name="direction">“值”数据方向</param>
        /// <returns>创建的gameobject</returns>
        public static GameObject EditOneValueObj(GameObject valueObj, string name, string address, VALUETYPE type, VALUEDIRECTION direction)
        {
            Value newValue = null;
            Value valueScript = null;
            QM.Log($"编辑修改：{name}, {address}, {type.ToString()}, {direction.ToString()}");

            if (valueObj == null)
            {
                valueObj = new GameObject();
                Undo.RegisterCreatedObjectUndo(valueObj, "Add " + valueObj.name);//针对新创建的对象注册撤销操作 
            }

            valueObj.name = name;//设置名称

            //添加Input类型
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
            //添加Output类型
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
            //添加Middle类型
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


            if (newValue != null)//若添加了符合要求的脚本
            {
                Undo.RegisterCreatedObjectUndo(newValue, "Add " + newValue.name);//针对新创建的对象注册撤销操作 
                valueScript = newValue;
            }
            else//若没有添加符合要求的脚本，就获取此对象上原来的脚本
                valueScript = valueObj.gameObject.GetComponent<Value>();

            var allvaluescripts = valueObj.GetComponents<Value>();//获取所有的Value脚本
            foreach (var vals in allvaluescripts)//销毁多余不匹配的Value脚本
            {
                if (vals != valueScript)
                {
                    Undo.DestroyObjectImmediate(vals);//撤销销毁对象操作（*要放在销毁语句前）
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

        /// <summary>
        /// 为指定对象添加指定材质
        /// </summary>
        /// <param name="go"></param>
        /// <param name="material"></param>
        /// <returns></returns>
        public static bool AddMaterialToGameobject(GameObject go, Material material)
        {
            if (go == null)//未选中某个对象
                return false;

            MeshRenderer meshRenderer = go.GetComponent<MeshRenderer>();
            if (meshRenderer == null)//不存在MeshRenderer组件则返回false
                return false;

            Undo.RecordObject(meshRenderer, "Add Material to Gameobject");//撤销操作
            meshRenderer.sharedMaterial = material;
            return true;//能正确添加材质就返回true
        }


        #endregion
#endif

        #region 搜索的方法

        /// <summary>
        /// 判断当前场景中是否具有指定type类型的根对象
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
        /// 获取当前场景中具有指定type类型的根对象
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
        /// 获取当前场景下所有挂载有指定type类型组件的对象
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<GameObject> GetGameObjectsInCurrentScene(Type type)
        {
            //Type searchType = Type.GetType(type.ToString());//不需要多转一步

            Scene currentScene = SceneManager.GetActiveScene();
            List<GameObject> rootGameObjs = currentScene.GetRootGameObjects().ToList();//获取当前场景下的所有根对象列表
            List<GameObject> needGameObjs = new List<GameObject>();
            rootGameObjs.ForEach(rgos =>
            {
                List<Component> comps = rgos.GetComponentsInChildren(type).ToList();//获取此根对象下具有type的所有子组件
                comps.ForEach(comp => { needGameObjs.Add(comp.gameObject); });//将所有子组件对应的gameobject加入到列表中
            });

            return needGameObjs;
        }


        /// <summary>
        /// 获取指定对象下挂载有type类型组件的对象
        /// </summary>
        /// <param name="parentObj"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<GameObject> GetGameObjectsUnderGivenGameobject(GameObject parentObj, Type type)
        {
            List<GameObject> needGameObjs = new List<GameObject>();
            List<Component> comps = parentObj.GetComponentsInChildren(type).ToList();//获取此根对象下具有type的所有子组件
            comps.ForEach(comp => { needGameObjs.Add(comp.gameObject); });//将所有子组件对应的gameobject加入到列表中
            return needGameObjs;
        }



        /// <summary>
        /// 通过名称获取指定对象上的子对象
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

        #region 场景中对象选择的方法


        /// <summary>
        /// 在场景中选中给定的对象
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
        /// 重置选择对象为空
        /// </summary>
        public static void SelectObjectIsNull()
        {
            Selection.objects = null;
        }

        #endregion

        #region Excel相关方法





        #endregion




    }

}
