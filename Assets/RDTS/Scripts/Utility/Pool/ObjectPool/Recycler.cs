///********************************************************************************************
///this is part of Robot Digital Twin System.                                                 *
///@Copyright by Shaw.S , 2023                                                                *
///Do not distribute without authorization,thanks!                                            *
///********************************************************************************************
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;


namespace RDTS.Utility
{

    /// <summary>
    /// ���������Դ���ײ�м�⡣ 
    /// ���ֻ��շ�ʽ��1.��ײ�д���ʱ(��ʱ)���գ���isRecycle = true ��������ʱʱ��recycleDelay�������ձ���⵽�ĵ�������
    ///               2.�����źŵ��������գ���ȡSignalStartRecycle�źŵ�Valueֵ����ֵ��falseתΪtrueʱ���л��գ�����ȫ�����б��еĶ���
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class Recycler : BaseWorkEffector
    {
        public ObjectPoolUtility objectPoolUtility;//�����Ķ����

        [BoxGroup("Settings")] public bool isRecycle = false;//�Ƿ�������
        [BoxGroup("Settings")] public float recycleDelay = 0;//���յ��ӳ�ʱ��

        [BoxGroup("Signal Input")] public ValueMiddleBool SignalStartRecycle;//���ܿ�ʼ�����ź�
        [BoxGroup("Signal Input")] [ReadOnly] public bool _lastSignalStartRecycle = false;

        /* �����¼� */
        [Foldout("Events")] public EventEffect EvenRecycleStart;//���տ�ʼʱ���õ�Even����
        [Foldout("Events")] public EventEffect EvenRecycleEnd;//���ս���ʱ���õ�Even����


        private BoxCollider _boxCollider;//�˽ű����ڵ���ײ��
        private Coroutine recycleCoroutine;///�ñ����洢Э����Ϣ�����Թر�Э��





        // Start is called before the first frame update
        void Start()
        {
            //���Ĭ�ϵ�����Tag��Layer
            AddDefaultTagAndLayer();
            //������ײ��Ϊ������ʽ
            _boxCollider = GetComponent<BoxCollider>();
            _boxCollider.isTrigger = true;
            //���ù����ź�
            SetSignalWorkStart(false);
            SetSignalWorkEnd(false);
        }


        // Update is called once per frame
        void Update()
        {
            if (StopEffectorWorkBySignal())
                return;

            //�����źŻ��շ�ʽ�������������ӳ�
            if (SignalStartRecycle != null && SignalStartRecycle.Value && !_lastSignalStartRecycle)//�ź�ֵ��falseתΪtrueʱ
            {
                //���ù�����ʼ�ź�
                SetSignalWorkStart(true);
                SetSignalWorkEnd(false);

                RecyclePoolObjectOfList(objectPoolUtility, EffectObjects);
                EvenRecycleStart.Invoke();
            }

            if(SignalStartRecycle != null) _lastSignalStartRecycle = SignalStartRecycle.Value;


        }


        void Reset()
        {
            AddDefaultTagAndLayer();//���Ĭ�ϵ�����Tag��Layer
        }


        private void OnTriggerEnter(Collider other)
        {
            if (StopEffectorWorkBySignal())
                return;

            GameObject go = other.gameObject;
            //�ж���ײ�����Ķ����Ƿ����Tag��Layer������
            if (!SusGameObjectLimitTagAndlayer(go))
                return;


            if (!EffectObjects.Contains(go))
            {
                EffectObjects.Add(go);//���뵽�б�
                effectObjPerGo[go] = new EffectObject() { rigidbody = other.GetComponent<Rigidbody>() ?? null };//�����ֵ�
            }
              

            //���ռ�⵽�Ķ���
            if(isRecycle)
            {
                //���ù�����ʼ�ź�
                SetSignalWorkStart(true);
                SetSignalWorkEnd(false);

                RecycleOnePoolObject(objectPoolUtility, go, recycleDelay);//�ӳٻ��ն���

                EvenRecycleStart.Invoke();
               
            }




        }




        private void OnTriggerExit(Collider other)
        {
            if (StopEffectorWorkBySignal())
                return;

            GameObject go = other.gameObject;



            if (EffectObjects.Contains(go))
            {
                EffectObjects.Remove(go);//���б��Ƴ�
                if (effectObjPerGo.ContainsKey(go))//���ֵ��Ƴ�
                    effectObjPerGo.Remove(go);

                EvenRecycleEnd.Invoke();
            }
              



        }



        /// <summary>
        /// ����һ���ض���
        /// </summary>
        /// <param name="go">��Ҫ���յĶ���</param>
        /// <param name="delay">�ӳ�ʱ��</param>
        public void RecycleOnePoolObject(ObjectPoolUtility objectPoolUtility, GameObject go, float delay = 0f)
        {
            if (go == null)
                return;

            recycleCoroutine = StartCoroutine(DelayRecycleObject(objectPoolUtility, go, delay));///����Э��
        }


        /// <summary>
        /// Э�̷������ӳٻ���һ���ض���
        /// </summary>
        /// <param name="go"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        protected IEnumerator DelayRecycleObject(ObjectPoolUtility objectPoolUtility, GameObject go, float delay = 0)
        {
            yield return new WaitForSeconds(delay);//�ӳ�ʱ��

            if(objectPoolUtility != null)
            {
                objectPoolUtility.RecycleGivenPoolObject(go);

                //�Ƴ����������Ӧ�Ķ���
                if (EffectObjects.Contains(go))
                {
                    EffectObjects.Remove(go);//���б��Ƴ�
                    if (effectObjPerGo.ContainsKey(go))//���ֵ��Ƴ�
                        effectObjPerGo.Remove(go);
                }
                  
            }

            //���ù�������ź�
            SetSignalWorkStart(false);
            SetSignalWorkEnd(true);

            EvenRecycleEnd.Invoke();
            StopCoroutine(recycleCoroutine);///�ر�Э��
        }





        /// <summary>
        /// ����ָ���б��еĳض���
        /// </summary>
        /// <param name="objList">���ն�����б�</param>
        /// <param name="delay">�ӳ�ʱ��</param>
        public void RecyclePoolObjectOfList(ObjectPoolUtility objectPoolUtility, List<GameObject> objList, float delay = 0f)
        {
            if (objList == null || objList.Count == 0)
            {
                if(delay == 0)
                {
                    //���ù����ź�
                    SetSignalWorkStart(false);
                    SetSignalWorkEnd(false);
                }
                return;
            }
               

            recycleCoroutine = StartCoroutine(DelayRecycleObjectOfList(objectPoolUtility, objList, delay));//����Э��
        }





        /// <summary>
        /// Э�̷������ӳٻ���һ���б�Ķ���
        /// </summary>
        /// <param name="objList"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        protected IEnumerator DelayRecycleObjectOfList(ObjectPoolUtility objectPoolUtility, List<GameObject> objList, float delay = 0f)
        {
            yield return new WaitForSeconds(delay);//�ӳ�ʱ��
            ///QM.Log("DelayRecycleObjectOfList �� " + objList.Count);

            for (int i = objList.Count-1; i >= 0; i--)
            {
                var robj = objList[i];
                if (objectPoolUtility != null)
                {
                    objectPoolUtility.RecycleGivenPoolObject(robj);//�ڸ����Ķ�����м������ڲ��õĳض����ٽ��л���
                    EffectObjects.Remove(robj);
                    if (effectObjPerGo.ContainsKey(robj))//���ֵ��Ƴ�
                        effectObjPerGo.Remove(robj);
                }
               /// QM.Log("DelayRecycleObjectOfList �� " + i);
            }


            objList.Clear();
            //���ù�������ź�
            SetSignalWorkStart(false);
            SetSignalWorkEnd(true);

            EvenRecycleEnd.Invoke();
            StopCoroutine(recycleCoroutine);///�ر�Э��
        }



    }


}
