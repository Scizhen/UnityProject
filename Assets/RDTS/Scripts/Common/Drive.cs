//*************************************************************************
//Thanks for the code reference game4automation provides.                 *
//                                                                        *
//*************************************************************************  
using System;
using NaughtyAttributes;
using UnityEngine;
using System.Collections.Generic;
using RDTS.Method;

#if GAME4AUTOMATION_INTERACT
using XdeEngine.Core;
using XdeEngine.Core.Monitoring;
#endif

namespace RDTS
{
    /// <summary>
    /// 基础的驱动脚本，可与DriveBehaviours共同使用，并用Value信号来控制
    /// </summary>
    [SelectionBase]
    //! The drive is moving components including all sub components along the local axis of the game object.
    //! Rotational and linear movements are possible. A drive can be enhanced by DriveBehaviours which are adding special
    //! behaviours as well as Input and Output signals to drives.
    //！可以进行旋转和直线运动。 DriveBehaviours 可以增强驱动器，它为驱动器添加特殊行为以及输入和输出信号。
    public class Drive : BaseDrive
    {
        #region PublicVariables

        [Header("Settings")] [OnValueChanged("CalculateVectors")]
        public DIRECTION
            Direction; //!< The direction in local coordinate system of the GameObject where the drive is attached to. 驱动方向

        [OnValueChanged("CalculateVectors")]
        public bool ReverseDirection; //!< Set to *true* if Direction needs to be inverted. 方向反转

        [Tooltip("Offeset for defining another Drive 0 position. Drive will start at simulation start at Offset.")]
        public float Offset; //!< Start offset of the drive from zero position in millimeters or degrees. 偏移量

        public float StartPosition; //!< Start Position off the Drive 起始位置

        public float SpeedScaleTransportSurface = 1; //!< Scale of the Speed for radial transportsurfaces to feed in mm/s on radius  径向传输表面的速度比例，以 mm/s 为单位进给半径

        [Tooltip(
            "Should be normally turned off. If set to true the RigidBodies are moved. Use it if moving part has attached colliders. If false the transforms are moved")]
        public bool MoveThisRigidBody = false; //!< If set to true the RigidBodies are moved (use it if moving) part has attached colliders, if false the transforms are moved

        [NaughtyAttributes.ReadOnly] public bool EditorMoveMode = false;

        [ReorderableList] public List<TransportSurface> TransportSurfaces; //!< The transport surface the drive is controlling. Is null if drive is not controlling a transport surface.
                                                                           //驱动器控制的传输表面。 如果驱动器未控制传输表面，则为空 

        [BoxGroup("Limits")] public bool UseLimits;///使用限制
        /* Linear下：在Gizmos中显示LowerLimit处为一个立方体，UpperLimit处为箭头
         * Rotation下：在Gizmos中显示一个对应角度的圆
         */
        [ShowIf("UseLimits")] [BoxGroup("Limits")]
        public float LowerLimit = 0; //! Lower Drive Limit, Upper and Lower = 0 if this should not be used

        [ShowIf("UseLimits")] [BoxGroup("Limits")]
        public float UpperLimit = 1000; //! Upper Drive Limit, Upper and Lower = 0 if this should not be used

        [ShowIf("UseLimits")] [BoxGroup("Limits")]
        public bool JumpToLowerLimitOnUpperLimit = false;//若为true，达到UpperLimit后则重新跳回LowerLimit处

        [ShowIf("UseLimits")]
        [BoxGroup("Limits")]
        [Tooltip("If assigned the Raycast measurment is the basis for the drive Limits")]//如果分配了 Raycast 测量值，则是驱动器限制的基础
        public Sensor LimitRayCast;

        [Space(10)] [BoxGroup("Acceleration")]
        public bool UseAcceleration = false; //!< If set to true the drive uses the acceleration  是否使用加速度

        [BoxGroup("Acceleration")] [ShowIf("UseAcceleration")]
        public bool SmoothAcceleration = false; //!< if set to true the drive uses smooth acceleration with a sinoide function
                                                //如果设置为 true，则驱动器使用带有正弦函数的平滑加速

        [ShowIf("UseAcceleration")] [BoxGroup("Acceleration")]
        public float Acceleration = 100; //!< The acceleration in millimeter per second^2    加速度


        [Header("Drive IO's")] public bool JogForward = false; //!< A jog bit for jogging forward with the Target Speed 用于以目标速度向前驱动的慢跑位
        public bool JogBackward = false; //!< A jog bit for jogging backwards with the Target Speed  向后驱动
        public float TargetPosition; //!< The target position of the Drive  目标位置
        public float TargetSpeed = 100; //!< The target speed in millimeter / second of the Drive   目标速度

        public bool TargetStartMove = false; //!< When changing to true the Drive is starting to move to the TargetPosition       
                                             //当更改为 true 时，Drive 开始移动到 TargetPosition

        [HideInInspector]
        public bool BlockDestination = true; //!< If Block Drive is true it will not drive to its Target Positon, Jogging is possible  如果Block Drive为真，它不会驱动到它的目标位置，Jogging是可能的

        public bool ResetDrive = false; //!< Resets the Drive to the zero position and stops all movements  重置回零位置，并停止所有的运动
        public bool _StopDrive = false; //!< Stops the Drive at the current position   在当前位置处停止
        [ReadOnly] public float CurrentSpeed; //!< The current speed of the drive   当前速度
        [ReadOnly] public float CurrentPosition; //!< The current position of the drive   当前位置
        [ReadOnly] public bool IsStopped = false; //!< Is true if Drive is stopped   是否停止
        [ReadOnly] public bool IsRunning = false; //!< Is true if Drive is running   是否运动
        [ReadOnly] public bool IsAtTargetSpeed = false; //!< Is true if Drive is at target speed    到达目标速度则为true   
        [ReadOnly] public bool IsAtTarget = false; //!< Is true if Drive is at target position   到达目标位置则为true
        [ReadOnly] public bool IsAtLowerLimit = false; //!< Is true if Drive is jogging and reaching lower Limit    JogBackward为true且到达LowerLimit处
        [ReadOnly] public bool IsAtUpperLimit = false; //!< Is true if Drive is jogging and reaching upper Limit    JogForward为true且到达UpperLimit处
        [HideInInspector] public bool HideGizmos = false;

        // XDE Integration
#if GAME4AUTOMATION_INTERACT
        [HideInInspector]
        public XdeUnitJointMonitor jointmonitor;
        [HideInInspector]
        public XdeUnitJointPDController jointcontroller;
#endif
#if !GAME4AUTOMATION_INTERACT
        [HideInInspector]
#endif
        public bool UseInteract = false;

        [HideInInspector] public bool IsRotation = false;

        #endregion

        #region Private Variables

        private bool _jogactive;
        private float _lastspeed;
        private float _currentdestination;//当前的目标点
        private float _timestartacceleration;
        private double _currentacceleration;//当前的加速度
        private bool _laststartdrivetotarget;//记录上次TargetStartMove的值
        private bool _isdrivingtotarget = false;//是否向目标点驱动
        private bool _drivetostarted = false;
        private float _lastcurrentposition;
        private bool _istransportsurface = false;//是否驱动运输面
        private bool _lastisattarget = false;
        private float _currentstoppos;
        private bool _stopjogging = false;

        private Vector3 _localdirection;
        private Vector3 _positiondirection;
        private Vector3 _globaldirection;
        private Vector3 _localdirectionscale;
        private Vector3 _localstartpos;
        private Vector3 _localstartrot;
        private float _localscale;
        private Rigidbody _rigidbody;
        private Vector3 _rotationpoint;
        private TransportSurface[] _transportsurfaces;
        private Vector3 _globalpos;
        private Quaternion _globalrot;
        private bool _lastjog;//记录上次的驱动状态
        private bool _limitraycastnotnull;//LimitRayCast是否为空

        private bool articulatedbodynotnull;
        private ArticulationBody articulatedbody;

        private bool _accelerationstarted = false;//是否开始加速
        private bool _decelerationstarted = false;//是否开始减速

        #endregion

        #region Public Events

        public delegate void
            OnAtPositionEvent(Drive drive); //!< Delegate function for the Drive reaching the desitionationPosition

        public event OnAtPositionEvent OnAtPosition;

        public delegate void OnJumpToLowerLimitEvent(Drive drive);

        public event OnJumpToLowerLimitEvent OnJumpToLowerLimit;

        #endregion

        #region Public Methods

#if GAME4AUTOMATION_INTERACT
        [Button("Kinematize (Interact)")]
        public void Kinematize()
        {
            CalculateVectors();
            Game4AutomationPhysics.Kinematize(gameObject);
        }

        [Button("Unkinematize (Interact)")]
        public void Uninematize()
        {
            Game4AutomationPhysics.UnKinematize(gameObject);
        }
#endif

        //! Starts the drive to move forward with the target speed.
        public void Forward()
        {
            JogForward = true;
            JogBackward = false;
        }

        //! Starts the drive to move forward with the target speed.
        public void Backward()
        {
            JogForward = false;
            JogBackward = true;
        }

        //! Starts the drive to drive to the given Target with the target speed.
        /// <summary>
        /// 启动驱动器，以目标速度行驶到给定的目标，由各类Drive Behaviour调用
        /// </summary>
        /// <param name="Target"></param>
        public void DriveTo(float Target)
        {
            BlockDestination = false;
            TargetPosition = Target;//设置目标位置
            _currentdestination = TargetPosition;
            TargetStartMove = true;
            _drivetostarted = true;
            /*_StopDrive = false;*/
        }

        //! Starts the drive - it will speed up with sinoide if turned on

        public void Accelerate()
        {
            _accelerationstarted = true;
            IsAtTarget = false;
            _decelerationstarted = false;
            _timestartacceleration = Time.time;
            _StopDrive = false;
            IsStopped = false;
        }


        public void Decelerate()//减速
        {
            _decelerationstarted = true;
            _accelerationstarted = false;
            _timestartacceleration = Time.time;
            IsAtTarget = false;
            _StopDrive = false;
            IsStopped = false;
        }

        //! Stops the drive at the current position
        public void Stop()
        {
            TargetStartMove = false;
            _decelerationstarted = false;
            _accelerationstarted = false;
            _currentacceleration = 0;
            IsRunning = false;
            JogForward = false;
            JogBackward = false;
            CurrentSpeed = 0;
            _StopDrive = false;
            IsStopped = true;
        }

        //! Gets the axis vector of the drive
        public Vector3 GetLocalDirection()
        {
            return _localdirection;
        }

        public void StartEditorMoveMode()
        {
            CalculateVectors();
            EditorMoveMode = true;
#if UNITY_EDITOR
            Global.SetLockObject(this.gameObject, true);
#endif
        }

        public void SetPositionEditorMoveMode(float editorposition)
        {
            if (EditorMoveMode)
            {
                CurrentPosition = editorposition;
                SetPosition();
            }
        }

        public void EndEditorMoveMode()
        {
#if UNITY_EDITOR
            Global.SetLockObject(this.gameObject, false);
#endif
            CurrentPosition = 0;
            SetPosition();
            EditorMoveMode = false;
        }


        //! Gets the start position of the drive in local scale
        public Vector3 GetStartPos()
        {
            return _localstartpos;
        }

        //! Gets the start position of the drive in local scale
        public Vector3 GetStartRot()
        {
            return _localstartrot;
        }

        //! Sets the position of the drive to 0
        public void SetDriveTransformToZero()
        {
            CalculateVectors();
            if (!IsRotation)
            {
                transform.localPosition = _localdirection * 0;
            }
            else
            {
                transform.localRotation = Quaternion.Euler(_localdirection * 0);
            }
        }

        //! Sets the position of the drive to the given value
        //! @param value the position the drive in millimeters or degrees
        /// <summary>
        /// 将驱动器的位置设置为给定值，以毫米或度为单位计算Drrive的位置
        /// </summary>
        /// <param name="value"></param>
        public void SetDriveTransformToValue(float value)
        {
            CalculateVectors();
            if (!IsRotation)
            {
                transform.localPosition = _localdirection * value;
            }
            else
            {
                transform.localRotation = Quaternion.Euler(_localdirection * value);
            }
        }


        //! Adds a Transport Surface to the Drive
        public void AddTransportSurface(TransportSurface trans)
        {
            if (TransportSurfaces == null)
                TransportSurfaces = new List<TransportSurface>();

            if (!TransportSurfaces.Contains(trans))
                TransportSurfaces.Add(trans);
        }

        //! Removes a Transport Surface fron the Drive
        public void RemoveTransportSurface(TransportSurface trans)
        {
            if (TransportSurfaces != null)
                if (TransportSurfaces.Contains(trans))
                    TransportSurfaces.Remove(trans);
        }

        #endregion

        #region PrivateMethods

        /// <summary>
        /// 计算全局、局部、位置的方向向量；
        /// 局部scale；
        /// 是否反向
        /// </summary>
        public void CalculateVectors()
        {
            _localdirection = DirectionToVector(Direction);//Drive的方向向量（单位向量）
            _globaldirection = transform.TransformDirection(_localdirection);//变换到全局坐标系
            if (!ReferenceEquals(transform.parent, null))//Drive组件附着的gameobject存在父对象
            {
                _positiondirection = transform.parent.transform.InverseTransformDirection(_globaldirection);//有父对象就转换成相对于父对象的局部坐标系
            }
            else
            {
                _positiondirection = _globaldirection;//无父对象就用全局坐标系
            }

            //局部scale
            if (transform.parent != null)
                _localscale = GetLocalScale(transform.parent.transform, Direction);
            else
                _localscale = 1;

            _localstartpos = transform.localPosition;
            _localstartrot = transform.localEulerAngles;
            //是否反转方向
            if (ReverseDirection)
            {
                _globaldirection = -_globaldirection;
                _localdirection = -_localdirection;
                _positiondirection = -_positiondirection;
            }

            IsRotation = false;
            if (Direction == DIRECTION.RotationX || Direction ==
                DIRECTION.RotationY || Direction == DIRECTION.RotationZ)
            {
                IsRotation = true;//是旋转运动
            }

#if GAME4AUTOMATION_INTERACT
            if (UseInteract && !Application.isPlaying)
                Game4AutomationPhysics.Kinematize(gameObject);
#endif
        }

        /// <summary>
        /// 计算新的位置或旋转角度
        /// </summary>
        private void SetPosition()
        {
            if (Direction == DIRECTION.Virtual)
            {
                float scale = 1;
                if (articulatedbodynotnull)
                {
                    if (articulatedbody.jointType != ArticulationJointType.RevoluteJoint)
                        scale = 1/ RDTSController.Scale;
                    ArticulationDrive currentDrive = articulatedbody.xDrive;
                    if (CurrentPosition > currentDrive.upperLimit)
                    {
                        currentDrive.target = currentDrive.upperLimit;
                    }
                    else if (CurrentPosition < currentDrive.lowerLimit)
                    {
                        currentDrive.target = currentDrive.lowerLimit;
                    }
                    else
                    {
                        currentDrive.target = CurrentPosition*scale;
                    }
                    articulatedbody.xDrive = currentDrive;
                }
                return;
            }
            
            if (!UseInteract)//
            {
                if (!_istransportsurface)//如果不是驱动运输面
                {
                    if (!IsRotation)//线性驱动
                    {
                        Vector3 localpos = _localstartpos +
                                           _positiondirection *
                                           ((CurrentPosition + Offset) / RDTSController.Scale) /
                                           _localscale;//计算位置（RDTSController.Scale：控制器脚本中设置的比例系数）

                        if (MoveThisRigidBody)//操作“刚体”
                        {
                            if (!ReferenceEquals(transform.parent, null))
                                _globalpos = transform.parent.TransformPoint(localpos);
                            else
                                _globalpos = localpos;
                            _rigidbody.MovePosition(_globalpos);
                        }
                        else//操作“变换”
                        {
                            transform.localPosition = localpos;
                        }
                    }
                    else//旋转驱动
                    {
                        Quaternion.Euler(_localstartrot + _localdirection * (CurrentPosition + Offset));
                        Quaternion localrot =
                            Quaternion.Euler(_localstartrot + _localdirection * (CurrentPosition + Offset));//计算旋转角度
                        if (MoveThisRigidBody)//操作“刚体”
                        {
                            if (!ReferenceEquals(transform.parent, null))
                            {
                                _globalrot = transform.parent.rotation * localrot;
                                _globalpos = transform.parent.TransformPoint(_localstartpos);
                                _rigidbody.MovePosition(_globalpos);
                            }
                            else
                            {
                                _globalrot = localrot;
                            }

                            _rigidbody.MoveRotation(_globalrot);
                        }
                        else//操作“变换”
                            transform.localRotation = localrot;
                    }
                }
            }
            else
            {
#if GAME4AUTOMATION_INTERACT
                   Game4AutomationPhysics.SetPosition(this,CurrentPosition);
#endif
            }
        }


#if GAME4AUTOMATION_INTERACT

        public override void AwakeAlsoDeactivated()
        {
            Game4AutomationPhysics.EnableDrive(this,this.enabled);
        }
        
#endif
        
        private new void Awake()
        {
            IsAtTarget = true;
            BlockDestination = true;
            _limitraycastnotnull = LimitRayCast != null;
            base.Awake();
        }

        // When Script is added or reset ist pushed
        private void Reset()
        {
#if GAME4AUTOMATION_INTERACT
            Game4AutomationPhysics.InitDrive(this);
#endif

            if (!UseInteract)
            {
                /// Automatically create RigidBody if not there  没有刚体组件则自动传教刚体
                _rigidbody = gameObject.GetComponent<Rigidbody>();
                if (_rigidbody == null)
                {
                    _rigidbody = gameObject.AddComponent<Rigidbody>();
                }

                _rigidbody.isKinematic = true;
                _rigidbody.useGravity = false;
            }

            // Automatically add Transportsurface if one is existent in this or any sub object and no other drive is in between
            // 如果此对象或任何子对象中存在Transportsurface，并且中间没有其他驱动器，则自动添加Transportsurface
            var surfaces = gameObject.GetComponentsInChildren<TransportSurface>();
            foreach (var surface in surfaces)
            {
                // check if this drive is directly upwards to surface
                if (surface.GetComponentInParent<Drive>() == this)
                {
                    AddTransportSurface(surface);
                }
            }
        }

        // Simulation Scripts - Start, Update ....
        private void Start()
        {
            // Use Articulated bodies
            articulatedbody = GetComponent<ArticulationBody>();
            if (articulatedbody != null)
                articulatedbodynotnull = true;
            
            if (EditorMoveMode)
            {
                EndEditorMoveMode();
            }

            _rigidbody = gameObject.GetComponent<Rigidbody>();//获取刚体组件
            
            if (UseInteract)
            {
#if GAME4AUTOMATION_INTERACT
                jointmonitor = GetComponent<XdeUnitJointMonitor>();
                jointcontroller = GetComponent<XdeUnitJointPDController>();
#endif
#if !GAME4AUTOMATION_INTERACT
                Error("INTERACT is not installed or not enabled - please check Game4Automation main menu and enable INTERACT");
#endif
            }

            CalculateVectors();//计算所需的方向向量、scale
            _transportsurfaces = new TransportSurface[TransportSurfaces.Count];
            // Init Transportscripts  
            // At TransportScript to Transport Surfaces
            for (int i = 0; i < TransportSurfaces.Count; i++)
            {
                if (TransportSurfaces[i] == null)
                {
                    ErrorMessage(
                        "Transportsurface Script needs to be assigned to Drive in Array [Transport Surfaces] at Position " +
                        i.ToString());
                }
                else
                {
                    _transportsurfaces[i] = TransportSurfaces[i];
                    _transportsurfaces[i].TransportDirection = _globaldirection;///设置运输面中的运输方向
                    _transportsurfaces[i].SetSpeed(CurrentSpeed * SpeedScaleTransportSurface);///设置运输面的速度
                }
            }

            if (_transportsurfaces.Length > 0)//运输面列表不为空则说明驱动的是运输面
            {
                _istransportsurface = true;
            }

            CurrentPosition = StartPosition;//当前位置为开始位置
        }

        //重置驱动状态
        public void DriveReset()
        {
            CurrentPosition = Offset;
            CurrentSpeed = 0;
            IsRunning = false;
            BlockDestination = true;
        }

        private void FixedUpdate()
        {
            if (ResetDrive)//外部控制—重置状态
            {
                DriveReset();
            }

            if (_StopDrive)//停止驱动
            {
                Stop();
            }

            /* 停止驱动 */
            // Jog stopped
            if (_lastjog && !JogBackward && !JogForward && !UseAcceleration)//Jog驱动信号由true转变为false且不使用加速度，则立马停止
            {
                Stop();
            }

            if (_lastjog && !JogBackward && !JogForward && UseAcceleration)//Jog驱动信号由true转变为false且使用加速度，则使用加速度来逐渐停止
            {
                _stopjogging = true;
                if (CurrentSpeed > 0)
                    _currentacceleration = -Acceleration;
                else
                    _currentacceleration = Acceleration;
            }

            // Drive Decellerated totally - stop drive  
            if (_decelerationstarted && CurrentSpeed < 0)//在减速，且当前速度减到小于0，则停止
            {
                Stop();
            }


            var newtarget = false;

            /* 有新的目标点 */
            // New Target Position
            if (_laststartdrivetotarget != TargetStartMove && TargetStartMove)//开始向TargetPosition驱动
            {
                IsStopped = false;
                _stopjogging = false;
                BlockDestination = false;
                _currentdestination = TargetPosition;//设置当前目标点为TargetPosition
                _currentacceleration = Acceleration;//设置当前加速度
                _isdrivingtotarget = true;//正在向目标点驱动
                _timestartacceleration = Time.time;//记录此刻的时间
                IsAtTarget = false;//置位
                //CurrentSpeed = 0;
                _StopDrive = false;
                if (_drivetostarted)//当被Drive Behavior脚本控制时
                {
                    TargetStartMove = false;
                    _drivetostarted = false;
                }

                newtarget = true;
            }

            // Calculate Position if Speed > 0  速度大于0时计算当前的位置
            if (!IsStopped)
                if (!ResetDrive && (CurrentSpeed != 0) && !_StopDrive)
                {
                    CurrentPosition = CurrentPosition +
                                      CurrentSpeed * RDTSController.SpeedOverride * Time.deltaTime;
                }


            // Need to slow down - negative acceleration
            if (_isdrivingtotarget && !_StopDrive && !ResetDrive && !JogBackward && !JogForward)//正在向目标点驱动，但还未停止、未重置、Jog驱动信号均为false
            {
                if (UseAcceleration)//使用加速度则逐渐停止
                {
                    if (CurrentSpeed > 0)
                    {
                        _currentstoppos = CurrentPosition + (CurrentSpeed * CurrentSpeed) / (2 * Acceleration);
                    }
                    else
                    {
                        _currentstoppos = CurrentPosition - (CurrentSpeed * CurrentSpeed) / (2 * Acceleration);
                    }
                }
                else//无加速度则直接停止
                {
                    _currentstoppos = CurrentPosition;
                }
            }

            if (JogBackward || JogForward)
                IsStopped = false;

            // Calculate Acceleration
            if (!IsStopped)
                if ((_accelerationstarted || _decelerationstarted) ||
                    ((!IsAtTarget && _isdrivingtotarget) && !_StopDrive && !ResetDrive &&
                     UseAcceleration && !JogBackward && !JogForward &&
                     !_stopjogging))//开始加速 或开始减速 或未到达目标点、在向目标点驱动、使用了加速度、未停止驱动、未重置驱动、Jog驱动信号为false
                {
                    if (SmoothAcceleration == false)
                    {
                        if (!_accelerationstarted && !_decelerationstarted)
                        {
                            if (_currentdestination > _currentstoppos)
                            {
                                _currentacceleration = Acceleration;
                            }
                            else
                            {
                                _currentacceleration = -Acceleration;
                            }
                        }
                        else//若处于开始加速或开始减速的状态，则设置当前的加速度
                        {
                            if (_accelerationstarted)
                                _currentacceleration = Acceleration;
                            if (_decelerationstarted)
                                _currentacceleration = -Acceleration;
                        }
                    }
                    else
                    {
                        // Sinoide Calculation  正弦计算
                        float timespeedup = TargetSpeed / Acceleration;
                        float timedelta = Time.time - _timestartacceleration;//
                        float dir = 1;

                        if (_currentdestination < _currentstoppos)
                        {
                            dir = -1;
                        }

                        if (_decelerationstarted)
                            dir = -1;


                        double f = (-Math.Abs(TargetSpeed) * 4 * Math.PI) /
                                   (Math.Sin(2 * Math.PI) * timespeedup - 2 * Math.PI * timespeedup);
                        _currentacceleration = dir * f * (Math.Sin(Math.PI * timedelta / timespeedup)) *
                                               (Math.Sin(Math.PI * timedelta / timespeedup));
                    }
                }

            /* 在JogBackward或JogForward控制下时，计算加速度 */
            // Calculate Acceleration if Jogging   
            if (!IsStopped)
                if ((JogBackward || JogForward) && UseAcceleration)//Jog驱动信号有一个为true，且使用加速度
                {
                    _stopjogging = false;
                    if (JogForward)
                    {
                        if (CurrentSpeed < TargetSpeed)
                            _currentacceleration = Acceleration;
                        if (CurrentSpeed > TargetSpeed)
                            _currentacceleration = -Acceleration;
                    }
                    else
                    {
                        if (CurrentSpeed < TargetSpeed)
                            _currentacceleration = -Acceleration;
                        if (CurrentSpeed > TargetSpeed)
                            _currentacceleration = Acceleration;
                    }
                }

            /* 到达目标点 */
            // Drive at Target Position
            if (!IsStopped)
                if (!JogForward && !JogBackward && !newtarget)
                {
                    if ((_isdrivingtotarget && CurrentSpeed > 0 && CurrentPosition >= _currentdestination &&
                         _lastcurrentposition < _currentdestination) ||
                        (_isdrivingtotarget && CurrentSpeed < 0 && CurrentPosition <= _currentdestination &&
                         _lastcurrentposition > _currentdestination))
                    {
                        Stop();//停止
                        BlockDestination = true;
                        CurrentPosition = _currentdestination;//当前位置即为当前的目标点
                        IsAtTarget = true;//到达目标点置位
                        _isdrivingtotarget = false;//已到达目标点则不在是“向目标点驱动”状态
                        _stopjogging = false;
                    }
                }

            /* 计算当前速度 */
            // Calculate Speed
            if (!IsStopped)
                if (!ResetDrive && !_StopDrive && (!IsAtTarget || JogBackward || JogForward))//未重置驱动状态、未停止驱动、（未到达目标点 或Jog驱动信号有一个为true）
                {
                    if (!UseAcceleration)//未使用加速度
                    {
                        if (!JogForward && !JogBackward && !BlockDestination)
                        {
                            if (CurrentPosition < _currentdestination)
                                CurrentSpeed = TargetSpeed;//当前位置小于当前目标点，则当前速度为目标速度
                            if (CurrentPosition > _currentdestination)
                                CurrentSpeed = -TargetSpeed;//当前位置大于当前目标点，则当前速度为目标速度的负值
                        }
                        else
                        {
                            if (JogForward)
                                CurrentSpeed = TargetSpeed;//正向驱动，当前速度即为目标速度
                            if (JogBackward)
                                CurrentSpeed = -TargetSpeed;//反向驱动，当前速度为目标速度的负值
                        }
                    }
                    else//使用加速度
                    {
                        CurrentSpeed = CurrentSpeed + (float) _currentacceleration * Time.fixedDeltaTime;//计算当前速度 = 速度 + 加速度*时间
                        // Limit Speed to maximum 限制速度为最大值
                        if (CurrentSpeed > 0 && CurrentSpeed > TargetSpeed && _currentacceleration > 0)
                        {
                            _accelerationstarted = false;
                            _currentacceleration = 0;
                            CurrentSpeed = TargetSpeed;
                        }

                        if (CurrentSpeed < 0 && CurrentSpeed < -TargetSpeed && _currentacceleration < 0)
                        {
                            _decelerationstarted = false;
                            _currentacceleration = 0;
                            CurrentSpeed = -TargetSpeed;
                        }
                    }
                }


            // Drive at Target Position
            if (!IsStopped)
                if (!JogForward && !JogBackward && _stopjogging)
                {
                    if ((CurrentSpeed > 0 && _lastspeed < 0) || (CurrentSpeed < 0 && _lastspeed > 0))
                    {
                        Stop();
                        _currentacceleration = 0;
                        _stopjogging = false;
                        IsAtTarget = false;
                        _currentdestination = CurrentPosition;
                    }
                }

            // Set new Position
            SetPosition();//计算位置或旋转角度

            /* 使用限制 */
            if (UseLimits)
            {
                IsAtLowerLimit = false;
                IsAtUpperLimit = false;
                var currpos = CurrentPosition;
                if (_limitraycastnotnull)//LimitRayCast不为空
                    currpos = LimitRayCast.RayCastDistance;
                if (JogForward && currpos >= UpperLimit)//向前Jog驱动，且（当前位置大于最大点 或 光线检测距离大于最大限制值）
                {
                    if (!JumpToLowerLimitOnUpperLimit)//无需到达最大点时跳转回最小点
                    {
                        CurrentSpeed = 0;
                        CurrentPosition = UpperLimit;
                        IsAtUpperLimit = true;
                    }
                    else//需跳转
                    {
                        CurrentPosition = currpos - UpperLimit;//当前位置变为LowerLimit
                        if (OnJumpToLowerLimit != null)
                            OnJumpToLowerLimit.Invoke(this);//调用相关方法
                    }
                }

                if (JogBackward && currpos <= LowerLimit)//向后Jog驱动，且（当前位置小于最小点 或 光线检测距离小于于最小限制值）
                {
                    CurrentSpeed = 0;//速度=0，停止
                    CurrentPosition = LowerLimit;
                    IsAtLowerLimit = true;//置位
                }

                if (!JogForward && !JogBackward)
                {
                    if (!_limitraycastnotnull)//未使用LimitRayCast
                    {
                        // Normal Limits
                        if (currpos > UpperLimit)
                            CurrentPosition = UpperLimit;
                        if (currpos < LowerLimit)
                            CurrentPosition = LowerLimit;
                    }
                    else//使用LimitRayCast
                    {
                        //  With Raycast
                        var diff = 0.0f;
                        if (currpos > UpperLimit)
                            diff = UpperLimit - currpos;
                        if (currpos < LowerLimit)
                            diff = LowerLimit - currpos;
                        CurrentPosition = CurrentPosition - diff;
                    }
                }
            }


            //  Current Values / Status  依据当前速度判断是否在运行
            if (CurrentSpeed == 0)
            {
                IsRunning = false;
            }
            else
            {
                IsRunning = true;
            }

            //判断是否到达目标速度
            if (CurrentSpeed == TargetSpeed && TargetSpeed != 0)
                IsAtTargetSpeed = true;
            else
                IsAtTargetSpeed = false;
            //判断是否到达目标点
            if (CurrentPosition == _currentdestination)
            {
                IsAtTarget = true;
                if (_lastisattarget != IsAtTarget && OnAtPosition != null)
                    OnAtPosition(this);
            }
            else
            {
                IsAtTarget = false;
            }

            if (_transportsurfaces != null && (_lastspeed != CurrentSpeed))//存在运输面，且当前速度在变化
            {
                for (int i = 0; i < _transportsurfaces.Length; i++)
                {
                    _transportsurfaces[i].SetSpeed(CurrentSpeed * SpeedScaleTransportSurface);//设置输送面速度
                }
            }


            _laststartdrivetotarget = TargetStartMove;//记录上次TargetStartMove的值
            _lastspeed = CurrentSpeed;//记录上次速度
            _lastisattarget = IsAtTarget;//记录上次 是否到达目标
            _lastcurrentposition = CurrentPosition;//记录上次的位置
            _lastjog = JogBackward || JogForward;//记录上次的驱动状态
        }

        #endregion
    }
}