///********************************************************************************************
///this is part of Robot Digital Twin System.                                                 *
///@Copyright by Shaw.S , 2022                                                                *
///Do not distribute without authorization,thanks!                                            *
///********************************************************************************************
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;


namespace RDTS.Utility
{
    /// <summary>
    /// ����ص�Ч���࣬�ṩһϵ�й��ڳض���Ĵ��������á���Ĭ����⡢������ա����ٵķ���������ЧӦ�����ʵ�ָ�����湦�ܡ�
    /// </summary>
    public class ObjectPoolUtility : BaseObjectPool
    {
        /// <summary>
        /// �ض��󽻸���ʽ
        /// </summary>
        public enum DeliverWay
        {
            Signal,//�ź�
            Distance,//��������
            Interval//���ʱ��
        }

        public GameObject prototypeObject;//�ض���ԭ��(�������ж��������)
        [SerializeField][NaughtyAttributes.ReadOnly]
        private int number = 0;//�ض��������
        public int presetNumber;//��Ԥ�ȴ����õĳض�������
        [NaughtyAttributes.ReadOnly]
        public List<PoolObject> poolObjects = new List<PoolObject>();//�ض����б�

        [Header("Deliver Setting")]
        public bool StartDeliver = false;//�Ƿ����ض���Ľ���
        protected bool _LastStartDeliver = false;//StartDeliver�ϵ�һ��ֵ
        public DeliverWay deliverWay = DeliverWay.Signal;//ѡ�񽻸���ʽ
        [ShowIf("deliverWay", DeliverWay.Signal)] public ValueMiddleBool signal;//�źſ���
        [ShowIf("deliverWay", DeliverWay.Distance)] public float distance = 300;//�������
        //[ShowIf("deliverWay", DeliverWay.Interval)] public float startInterval = 0;//��ʼʱ��
        [ShowIf("deliverWay", DeliverWay.Interval)] public float interval = 3;//���ʱ��
        private bool _deliverSignalNotNull = false;//�ź��Ƿ�Ϊ��
        private bool _lastSignalValue = false;//��¼�źŵ���һ��ֵ
        private PoolObject _lastDeliverPoolObj;//��һ����������ȥ�ĳض���
        private Coroutine IntervalCoro;//���ʱ���Э��

        ///���ֵ���ʽ��¼ÿһ��ID��Ӧ�ĳض���
        //ȫ���ĳض���
        private Dictionary<int, PoolObject> poolObjectPerID = new Dictionary<int, PoolObject>();
        //�����õĳض���
        private Dictionary<int, PoolObject> adoptedPoolObjPerID = new Dictionary<int, PoolObject>();
        //��⵽gameobject����ȡPoolObject
        public Dictionary<GameObject, PoolObject> adoptedPoolObjPerObj = new Dictionary<GameObject, PoolObject>();
        //���ڳ�Ĭ״̬�ĳض���
        private Dictionary<int, PoolObject> silentPoolObjPerID = new Dictionary<int, PoolObject>();
        //���ڼ���״̬�ĳض���
        private Dictionary<int, PoolObject> activePoolObjPerID = new Dictionary<int, PoolObject>();
        //��������״̬�ĳض���
        private Dictionary<int, PoolObject> brokenPoolObjPerID = new Dictionary<int, PoolObject>();
        /// <summary>����ض������Ķ���</summary>
        private Queue<PoolObject> poolObjAssign = new Queue<PoolObject>();




        private void Start()
        {

            Initialization();



        }

        private void Update()
        {
            DeliverControl(deliverWay);
        }


        private void OnDisable()
        {
            StopAllCoroutines();//�ر�����Э��
        }


        ///��ʼ��
        void Initialization()
        {
            ///��Ԥ�����������ض���
            if (presetNumber > 0)
                CreatePoolObjectAsNeeded(presetNumber);

            ///�ź��ж�
            if (signal != null)
            {
                _deliverSignalNotNull = true;
                _lastSignalValue = signal.Value;
            }
            else
                _deliverSignalNotNull = false;

            ///���ʱ�䴦��
            //if (StartDeliver && deliverWay == DeliverWay.Interval && interval > 0)
            //{
            //    InvokeRepeating("DeliverPoolObjectViaInterval", startInterval, interval);//��startInterval�����÷�����Ȼ��ÿinterval�����һ��
            //}
            _LastStartDeliver = false;

            ///������봦��
            if (deliverWay == DeliverWay.Distance)
            {
                _lastDeliverPoolObj = null;
            }

            ///Դ����Ĵ���
            SetPrototypeObj();
        }


        #region �ض��󽻸��ķ���

        protected void DeliverControl(DeliverWay way)
        {
            switch(way)
            {
                case DeliverWay.Signal:
                    if (StartDeliver)  
                        DeliverPoolObjectViaSignal();
                    break;
                case DeliverWay.Distance:
                    if (StartDeliver) 
                        DeliverPoolObjectViaDistance();
                    break;
                case DeliverWay.Interval:
                    ///...�ڳ�ʼ�����Ѵ���...
                    if(!_LastStartDeliver && StartDeliver)
                    {
                        IntervalCoro = StartCoroutine(DeliverPoolObjectViaInterval(interval));///����Э��
                        _LastStartDeliver = true;
                        QM.Log("����IntervalЭ��");
                    }
                    if(_LastStartDeliver && !StartDeliver)
                    {
                        StopCoroutine(IntervalCoro);///�ر�Э��
                        _LastStartDeliver = false;
                        QM.Log("�ر�IntervalЭ��");
                    }
                    break;
            }
        }


        /// <summary>���źſ��ƶԳض���Ĳ���</summary>
        protected void DeliverPoolObjectViaSignal()
        {
            if (_deliverSignalNotNull && !_lastSignalValue && signal.Value)//�źŴ��ڣ����źŵ���һ��ֵΪfalse�����źŵ�ǰֵΪtrue
                AdoptPoolObject();

            _lastSignalValue = signal.Value;//ʹ�ú��¼��ǰֵΪ��һ��ֵ
        }

        /// <summary>�ɾ��������ƶԳض���Ĳ���</summary>
        protected void DeliverPoolObjectViaDistance()
        {
            if(_lastDeliverPoolObj != null)
            {
                float _distance = Vector3.Distance(_lastDeliverPoolObj.poolObject.transform.position, prototypeObject.transform.position) 
                    * RDTSController.Scale;

                if (_distance >= distance)
                {
                    _lastDeliverPoolObj = AdoptPoolObject();//����һ���ض��󣬲�������Ϊ"��һ���ض���"
                }
            }
            else
                _lastDeliverPoolObj = AdoptPoolObject();
        }

        /// <summary>��ʱ�������ƶԳض���Ĳ���</summary>
        protected void DeliverPoolObjectViaInterval()
        {
            AdoptPoolObject();
        }


        /// <summary>
        /// ��ʱ�������ƶԳض���Ĳ���
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        protected IEnumerator DeliverPoolObjectViaInterval(float interval)
        {
            for (; ; )
            {
                DeliverPoolObjectViaInterval();
                yield return new WaitForSeconds(interval);//�ӳ�ʱ��       
                
            }
                
            
        }


        #endregion

        #region Դ����Ĵ���

        protected void SetPrototypeObj(bool active = false)
        {
            if(prototypeObject != null)
                prototypeObject.SetActive(active);
        }


        #endregion


        #region �ض���Ĵ�����

        /// <summary>
        /// ����һ���ض��󣺷���ID��Ĭ�Ͻ��롰��Ĭ״̬��
        /// </summary>
        /// <param name="prototype"></param>
        /// <returns></returns>
        protected PoolObject CreatePoolObject(GameObject prototype)
        {
            if(prototype == null)
                return null;

            number++;
            int newID = number - 1;
            bool isContains = false;

            do{
                newID++;
                isContains = CheckIDIsNotContains(newID);//����ID������������Ϊtrue
            } while (!isContains);

            GameObject newPoolObj = Instantiate<GameObject>(prototype);//����Դ����ʵ��
            newPoolObj.transform.SetParent(this.transform);//���ø���

            //��ȡһ���µĳض���
            PoolObject poolObj = GetPoolObject(prototype.name + newID.ToString(), newID, newPoolObj, PoolObjectState.Silent);
            SilentPoolObject(poolObj);
            ///��¼
            poolObjectPerID[newID] = poolObj;
            poolObjects.Add(poolObj);


            return poolObj;
        }

        /// <summary>����ָ�����������ض���</summary>
        protected void CreatePoolObjectAsNeeded(int number)
        {
            for (int i = 0; i < number; i++)
                CreatePoolObject(prototypeObject);
        }

        /// <summary>
        /// ���һ���ض������𻵷���false
        /// </summary>
        /// <param name="poolObj"></param>
        /// <returns>trueΪ������falseΪ��</returns>
        protected bool CheckPoolObject(PoolObject poolObj)
        {
            if (poolObj == null)
                return false;

            PoolObjectState state = poolObj.state;
            int ID = poolObj.ID;

            ///���ض������Ч�ԣ�״̬������״̬��ID����Ч���������
            if (state != PoolObjectState.Broken && CheckIDIsValid(ID) && CheckIDIsUnique(ID) && poolObj.poolObject!=null)
            {
                return true;
            }
            else
                return false;

            
        }

        /// <summary>
        /// ����һ���ض����ȴӶ����л�ȡ��������Ϊ�������´���
        /// </summary>
        /// <returns></returns>
        protected PoolObject AdoptPoolObject()
        {
            PoolObject poolObj = null;
            if(poolObjAssign.Count>0)
                poolObj = poolObjAssign.Dequeue();///�Ӷ����е���һ���ض���

            if (poolObj != null)//�������п��õĳض���
            {
                int ID = poolObj.ID;
                if (CheckPoolObject(poolObj))
                {
                    ActivePoolObject(poolObj);
                    adoptedPoolObjPerID[ID] = poolObj;///

                    poolObj.RecordInfo_adopt();

                    return poolObj;
                }
                else
                {
                    BreakPoolObject(poolObj);
                }
            }
            else//���������´���
            {
                PoolObject newPoolObj = CreatePoolObject(prototypeObject);

                ActivePoolObject(newPoolObj);
                adoptedPoolObjPerID[newPoolObj.ID] = newPoolObj;
                poolObjAssign.Dequeue();//��Ҫ�������еĴ˶������

                newPoolObj.RecordInfo_adopt();

                return newPoolObj;
            }


            return null;
        }

        /// <summary>
        /// ����һ���ض���
        /// </summary>
        /// <param name="poolObj"></param>
        protected void RecyclePoolObject(PoolObject poolObj)
        {
            if (poolObj == null)
                return;

            if(CheckPoolObject(poolObj))
            {
                SilentPoolObject(poolObj);
                ///���踸����
                poolObj.poolObject.transform.SetParent(this.transform);
                ///����λ��
                poolObj.ResetPosition();
                poolObj.ResetRotation();
                ///���մ���
                poolObj.recyclingTimes++;
                ///��Ϣ��¼
                poolObj.RecordInfo_recycle();
            }
            else
            {
                BreakPoolObject(poolObj);
            }

            adoptedPoolObjPerID.Remove(poolObj.ID);

            ///ÿһ�λ���ʱ����һ���𻵵ĳض���
            ClearBrokenPoolObjects();
        }

        /// <summary>����һ���ض���</summary>
        protected void DestroyPoolObject(PoolObject poolObj)
        {
            if (poolObj == null)
                return;

            if (poolObj.state != PoolObjectState.Broken)
                return;

            int id = poolObj.ID;

            if (poolObjectPerID.ContainsKey(id))
                poolObjectPerID.Remove(id);
            if (adoptedPoolObjPerID.ContainsKey(id))
                adoptedPoolObjPerID.Remove(id);
            if (brokenPoolObjPerID.ContainsKey(id))
                brokenPoolObjPerID.Remove(id);

            if (poolObjects.Contains(poolObj))
                poolObjects.Remove(poolObj);

            number--;

            poolObj.RecordInfo_break();

            Destroy(poolObj.poolObject);
            
        }

        /// <summary>��������𻵵ĳض���</summary>
        protected void ClearBrokenPoolObjects()
        {
            if (brokenPoolObjPerID.Count == 0)
                return;

            foreach(int id in brokenPoolObjPerID.Keys)
            {
                DestroyPoolObject(brokenPoolObjPerID[id]);
            }

        }

        /// <summary>������еĳض���</summary>
        protected void ClearPoolObjects()
        {
            adoptedPoolObjPerID.Clear();
            adoptedPoolObjPerObj.Clear();
            silentPoolObjPerID.Clear();
            activePoolObjPerID.Clear();
            brokenPoolObjPerID.Clear();
            poolObjAssign.Clear();
            poolObjects.Clear();

            foreach(int id in poolObjectPerID.Keys)
            {
#if UNITY_EDITOR
                DestroyImmediate(poolObjectPerID[id].poolObject);
#else
                Destroy(poolObjectPerID[id].poolObject);
#endif

            }
            poolObjectPerID.Clear();

            number = 0;

        }



        /// <summary>���ID�Ƿ���Ч�����Ƿ��Ǽ�¼���ֵ��е�ֵ</summary>
        protected bool CheckIDIsValid(int ID)
        {
            if (ID < 0)//ID������С��0
                return false;

            List<int> IDs = new List<int>(poolObjectPerID.Keys);//����ת�����б�
            if (!IDs.Contains(ID))//���б��в�������ID
                return false;
            else
                return true;


        }

        /// <summary>���ID�Ƿ񲻴��ڣ������ڷ���true</summary>
        protected bool CheckIDIsNotContains(int ID)
        {
            if (ID < 0)//ID������С��0
                return false;

            List<int> IDs = new List<int>(poolObjectPerID.Keys);//����ת�����б�
            if (!IDs.Contains(ID))//���б��в�������ID
                return true;
            else
                return false;//������ID

        }

        /// <summary>���ID�Ƿ���Ψһ��</summary>
        protected bool CheckIDIsUnique(int ID)
        {
            if (ID < 0)//ID������С��0
                return false;

            List<int> IDs = new List<int>(poolObjectPerID.Keys);//����ת�����б�
            if (!IDs.Contains(ID))//���б��в�������ID����ֱ�ӷ���
                return false;

            IDs.Remove(ID);//����ID�Ƴ����ٽ���Ψһ�Լ��
            bool isUnique = true;//�ȼ���IDʱΨһ��

            foreach(int id in IDs)
            {
                if(id == ID)//����ID��Ψһ
                {
                    isUnique = false;
                    break;
                }
            }

            return isUnique;
        }

        /// <summary>����adoptedPoolObjPerObj�ֵ��Ԫ��,������</summary>
        protected Dictionary<GameObject, PoolObject> CopyFromDictionary()
        {
            if (adoptedPoolObjPerID == null)
                return null;

            adoptedPoolObjPerObj.Clear();
            foreach(int id in adoptedPoolObjPerID.Keys)
            {
                adoptedPoolObjPerObj[adoptedPoolObjPerID[id].poolObject] = adoptedPoolObjPerID[id];
            }

            return adoptedPoolObjPerObj;
        }
        /// <summary>��ȡadoptedPoolObjPerObj�ֵ��Ԫ��,������</summary>
        protected PoolObject GetPoolObjectFromDictionary(GameObject go)
        {
            if (go == null)
                return null;

            PoolObject poolObj = null;
            try
            {
                CopyFromDictionary();
                if(adoptedPoolObjPerObj.ContainsKey(go))
                    poolObj = adoptedPoolObjPerObj[go];
            }
            catch (Exception e)
            {
                QM.Error(e.ToString());
            }

            return poolObj;
        }


        Coroutine c;///�ñ����洢Э����Ϣ�����Թر�Э��
        protected IEnumerator DelayRecyclePoolObject(PoolObject poolObj, float delay)
        {
            yield return new WaitForSeconds(delay);//�ӳ�ʱ��
            RecyclePoolObject(poolObj);
            StopCoroutine(c);///�ر�Э��
        }




        /// <summary>��Ĭ(����)�ض���</summary>
        protected void SilentPoolObject(PoolObject poolObj)
        {
            try
            {
                //poolObj.state = PoolObjectState.Silent;
                poolObj.SetPoolObjectState(PoolObjectState.Silent);
                poolObj.poolObject?.SetActive(false);

                int id = poolObj.ID;
                silentPoolObjPerID[id] = poolObj;
                poolObjAssign.Enqueue(poolObj);///���

                if (activePoolObjPerID.ContainsKey(id))//����ʱ�Ĵ���
                    activePoolObjPerID.Remove(id);

            }
            catch (Exception e)
            {
                QM.Error(e.ToString());
            }

        }

        /// <summary>����(���óض���)</summary>
        protected void ActivePoolObject(PoolObject poolObj)
        {
            try
            {
                //poolObj.state = PoolObjectState.Active;
                poolObj.SetPoolObjectState(PoolObjectState.Active);
                poolObj.poolObject?.SetActive(true);

                int id = poolObj.ID;
                activePoolObjPerID[id] = poolObj;

                silentPoolObjPerID.Remove(id);
            }
            catch (Exception e)
            {
                QM.Error(e.ToString());
            }

        }

        /// <summary>���ض�����Ϊ����״̬</summary>
        protected void BreakPoolObject(PoolObject poolObj)
        {
            try
            {
               // poolObj.state = PoolObjectState.Broken;
                poolObj.SetPoolObjectState(PoolObjectState.Broken);
                poolObj.poolObject?.SetActive(false);

                int id = poolObj.ID;
                brokenPoolObjPerID[id] = poolObj;

                if(silentPoolObjPerID.ContainsKey(id))
                    silentPoolObjPerID.Remove(id);
                if(activePoolObjPerID.ContainsKey(id))
                    activePoolObjPerID.Remove(id);

            }
            catch
            {

            }
        }


        /// <summary>��ȡһ���µĳض���</summary>
        protected PoolObject GetPoolObject(string name, int ID, GameObject poolObject, PoolObjectState state)
        {
            return new PoolObject( this, name, ID, poolObject, state);
        }

        protected void ConfigurePoolObject(string name, int ID, GameObject poolObject, PoolObjectState state)
        {

        }







        #endregion

        #region ��������

        /// <summary> ����һ���ض��� </summary>
        public PoolObject RequestOnePoolObject()
        {
            return  AdoptPoolObject();
        }


        /// <summary>����gameobject��ȡ��Ӧ�ĳض������еĻ���</summary>
        public PoolObject GetPoolObjByGivenObj(GameObject go)
        {
            if (go == null)
                return null;

            CopyFromDictionary();//�ȸ���һ���ֵ���Ϣ
            if (adoptedPoolObjPerObj.ContainsKey(go))
                return adoptedPoolObjPerObj[go];
            else
                return null;
        }
        
        
        /// <summary>�ӳٻ���ָ����gameobject�����ⲿ����</summary>
        public void RecycleGivenPoolObject(GameObject go, float delay = 0)
        {
            PoolObject poolObj = GetPoolObjectFromDictionary(go);
            if (poolObj != null)
                c = StartCoroutine(DelayRecyclePoolObject(poolObj, delay));///����Э��

        }

        /// <summary>��������ָ����gameobject�����ⲿ����</summary>
        public void RecycleGivenPoolObject(GameObject go)
        {
            PoolObject poolObj = GetPoolObjectFromDictionary(go);
            RecyclePoolObject(poolObj);
        }

        /// <summary>����ָ���ض���ĸ�����</summary>
        public void UpdatePoolObjParent(PoolObject poolObj, GameObject parent)
        {
            poolObj.UpdateParent(parent);
        }


        ///�ṩ��ȡ�ֵ���Ϣ�ķ���

        public Dictionary<int, PoolObject> GetDict_poolObjectPerID()
        {
            return poolObjectPerID;
        }

        public Dictionary<int, PoolObject> GetDict_adoptedPoolObjPerID()
        {
            return adoptedPoolObjPerID;
        }

        public Dictionary<int, PoolObject> GetDict_silentPoolObjPerID()
        {
            return silentPoolObjPerID;
        }

        public Dictionary<int, PoolObject> GetDict_activePoolObjPerID()
        {
            return activePoolObjPerID;
        }

        public Dictionary<int, PoolObject> GetDict_brokenPoolObjPerID()
        {
            return brokenPoolObjPerID;
        }

        public Dictionary<GameObject, PoolObject> GetDict_adoptedPoolObjPerObj()
        {
            CopyFromDictionary();//�����ֵ���Ϣ
            return adoptedPoolObjPerObj;
        }


        #endregion
    }
}
