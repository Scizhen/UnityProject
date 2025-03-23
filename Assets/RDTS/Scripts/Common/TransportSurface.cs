//*************************************************************************
//Thanks for the code reference game4automation provides.                 *
//                                                                        *
//*************************************************************************
using System;
using NaughtyAttributes;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RDTS
{
    //! Transport Surface - this class is needed together with Drives to model conveyor systems. The transport surface is transporting
    //! rigid bodies which are colliding with its surface
    //！这个类需要与 Drives 一起为输送系统建模，运输表面正在运输与其表面碰撞的刚体
    public class TransportSurface : BaseTransportSurface
    {
        #region Public Variables

        public Vector3 TransportDirection; //!< The direction in local coordinate system of Transport surface - is initialized normally by the Drive 运输面局部坐标系中的方向 - 通常由Drive初始化
        public float TextureScale = 10; //!< The texture scale what influcences the texture speed - needs to be set manually     影响纹理速度的纹理比例 - 需要手动设置

        public RigidbodyConstraints ConstraintsEnter;//碰撞对象进入时对其刚体的约束
        public RigidbodyConstraints ConstraintsExit;//碰撞对象退出时对其刚体的约束
        public bool Radial = false;
        [HideInInspector] public Drive UseThisDrive;
        public float speed = 0; //!< the current speed of the transport surface - is set by the drive   运输面当前的速度，由Drive设定
        [InfoBox("Standard Setting for layer is Transport")]
        [OnValueChanged("RefreshReferences")] 
        public string Layer = RDTSLayer.Transport.ToString();//设置挂载此脚本的对象的Layer
        [InfoBox("For Best performance unselect UseMeshCollider, for good transfer between conveyors select this")]//为了获得最佳性能，请取消选择 UseMeshCollider；为了在输送机之间进行良好的传输，请选择此项
        [OnValueChanged("RefreshReferences")] 
        public bool UseMeshCollider = false;//是否使用MeshCollider的碰撞
        public Drive ParentDrive; //!< needs to be set to true if transport surface moves based on another drive - transport surface and it's drive are not allowed to be a children of the parent drive.
                                  //如果传输表面基于另一个驱动器移动，则需要设置为 true - 传输表面及其驱动器不允许成为父驱动器的子驱动器
        public delegate void
            OnEnterExitDelegate(Collision collission,TransportSurface surface); //!< Delegate function for GameObjects entering the Sensor.

        public event OnEnterExitDelegate OnEnter;
        public event OnEnterExitDelegate OnExit;


        #endregion

        #region Private Variables
        private MeshRenderer _meshrenderer;
        private Collider _collider;
        private Rigidbody _rigidbody;
        private bool _isMeshrendererNotNull;
        private bool parentdrivenotnull;
        private Transform _parent;
        private Vector3 parentposbefore;
        private Quaternion parentrotbefore;
        private Quaternion parenttextrotbefore;
        #endregion

        #region Public Methods

        //! Gets a center point on top of the transport surface
        /// <summary>
        /// 获取传输表面顶部的中心点
        /// </summary>
        /// <returns></returns>
        public Vector3 GetMiddleTopPoint()
        {
            
            var collider = gameObject.GetComponent<Collider>();
            if (collider!=null)
            {
                var vec = new Vector3(collider.bounds.center.x, collider.bounds.center.y + collider.bounds.extents.y,
                    collider.bounds.center.z);
                return vec;
            }
            else
                return Vector3.zero;
        }

        //! Sets the speed of the transport surface (used by the drive)
        /// <summary>
        /// 设置运输面的速度
        /// </summary>
        /// <param name="_speed"></param>
        public void SetSpeed(float _speed)
        {
            speed = _speed;
        }

        #endregion

        #region Private Methods

        //当该碰撞体/刚体已开始接触另一个刚体/碰撞体时调用
        private void OnCollisionEnter(Collision other)
        {
          // Global.DebugArrow(other.rigidbody.transform.position, other.rigidbody.velocity, Color.green, 0.5f);
            other.rigidbody.constraints = ConstraintsEnter;//设置该刚体的模拟自由度
            var mu = other.gameObject.GetComponentInParent<MU>();
            mu?.TransportSurfaces.Add(this);
            if (OnEnter!=null)
             OnEnter.Invoke(other,this);//调用进入方法
        }
        //当该碰撞体/刚体已停止接触另一个刚体/碰撞体时调用
        private void OnCollisionExit(Collision other)
        {
           //   Global.DebugArrow(other.rigidbody.transform.position,other.rigidbody.velocity,Color.red,0.5f);
           var mu = other.gameObject.GetComponentInParent<MU>();
           if (mu?.TransportSurfaces.Count==0)
               other.rigidbody.constraints = ConstraintsExit;//设置该刚体的模拟自由度
           if (OnExit!=null)
               OnExit.Invoke(other,this);//调用退出方法
        }

        /// <summary>
        /// 使用MeshCollider还是BoxCollider；
        /// 刚体存在则禁用重力、激活运动学属性
        /// </summary>
        private void RefreshReferences()
        {
            var _mesh = GetComponent<MeshCollider>();
            var _box = GetComponent<BoxCollider>();
            if (UseMeshCollider)//若使用MeshCollider
            {
                if (_box!=null)
                    DestroyImmediate(_box);
                if (_mesh==null)
                {
                    _mesh = gameObject.AddComponent<MeshCollider>();
                }
            }
            else//使用BoxCollider
            {
                if (_mesh!=null)
                    DestroyImmediate(_mesh);
                if (_box==null)
                {
                    _box= gameObject.AddComponent<BoxCollider>();
                }
            }

            _rigidbody = gameObject.GetComponent<Rigidbody>();
            if (_rigidbody != null)
            {
                _rigidbody.isKinematic = true;
                _rigidbody.useGravity = false;
            }

            _collider = gameObject.GetComponent<Collider>();
            _meshrenderer = gameObject.GetComponent<MeshRenderer>();

        }


        private void Reset()
        {
            gameObject.layer = LayerMask.NameToLayer(Layer);
            RefreshReferences();

            /* 增加Drive与Transport的联系 */
            // Add transport surface to drive if a drive is existing in this or an upper object
            //如果Drive存在于此对象或上层对象中，则将传输表面添加到驱动器Drive
            if (UseThisDrive != null)
            {
                UseThisDrive.AddTransportSurface(this);
                return;
            }

            var drive = gameObject.GetComponentInParent<Drive>();
            if (drive != null)
                drive.AddTransportSurface(this);
        }
        
        [Button("Destroy Transport Surface")]
        private void DestroyTransportSurface()//销毁此TransportSurface
        {
            var drive = gameObject.GetComponentInParent<Drive>();
            if (drive != null)
                drive.RemoveTransportSurface(this);
            Object.DestroyImmediate(this);
        }
        

        void Start()
        {
            RefreshReferences();
            _isMeshrendererNotNull = _meshrenderer != null;
            Reset();
            SetSpeed(speed);
            parentposbefore = Vector3.zero;
            parentrotbefore = Quaternion.identity;
            parenttextrotbefore  = Quaternion.identity;
            parentdrivenotnull = ParentDrive != null;
        }
      

        void Update()
        {
            if (speed != 0) 
            {
                Vector3 mov = Vector3.zero;
                if (parentdrivenotnull)//若ParentDrive存在
                {
                    if (parenttextrotbefore == Quaternion.identity)
                    {
                        parenttextrotbefore = ParentDrive.transform.rotation;
                    }
                    var parentrot = ParentDrive.transform.rotation;
                    var deltarot = parentrot * Quaternion.Inverse(parenttextrotbefore);
                    var newrot = deltarot * _rigidbody.rotation;
                    mov = newrot * TransportDirection * TextureScale* Time.time * speed *
                        RDTSController.SpeedOverride  / RDTSController.Scale;
                }
                else
                {
                    mov = TextureScale * TransportDirection * Time.time * speed *
                        RDTSController.SpeedOverride  / RDTSController.Scale;
                }
                
                Vector2 vector2 =  new Vector2();
                if (!Radial)
                {
                    var globalrot = this.transform.rotation.eulerAngles;
                    Vector3 vector3 = new Vector3(mov.x, mov.z, 0);
                    var textdir = Quaternion.Euler(0, 0, globalrot.y) * vector3;
                    vector2 = new Vector2(textdir.x, textdir.y);
                }
                else
                {
                    vector2 = new Vector2(mov.x, mov.y);
                }

                if (parentdrivenotnull)
                   parenttextrotbefore = ParentDrive.transform.rotation;
                if (_isMeshrendererNotNull)
                {
                    _meshrenderer.material.mainTextureOffset = vector2;///设置纹理的偏移，使得仿真时输送面呈现移动的效果
                }
            }
        }
     
        void FixedUpdate()
        {

                if (!Radial)
                {
                    Vector3 newpos, mov;
                    newpos = _rigidbody.position;
                    
                    // Linear Conveyor
                    if (parentdrivenotnull)//一般情况为null
                    {
                        if (parentposbefore == Vector3.zero)
                        {
                            parentposbefore = ParentDrive.transform.position;
                        }
                        if (parentrotbefore == Quaternion.identity)
                        {
                            parentrotbefore = ParentDrive.transform.rotation;
                        }
                        var dir = TransportDirection;//会由Drive中设置运输方向
                        var parentpos = ParentDrive.transform.position;
                        var deltaparent = parentpos - parentposbefore;
                        var parentrot = ParentDrive.transform.rotation;
                        
                        mov = parentrot * dir * Time.fixedDeltaTime * speed *
                              RDTSController.SpeedOverride /
                              RDTSController.Scale;
                        
                        var dirtotal = mov + deltaparent;   // ParentDrive separate
                        var dirback = -mov;                // ParentDrive separate
                       
                        _rigidbody.position = (_rigidbody.position + dirback );
                        Physics.SyncTransforms();//将 Transform 更改应用于物理引擎
                        _rigidbody.MovePosition(_rigidbody.position + dirtotal);
                        _rigidbody.MoveRotation(parentrot.normalized);
                        
                        parentposbefore = ParentDrive.transform.position;
                        parentrotbefore = ParentDrive.transform.rotation;
                    }
                    else
                    {
                        if (speed != 0)//由Drive中赋值的速度来控制运输面上的物体移动
                        {
                            mov = TransportDirection * Time.fixedDeltaTime * speed *
                                  RDTSController.SpeedOverride /
                                  RDTSController.Scale;
                            _rigidbody.position = (_rigidbody.position - mov);//将输送面往后移动
                            Physics.SyncTransforms();//将 Transform 更改应用于物理引擎
                            _rigidbody.MovePosition(_rigidbody.position + mov);//将输送面和碰撞物体一起向前移动

                           /* _rigidbody.position和_rigidbody.MovePosition的结合使用，呈现出输送面不动，与输送面碰撞的物体被运输的现象 */
                        }
                    }
                }
                else
                {
                    Quaternion nextrot;
                    // Radial Conveyor
                    if (ParentDrive!=null)
                    {
                        Error("Not implemented!");
                    }
                    else
                    {
                        if (speed != 0)
                        {
                            _rigidbody.rotation = _rigidbody.rotation * Quaternion.AngleAxis(
                                -speed * Time.fixedDeltaTime *
                                RDTSController.SpeedOverride, transform.InverseTransformVector(TransportDirection));
                            nextrot = _rigidbody.rotation * Quaternion.AngleAxis(
                                +speed * Time.fixedDeltaTime * RDTSController.SpeedOverride,
                                transform.InverseTransformVector(TransportDirection));
                            _rigidbody.MoveRotation(nextrot);
                        }
                    }
                }
           
        }
        #endregion
    }
}