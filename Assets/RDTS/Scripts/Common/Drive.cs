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
    /// �����������ű�������DriveBehaviours��ͬʹ�ã�����Value�ź�������
    /// </summary>
    [SelectionBase]
    //! The drive is moving components including all sub components along the local axis of the game object.
    //! Rotational and linear movements are possible. A drive can be enhanced by DriveBehaviours which are adding special
    //! behaviours as well as Input and Output signals to drives.
    //�����Խ�����ת��ֱ���˶��� DriveBehaviours ������ǿ����������Ϊ���������������Ϊ�Լ����������źš�
    public class Drive : BaseDrive
    {
        #region PublicVariables

        [Header("Settings")] [OnValueChanged("CalculateVectors")]
        public DIRECTION
            Direction; //!< The direction in local coordinate system of the GameObject where the drive is attached to. ��������

        [OnValueChanged("CalculateVectors")]
        public bool ReverseDirection; //!< Set to *true* if Direction needs to be inverted. ����ת

        [Tooltip("Offeset for defining another Drive 0 position. Drive will start at simulation start at Offset.")]
        public float Offset; //!< Start offset of the drive from zero position in millimeters or degrees. ƫ����

        public float StartPosition; //!< Start Position off the Drive ��ʼλ��

        public float SpeedScaleTransportSurface = 1; //!< Scale of the Speed for radial transportsurfaces to feed in mm/s on radius  �����������ٶȱ������� mm/s Ϊ��λ�����뾶

        [Tooltip(
            "Should be normally turned off. If set to true the RigidBodies are moved. Use it if moving part has attached colliders. If false the transforms are moved")]
        public bool MoveThisRigidBody = false; //!< If set to true the RigidBodies are moved (use it if moving) part has attached colliders, if false the transforms are moved

        [NaughtyAttributes.ReadOnly] public bool EditorMoveMode = false;

        [ReorderableList] public List<TransportSurface> TransportSurfaces; //!< The transport surface the drive is controlling. Is null if drive is not controlling a transport surface.
                                                                           //���������ƵĴ�����档 ���������δ���ƴ�����棬��Ϊ�� 

        [BoxGroup("Limits")] public bool UseLimits;///ʹ������
        /* Linear�£���Gizmos����ʾLowerLimit��Ϊһ�������壬UpperLimit��Ϊ��ͷ
         * Rotation�£���Gizmos����ʾһ����Ӧ�Ƕȵ�Բ
         */
        [ShowIf("UseLimits")] [BoxGroup("Limits")]
        public float LowerLimit = 0; //! Lower Drive Limit, Upper and Lower = 0 if this should not be used

        [ShowIf("UseLimits")] [BoxGroup("Limits")]
        public float UpperLimit = 1000; //! Upper Drive Limit, Upper and Lower = 0 if this should not be used

        [ShowIf("UseLimits")] [BoxGroup("Limits")]
        public bool JumpToLowerLimitOnUpperLimit = false;//��Ϊtrue���ﵽUpperLimit������������LowerLimit��

        [ShowIf("UseLimits")]
        [BoxGroup("Limits")]
        [Tooltip("If assigned the Raycast measurment is the basis for the drive Limits")]//��������� Raycast ����ֵ���������������ƵĻ���
        public Sensor LimitRayCast;

        [Space(10)] [BoxGroup("Acceleration")]
        public bool UseAcceleration = false; //!< If set to true the drive uses the acceleration  �Ƿ�ʹ�ü��ٶ�

        [BoxGroup("Acceleration")] [ShowIf("UseAcceleration")]
        public bool SmoothAcceleration = false; //!< if set to true the drive uses smooth acceleration with a sinoide function
                                                //�������Ϊ true����������ʹ�ô������Һ�����ƽ������

        [ShowIf("UseAcceleration")] [BoxGroup("Acceleration")]
        public float Acceleration = 100; //!< The acceleration in millimeter per second^2    ���ٶ�


        [Header("Drive IO's")] public bool JogForward = false; //!< A jog bit for jogging forward with the Target Speed ������Ŀ���ٶ���ǰ����������λ
        public bool JogBackward = false; //!< A jog bit for jogging backwards with the Target Speed  �������
        public float TargetPosition; //!< The target position of the Drive  Ŀ��λ��
        public float TargetSpeed = 100; //!< The target speed in millimeter / second of the Drive   Ŀ���ٶ�

        public bool TargetStartMove = false; //!< When changing to true the Drive is starting to move to the TargetPosition       
                                             //������Ϊ true ʱ��Drive ��ʼ�ƶ��� TargetPosition

        [HideInInspector]
        public bool BlockDestination = true; //!< If Block Drive is true it will not drive to its Target Positon, Jogging is possible  ���Block DriveΪ�棬����������������Ŀ��λ�ã�Jogging�ǿ��ܵ�

        public bool ResetDrive = false; //!< Resets the Drive to the zero position and stops all movements  ���û���λ�ã���ֹͣ���е��˶�
        public bool _StopDrive = false; //!< Stops the Drive at the current position   �ڵ�ǰλ�ô�ֹͣ
        [ReadOnly] public float CurrentSpeed; //!< The current speed of the drive   ��ǰ�ٶ�
        [ReadOnly] public float CurrentPosition; //!< The current position of the drive   ��ǰλ��
        [ReadOnly] public bool IsStopped = false; //!< Is true if Drive is stopped   �Ƿ�ֹͣ
        [ReadOnly] public bool IsRunning = false; //!< Is true if Drive is running   �Ƿ��˶�
        [ReadOnly] public bool IsAtTargetSpeed = false; //!< Is true if Drive is at target speed    ����Ŀ���ٶ���Ϊtrue   
        [ReadOnly] public bool IsAtTarget = false; //!< Is true if Drive is at target position   ����Ŀ��λ����Ϊtrue
        [ReadOnly] public bool IsAtLowerLimit = false; //!< Is true if Drive is jogging and reaching lower Limit    JogBackwardΪtrue�ҵ���LowerLimit��
        [ReadOnly] public bool IsAtUpperLimit = false; //!< Is true if Drive is jogging and reaching upper Limit    JogForwardΪtrue�ҵ���UpperLimit��
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
        private float _currentdestination;//��ǰ��Ŀ���
        private float _timestartacceleration;
        private double _currentacceleration;//��ǰ�ļ��ٶ�
        private bool _laststartdrivetotarget;//��¼�ϴ�TargetStartMove��ֵ
        private bool _isdrivingtotarget = false;//�Ƿ���Ŀ�������
        private bool _drivetostarted = false;
        private float _lastcurrentposition;
        private bool _istransportsurface = false;//�Ƿ�����������
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
        private bool _lastjog;//��¼�ϴε�����״̬
        private bool _limitraycastnotnull;//LimitRayCast�Ƿ�Ϊ��

        private bool articulatedbodynotnull;
        private ArticulationBody articulatedbody;

        private bool _accelerationstarted = false;//�Ƿ�ʼ����
        private bool _decelerationstarted = false;//�Ƿ�ʼ����

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
        /// ��������������Ŀ���ٶ���ʻ��������Ŀ�꣬�ɸ���Drive Behaviour����
        /// </summary>
        /// <param name="Target"></param>
        public void DriveTo(float Target)
        {
            BlockDestination = false;
            TargetPosition = Target;//����Ŀ��λ��
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


        public void Decelerate()//����
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
        /// ����������λ������Ϊ����ֵ���Ժ��׻��Ϊ��λ����Drrive��λ��
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
        /// ����ȫ�֡��ֲ���λ�õķ���������
        /// �ֲ�scale��
        /// �Ƿ���
        /// </summary>
        public void CalculateVectors()
        {
            _localdirection = DirectionToVector(Direction);//Drive�ķ�����������λ������
            _globaldirection = transform.TransformDirection(_localdirection);//�任��ȫ������ϵ
            if (!ReferenceEquals(transform.parent, null))//Drive������ŵ�gameobject���ڸ�����
            {
                _positiondirection = transform.parent.transform.InverseTransformDirection(_globaldirection);//�и������ת��������ڸ�����ľֲ�����ϵ
            }
            else
            {
                _positiondirection = _globaldirection;//�޸��������ȫ������ϵ
            }

            //�ֲ�scale
            if (transform.parent != null)
                _localscale = GetLocalScale(transform.parent.transform, Direction);
            else
                _localscale = 1;

            _localstartpos = transform.localPosition;
            _localstartrot = transform.localEulerAngles;
            //�Ƿ�ת����
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
                IsRotation = true;//����ת�˶�
            }

#if GAME4AUTOMATION_INTERACT
            if (UseInteract && !Application.isPlaying)
                Game4AutomationPhysics.Kinematize(gameObject);
#endif
        }

        /// <summary>
        /// �����µ�λ�û���ת�Ƕ�
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
                if (!_istransportsurface)//�����������������
                {
                    if (!IsRotation)//��������
                    {
                        Vector3 localpos = _localstartpos +
                                           _positiondirection *
                                           ((CurrentPosition + Offset) / RDTSController.Scale) /
                                           _localscale;//����λ�ã�RDTSController.Scale���������ű������õı���ϵ����

                        if (MoveThisRigidBody)//���������塱
                        {
                            if (!ReferenceEquals(transform.parent, null))
                                _globalpos = transform.parent.TransformPoint(localpos);
                            else
                                _globalpos = localpos;
                            _rigidbody.MovePosition(_globalpos);
                        }
                        else//�������任��
                        {
                            transform.localPosition = localpos;
                        }
                    }
                    else//��ת����
                    {
                        Quaternion.Euler(_localstartrot + _localdirection * (CurrentPosition + Offset));
                        Quaternion localrot =
                            Quaternion.Euler(_localstartrot + _localdirection * (CurrentPosition + Offset));//������ת�Ƕ�
                        if (MoveThisRigidBody)//���������塱
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
                        else//�������任��
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
                /// Automatically create RigidBody if not there  û�и���������Զ����̸���
                _rigidbody = gameObject.GetComponent<Rigidbody>();
                if (_rigidbody == null)
                {
                    _rigidbody = gameObject.AddComponent<Rigidbody>();
                }

                _rigidbody.isKinematic = true;
                _rigidbody.useGravity = false;
            }

            // Automatically add Transportsurface if one is existent in this or any sub object and no other drive is in between
            // ����˶�����κ��Ӷ����д���Transportsurface�������м�û�����������������Զ����Transportsurface
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

            _rigidbody = gameObject.GetComponent<Rigidbody>();//��ȡ�������
            
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

            CalculateVectors();//��������ķ���������scale
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
                    _transportsurfaces[i].TransportDirection = _globaldirection;///�����������е����䷽��
                    _transportsurfaces[i].SetSpeed(CurrentSpeed * SpeedScaleTransportSurface);///������������ٶ�
                }
            }

            if (_transportsurfaces.Length > 0)//�������б�Ϊ����˵����������������
            {
                _istransportsurface = true;
            }

            CurrentPosition = StartPosition;//��ǰλ��Ϊ��ʼλ��
        }

        //��������״̬
        public void DriveReset()
        {
            CurrentPosition = Offset;
            CurrentSpeed = 0;
            IsRunning = false;
            BlockDestination = true;
        }

        private void FixedUpdate()
        {
            if (ResetDrive)//�ⲿ���ơ�����״̬
            {
                DriveReset();
            }

            if (_StopDrive)//ֹͣ����
            {
                Stop();
            }

            /* ֹͣ���� */
            // Jog stopped
            if (_lastjog && !JogBackward && !JogForward && !UseAcceleration)//Jog�����ź���trueת��Ϊfalse�Ҳ�ʹ�ü��ٶȣ�������ֹͣ
            {
                Stop();
            }

            if (_lastjog && !JogBackward && !JogForward && UseAcceleration)//Jog�����ź���trueת��Ϊfalse��ʹ�ü��ٶȣ���ʹ�ü��ٶ�����ֹͣ
            {
                _stopjogging = true;
                if (CurrentSpeed > 0)
                    _currentacceleration = -Acceleration;
                else
                    _currentacceleration = Acceleration;
            }

            // Drive Decellerated totally - stop drive  
            if (_decelerationstarted && CurrentSpeed < 0)//�ڼ��٣��ҵ�ǰ�ٶȼ���С��0����ֹͣ
            {
                Stop();
            }


            var newtarget = false;

            /* ���µ�Ŀ��� */
            // New Target Position
            if (_laststartdrivetotarget != TargetStartMove && TargetStartMove)//��ʼ��TargetPosition����
            {
                IsStopped = false;
                _stopjogging = false;
                BlockDestination = false;
                _currentdestination = TargetPosition;//���õ�ǰĿ���ΪTargetPosition
                _currentacceleration = Acceleration;//���õ�ǰ���ٶ�
                _isdrivingtotarget = true;//������Ŀ�������
                _timestartacceleration = Time.time;//��¼�˿̵�ʱ��
                IsAtTarget = false;//��λ
                //CurrentSpeed = 0;
                _StopDrive = false;
                if (_drivetostarted)//����Drive Behavior�ű�����ʱ
                {
                    TargetStartMove = false;
                    _drivetostarted = false;
                }

                newtarget = true;
            }

            // Calculate Position if Speed > 0  �ٶȴ���0ʱ���㵱ǰ��λ��
            if (!IsStopped)
                if (!ResetDrive && (CurrentSpeed != 0) && !_StopDrive)
                {
                    CurrentPosition = CurrentPosition +
                                      CurrentSpeed * RDTSController.SpeedOverride * Time.deltaTime;
                }


            // Need to slow down - negative acceleration
            if (_isdrivingtotarget && !_StopDrive && !ResetDrive && !JogBackward && !JogForward)//������Ŀ�������������δֹͣ��δ���á�Jog�����źž�Ϊfalse
            {
                if (UseAcceleration)//ʹ�ü��ٶ�����ֹͣ
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
                else//�޼��ٶ���ֱ��ֹͣ
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
                     !_stopjogging))//��ʼ���� ��ʼ���� ��δ����Ŀ��㡢����Ŀ���������ʹ���˼��ٶȡ�δֹͣ������δ����������Jog�����ź�Ϊfalse
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
                        else//�����ڿ�ʼ���ٻ�ʼ���ٵ�״̬�������õ�ǰ�ļ��ٶ�
                        {
                            if (_accelerationstarted)
                                _currentacceleration = Acceleration;
                            if (_decelerationstarted)
                                _currentacceleration = -Acceleration;
                        }
                    }
                    else
                    {
                        // Sinoide Calculation  ���Ҽ���
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

            /* ��JogBackward��JogForward������ʱ��������ٶ� */
            // Calculate Acceleration if Jogging   
            if (!IsStopped)
                if ((JogBackward || JogForward) && UseAcceleration)//Jog�����ź���һ��Ϊtrue����ʹ�ü��ٶ�
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

            /* ����Ŀ��� */
            // Drive at Target Position
            if (!IsStopped)
                if (!JogForward && !JogBackward && !newtarget)
                {
                    if ((_isdrivingtotarget && CurrentSpeed > 0 && CurrentPosition >= _currentdestination &&
                         _lastcurrentposition < _currentdestination) ||
                        (_isdrivingtotarget && CurrentSpeed < 0 && CurrentPosition <= _currentdestination &&
                         _lastcurrentposition > _currentdestination))
                    {
                        Stop();//ֹͣ
                        BlockDestination = true;
                        CurrentPosition = _currentdestination;//��ǰλ�ü�Ϊ��ǰ��Ŀ���
                        IsAtTarget = true;//����Ŀ�����λ
                        _isdrivingtotarget = false;//�ѵ���Ŀ��������ǡ���Ŀ���������״̬
                        _stopjogging = false;
                    }
                }

            /* ���㵱ǰ�ٶ� */
            // Calculate Speed
            if (!IsStopped)
                if (!ResetDrive && !_StopDrive && (!IsAtTarget || JogBackward || JogForward))//δ��������״̬��δֹͣ��������δ����Ŀ��� ��Jog�����ź���һ��Ϊtrue��
                {
                    if (!UseAcceleration)//δʹ�ü��ٶ�
                    {
                        if (!JogForward && !JogBackward && !BlockDestination)
                        {
                            if (CurrentPosition < _currentdestination)
                                CurrentSpeed = TargetSpeed;//��ǰλ��С�ڵ�ǰĿ��㣬��ǰ�ٶ�ΪĿ���ٶ�
                            if (CurrentPosition > _currentdestination)
                                CurrentSpeed = -TargetSpeed;//��ǰλ�ô��ڵ�ǰĿ��㣬��ǰ�ٶ�ΪĿ���ٶȵĸ�ֵ
                        }
                        else
                        {
                            if (JogForward)
                                CurrentSpeed = TargetSpeed;//������������ǰ�ٶȼ�ΪĿ���ٶ�
                            if (JogBackward)
                                CurrentSpeed = -TargetSpeed;//������������ǰ�ٶ�ΪĿ���ٶȵĸ�ֵ
                        }
                    }
                    else//ʹ�ü��ٶ�
                    {
                        CurrentSpeed = CurrentSpeed + (float) _currentacceleration * Time.fixedDeltaTime;//���㵱ǰ�ٶ� = �ٶ� + ���ٶ�*ʱ��
                        // Limit Speed to maximum �����ٶ�Ϊ���ֵ
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
            SetPosition();//����λ�û���ת�Ƕ�

            /* ʹ������ */
            if (UseLimits)
            {
                IsAtLowerLimit = false;
                IsAtUpperLimit = false;
                var currpos = CurrentPosition;
                if (_limitraycastnotnull)//LimitRayCast��Ϊ��
                    currpos = LimitRayCast.RayCastDistance;
                if (JogForward && currpos >= UpperLimit)//��ǰJog�������ң���ǰλ�ô������� �� ���߼���������������ֵ��
                {
                    if (!JumpToLowerLimitOnUpperLimit)//���赽������ʱ��ת����С��
                    {
                        CurrentSpeed = 0;
                        CurrentPosition = UpperLimit;
                        IsAtUpperLimit = true;
                    }
                    else//����ת
                    {
                        CurrentPosition = currpos - UpperLimit;//��ǰλ�ñ�ΪLowerLimit
                        if (OnJumpToLowerLimit != null)
                            OnJumpToLowerLimit.Invoke(this);//������ط���
                    }
                }

                if (JogBackward && currpos <= LowerLimit)//���Jog�������ң���ǰλ��С����С�� �� ���߼�����С������С����ֵ��
                {
                    CurrentSpeed = 0;//�ٶ�=0��ֹͣ
                    CurrentPosition = LowerLimit;
                    IsAtLowerLimit = true;//��λ
                }

                if (!JogForward && !JogBackward)
                {
                    if (!_limitraycastnotnull)//δʹ��LimitRayCast
                    {
                        // Normal Limits
                        if (currpos > UpperLimit)
                            CurrentPosition = UpperLimit;
                        if (currpos < LowerLimit)
                            CurrentPosition = LowerLimit;
                    }
                    else//ʹ��LimitRayCast
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


            //  Current Values / Status  ���ݵ�ǰ�ٶ��ж��Ƿ�������
            if (CurrentSpeed == 0)
            {
                IsRunning = false;
            }
            else
            {
                IsRunning = true;
            }

            //�ж��Ƿ񵽴�Ŀ���ٶ�
            if (CurrentSpeed == TargetSpeed && TargetSpeed != 0)
                IsAtTargetSpeed = true;
            else
                IsAtTargetSpeed = false;
            //�ж��Ƿ񵽴�Ŀ���
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

            if (_transportsurfaces != null && (_lastspeed != CurrentSpeed))//���������棬�ҵ�ǰ�ٶ��ڱ仯
            {
                for (int i = 0; i < _transportsurfaces.Length; i++)
                {
                    _transportsurfaces[i].SetSpeed(CurrentSpeed * SpeedScaleTransportSurface);//�����������ٶ�
                }
            }


            _laststartdrivetotarget = TargetStartMove;//��¼�ϴ�TargetStartMove��ֵ
            _lastspeed = CurrentSpeed;//��¼�ϴ��ٶ�
            _lastisattarget = IsAtTarget;//��¼�ϴ� �Ƿ񵽴�Ŀ��
            _lastcurrentposition = CurrentPosition;//��¼�ϴε�λ��
            _lastjog = JogBackward || JogForward;//��¼�ϴε�����״̬
        }

        #endregion
    }
}