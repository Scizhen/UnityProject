//*************************************************************************
//Thanks for the code reference game4automation provides.                 *
//                                                                        *
//*************************************************************************
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;
namespace RDTS
{
    [System.Serializable]
    public class EventMUSensor : UnityEvent<MU, bool>
    {
    }

    [System.Serializable]
    public class EventGameobjectSensor : UnityEvent<GameObject, bool>
    {
    }

    /// <summary>
    /// 检测MU和设定Layer的对象、绘制辅助光线、显示相关状态信息
    /// </summary>
    [SelectionBase]
    //! The sensor is used for detecting MUs.  用于检测MU
    //! Sensors are using Box Colliders for detecting MUs. The Sensor should be on Layer *Sensor*  使用碰撞盒BoxCollider来检测MU，且此层级Layer应设为Sensor
    //! Layer settings are used. A behavior component (e.g. *Sensor_Standard*) must be added to the Sensor for providing connection to Values Input and 
    //! outputs.
    [ExecuteInEditMode]
    public class Sensor : BaseSensor, IValueInterface
    {
        // Public - UI Variables 
        [BoxGroup("Settings")] public bool DisplayStatus = true;//是否显示Gizmo状态
        [BoxGroup("Settings")]
        public string
            LimitSensorToTag; //!< Limits the function of the sensor to a certain MU tag - also MU names are working
                              //将传感器的功能限制为某个 MU 标签 - MU 名称也有效
        [BoxGroup("Settings")] public bool UseRaycast = false;//是否使用射线检测

        [BoxGroup("Settings")] [ShowIf("UseRaycast")] public Vector3 RayCastDirection = new Vector3(1, 0, 0);//射线方向
        [BoxGroup("Settings")] [ShowIf("UseRaycast")] public float RayCastLength = 1000f;//射线长度
        [BoxGroup("Settings")] [ShowIf("UseRaycast")] public float RayCastDisplayWidth = 0.01f;//线渲染器的射线宽度
        [BoxGroup("Settings")] [ShowIf("UseRaycast")] [ReorderableList] public List<string> AdditionalRayCastLayers;
        [BoxGroup("Settings")] [ShowIf("UseRaycast")] public bool ShowSensorLinerenderer = false;//线渲染器：用于在 3D 空间中绘制自由浮动的线


        //!<  Display the status of the sensor by changing the material (color).
        [BoxGroup("Settings")] public Material MaterialOccupied; //!<  Material for displaying the occupied status. 检测到对象时显示的材质
        [BoxGroup("Settings")] public Material MaterialNotOccupied; //!<  Material for displaying the not occupied status. 未检测到对象时显示的材质
        [BoxGroup("Settings")] public bool PauseOnSensor = false; //!<  Pause simulation if sensor is getting high - can be used for debuging  暂停，检查该Inspector上的某些值
        [BoxGroup("Interface Connection")] public ValueInputBool SensorOccupied; //! Boolean Value input for the Sensor signal. 输出信号


        private bool _isOccupiedNotNull;

        [Foldout("Events")]
        public EventMUSensor
            EventMUSensor; //!<  Unity event which is called for MU enter and exit. On enter it passes MU and true. On exit it passes MU and false.
        [Foldout("Events")]
        public EventGameobjectSensor
        EventNonMUGameObjectSensor; //!<  Unity event which is called for non MU objects enter and exit. On enter it passes gameobject (on which the collider was detected) and true. On exit it passes gameobject and false.

        [Foldout("Status")] public bool Occupied = false; //!<  True if sensor is occupied. 若检测到对象为true
        [Foldout("Status")] public GameObject LastTriggeredBy; //!< Last MU which has triggered the sensor. 上一个被检测到的对象
        [Foldout("Status")] [ShowIf("UseRaycast")] public float RayCastDistance; //!< Last RayCast Distance if Raycast is used   射线原点到检测目标的距离
        [Foldout("Status")] public int LastTriggeredID; //!< Last MUID which has triggered the sensor. 最近被检测到的MU的ID
        [Foldout("Status")] public int LastTriggeredGlobalID; //!<  Last GloabalID which has triggerd the sensor. 最近被检测到的MU的GlobalID
        [Foldout("Status")] public int Counter;//从仿真开始检测到的数量和
        [Foldout("Status")] public List<MU> CollidingMus; // Currently colliding MUs with the sensor.  被Sensor检测到的MU
        [Foldout("Status")]
        public List<GameObject>//被Sensor检测到的对象（MU下可能包含其他对象）
            CollidingObjects; // Currently colliding GameObjects with the sensor (which can be more than MU because a MU can contain several GameObjects.


        public delegate void
            OnEnterDelegate(GameObject obj); //!< Delegate function for GameObjects entering the Sensor.

        public event OnEnterDelegate EventEnter;

        public delegate void OnExitDelegate(GameObject obj); //!< Delegate function for GameObjects leaving the Sensor.

        public event OnExitDelegate EventExit;


        // Private Variables
        private bool _occupied = false;
        private MeshRenderer _meshrenderer;
        private BoxCollider _boxcollider;
        private int layermask;//层遮罩
        [SerializeField] [HideInInspector] private float scale = 1000;
        private RaycastHit hit;//当前射线命中的对象。RaycastHit：从射线投射获取信息
        private RaycastHit lasthit;//上一次射线命中的对象
        private bool raycasthasthit;//为true则射线接触（命中）到某对象，为false则射线未接触到对象
        private bool lastraycasthasthit;//上一次射线是否接触到对象
        private bool raycasthitchanged;//射线命中的对象是否改变
        private Vector3 startposraycast;//线渲染器绘制的线的起始点（射线的起始点）
        private Vector3 endposraycast;//线渲染器绘制的线的末端点（射线的末端点）

        private LineRenderer linerenderer;//线渲染器

        //! Delete all MUs in Sensor Area.
        /// <summary>
        /// 删除所有检测到的MU
        /// </summary>
        public void DeleteMUs()
        {
            var tmpcolliding = CollidingObjects;
            foreach (var obj in tmpcolliding.ToArray())
            {
                var mu = GetTopOfMu(obj);
                if (mu != null)
                {
                    Destroy(mu.gameObject);
                }

                CollidingObjects.Remove(obj);
            }
        }


       
        // Use this when Script is inserted or Reset is pressed
        private void Reset()
        {
            ///设置触发Layer
            AdditionalRayCastLayers = new List<string>();
            AdditionalRayCastLayers.Add(RDTSLayer.MU.ToString());
            AdditionalRayCastLayers.Add(RDTSLayer.Obstacle.ToString());
            AdditionalRayCastLayers.Add(RDTSLayer.MovableTransport.ToString());

            ///设置sensor材质
            if (MaterialOccupied == null)
            {
                MaterialOccupied = UnityEngine.Resources.Load(RDTSPath.path_SensorOccupied, typeof(Material)) as Material;//加载指定路径的材质
            }

            if (MaterialNotOccupied == null)
            {
                MaterialNotOccupied = UnityEngine.Resources.Load(RDTSPath.path_SensorNotOccupied, typeof(Material)) as Material;//加载指定路径的材质
            }
            ///有碰撞盒就获取，没有则启用射线投射
            _boxcollider = GetComponent<BoxCollider>();
            if (_boxcollider != null)
                _boxcollider.isTrigger = true;
            else
                UseRaycast = true;
        }

        // Use this for initialization
        private void Start()
        {
            _isOccupiedNotNull = SensorOccupied != null;//检测信号是否不为空
            CollidingObjects = new List<GameObject>();
            CollidingMus = new List<MU>();
            if (LimitSensorToTag == null)
                LimitSensorToTag = "";
            _boxcollider = GetComponent<BoxCollider>();
            if (_boxcollider != null)
            {
                _meshrenderer = _boxcollider.gameObject.GetComponent<MeshRenderer>();
            }

            if (_boxcollider == null && !UseRaycast && Application.isPlaying)
            {
                ErrorMessage("Sensors which are not using a Raycast need to have a BoxCollider on the same Gameobject as this Sensor script is attached to");
            }

            if (Application.isPlaying)///在运行模式下
            {
                scale = RDTSController.Scale;
                AdditionalRayCastLayers.Add(LayerMask.LayerToName(gameObject.layer));//增加此对象的层级
                // create line renderer for raycast if not existing
                //若射线投射没有线渲染器，就为它创建
                if (UseRaycast && ShowSensorLinerenderer)
                {
                    linerenderer = GetComponent<LineRenderer>();
                    if (linerenderer == null)
                        linerenderer = gameObject.AddComponent<LineRenderer>();
                }

            }

            if (AdditionalRayCastLayers == null)
                AdditionalRayCastLayers = new List<string>();
            layermask = LayerMask.GetMask(AdditionalRayCastLayers.ToArray());
            ShowStatus();
        }

        /// <summary>
        /// 用线渲染器绘制光线
        /// </summary>
        private void DrawLine()
        {
            if (ShowSensorLinerenderer)
            {
                List<Vector3> pos = new List<Vector3>();
                pos.Add(startposraycast);
                pos.Add(endposraycast);
                linerenderer.startWidth = RayCastDisplayWidth;
                linerenderer.endWidth = RayCastDisplayWidth;
                linerenderer.SetPositions(pos.ToArray());
                linerenderer.useWorldSpace = true;
                if (raycasthasthit)
                {
                    linerenderer.material = MaterialOccupied;
                }
                else
                {
                    linerenderer.material = MaterialNotOccupied;
                }
            }
        }

        /// <summary>
        /// 用Debug类绘制光线，若光线与碰撞体相交则呈现红色且长度为原点到碰撞点，未相交则呈现黄色且长度为给定的长度
        /// </summary>
        private void Raycast()
        {
            if (!Application.isPlaying)//不在play模式下
            {
                var list = new List<string>(AdditionalRayCastLayers);
                list.Add(LayerMask.LayerToName(gameObject.layer));
                layermask = LayerMask.GetMask(list.ToArray());
            }

            float scale = 1000;
            raycasthitchanged = false;
            var globaldir = transform.TransformDirection(RayCastDirection);
            var display = Vector3.Normalize(globaldir) * RayCastLength / scale;//整条光线的长度
            startposraycast = transform.position;
            if (Physics.Raycast(transform.position, globaldir, out hit, RayCastLength / scale, layermask))//向场景中的所有碰撞体投射一条射线。 如果射线与任何碰撞体相交，返回 true，否则返回 false
            {
                //hit.distance：从射线原点到撞击点的距离
                var dir = Vector3.Normalize(globaldir) * hit.distance;//射线原点到撞击点的距离
                if (Application.isPlaying)
                    scale = RDTSController.Scale;

                RayCastDistance = hit.distance * scale;
                if (DisplayStatus) Debug.DrawRay(transform.position, dir, Color.red, 0, true);//若DisplayStatus为true，绘制红色射线
                raycasthasthit = true;//设置标志位
                if (hit.collider != lasthit.collider)//若射线命中的碰撞体与上一次命中的不同
                    raycasthitchanged = true;
                endposraycast = startposraycast + dir;//射线末端点坐标
            }
            else
            {
                if (DisplayStatus) Debug.DrawRay(transform.position, display, Color.yellow, 0, true);//若DisplayStatus为true，绘制黄色射线
                raycasthasthit = false;//重置标志位
                endposraycast = startposraycast + display;
                RayCastDistance = 0;
            }

        }

        // Shows Status of Sensor
        /// <summary>
        /// 显示相关的状态信息
        /// </summary>
        private void ShowStatus()
        {

            if (CollidingObjects.Count == 0)//没有检测到碰撞对象
            {
                LastTriggeredBy = null;
                LastTriggeredID = 0;
                LastTriggeredGlobalID = 0;
            }
            else
            {
                GameObject obj = CollidingObjects[CollidingObjects.Count - 1];
                if (!ReferenceEquals(obj, null))//碰撞对象不为空
                {
                    var LastTriggeredByMU = GetTopOfMu(obj);//获取碰撞对象最顶层的MU组件
                    if (!ReferenceEquals(LastTriggeredByMU, null))//是MU
                        LastTriggeredBy = LastTriggeredByMU.gameObject;
                    else//不是MU，检测到其他层级
                        LastTriggeredBy = obj;

                    if (LastTriggeredByMU != null)
                    {
                        LastTriggeredID = LastTriggeredByMU.ID;
                        LastTriggeredGlobalID = LastTriggeredByMU.GlobalID;
                    }
                }
            }

            //根据碰撞和检测到的物体数量设置材质，以改变Sensor对象的状态
            if (CollidingObjects.Count > 0)
            {
                _occupied = true;
                if (DisplayStatus && _meshrenderer != null)
                {
                    _meshrenderer.material = MaterialOccupied;
                }
            }
            else
            {
                _occupied = false;
                if (DisplayStatus && _meshrenderer != null)
                {
                    _meshrenderer.material = MaterialNotOccupied;
                }
            }

            Occupied = _occupied;
        }

        // ON Collission Enter 进入碰撞（触发）时被调用
        private void OnTriggerEnter(Collider other)
        {
            GameObject obj = other.gameObject;
            var tmpcolliding = CollidingObjects;
            var muobj = GetTopOfMu(obj);//获取根MU对象

            if ((LimitSensorToTag == "" || ((muobj.tag == LimitSensorToTag) || muobj.Name == LimitSensorToTag)))//未限制标签 或检测到的MU的标签是限制的标签 或检测到的MU的名称是限制的标签
            {
                if (PauseOnSensor)
                    Debug.Break();//暂停该 Editor。 想要检查该 Inspector 上的某些值并且无法手动暂停它时，这非常有用
                if (!CollidingObjects.Contains(obj))
                    CollidingObjects.Add(obj);


                ShowStatus();//显示状态信息

                if (muobj != null)
                {
                    if (!CollidingMus.Contains(muobj))
                    {
                        if (EventEnter != null)
                            EventEnter(muobj.gameObject);
                        Counter++;
                        muobj.EventMUEnterSensor(this);
                        CollidingMus.Add(muobj);
                        if (EventMUSensor != null)
                            EventMUSensor.Invoke(muobj, true);//检测到是MU的Event调用
                    }
                }
                else
                {
                    if (EventEnter != null)
                        EventEnter(obj);
                    if (EventNonMUGameObjectSensor != null)
                        EventNonMUGameObjectSensor.Invoke(obj, true);//检测到的不是MU的Event调用
                }
            }
        }

        public void OnMUPartsDestroyed(GameObject obj)
        {
            CollidingObjects.Remove(obj);
        }

        //移除此MU
        public void OnMUDelete(MU muobj)
        {

            CollidingObjects.Remove(muobj.gameObject);

            // Check if remaining colliding objects belong to same mu
            var coolliding = CollidingObjects.ToArray();
            var i = 0;
            do
            {
                if (i < coolliding.Length)
                {
                    var thismuobj = GetTopOfMu(coolliding[i]);
                    if (thismuobj == muobj)
                    {
                        CollidingObjects.Remove(coolliding[i]);
                    }
                }

                i++;
            } while (i < coolliding.Length);
            CollidingMus.Remove(muobj);
            if (EventExit != null)
                EventExit(muobj.gameObject);
            if (EventMUSensor != null)
                EventMUSensor.Invoke(muobj, false);
            muobj.EventMUExitSensor(this);
            LastTriggeredBy = null;
            LastTriggeredID = 0;
            LastTriggeredGlobalID = 0;
            ShowStatus();
        }



        // ON Collission Exit  退出碰撞（触发）时被调用
        private void OnTriggerExit(Collider other)
        {
            GameObject obj = other.gameObject;
            if (!ReferenceEquals(obj, null))
            {

                var muobj = GetTopOfMu(obj);
                var tmpcolliding = CollidingObjects;
                var dontdelete = false;
                CollidingObjects.Remove(obj);

                // Check if remaining colliding objects belong to same mu
                foreach (var thisobj in CollidingObjects)
                {
                    var thismuobj = GetTopOfMu(thisobj);
                    if (thismuobj == muobj)
                    {
                        dontdelete = true;
                    }
                }

                if (!dontdelete)
                {

                    if (muobj != null && CollidingMus.Contains(muobj))
                    {
                        CollidingMus.Remove(muobj);
                        if (EventExit != null)
                            EventExit(muobj.gameObject);
                        if (EventMUSensor != null)
                            EventMUSensor.Invoke(muobj, false);
                        muobj.EventMUExitSensor(this);
                    }
                    else
                    {
                        if (EventNonMUGameObjectSensor != null)
                            EventNonMUGameObjectSensor.Invoke(obj, false);
                        if (EventExit != null)
                            EventExit(obj);
                    }
                }
                ShowStatus();
            }
        }

        private void FixedUpdate()
        {
            if (Application.isPlaying && UseRaycast)
            {
                Raycast();

                // last raycast has left 
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
            if (Application.isPlaying)
                // Set external Value Outputs
                if (_isOccupiedNotNull)
                    SensorOccupied.Value = Occupied;
        }

        private void Update()
        {
            if (!Application.isPlaying && UseRaycast)
            {
                layermask = LayerMask.GetMask(AdditionalRayCastLayers.ToArray());
                Raycast();//Debug类绘制光线，无宽度
            }

            if (Application.isPlaying && UseRaycast && DisplayStatus)
            {
                DrawLine();//线渲染器绘制光线，有宽度
            }
        }
    }
}