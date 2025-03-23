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
    /// 加工器，自带碰撞盒检测。检测到对象时进入加工开始状态，当对象离开碰撞盒检测范围则进入加工完成状态，应挂载于加工器件上如钻头等。
    /// 向外提供加工完成的对象数量，但需要接收外部信号来清空加工完成对象的列表。
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class Processor : BaseWorkEffector
    {
        [NaughtyAttributes.ReadOnly] public List<GameObject> ProcessObjects;//加工完成对象列表


        [BoxGroup("Signal Input")]
        public ValueMiddleBool SignalClear; //列表清空信号，当信号值由false转为true时来清空加工完成对象的列表

        [BoxGroup("Signal Output")] public ValueMiddleInt SignalProcessNumber; //加工对象数量信号

        /* 调用事件 */
        [Foldout("Events")] public EventEffect EvenProcessStart;
        [Foldout("Events")] public EventEffect EvenProcessEnd;



        private BoxCollider _boxCollider;//此脚本对于的碰撞盒
        private bool _lastSignalClear = false;

        // Start is called before the first frame update
        void Start()
        {
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

            //接收外部信号，清空加工完成对象列表
            if (SignalClear != null && SignalClear.Value && !_lastSignalClear)//信号值由false转为true时
            {
                ProcessObjects.Clear();
            }

            if(SignalClear != null) _lastSignalClear = SignalClear.Value;
            if (SignalProcessNumber != null) SignalProcessNumber.Value = ProcessObjects.Count;//输出加工完成对象数量
        }


        void Reset()
        {
            AddDefaultTagAndLayer();//添加默认的限制Tag和Layer
        }




        protected void OnTriggerEnter(Collider other)
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


            //设置工作开始信号
            SetSignalWorkStart(true);
            SetSignalWorkEnd(false);


            EvenProcessStart.Invoke();

        }


        protected void OnTriggerExit(Collider other)
        {
            if (StopEffectorWorkBySignal())
                return;

            GameObject go = other.gameObject;

            if (EffectObjects.Contains(go))
            {
                EffectObjects.Remove(go);//从列表移除
                if(!ProcessObjects.Contains(go))
                    ProcessObjects.Add(go);//加入加工完成对象列表
            }

            //设置工作完成信号
            SetSignalWorkStart(false);
            SetSignalWorkEnd(true);

            EvenProcessEnd.Invoke();

        }







    }


}
