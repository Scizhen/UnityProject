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
    /// 装载器，搭配其他效应器使用（一般是检测器、调整器），通过接收信号对列表中对象设置新的父对象，适用于需要将对象进行搬运、固定等场合，
    /// 可设置装载对象的数量。
    /// </summary>
    public class Loader : BaseWorkEffector
    {
        public Detector detector;//基于的检测器

        [BoxGroup("Setting")]public int loadNumber = 1;//装载数量
        [BoxGroup("Setting")] public GameObject parentLoad;//装载父对象，若为空则默认将此脚本挂载的对象作为父对象
        [BoxGroup("Setting")] public GameObject parentAfterLoad;//装载完成后的新父对象，可无

        [BoxGroup("Signal Input")] public ValueMiddleBool SignalStartLoad; //开始装载信号
        [BoxGroup("Signal Input")] public ValueMiddleBool SignalEndLoad; //结束装载信号
        [BoxGroup("Signal Input")] [ReadOnly] public bool _lastSignalStartLoad = false;
        [BoxGroup("Signal Input")] [ReadOnly] public bool _lastSignalEndLoad = false;

        /* 调用事件 */
        [Foldout("Events")] public EventEffect EvenLoadStart;
        [Foldout("Events")] public EventEffect EvenLoadEnd;



        // Start is called before the first frame update
        void Start()
        {
            SignalStartLoad.Value = false;
            SignalEndLoad.Value = false;
            _lastSignalStartLoad = false;
            _lastSignalEndLoad = false;

            //设置工作信号
            SetSignalWorkStart(false);
            SetSignalWorkEnd(false);
        }

        // Update is called once per frame
        void Update()
        {

            if (StopEffectorWorkBySignal())
                return;

            //获取要装载的对象
            GetLoadedObject();

            ///接受装载信号，进行装载
            if (SignalStartLoad != null && SignalStartLoad.Value && !_lastSignalStartLoad)//信号值由false转为true时
            {
                //设置工作开始信号
                SetSignalWorkStart(true);
                SetSignalWorkEnd(false);
                LoadObject();
                EvenLoadStart.Invoke();
            }


            ///接受卸载信号，结束装载
            if (SignalEndLoad != null && SignalEndLoad.Value && !_lastSignalEndLoad)//信号值由false转为true时
            {
                UnloadObject();

                //设置工作完成信号
                SetSignalWorkStart(false);
                SetSignalWorkEnd(true);

                EvenLoadEnd.Invoke();
            }

            if(SignalStartLoad != null) _lastSignalStartLoad = SignalStartLoad.Value;//记录成为上一个值
            if (SignalEndLoad != null) _lastSignalEndLoad = SignalEndLoad.Value;//记录成为上一个值

        }


        /// <summary>
        /// 获取要被装载的对象
        /// </summary>
        void GetLoadedObject()
        {
            if (detector == null || detector.EffectObjects == null || detector.EffectObjects.Count == 0)
                return;

            if (detector.EffectObjects.Count < loadNumber)
                return;

            EffectObjects = detector.EffectObjects.GetRange(0, loadNumber);//获取指定数量的装载对象
        }


        /// <summary>将LoadObjects列表中的对象进行装载 </summary>
        void LoadObject()
        {
            //设置此脚本挂载的对象为父对象（即将池对象装载至此对象上）
            EffectObjects.ForEach(lobj => {
                lobj.transform.SetParent((parentLoad != null)? parentLoad.transform : this.gameObject.transform);
            });
        }
        /// <summary>将LoadObjects列表中的对象进行卸载</summary>
        void UnloadObject()
        {
            EffectObjects.ForEach(lobj => {
                lobj.transform.SetParent((parentAfterLoad != null) ? parentAfterLoad.transform : null);
            });

            EffectObjects.Clear();//清空装载列表
        }




    }




}
