using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;


namespace RDTS.Utility
{
    public class Catcher : BaseWorkEffector
    {
        public enum CatchMode
        {
            Grip,
            Suck
        }


        public Detector detector;//配合的检测器

        [BoxGroup("Settings")] public CatchMode catchMode = CatchMode.Suck;//捕捉模式
        [BoxGroup("Settings")] [ReadOnly] public string CatchStatus = "Idle";//捕捉状态
        [BoxGroup("Settings")] public GameObject catchParent;//抓取时的父对象
        [BoxGroup("Settings")] public GameObject newParent;//放置后的父对象
        [BoxGroup("Settings")] [ReadOnly] public int currentEffectObjectsCount = 0;
        [BoxGroup("Settings")][ReadOnly] public int _lastEffectObjectsCount = 0;
        [ShowIf("catchMode", CatchMode.Grip)] [BoxGroup("Settings")] public ValueOutputBool GripperClose;
        [ShowIf("catchMode", CatchMode.Grip)] [BoxGroup("Settings")] public ValueOutputBool GripperOpen;
        [ShowIf("catchMode", CatchMode.Grip)] [BoxGroup("Settings")] public ValueInputBool GripperIsClose;
        [ShowIf("catchMode", CatchMode.Grip)] [BoxGroup("Settings")] public ValueInputBool GripperIsOpen;

        [BoxGroup("Signal Input")] public ValueOutputBool SignalPick; //抓取信号
        [BoxGroup("Signal Input")] public ValueOutputBool SignalPlace; //放置信号
        private bool _lastSignalPick = false;
        private bool _lastSignalPlacee = false;

        [BoxGroup("Signal Output")]
        public ValueMiddleInt PickObjCount; //输出已抓取对象信号


        /* 调用事件 */
        [Foldout("Events")] public EventEffect EvenCatchStart;
        [Foldout("Events")] public EventEffect EvenCatchEnd;


        private bool _lastGripperIsClose = false;
        private bool _lastGripperIsOpen = false;
       

        // Start is called before the first frame update
        void Start()
        {
            CatchStatus = "Idle";//初始状态为打开
            if(catchMode == CatchMode.Grip)
                MakeGripperOpen();
            if(SignalPick != null)
                _lastSignalPick = SignalPick.Value;
            if (SignalPlace != null)
                _lastSignalPlacee = SignalPlace.Value;

            if (GripperIsClose != null)
                _lastGripperIsClose = GripperIsClose.Value;
            if (GripperIsOpen != null)
                _lastGripperIsOpen = GripperIsOpen.Value;


        }

        // Update is called once per frame
        void Update()
        {

            

        }


        private void FixedUpdate()
        {
            //接收Pick信号
            if (SignalPick != null && SignalPick.Value && !_lastSignalPick && CatchStatus == "Idle")
            {
                CatchStatus = "Pick";
                //设置工作开始信号
                SetSignalWorkStart(true);
                SetSignalWorkEnd(false);
            }
            //接收Place信号
            if (SignalPlace != null && SignalPlace.Value && !_lastSignalPlacee && CatchStatus == "Picked")
            {
                CatchStatus = "Place";
            }

            //捕捉模式
            if (catchMode == CatchMode.Grip)
                GripperControl();
            else
                SuckerControl();

            //记录控制信号值
            if (SignalPick != null)
                _lastSignalPick = SignalPick.Value;
            if (SignalPlace != null)
                _lastSignalPlacee = SignalPlace.Value;
            if (GripperIsClose != null)
                _lastGripperIsClose = GripperIsClose.Value;
            if (GripperIsOpen != null)
                _lastGripperIsOpen = GripperIsOpen.Value;


            if (PickObjCount != null)
                PickObjCount.Value = EffectObjects.Count;
            currentEffectObjectsCount = EffectObjects.Count;
        }



        /// <summary>
        /// 吸盘工具的吸取控制
        /// </summary>
        void SuckerControl()
        {

            switch (CatchStatus)
            {
                case "Idle":
                    ResetCatcherSignal();
                    break;
                case "Picked":
                    _lastEffectObjectsCount = EffectObjects.Count;
                    break;
                case "Pick":
                    if (EffectObjects.Count > _lastEffectObjectsCount)
                    {
                        CatchStatus = "Picked";
                    }
                    else
                    {
                        CatcherPick();
                    }

                    break;
                case "Place":
                    if (EffectObjects.Count < _lastEffectObjectsCount)
                    {
                        CatchStatus = "Idle";
                        _lastEffectObjectsCount = EffectObjects.Count;

                        //设置工作结束信号
                        SetSignalWorkStart(false);
                        SetSignalWorkEnd(true);
                    }
                    else
                    {
                        CatcherPlace();
                    }

                    break;


            }


        }


        /// <summary>
        /// 夹爪工具的夹取控制
        /// </summary>
        void GripperControl()
        {
          
            
            switch (CatchStatus)
            {
                case "Idle":
                    ResetCatcherSignal();
                    break;
                case "Picked":
                    _lastEffectObjectsCount = EffectObjects.Count;
                    break;
                case "Pick":
                    if(EffectObjects.Count > _lastEffectObjectsCount)//当前对象数量 > 之前对象数量时，说明抓取成功
                    {
                        CatchStatus = "Picked";
                    }
                    else
                    {
                        MakeGripperClose();
                        //若夹爪闭合，进行抓取
                        if (GripperIsClose.Value && !_lastGripperIsClose)
                        {
                            CatcherPick();

                        }
                    }

                    
                    break;
                case "Place":
                    if (EffectObjects.Count < _lastEffectObjectsCount)//当前对象数量 < 之前对象数量时，说明放置成功
                    {
                        CatchStatus = "Idle";
                        _lastEffectObjectsCount = EffectObjects.Count;

                        //设置工作结束信号
                        SetSignalWorkStart(false);
                        SetSignalWorkEnd(true);
                    }
                    else
                    {
                        MakeGripperOpen();
                        //若夹爪张开，进行放置
                        if (GripperIsOpen.Value && !_lastGripperIsOpen)
                        {
                            CatcherPlace();
                        }
                    }

                    break;


            }



        }


        /// <summary>
        /// 夹爪抓取对象
        /// </summary>
        void CatcherPick()
        {
            var effectObjs = detector.EffectObjects;
            if (effectObjs.Count == 0 || effectObjs == null)
                return;

            effectObjs.ForEach(obj => {
                obj.transform.SetParent((catchParent == null)? this.transform: catchParent.transform);//抓取对象的父对象为空，则默认将挂载此脚本的对象作为父对象
                detector.effectObjPerGo[obj].rigidbody.isKinematic = true;
                if(!EffectObjects.Contains(obj))
                    EffectObjects.Add(obj);
            });



        }
        /// <summary>
        /// 夹爪放置对象
        /// </summary>
        void CatcherPlace()
        {
            var length = EffectObjects.Count;
            if (length == 0)
                return;

            for (int i=0; i< length; i++)
            {
                var obj = EffectObjects[i];
                EffectObjects[i].transform.SetParent((newParent == null)? null:newParent.transform);//放置时若未设置了新的父对象，则将对象父对象设为空
                if(detector.effectObjPerGo.ContainsKey(obj))
                detector.effectObjPerGo[obj].rigidbody.isKinematic = false;
                if (EffectObjects.Contains(obj))
                    EffectObjects.Remove(obj);
            }
       


        }



        /// <summary>
        /// 张开夹爪
        /// </summary>
        void MakeGripperOpen()
        {
            GripperClose.Value = false;
            GripperOpen.Value = true;
        }
        /// <summary>
        /// 闭合夹爪
        /// </summary>
        void MakeGripperClose()
        {
            GripperClose.Value = true;
            GripperOpen.Value = false;
        }

        /// <summary>
        /// 重置抓取/放置信号
        /// </summary>
        void ResetCatcherSignal()
        {
            SignalPick.Value = false;
            SignalPlace.Value = false;
        }


    }



}
