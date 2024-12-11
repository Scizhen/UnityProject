using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;

namespace RDTS
{
    public class EffectObject
    {
        ///public ObjectPoolUtility objectPool;//对象的池对象
        public Rigidbody rigidbody;//刚体组件
    }


    [System.Serializable]
    public class EventEffect : UnityEvent
    {

    }

    /// <summary>
    /// 效应器基类
    /// </summary>
    public class BaseEffector : RDTSBehavior, IValueInterface
    {
        [ReadOnly] public string effectorStatus = status[0];
        [ReadOnly] public List<GameObject> EffectObjects = new List<GameObject>();//效果对象列表，效应器作用的对象集
        [Foldout("Limit")] [ReorderableList] public List<string> LimitTag = new List<string>();//限制标签
        [Foldout("Limit")] [ReorderableList] public List<string> LimitLayer = new List<string>();//限制层级
        [HideInInspector] public Dictionary<GameObject, EffectObject> effectObjPerGo = new Dictionary<GameObject, EffectObject>();//记录检测对象信息的字典

        protected static List<string> status = new List<string>() { "working", "stop"};

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        void Reset()
        {
            
        }


        /// <summary>
        /// 设置检测对象信息的字典
        /// </summary>
        /// <param name="dict"></param>
        protected void SetEffectObjPerGoDictionary(Dictionary<GameObject, EffectObject> dict)
        {
            if (dict == null)
                return;

            effectObjPerGo = dict;
        }


        /// <summary>
        /// 添加限制作用的标签
        /// </summary>
        /// <param name="tag"></param>
        protected void AddLimitEffectTag(string tag)
        {
            if(LimitTag != null && !LimitTag.Contains(tag))
                LimitTag.Add(tag);
        }
        

        /// <summary>
        /// 添加限制作用的层级
        /// </summary>
        /// <param name="layer"></param>
        protected void AddLimitEffectLayer(string layer)
        {
            if(LimitLayer!= null && !LimitLayer.Contains(layer))
                LimitLayer.Add(layer);
        }


        /// <summary>
        /// 添加默认地限制标签和层级
        /// </summary>
        protected void AddDefaultTagAndLayer()
        {
            AddLimitEffectTag(RDTSTag.Product.ToString());
            AddLimitEffectTag(RDTSTag.Workpiece.ToString());
            AddLimitEffectTag(RDTSTag.AGV.ToString());

            AddLimitEffectLayer(RDTSLayer.MU.ToString());
        }


        /// <summary>
        /// 有限制地添加对象
        /// </summary>
        /// <param name="go"></param>
        protected void AddLimitToEffectObjects(GameObject go)
        {

            LimitTag?.ForEach(lt => {
                if (lt == go.tag && !EffectObjects.Contains(go))//标签在限制范围内，且还未记录再列表中
                    EffectObjects.Add(go);
            });


            LimitLayer?.ForEach(lt => {
                if (lt == RDTS.Utility.QM.QueryLayerName(go.layer) && !EffectObjects.Contains(go))//层级在限制范围内，且还未记录再列表中
                    EffectObjects.Add(go);
            });


        }


        /// <summary>
        /// 判断指定对象的Tag和Layer是否处于限制范围内
        /// </summary>
        /// <param name="go"></param>
        /// <returns></returns>
        protected bool SusGameObjectLimitTagAndlayer(GameObject go)
        {
            if (go == null)
                return false;

            if (LimitTag.Contains(go.tag))
                return true;


            if(LimitLayer.Contains(RDTS.Utility.QM.QueryLayerName(go.layer)))
                return true;
            
            return false;
        }


    }




}
