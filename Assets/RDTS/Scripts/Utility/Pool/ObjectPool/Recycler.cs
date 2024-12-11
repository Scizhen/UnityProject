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
    /// 回收器，自带碰撞盒检测。 
    /// 两种回收方式：1.碰撞盒触发时(延时)回收，将isRecycle = true 并设置延时时间recycleDelay，仅回收被检测到的单个对象；
    ///               2.接受信号的立即回收，读取SignalStartRecycle信号的Value值，当值从false转为true时进行回收，回收全部在列表中的对象
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class Recycler : BaseWorkEffector
    {
        public ObjectPoolUtility objectPoolUtility;//关联的对象池

        [BoxGroup("Settings")] public bool isRecycle = false;//是否开启回收
        [BoxGroup("Settings")] public float recycleDelay = 0;//回收的延迟时间

        [BoxGroup("Signal Input")] public ValueMiddleBool SignalStartRecycle;//接受开始回收信号
        [BoxGroup("Signal Input")] [ReadOnly] public bool _lastSignalStartRecycle = false;

        /* 调用事件 */
        [Foldout("Events")] public EventEffect EvenRecycleStart;//回收开始时调用的Even方法
        [Foldout("Events")] public EventEffect EvenRecycleEnd;//回收结束时调用的Even方法


        private BoxCollider _boxCollider;//此脚本对于的碰撞盒
        private Coroutine recycleCoroutine;///用变量存储协程信息，用以关闭协程





        // Start is called before the first frame update
        void Start()
        {
            //添加默认的限制Tag和Layer
            AddDefaultTagAndLayer();
            //设置碰撞盒为触发形式
            _boxCollider = GetComponent<BoxCollider>();
            _boxCollider.isTrigger = true;
            //设置工作信号
            SetSignalWorkStart(false);
            SetSignalWorkEnd(false);
        }


        // Update is called once per frame
        void Update()
        {
            if (StopEffectorWorkBySignal())
                return;

            //接收信号回收方式，立即回收无延迟
            if (SignalStartRecycle != null && SignalStartRecycle.Value && !_lastSignalStartRecycle)//信号值由false转为true时
            {
                //设置工作开始信号
                SetSignalWorkStart(true);
                SetSignalWorkEnd(false);

                RecyclePoolObjectOfList(objectPoolUtility, EffectObjects);
                EvenRecycleStart.Invoke();
            }

            if(SignalStartRecycle != null) _lastSignalStartRecycle = SignalStartRecycle.Value;


        }


        void Reset()
        {
            AddDefaultTagAndLayer();//添加默认的限制Tag和Layer
        }


        private void OnTriggerEnter(Collider other)
        {
            if (StopEffectorWorkBySignal())
                return;

            GameObject go = other.gameObject;
            //判断碰撞触发的对象是否符合Tag和Layer的限制
            if (!SusGameObjectLimitTagAndlayer(go))
                return;


            if (!EffectObjects.Contains(go))
            {
                EffectObjects.Add(go);//加入到列表
                effectObjPerGo[go] = new EffectObject() { rigidbody = other.GetComponent<Rigidbody>() ?? null };//加入字典
            }
              

            //回收检测到的对象
            if(isRecycle)
            {
                //设置工作开始信号
                SetSignalWorkStart(true);
                SetSignalWorkEnd(false);

                RecycleOnePoolObject(objectPoolUtility, go, recycleDelay);//延迟回收对象

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
                EffectObjects.Remove(go);//从列表移除
                if (effectObjPerGo.ContainsKey(go))//从字典移除
                    effectObjPerGo.Remove(go);

                EvenRecycleEnd.Invoke();
            }
              



        }



        /// <summary>
        /// 回收一个池对象
        /// </summary>
        /// <param name="go">所要回收的对象</param>
        /// <param name="delay">延迟时间</param>
        public void RecycleOnePoolObject(ObjectPoolUtility objectPoolUtility, GameObject go, float delay = 0f)
        {
            if (go == null)
                return;

            recycleCoroutine = StartCoroutine(DelayRecycleObject(objectPoolUtility, go, delay));///开启协程
        }


        /// <summary>
        /// 协程方法：延迟回收一个池对象
        /// </summary>
        /// <param name="go"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        protected IEnumerator DelayRecycleObject(ObjectPoolUtility objectPoolUtility, GameObject go, float delay = 0)
        {
            yield return new WaitForSeconds(delay);//延迟时间

            if(objectPoolUtility != null)
            {
                objectPoolUtility.RecycleGivenPoolObject(go);

                //移除检测器中相应的对象
                if (EffectObjects.Contains(go))
                {
                    EffectObjects.Remove(go);//从列表移除
                    if (effectObjPerGo.ContainsKey(go))//从字典移除
                        effectObjPerGo.Remove(go);
                }
                  
            }

            //设置工作完成信号
            SetSignalWorkStart(false);
            SetSignalWorkEnd(true);

            EvenRecycleEnd.Invoke();
            StopCoroutine(recycleCoroutine);///关闭协程
        }





        /// <summary>
        /// 回收指定列表中的池对象
        /// </summary>
        /// <param name="objList">回收对象的列表</param>
        /// <param name="delay">延迟时间</param>
        public void RecyclePoolObjectOfList(ObjectPoolUtility objectPoolUtility, List<GameObject> objList, float delay = 0f)
        {
            if (objList == null || objList.Count == 0)
            {
                if(delay == 0)
                {
                    //设置工作信号
                    SetSignalWorkStart(false);
                    SetSignalWorkEnd(false);
                }
                return;
            }
               

            recycleCoroutine = StartCoroutine(DelayRecycleObjectOfList(objectPoolUtility, objList, delay));//开启协程
        }





        /// <summary>
        /// 协程方法：延迟回收一个列表的对象
        /// </summary>
        /// <param name="objList"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        protected IEnumerator DelayRecycleObjectOfList(ObjectPoolUtility objectPoolUtility, List<GameObject> objList, float delay = 0f)
        {
            yield return new WaitForSeconds(delay);//延迟时间
            ///QM.Log("DelayRecycleObjectOfList — " + objList.Count);

            for (int i = objList.Count-1; i >= 0; i--)
            {
                var robj = objList[i];
                if (objectPoolUtility != null)
                {
                    objectPoolUtility.RecycleGivenPoolObject(robj);//在给定的对象池中检索正在采用的池对象，再进行回收
                    EffectObjects.Remove(robj);
                    if (effectObjPerGo.ContainsKey(robj))//从字典移除
                        effectObjPerGo.Remove(robj);
                }
               /// QM.Log("DelayRecycleObjectOfList — " + i);
            }


            objList.Clear();
            //设置工作完成信号
            SetSignalWorkStart(false);
            SetSignalWorkEnd(true);

            EvenRecycleEnd.Invoke();
            StopCoroutine(recycleCoroutine);///关闭协程
        }



    }


}
