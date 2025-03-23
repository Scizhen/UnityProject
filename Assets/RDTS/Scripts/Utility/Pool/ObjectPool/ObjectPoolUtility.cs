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
    /// 对象池的效用类，提供一系列关于池对象的创建、采用、沉默、检测、激活、回收、销毁的方法，搭配效应器类可实现各类仿真功能。
    /// </summary>
    public class ObjectPoolUtility : BaseObjectPool
    {
        /// <summary>
        /// 池对象交付方式
        /// </summary>
        public enum DeliverWay
        {
            Signal,//信号
            Distance,//给定距离
            Interval//间隔时间
        }

        public GameObject prototypeObject;//池对象原型(需对其进行冻结和隐藏)
        [SerializeField][NaughtyAttributes.ReadOnly]
        private int number = 0;//池对象的总数
        public int presetNumber;//需预先创建好的池对象数量
        [NaughtyAttributes.ReadOnly]
        public List<PoolObject> poolObjects = new List<PoolObject>();//池对象列表

        [Header("Deliver Setting")]
        public bool StartDeliver = false;//是否开启池对象的交付
        protected bool _LastStartDeliver = false;//StartDeliver上的一个值
        public DeliverWay deliverWay = DeliverWay.Signal;//选择交付方式
        [ShowIf("deliverWay", DeliverWay.Signal)] public ValueMiddleBool signal;//信号控制
        [ShowIf("deliverWay", DeliverWay.Distance)] public float distance = 300;//间隔距离
        //[ShowIf("deliverWay", DeliverWay.Interval)] public float startInterval = 0;//起始时间
        [ShowIf("deliverWay", DeliverWay.Interval)] public float interval = 3;//间隔时间
        private bool _deliverSignalNotNull = false;//信号是否不为空
        private bool _lastSignalValue = false;//记录信号的上一个值
        private PoolObject _lastDeliverPoolObj;//上一个被交付出去的池对象
        private Coroutine IntervalCoro;//间隔时间的协程

        ///以字典形式记录每一个ID对应的池对象
        //全部的池对象
        private Dictionary<int, PoolObject> poolObjectPerID = new Dictionary<int, PoolObject>();
        //被采用的池对象
        private Dictionary<int, PoolObject> adoptedPoolObjPerID = new Dictionary<int, PoolObject>();
        //检测到gameobject来获取PoolObject
        public Dictionary<GameObject, PoolObject> adoptedPoolObjPerObj = new Dictionary<GameObject, PoolObject>();
        //处于沉默状态的池对象
        private Dictionary<int, PoolObject> silentPoolObjPerID = new Dictionary<int, PoolObject>();
        //处于激活状态的池对象
        private Dictionary<int, PoolObject> activePoolObjPerID = new Dictionary<int, PoolObject>();
        //处于破损状态的池对象
        private Dictionary<int, PoolObject> brokenPoolObjPerID = new Dictionary<int, PoolObject>();
        /// <summary>处理池对象分配的队列</summary>
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
            StopAllCoroutines();//关闭所有协程
        }


        ///初始化
        void Initialization()
        {
            ///按预设数量创建池对象
            if (presetNumber > 0)
                CreatePoolObjectAsNeeded(presetNumber);

            ///信号判断
            if (signal != null)
            {
                _deliverSignalNotNull = true;
                _lastSignalValue = signal.Value;
            }
            else
                _deliverSignalNotNull = false;

            ///间隔时间处理
            //if (StartDeliver && deliverWay == DeliverWay.Interval && interval > 0)
            //{
            //    InvokeRepeating("DeliverPoolObjectViaInterval", startInterval, interval);//在startInterval秒后调用方法，然后每interval秒调用一次
            //}
            _LastStartDeliver = false;

            ///间隔距离处理
            if (deliverWay == DeliverWay.Distance)
            {
                _lastDeliverPoolObj = null;
            }

            ///源对象的处理
            SetPrototypeObj();
        }


        #region 池对象交付的方法

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
                    ///...在初始化中已处理...
                    if(!_LastStartDeliver && StartDeliver)
                    {
                        IntervalCoro = StartCoroutine(DeliverPoolObjectViaInterval(interval));///开启协程
                        _LastStartDeliver = true;
                        QM.Log("开启Interval协程");
                    }
                    if(_LastStartDeliver && !StartDeliver)
                    {
                        StopCoroutine(IntervalCoro);///关闭协程
                        _LastStartDeliver = false;
                        QM.Log("关闭Interval协程");
                    }
                    break;
            }
        }


        /// <summary>由信号控制对池对象的采用</summary>
        protected void DeliverPoolObjectViaSignal()
        {
            if (_deliverSignalNotNull && !_lastSignalValue && signal.Value)//信号存在，且信号的上一个值为false，且信号当前值为true
                AdoptPoolObject();

            _lastSignalValue = signal.Value;//使用后记录当前值为上一个值
        }

        /// <summary>由距离间隔控制对池对象的采用</summary>
        protected void DeliverPoolObjectViaDistance()
        {
            if(_lastDeliverPoolObj != null)
            {
                float _distance = Vector3.Distance(_lastDeliverPoolObj.poolObject.transform.position, prototypeObject.transform.position) 
                    * RDTSController.Scale;

                if (_distance >= distance)
                {
                    _lastDeliverPoolObj = AdoptPoolObject();//采用一个池对象，并将其设为"上一个池对象"
                }
            }
            else
                _lastDeliverPoolObj = AdoptPoolObject();
        }

        /// <summary>由时间间隔控制对池对象的采用</summary>
        protected void DeliverPoolObjectViaInterval()
        {
            AdoptPoolObject();
        }


        /// <summary>
        /// 由时间间隔控制对池对象的采用
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        protected IEnumerator DeliverPoolObjectViaInterval(float interval)
        {
            for (; ; )
            {
                DeliverPoolObjectViaInterval();
                yield return new WaitForSeconds(interval);//延迟时间       
                
            }
                
            
        }


        #endregion

        #region 源对象的处理

        protected void SetPrototypeObj(bool active = false)
        {
            if(prototypeObject != null)
                prototypeObject.SetActive(active);
        }


        #endregion


        #region 池对象的处理方法

        /// <summary>
        /// 创建一个池对象：分配ID，默认进入“沉默状态”
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
                isContains = CheckIDIsNotContains(newID);//若此ID不被包括，则为true
            } while (!isContains);

            GameObject newPoolObj = Instantiate<GameObject>(prototype);//创建源对象实例
            newPoolObj.transform.SetParent(this.transform);//设置父级

            //获取一个新的池对象
            PoolObject poolObj = GetPoolObject(prototype.name + newID.ToString(), newID, newPoolObj, PoolObjectState.Silent);
            SilentPoolObject(poolObj);
            ///记录
            poolObjectPerID[newID] = poolObj;
            poolObjects.Add(poolObj);


            return poolObj;
        }

        /// <summary>按照指定数量创建池对象</summary>
        protected void CreatePoolObjectAsNeeded(int number)
        {
            for (int i = 0; i < number; i++)
                CreatePoolObject(prototypeObject);
        }

        /// <summary>
        /// 检查一个池对象，若损坏返回false
        /// </summary>
        /// <param name="poolObj"></param>
        /// <returns>true为正常，false为损坏</returns>
        protected bool CheckPoolObject(PoolObject poolObj)
        {
            if (poolObj == null)
                return false;

            PoolObjectState state = poolObj.state;
            int ID = poolObj.ID;

            ///检查池对象的有效性：状态不是损坏状态，ID号有效，对象存在
            if (state != PoolObjectState.Broken && CheckIDIsValid(ID) && CheckIDIsUnique(ID) && poolObj.poolObject!=null)
            {
                return true;
            }
            else
                return false;

            
        }

        /// <summary>
        /// 采用一个池对象，先从队列中获取，若队列为空则重新创建
        /// </summary>
        /// <returns></returns>
        protected PoolObject AdoptPoolObject()
        {
            PoolObject poolObj = null;
            if(poolObjAssign.Count>0)
                poolObj = poolObjAssign.Dequeue();///从队列中弹出一个池对象

            if (poolObj != null)//队列中有可用的池对象
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
            else//否则需重新创建
            {
                PoolObject newPoolObj = CreatePoolObject(prototypeObject);

                ActivePoolObject(newPoolObj);
                adoptedPoolObjPerID[newPoolObj.ID] = newPoolObj;
                poolObjAssign.Dequeue();//需要将队列中的此对象清掉

                newPoolObj.RecordInfo_adopt();

                return newPoolObj;
            }


            return null;
        }

        /// <summary>
        /// 回收一个池对象
        /// </summary>
        /// <param name="poolObj"></param>
        protected void RecyclePoolObject(PoolObject poolObj)
        {
            if (poolObj == null)
                return;

            if(CheckPoolObject(poolObj))
            {
                SilentPoolObject(poolObj);
                ///重设父对象
                poolObj.poolObject.transform.SetParent(this.transform);
                ///重置位姿
                poolObj.ResetPosition();
                poolObj.ResetRotation();
                ///回收次数
                poolObj.recyclingTimes++;
                ///信息记录
                poolObj.RecordInfo_recycle();
            }
            else
            {
                BreakPoolObject(poolObj);
            }

            adoptedPoolObjPerID.Remove(poolObj.ID);

            ///每一次回收时清理一下损坏的池对象
            ClearBrokenPoolObjects();
        }

        /// <summary>销毁一个池对象</summary>
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

        /// <summary>清空所有损坏的池对象</summary>
        protected void ClearBrokenPoolObjects()
        {
            if (brokenPoolObjPerID.Count == 0)
                return;

            foreach(int id in brokenPoolObjPerID.Keys)
            {
                DestroyPoolObject(brokenPoolObjPerID[id]);
            }

        }

        /// <summary>清空所有的池对象</summary>
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



        /// <summary>检查ID是否有效，即是否是记录在字典中的值</summary>
        protected bool CheckIDIsValid(int ID)
        {
            if (ID < 0)//ID不可能小于0
                return false;

            List<int> IDs = new List<int>(poolObjectPerID.Keys);//将键转换成列表
            if (!IDs.Contains(ID))//若列表中不包括此ID
                return false;
            else
                return true;


        }

        /// <summary>检查ID是否不存在，不存在返回true</summary>
        protected bool CheckIDIsNotContains(int ID)
        {
            if (ID < 0)//ID不可能小于0
                return false;

            List<int> IDs = new List<int>(poolObjectPerID.Keys);//将键转换成列表
            if (!IDs.Contains(ID))//若列表中不包括此ID
                return true;
            else
                return false;//包括此ID

        }

        /// <summary>检查ID是否是唯一的</summary>
        protected bool CheckIDIsUnique(int ID)
        {
            if (ID < 0)//ID不可能小于0
                return false;

            List<int> IDs = new List<int>(poolObjectPerID.Keys);//将键转换成列表
            if (!IDs.Contains(ID))//若列表中不包括此ID，则直接返回
                return false;

            IDs.Remove(ID);//将此ID移除，再进行唯一性检测
            bool isUnique = true;//先假设ID时唯一的

            foreach(int id in IDs)
            {
                if(id == ID)//检查出ID不唯一
                {
                    isUnique = false;
                    break;
                }
            }

            return isUnique;
        }

        /// <summary>更新adoptedPoolObjPerObj字典的元素,并返回</summary>
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
        /// <summary>获取adoptedPoolObjPerObj字典的元素,并返回</summary>
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


        Coroutine c;///用变量存储协程信息，用以关闭协程
        protected IEnumerator DelayRecyclePoolObject(PoolObject poolObj, float delay)
        {
            yield return new WaitForSeconds(delay);//延迟时间
            RecyclePoolObject(poolObj);
            StopCoroutine(c);///关闭协程
        }




        /// <summary>沉默(隐藏)池对象</summary>
        protected void SilentPoolObject(PoolObject poolObj)
        {
            try
            {
                //poolObj.state = PoolObjectState.Silent;
                poolObj.SetPoolObjectState(PoolObjectState.Silent);
                poolObj.poolObject?.SetActive(false);

                int id = poolObj.ID;
                silentPoolObjPerID[id] = poolObj;
                poolObjAssign.Enqueue(poolObj);///入队

                if (activePoolObjPerID.ContainsKey(id))//回收时的处理
                    activePoolObjPerID.Remove(id);

            }
            catch (Exception e)
            {
                QM.Error(e.ToString());
            }

        }

        /// <summary>激活(启用池对象)</summary>
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

        /// <summary>将池对象标记为破损状态</summary>
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


        /// <summary>获取一个新的池对象</summary>
        protected PoolObject GetPoolObject(string name, int ID, GameObject poolObject, PoolObjectState state)
        {
            return new PoolObject( this, name, ID, poolObject, state);
        }

        protected void ConfigurePoolObject(string name, int ID, GameObject poolObject, PoolObjectState state)
        {

        }







        #endregion

        #region 公共方法

        /// <summary> 请求一个池对象 </summary>
        public PoolObject RequestOnePoolObject()
        {
            return  AdoptPoolObject();
        }


        /// <summary>根据gameobject获取对应的池对象（若有的话）</summary>
        public PoolObject GetPoolObjByGivenObj(GameObject go)
        {
            if (go == null)
                return null;

            CopyFromDictionary();//先更新一下字典信息
            if (adoptedPoolObjPerObj.ContainsKey(go))
                return adoptedPoolObjPerObj[go];
            else
                return null;
        }
        
        
        /// <summary>延迟回收指定的gameobject，由外部调用</summary>
        public void RecycleGivenPoolObject(GameObject go, float delay = 0)
        {
            PoolObject poolObj = GetPoolObjectFromDictionary(go);
            if (poolObj != null)
                c = StartCoroutine(DelayRecyclePoolObject(poolObj, delay));///开启协程

        }

        /// <summary>立即回收指定的gameobject，由外部调用</summary>
        public void RecycleGivenPoolObject(GameObject go)
        {
            PoolObject poolObj = GetPoolObjectFromDictionary(go);
            RecyclePoolObject(poolObj);
        }

        /// <summary>更新指定池对象的父对象</summary>
        public void UpdatePoolObjParent(PoolObject poolObj, GameObject parent)
        {
            poolObj.UpdateParent(parent);
        }


        ///提供获取字典信息的方法

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
            CopyFromDictionary();//更新字典信息
            return adoptedPoolObjPerObj;
        }


        #endregion
    }
}
