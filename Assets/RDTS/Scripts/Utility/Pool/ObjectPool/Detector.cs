///********************************************************************************************
///this is part of Robot Digital Twin System.                                                 *
///@Copyright by Shaw.S , 2023                                                                *
///Do not distribute without authorization,thanks!                                            *
///********************************************************************************************
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RDTS.Utility
{

 
    [SelectionBase]
    [ExecuteInEditMode]
    /// <summary>
    /// 检测器：专门用来检测对象。
    /// 两种检测方式：1.碰撞触发检测（区域范围），需要添加BoxCollider组件，并设置isTrigger为true；
    ///               2.射线检测（线段范围），将useRaycast设为true，调整射线的方向和长度
    /// </summary>
    public class Detector : BaseDetectEffector
    {
        [BoxGroup("Signal Output")]
        public ValueMiddleBool SignalDetect; //检测信号
        [BoxGroup("Signal Output")]
        public ValueMiddleInt SignalCount; //输出已检测对象信号

        //[BoxGroup("Settings")] public bool pauseEditor = false; //!< 暂停，检查该Inspector上的某些值（还未增加此功能）
        [BoxGroup("Settings")] public bool useRaycast = true;//<! 是否使用射线检测
        [BoxGroup("Settings")] [ShowIf("useRaycast")] public Vector3 RayCastDirection = new Vector3(1, 0, 0);//<! 光线方向
        [BoxGroup("Settings")] [ShowIf("useRaycast")] public float RayCastLength = 1000f;//<! 射线长度
        //[BoxGroup("Settings")] [ShowIf("useRaycast")] public Material MaterialDetected; //!< 检测到对象时显示的材质
        //[BoxGroup("Settings")] [ShowIf("useRaycast")] public Material MaterialNotDetected; //!< 未检测到对象时显示的材质
        [BoxGroup("Settings")] [ShowIf("useRaycast")] public float RayCastDistance;//<! 射线原点到检测点的距离
        [BoxGroup("Settings")] public bool isGetCompetent = true;//<! 是否获取检测对象上的相关组件



        /* 调用事件 */
        [Foldout("Events")] public EventEffect EvenDetectStart;
        [Foldout("Events")] public EventEffect EvenDetectEnd;


        private bool _lastSignalClear = false;

        protected BoxCollider _boxCollider;

        protected RaycastHit hit;//当前射线命中的对象。RaycastHit：从射线投射获取信息
        protected RaycastHit lasthit;//上一次射线命中的对象
        protected bool raycasthasthit;//为true则射线接触（命中）到某对象，为false则射线未接触到对象
        protected bool lastraycasthasthit;//上一次射线是否接触到对象
        protected bool raycasthitchanged;//射线命中的对象是否改变
        protected Vector3 startposraycast;//射线的起始点
        protected Vector3 endposraycast;//射线的末端点

        protected int layermask;//遮罩层



        // Start is called before the first frame update
        private void Start()
        {
            if (SignalClear != null)
                _lastSignalClear = SignalClear.Value;

        }

        // Update is called once per frame
        private void Update()
        {

            if(SignalClear != null && SignalClear.Value && !_lastSignalClear)
            {
                ClearEffectObjects();
            }

            ///在编辑模式下，且开启了射线检测模式
            if (!Application.isPlaying && useRaycast)
            {
                DrawRaycast();//Debug类绘制光线，无宽度

                if (StopEffectorDetectBySignal())
                    return;

                if ((lastraycasthasthit && !raycasthasthit) || raycasthitchanged)
                {
                    if (lasthit.collider != null)
                        OnTriggerExit(lasthit.collider);//对象要离开射线检测范围时
                }

                if ((raycasthasthit && !lastraycasthasthit) || raycasthitchanged)
                {
                    // new raycast hit
                    OnTriggerEnter(hit.collider);//对象要进入射线检测范围时
                }

                lastraycasthasthit = raycasthasthit;
                lasthit = hit;
            }



            //有检测到的对象即输出true，反之输出false
            if (SignalDetect != null)
                SignalDetect.Value = (EffectObjects.Count != 0) ? true : false;
            //输出检测到的对象数量
            if (SignalCount != null)
                SignalCount.Value = EffectObjects.Count;
            //记录清空信号的值
            if (SignalClear != null)
                _lastSignalClear = SignalClear.Value;
        }

        private void FixedUpdate()
        {
          

            ///在运行模式下，且开启了射线检测模式
            if (Application.isPlaying && useRaycast)
            {
                DrawRaycast();

                if (StopEffectorDetectBySignal())
                    return;

                if ((lastraycasthasthit && !raycasthasthit) || raycasthitchanged)
                {
                    if (lasthit.collider != null)
                        OnTriggerExit(lasthit.collider);//对象要离开射线检测范围时
                }

                if ((raycasthasthit && !lastraycasthasthit) || raycasthitchanged)
                {
                    // new raycast hit
                    OnTriggerEnter(hit.collider);//对象要进入射线检测范围时
                }

                lastraycasthasthit = raycasthasthit;
                lasthit = hit;



            }
        }

        protected void OnEnable()
        {
            //设置碰撞盒为触发形式
            _boxCollider = GetComponent<BoxCollider>();
            if (_boxCollider != null)
                _boxCollider.isTrigger = true;
            else//若包围盒不存在则使用射线模式
                useRaycast = true;

            AddDefaultTagAndLayer();

            ////设置检测器材质
            //if (MaterialDetected == null)
            //{
            //    MaterialDetected = UnityEngine.Resources.Load(RDTSPath.path_SensorOccupied, typeof(Material)) as Material;//加载指定路径的材质
            //}

            //if (MaterialNotDetected == null)
            //{
            //    MaterialNotDetected = UnityEngine.Resources.Load(RDTSPath.path_SensorNotOccupied, typeof(Material)) as Material;//加载指定路径的材质
            //}
        }

        private void Reset()
        {
            //设置碰撞盒为触发形式
            _boxCollider = GetComponent<BoxCollider>();
            if (_boxCollider != null)
                _boxCollider.isTrigger = true;
            else//若包围盒不存在则使用射线模式
                useRaycast = true;

            AddDefaultTagAndLayer();
        }


        protected void OnTriggerEnter(Collider other)
        {
            if (StopEffectorDetectBySignal())
                return;

            GameObject go = other.gameObject;
            //判断碰撞触发的对象是否符合Tag和Layer的限制
            if (!SusGameObjectLimitTagAndlayer(go))
                return;

            if (!EffectObjects.Contains(go))
            {
                EffectObjects.Add(go);//加入到列表
                if(isGetCompetent)
                    effectObjPerGo[go] = new EffectObject() { rigidbody = other.GetComponent<Rigidbody>() ?? null };//加入字典
            }


            EvenDetectStart.Invoke();

        }




        protected void OnTriggerExit(Collider other)
        {
            if (StopEffectorDetectBySignal())
                return;


            GameObject go = other.gameObject;

            if (EffectObjects.Contains(go))
            {
                EffectObjects.Remove(go);//从列表移除
                if (effectObjPerGo.ContainsKey(go))//从字典移除
                    effectObjPerGo.Remove(go);          
            }



            EvenDetectEnd.Invoke();

        }


        /// <summary>从检测对象列表中移除指定的对象 </summary>
        public void RemoveGivenObject(GameObject go)
        {
            if (EffectObjects.Contains(go))
                EffectObjects.Remove(go);
        }


        /// <summary> 清空检测对象列表 </summary>
        public void ClearDetectObjects()
        {
            EffectObjects.Clear();
        }



        #region  射线检测模式


        protected void DrawRaycast()
        {

            float scale = 1000;
            raycasthitchanged = false;
            var globaldir = transform.TransformDirection(RayCastDirection);
            var display = Vector3.Normalize(globaldir) * RayCastLength / scale;//整条光线的长度
            startposraycast = transform.position;
            layermask = QM.GetLayerMask(LimitLayer);
            if (Physics.Raycast(startposraycast, globaldir, out hit, RayCastLength / scale, layermask))//向场景中的所有碰撞体投射一条射线。 如果射线与任何碰撞体相交，返回 true，否则返回 false
            {
                //hit.distance：从射线原点到撞击点的距离
                var dir = Vector3.Normalize(globaldir) * hit.distance;//射线原点到撞击点的距离

                RayCastDistance = hit.distance * scale;
                Debug.DrawRay(startposraycast, dir, Color.red, 0, true);//若DisplayStatus为true，绘制红色射线
                raycasthasthit = true;//设置标志位
                if (hit.collider != lasthit.collider)//若射线命中的碰撞体与上一次命中的不同
                    raycasthitchanged = true;
                endposraycast = startposraycast + dir;//射线末端点坐标
            }
            else
            {
                Debug.DrawRay(startposraycast, display, Color.yellow, 0, true);//若DisplayStatus为true，绘制黄色射线
                raycasthasthit = false;//重置标志位
                endposraycast = startposraycast + display;
                RayCastDistance = 0;
            }


        }





        #endregion




    }



}