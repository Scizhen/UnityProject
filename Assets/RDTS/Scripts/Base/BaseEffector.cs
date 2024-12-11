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
        ///public ObjectPoolUtility objectPool;//����ĳض���
        public Rigidbody rigidbody;//�������
    }


    [System.Serializable]
    public class EventEffect : UnityEvent
    {

    }

    /// <summary>
    /// ЧӦ������
    /// </summary>
    public class BaseEffector : RDTSBehavior, IValueInterface
    {
        [ReadOnly] public string effectorStatus = status[0];
        [ReadOnly] public List<GameObject> EffectObjects = new List<GameObject>();//Ч�������б�ЧӦ�����õĶ���
        [Foldout("Limit")] [ReorderableList] public List<string> LimitTag = new List<string>();//���Ʊ�ǩ
        [Foldout("Limit")] [ReorderableList] public List<string> LimitLayer = new List<string>();//���Ʋ㼶
        [HideInInspector] public Dictionary<GameObject, EffectObject> effectObjPerGo = new Dictionary<GameObject, EffectObject>();//��¼��������Ϣ���ֵ�

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
        /// ���ü�������Ϣ���ֵ�
        /// </summary>
        /// <param name="dict"></param>
        protected void SetEffectObjPerGoDictionary(Dictionary<GameObject, EffectObject> dict)
        {
            if (dict == null)
                return;

            effectObjPerGo = dict;
        }


        /// <summary>
        /// ����������õı�ǩ
        /// </summary>
        /// <param name="tag"></param>
        protected void AddLimitEffectTag(string tag)
        {
            if(LimitTag != null && !LimitTag.Contains(tag))
                LimitTag.Add(tag);
        }
        

        /// <summary>
        /// ����������õĲ㼶
        /// </summary>
        /// <param name="layer"></param>
        protected void AddLimitEffectLayer(string layer)
        {
            if(LimitLayer!= null && !LimitLayer.Contains(layer))
                LimitLayer.Add(layer);
        }


        /// <summary>
        /// ���Ĭ�ϵ����Ʊ�ǩ�Ͳ㼶
        /// </summary>
        protected void AddDefaultTagAndLayer()
        {
            AddLimitEffectTag(RDTSTag.Product.ToString());
            AddLimitEffectTag(RDTSTag.Workpiece.ToString());
            AddLimitEffectTag(RDTSTag.AGV.ToString());

            AddLimitEffectLayer(RDTSLayer.MU.ToString());
        }


        /// <summary>
        /// �����Ƶ���Ӷ���
        /// </summary>
        /// <param name="go"></param>
        protected void AddLimitToEffectObjects(GameObject go)
        {

            LimitTag?.ForEach(lt => {
                if (lt == go.tag && !EffectObjects.Contains(go))//��ǩ�����Ʒ�Χ�ڣ��һ�δ��¼���б���
                    EffectObjects.Add(go);
            });


            LimitLayer?.ForEach(lt => {
                if (lt == RDTS.Utility.QM.QueryLayerName(go.layer) && !EffectObjects.Contains(go))//�㼶�����Ʒ�Χ�ڣ��һ�δ��¼���б���
                    EffectObjects.Add(go);
            });


        }


        /// <summary>
        /// �ж�ָ�������Tag��Layer�Ƿ������Ʒ�Χ��
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
