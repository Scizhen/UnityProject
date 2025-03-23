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
    /// 转换器：自带碰撞盒检测，将一个检测到的对象转换成新的对象，适用于加工、组装、拆装等对象形态变化的场合
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class Converter : BaseWorkEffector
    {
        public ObjectPoolUtility objectPoolBeforeConverter;//关联的要回收对象的对象池
        public ObjectPoolUtility objectPoolAfterConverter;//关联的要创建对象的对象池 

        [BoxGroup("Settings")] public bool isConvert = false;//是否开启回收
        [BoxGroup("Settings")] public float convertDelay = 0f;//转换的延迟时间

        [BoxGroup("Signal Input")] public ValueMiddleBool SignalStartConvert;//接受开始转换信号
        [BoxGroup("Signal Input")] [ReadOnly] public bool _lastSignalStartConvert = false;

        /* 调用事件 */
        [Foldout("Events")] public EventEffect EvenConvertStart;
        [Foldout("Events")] public EventEffect EvenConvertEnd;

        private Coroutine convertCoroutine;///用变量存储协程信息，用以关闭协程

        // Start is called before the first frame update
        void Start()
        {
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
            if (SignalStartConvert != null && SignalStartConvert.Value && !_lastSignalStartConvert)//信号值由false转为true时
            {
                //设置工作开始信号
                SetSignalWorkStart(true);
                SetSignalWorkEnd(false);

                //将列表中的第一个对象转换成新对象
                if(EffectObjects.Count > 0)
                    ConvertToAnotherObject(EffectObjects[0], convertDelay);

                EvenConvertStart.Invoke();
            }

            if (SignalStartConvert != null) _lastSignalStartConvert = SignalStartConvert.Value;

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
            }

            if (isConvert)//开始转换的标志位是否打开
            {
                //将检测到的对象转换成新对象
                ConvertToAnotherObject(go, convertDelay);
            }
             
           

        }


        private void OnTriggerExit(Collider other)
        {
            if (StopEffectorWorkBySignal())
                return;


        }



        void ConvertToAnotherObject(GameObject go, float delay = 0f)
        {
            convertCoroutine = StartCoroutine(DelayRecycleObject(objectPoolBeforeConverter, go, delay));///开启协程

            //设置工作开始信号
            SetSignalWorkStart(true);
            SetSignalWorkEnd(false);

            EvenConvertStart.Invoke();
        }


        protected IEnumerator DelayRecycleObject(ObjectPoolUtility objectPoolUtility, GameObject go, float delay = 0)
        {
            yield return new WaitForSeconds(delay);//延迟时间

            if (objectPoolUtility != null)
            {
                objectPoolUtility.RecycleGivenPoolObject(go);

                //移除检测器中相应的对象
                if (EffectObjects.Contains(go))
                {
                    EffectObjects.Remove(go);//从列表移除
                }

            }

            //请求新对象
            RequestNewObject(objectPoolAfterConverter);
            //设置工作完成信号
            SetSignalWorkStart(false);
            SetSignalWorkEnd(true);

            EvenConvertEnd.Invoke();
            StopCoroutine(convertCoroutine);///关闭协程
        }



        /// <summary>
        /// 请求一个新的对象
        /// </summary>
        /// <param name="objectPoolUtility"></param>
        protected void RequestNewObject(ObjectPoolUtility objectPoolUtility)
        {
            objectPoolUtility.RequestOnePoolObject();
        }


    }



}