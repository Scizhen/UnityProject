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
    /// 调整器：需搭配其他效应器使用，用于调整对象的Transform、Rigidbody、MeshRender等组件。
    /// </summary>
    public class Adjuster : BaseWorkEffector
    {
        public BaseEffector effector;//配合的效应器

        [BoxGroup("Settings")] public float adjustDelay = 0;//调整的延迟时间
        [BoxGroup("Settings")] public bool isAdjustTransform = false;//是否进行变换的调整
        [BoxGroup("Settings")] public bool isAdjustRigidbody = false;//是否进行刚体的调整
        [BoxGroup("Settings")] public bool isAdjustMaterial = false;//是否进行材质的调整


        [BoxGroup("Adjust Transform")][ShowIf("isAdjustTransform")] public bool isLocal = true;//若为true则调整相对于父变换的坐标。为false则调整世界坐标
        [BoxGroup("Adjust Transform")][ShowIf("isAdjustTransform")] public Vector3 newPosition;//位置
        [BoxGroup("Adjust Transform")][ShowIf("isAdjustTransform")] public Vector3 newRotation;//旋转
        [BoxGroup("Adjust Transform")][ShowIf("isAdjustTransform")] public Vector3 newScale;//缩放

        [BoxGroup("Adjust Rigidbody")][ShowIf("isAdjustRigidbody")] public bool useGravity = true;//是否使用重力
        [BoxGroup("Adjust Rigidbody")][ShowIf("isAdjustRigidbody")] public bool isKinematic = false;//是否开启运动学

        [BoxGroup("Adjust Material")][ShowIf("isAdjustMaterial")] public Material newMaterial;//要进行调整的材质


        [BoxGroup("Signal Input")] public ValueMiddleBool SignalStartAdjust; //开始调整信号
        [BoxGroup("Signal Input")][ReadOnly] public  bool _lastSignalStartAdjust = false;
       
     


        /* 调用事件 */
        [Foldout("Events")] public EventEffect EvenAdjustStart;
        [Foldout("Events")] public EventEffect EvenAdjustEnd;

        private Coroutine adjustCoroutine;///用变量存储协程信息，用以关闭协程


        // Start is called before the first frame update
        void Start()
        {
            //设置工作信号
            SetSignalWorkStart(false);
            SetSignalWorkEnd(false);
        }


        // Update is called once per frame
        void FixedUpdate()
        {
            if(StopEffectorWorkBySignal())
                return;

            //搭配的效应器为空，也不会进行任何调整
            if (effector == null)
                return;

            //获取搭配效应器的效果对象
            EffectObjects = effector.EffectObjects;

            if (SignalStartAdjust != null &&  SignalStartAdjust.Value && !_lastSignalStartAdjust)//信号值由false转为true时
            {
                ///QM.Log("SignalStartAdjust");
                //若无需调整，则直接返回
                if (!isAdjustTransform && !isAdjustRigidbody && !isAdjustMaterial && EffectObjects.Count==0)
                    return;

                //设置工作开始信号
                SetSignalWorkStart(true);
                SetSignalWorkEnd(false);


                //进行调整工作
                AdjustObjects(adjustDelay);




            }


            //记录上一个信号值
            if(SignalStartAdjust != null)  _lastSignalStartAdjust = SignalStartAdjust.Value;


        }



        /// <summary>
        /// 延迟调整
        /// </summary>
        /// <param name="delay"></param>
        protected void AdjustObjects(float delay = 0f)
        {
            adjustCoroutine = StartCoroutine(DelayAjustObjects(delay));///开启协程
            EvenAdjustStart.Invoke();//开始调整时的事件调用
        }


        /// <summary>
        /// 协程方法，延迟调整
        /// </summary>
        /// <param name="delay"></param>
        /// <returns></returns>
        protected IEnumerator DelayAjustObjects(float delay = 0f)
        {
            yield return new WaitForSeconds(delay);//延迟时间

            //进行调整工作
            EffectObjects.ForEach(eobj => {

                ///调整对象的位姿变换：位置，旋转和缩放
                if (isAdjustTransform)
                {
                    if (isLocal)
                    {
                        eobj.transform.localPosition += newPosition;
                        //eobj.transform.localRotation = Quaternion.Euler(newRotation);//旋转到固定值newRotation
                        eobj.transform.Rotate(newRotation, Space.Self);//在原来角度上继续旋转newRotation
                        eobj.transform.localScale += newScale;
                    }
                    else
                    {
                        eobj.transform.position += newPosition;
                        //eobj.transform.rotation = Quaternion.Euler(newRotation);
                        eobj.transform.Rotate(newRotation, Space.World);
                        eobj.transform.localScale += newScale;
                    }
                }
                ///调整对象的刚体属性
                if (isAdjustRigidbody)
                {
                    //如何较好地设置刚体变量
                    effector.effectObjPerGo[eobj].rigidbody.useGravity = useGravity;
                    effector.effectObjPerGo[eobj].rigidbody.isKinematic = isKinematic;
                }
                ///调整对象的材质
                if (isAdjustMaterial)
                {
                    QM.AddMaterialToGameobject(eobj, newMaterial);
                }

            });

            //设置工作完成信号
            SetSignalWorkStart(false);
            SetSignalWorkEnd(true);

            EvenAdjustEnd.Invoke();//结束调整时的事件调用

        }



    }



}
