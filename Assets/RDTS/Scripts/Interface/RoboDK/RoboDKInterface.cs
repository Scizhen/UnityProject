///********************************************************************************************
///this is part of Robot Digital Twin System.                                                 *
///@Copyright by Shaw.S                                                                       *
///Thanks for the code reference game4automation provides.                                    *
///Do not distribute without authorization,thanks!                                            *
///********************************************************************************************
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NaughtyAttributes;
using UnityEditor;
using System.Threading;

namespace RDTS.Interface
{

    //! This is the first version of the RoboDK integration. Still in Beta and some interactions from Unity side
    //! like showing Targets, following in Editmode... are missing.
    public class RoboDKInterface : InterfaceThreadedBaseClass
    {
        public string RoboDKApplication = "C:/RoboDK/bin/RoboDK.exe";//RoboDK�����ִ�г����·��������RoboDK��·����
        public string RobotDKFile;//RoboDK�����·�������Խ������ļ�����unity�����У����߷��ڱ���
        public string RobotName;//Ҫ��RoboDK�ж�ȡ�Ļ����˵�����
        public string RobotDKIP = "localhost";
        public bool OpenRDKOnStart = true;//�Ƿ���Unity���п�ʼʱ��RoboDK

        [OnValueChanged("SyncInEditModeChanged")]
        /* public bool SyncInEditMode = true; */
        [Range(0, 10)]
        public float RDKSimulationSpeed = 1f;//RoboDK���������ٶ�
<<<<<<< HEAD
=======
        public float RobotSpeed = 1f;//RoboDK���������ٶ�
>>>>>>> origin/master

        public string RobotProgramm;//Ҫִ�е�RoboDK���򣨴˻�е��ִ����һ��program��
        public bool RunProgrammOnStart;//�Ƿ���Unity���п�ʼʱ����RobotProgramm����
        public bool LoopProgramm;//�Ƿ�ѭ������RoboDK����
        [NaughtyAttributes.ReadOnly] public bool ProgrammIsRunning;//RoboDK�����Ƿ���������

        public bool StopProgrammOnUnityStop;//�Ƿ�unityֹͣ����ʱֹͣRoboDK��������

        /* public string TeachPositionName = "Position1";
        public GameObject Target;
        public bool FollowTarget = false; */
        public GameObject TargetPrefab;//Ŀ����Ԥ����
        [HideInInspector] public GameObject DebugEndEffector;
        private float _oldspeed = 1f;
        [HideInInspector] public Vector3 EffectorPos;
        [HideInInspector] public Vector3 EffectorRot;
        [HideInInspector] public Vector3 OrigRot;
        [ReorderableList] public List<Drive> Axis;
        public float lerp = 0.2f;//��е������ʱ�Ĳ�ֵ�ı���ϵ��������1ʱ�൱��ֱ�ӽ�RoboDK�е���ֵ��ֵ��Drive
        [NaughtyAttributes.ReadOnly] public List<float> CurrentAxisPosition;//��ʾ��е�۸����ؽ������ֵ��CurrentPosition��

        private RoboDK.Item Robot;
        private RoboDK RDK;
        private List<Value> Signals;
        private Vector3 targetpos;

        private bool RDKIsConnected = false;//�Ƿ����ӵ�RoboDK

        //private Quaternion targetrot;
        //private Vector3 oldtargetpos;
        //private Quaternion oldtargetrot;
        private string _startprogramm = "";
        private RoboDK.Item runningprogramm;
        private bool _stopprogramm = false;
        private bool _movetotarget = false;
        // private Mat target;
        // private bool _synineditmodebeforestart = false;


        /// <summary>
        /// ��ȡRoboDK�е�Ŀ���
        /// </summary>
        [Button("Get Targets")]
        public void GetTargets()
        {
            if (RDK == null)
                Connect();
            var Targets = RDK.getItemList(RoboDK.ITEM_TYPE_TARGET);
            var rdktargets = GetComponentsInChildren<RoboDKTarget>();
            foreach (var target in rdktargets.ToArray())
            {
                DestroyImmediate(target.gameObject);
            }

            foreach (var target in Targets)
            {
                var pose = target.Pose();
                var rot = pose.ToTxyzRxyz();
                Quaternion quatrdk = Quaternion.Euler((float)rot[3], (float)rot[4], (float)rot[5]);
                Vector3 EulerRDK = new Vector3((float)rot[3], (float)rot[4], (float)rot[5]);
                Quaternion quatunity = GetRotationQuaternion(pose);
                Vector3 EulerUnity2 = quatunity.eulerAngles;
                var newtarget = Instantiate(TargetPrefab, gameObject.transform);
                newtarget.name = target.Name();

                newtarget.transform.localPosition = RDKToUnityPos(pose);
                newtarget.transform.rotation = quatunity;
                Debug.Log(target.Name() + "/" + target.Pose());
            }
        }

        private Vector3 RDKToUnityPos(Mat mat)
        {
            var pos = mat.Pos();
            float x = -(float)pos[0];
            float y = (float)pos[1];
            float z = (float)pos[2];
            float ux = x / 1000;
            float uy = z / 1000;
            float uz = -y / 1000;
            return new Vector3(ux, uy, uz);
        }

        private Quaternion RDKToUnityRot(Mat mat)
        {
            var rot = mat.ToXYZRPW();
            var rx = (float)rot[3];
            var ry = (float)rot[4];
            var rz = (float)rot[5];

            return Quaternion.Euler(rx, rz, ry);
        }
        //x=-x
        // y=z
        // z=-y


        public Quaternion GetRotationQuaternion(Mat mat)
        {
            var angle = GetAngles(mat);
            Quaternion qx = Quaternion.AngleAxis(angle.x, new Vector3(1, 0, 0));
            Quaternion qy = Quaternion.AngleAxis(-angle.z, new Vector3(0, 1, 0));
            Quaternion qz = Quaternion.AngleAxis(angle.y, new Vector3(0, 0, 1));
            Quaternion unityRotationQuaternion = qy * qx * qz; // exact order!
            return unityRotationQuaternion;
        }

        public static Vector3 GetAngles(Mat source)
        {
            double thetaX, thetaY, thetaZ = 0.0;
            thetaX = System.Math.Asin(source[2, 1]);
            var pi = System.Math.PI;
            if (thetaX < (pi / 2))
            {
                if (thetaX > (-pi / 2))
                {
                    thetaZ = System.Math.Atan2(-source[0, 1], source[1, 1]);
                    thetaY = System.Math.Atan2(-source[2, 0], source[2, 2]);
                }
                else
                {
                    thetaZ = -System.Math.Atan2(-source[0, 2], source[0, 0]);
                    thetaY = 0;
                }
            }
            else
            {
                thetaZ = System.Math.Atan2(source[0, 2], source[0, 0]);
                thetaY = 0;
            }

            float gradx = (float)((180 / pi) * thetaX);
            float grady = (float)((180 / pi) * thetaY);
            float gradz = (float)((180 / pi) * thetaZ);
            return new Vector3(gradx, grady, gradz);
        }

        // Start is called before the first frame update
        public Matrix4x4 GetTRSFlipped(Mat rdkmat)
        {
            Matrix4x4 mat = new Matrix4x4(
                new Vector4((float)rdkmat[0, 0], (float)rdkmat[1, 0], (float)rdkmat[2, 0], (float)rdkmat[3, 0]),
                new Vector4((float)rdkmat[0, 1], (float)rdkmat[1, 1], (float)rdkmat[2, 1], (float)rdkmat[3, 1]),
                new Vector4((float)rdkmat[0, 2], (float)rdkmat[1, 2], (float)rdkmat[2, 2], (float)rdkmat[3, 2]),
                new Vector4(0, 0, 0, 1));

            Matrix4x4 invert = new Matrix4x4(new Vector4(-1, 0, 0, 0), new Vector4(0, 1, 0, 0), new Vector4(0, 0, 1, 0),
                new Vector4(0, 0, 0, 1));
            var convert = invert * mat * invert;
            return convert;
        }

        public Vector3 GetPosition(Mat rdkmat)
        {
            var mat = GetTRSFlipped(rdkmat);
            Vector3 vect = new Vector3(mat.m30, mat.m31, mat.m32);
            return vect;
        }

        /// <summary>
        /// ����RoboDK
        /// </summary>
        void Connect()
        {
            var path = RobotDKFile;
            if (!Path.IsPathRooted(path))
            {
                path = Path.Combine(Application.dataPath, path);//��ȡRoboDK����·��
            }

            if (RDK == null)
                RDK = new RoboDK(RobotDKIP, !OpenRDKOnStart, -1, RobotDKFile, RoboDKApplication);//����RoboDK
            if (OpenRDKOnStart)
                RDK.ShowRoboDK();//��RoboDK
            RDK.Finish();
        }

        /// <summary>
        /// ��ʼ����
        /// </summary>
        void StartSimulation()
        {
            Connect();
            // Set to simulation mode:
            RDK.setRunMode(RoboDK.RUNMODE_SIMULATE);//���óɷ���ģʽ
            Robot = RDK.getItem(RobotName, RoboDK.ITEM_TYPE_ROBOT);//��������Ļ��������ƻ�ȡRoboDK�����ļ��ж�Ӧ�Ļ�����
        }

        /// <summary>
        /// ѡ��RoboDK�����ļ�
        /// </summary>
        [Button("Select RoboDK File")]
        void SelectFile()
        {
#if UNITY_EDITOR
            var oldFIlePath = RobotDKFile;
            RobotDKFile = EditorUtility.OpenFilePanel("Select RoboDK File", RobotDKFile, "rdk");
            if (RobotDKFile == "" || RobotDKFile == null)
                RobotDKFile = oldFIlePath;
#endif
        }

        /// <summary>
        /// ��RoboDK�����ļ�
        /// </summary>
        [Button("Open RoboDK File")]
        void OpenRDKFile()
        {
            var path = RobotDKFile;
            if (!Path.IsPathRooted(path))
            {
                path = Path.Combine(Application.dataPath, path);
            }

            RDK = new RoboDK(RobotDKIP, !OpenRDKOnStart, -1, RobotDKFile, RoboDKApplication);
            RDK.Disconnect();
        }

        /// <summary>
        /// ��RoboDK�е���IO����
        /// </summary>
        [Button("Import Robot IOs")]
        public void GetParams()
        {
            if (!Application.isPlaying)
                Connect();

            var parameters = RDK.getParams();

            foreach (var param in parameters)
            {
                var name = param[0];
                var value = param[1];
                var mysignal = GetValueObject(name);
                if (mysignal == null)
                {
                    InterfaceValue signal = new InterfaceValue();
                    signal.Name = name;
                    signal.SymbolName = name;
                    signal.Type = InterfaceValue.TYPE.INT;
                    signal.Direction = InterfaceValue.DIRECTION.OUTPUT;
                    var tmp = (AddValueObject(signal)).gameObject;
                    mysignal = tmp;
                }

                mysignal.GetComponent<Value>().SetValue(value);
                ;
            }
        }


        private Dictionary<string, string> SignalValue = new Dictionary<string, string>();//�洢signal��һ��ֵ
        /// <summary>
        /// ��ʼ���źţ���ȡ�Ӽ�ֵ���󣬲������źų�ֵ
        /// </summary>
        public void InitSignals()
        {
            Value[] signals = GetComponentsInChildren<Value>();//��ȡ�Ӽ���ֵ����
            DeleteValues();
            foreach (Value signal in signals)
            {
                InterfaceValue interfacesignal = signal.GetInterfaceValue();

                AddValueObject(interfacesignal);

                SignalValue.Add(interfacesignal.Name, interfacesignal.Value.GetValue().ToString());//�����ֵ�
            }

            foreach (var signal in InterfaceValues)
            {


                if (signal.Direction == InterfaceValue.DIRECTION.OUTPUT)
                {
                    var value = RDK.getParam(signal.Name);
                    signal.Value.SetValue(value);
                    SignalValue[signal.Name] = value;

                }

                if (signal.Direction == InterfaceValue.DIRECTION.INPUT)
                {
                    string signalvalue = signal.Value.GetValue().ToString();
                    RDK.setParam(signal.Name, signalvalue);
                    SignalValue[signal.Name] = signalvalue;

                }
            }

            Debug.Log("InterfaceSignals count: " + InterfaceValues.Count);
        }

        /* 
            private void SyncInEditModeChanged()
            {
                foreach (var axis in Axis)
                {
                    if (SyncInEditMode)
                        axis.StartEditorMoveMode();
                    else
                        axis.EndEditorMoveMode();
                }

                if (SyncInEditMode)
                {
                    EditorApplication.update += EditorUpdate;
                    EditorSceneManager.sceneSaving += OnSceneSave;
                }
                else
                {

                    EditorApplication.update -= EditorUpdate;
                    EditorSceneManager.sceneSaving -= OnSceneSave;
                }
            }

            private void OnSceneSave(Scene scene, string path)
            {
                OnDestroy();
            }


            private void OnDestroy()
            {
                if (Application.isPlaying)
                {
                    SyncInEditMode = _synineditmodebeforestart;
                    if (SyncInEditMode)
                    {
                        SyncInEditModeChanged();
                    }
                }

            }
        */


        /// <summary>
        /// �����źţ������ź����ͽ��н��������ź�ֵ�ı�Ž��н������ɼ��ټ�����Դ
        /// </summary>
        void UpdateSignals()
        {
            foreach (var signal in InterfaceValues)
            {
                //Output�����źţ���RoboDK��ȡ��Unity��
                if (signal.Direction == InterfaceValue.DIRECTION.OUTPUT)
                {
                    var value = RDK.getParam(signal.Name);
                    if (SignalValue[signal.Name] != value)//���ź�ֵ�����ı�ʱ�ٶ��źŽ��н���
                    {
                        signal.Value.SetValue(value);
                        SignalValue[signal.Name] = value;
                    }

                }
                //Input�����źţ���Unity���䵽RoboDK��
                if (signal.Direction == InterfaceValue.DIRECTION.INPUT)
                {
                    string signalvalue = signal.Value.GetValue().ToString();
                    if (SignalValue[signal.Name] != signalvalue)//���ź�ֵ�����ı�ʱ�ٶ��źŽ��н���
                    {
                        RDK.setParam(signal.Name, signalvalue);
                        SignalValue[signal.Name] = signalvalue;
                    }

                }
            }
        }


        //void UpdateSignals()
        //{
        //    foreach (var signal in InterfaceSignals)
        //    {
        //        if (signal.Direction == InterfaceValue.DIRECTION.OUTPUT)
        //        {
        //            var value = RDK.getParam(signal.Name);
        //            signal.Signal.SetValue(value);
        //        }

        //        if (signal.Direction == InterfaceValue.DIRECTION.INPUT)
        //        {
        //            string signalvalue = signal.Signal.GetValue().ToString();
        //            RDK.setParam(signal.Name, signalvalue);
        //        }
        //    }
        //}




        void Start()
        {
            /* _synineditmodebeforestart = SyncInEditMode;
            SyncInEditMode = false;
            SyncInEditModeChanged(); */
            if (Application.isPlaying)
            {


                /* if (Target != null)
                {
                    targetpos = Target.transform.localPosition;
                    targetrot = Target.transform.localRotation;
                }
                else
                {
                    targetpos = Vector3.zero;
                    targetrot = Quaternion.identity;
                }*/

                _oldspeed = RDKSimulationSpeed;
                StartSimulation();//��ʼ����
                InitAxis();//��ʼ����е�۹ؽ�
                SetSpeed();//�����ٶ�
                InitSignals();//��ʼ���ź�
                if (RobotProgramm != "" && RunProgrammOnStart)
                    StartProgramm();

              
            }
            else
            {
                /* if (SyncInEditMode)
                   SyncInEditModeChanged(); */
            }
        }


        private void EditorUpdate()
        {
            if (!Application.isPlaying)
            // if (!Application.isPlaying && SyncInEditMode)
            {
                if (RDK == null)
                {
                    StartSimulation();
                    InitAxis();
                }

                UpdateAxisPositionEditMode();
            }
        }

        /// <summary>
        /// ��ʼ�������˹ؽ���
        /// </summary>
        void InitAxis()
        {
            CurrentAxisPosition = new List<float>();
            RoboDK.Item[] robots = new[] { Robot };//��ȡRobot
            //��ȡ�����˹ؽ�����
            var joints = RDK.Joints(robots);
            for (int i = 0; i < joints[0].Length; i++)
            {
                CurrentAxisPosition.Add((float)joints[0][i]);
            }
        }


        /// <summary>
        /// ���¹ؽ������ֵ
        /// </summary>
        void UpdateAxisPosition()
        {
            if (RDK == null || Robot == null)
                return;
            RoboDK.Item[] robots = new[] { Robot };
            var joints = RDK.Joints(robots);
            for (int i = 0; i < joints[0].Length; i++)
            {
                CurrentAxisPosition[i] = (float)joints[0][i];
                //Axis[i].CurrentPosition = CurrentAxisPosition[i];
                if (lerp > 1) lerp = 1;
                Axis[i].CurrentPosition = Mathf.Lerp(Axis[i].CurrentPosition, CurrentAxisPosition[i], lerp * Mathf.Abs(CurrentAxisPosition[i] - Axis[i].CurrentPosition));//��ֵ��ʽ��ֵ��lerpֵԽС����е���˶�Խƽ�������ӳٻ�����
            }
        }

        void UpdateAxisPositionEditMode()
        {
            /*  if (SyncInEditMode)
              {
                  RoboDK.Item[] robots = new[] {Robot};
                  var joints = RDK.Joints(robots);
                  for (int i = 0; i < joints[0].Length; i++)
                  {
                      CurrentAxisPosition[i] = (float) joints[0][i];
                      if (Axis[i]!=null)
                         Axis[i].SetPositionEditorMoveMode(CurrentAxisPosition[i]);
                  }
              } */
        }


        private void StartProg(string programm)
        {
            if (RDK == null)
                Connect();
            if (RDK.Connected() == false)
            {
                Connect();
            }

            var prog = RDK.getItem(programm, RoboDK.ITEM_TYPE_PROGRAM);
            prog.RunProgram();
            runningprogramm = prog;
        }

        [Button("Start Programm")]
        public void StartProgramm()//��ʼ����һ��program
        {

            _startprogramm = RobotProgramm;
            if (!Application.isPlaying)
                StartProg(_startprogramm);

        }

        [Button("Stop Programm")]
        public void StopProgramm()
        {
            LoopProgramm = false;
            _stopprogramm = true;
            if (!Application.isPlaying)
                StopProg();

        }

        private void StopProg()
        {
            if (RDK == null)
                Connect();
            if (RDK.Connected() == false)
            {
                Connect();
            }

            runningprogramm.Stop();
            _stopprogramm = false;
        }

        public void StartProgramm(string programm)
        {
            _startprogramm = programm;

        }


        public void SetSpeed()
        {
            RDK.setSimulationSpeed(RDKSimulationSpeed);
<<<<<<< HEAD
            //Robot.setSpeed(400, 450, 400, 450);
            //Robot.setSpeed(800, 1000, 800, 1000);
=======
            //Robot.setSpeed(RDKSimulationSpeed*400, RDKSimulationSpeed * 450, RDKSimulationSpeed * 400, RDKSimulationSpeed * 450);
            Robot.setSpeed(RobotSpeed, RobotSpeed, RobotSpeed, RobotSpeed);
>>>>>>> origin/master
        }

        public void SetRobotSpeed(double speed_linear, double accel_linear, double speed_joints, double accel_joints)
        {
            Robot.setSpeed(speed_linear, accel_linear, speed_joints, accel_joints);
        }

        /* [Button("Teach current Position")]
         public void TeachCurrentPosition()
         {
          
             if (!Application.isPlaying)
             {
     #if UNITY_EDITOR
                 EditorUtility.DisplayDialog("Error",
                     "You need to be in Play mode to teach a position", "OK");
     #endif
                 return;
             }
             RDK.AddTarget(TeachPositionName);
         } */

        /*  [Button("Move to Target")]
          public void MoveToTarget()
          {
      
              if (!Application.isPlaying)
              {
      #if UNITY_EDITOR
                  EditorUtility.DisplayDialog("Error",
                      "You need to be in Play mode to move the robot to a Unity target", "OK");
      #endif
                  return;
              }
              var trans = new Vector3(targetpos.x * 1000, targetpos.y * 1000, targetpos.z * 1000);
              var rot = targetrot.eulerAngles;
              target = Mat.FromTxyzRxyz(-trans.x, -trans.z, trans.y, rot.x, rot.z, rot.y);
              _movetotarget = true;
              oldtargetpos = targetpos;
              oldtargetrot = targetrot;
          }
      */

        /// <summary>
        /// �߳�����ʱ����
        /// </summary>
        protected override void CommunicationThreadUpdate()
        {
            if (!RDKIsConnected)
                return;

            lock (InterfaceValues)
            {
                if (_oldspeed != RDKSimulationSpeed)
                {
                    _oldspeed = RDKSimulationSpeed;
                    SetSpeed();
                }

                UpdateAxisPosition();//���¹ؽ�������
                UpdateSignals();//�����ź�����
            }

            if (_startprogramm != "")
            {
                StartProg(_startprogramm);
                _startprogramm = "";
            }

            if (_stopprogramm)
            {
                StopProg();
                _stopprogramm = false;
            }

            if (_movetotarget)
            {
                //Robot.setPose(target);
                _movetotarget = false;
            }

            if (runningprogramm != null)
            {
                ProgrammIsRunning = true;
                if (!runningprogramm.Busy())
                {
                    if (LoopProgramm)
                    {
                        StartProg(RobotProgramm);
                    }
                    else
                    {
                        runningprogramm = null;
                        ProgrammIsRunning = false;
                    }
                }
            }
        }

        private void OnApplicationQuit()
        {
            CloseInterface();
        }


        /// <summary>
        /// �̹߳ر�ʱ����
        /// </summary>
        protected override void CommunicationThreadClose()
        {
            if (StopProgrammOnUnityStop && RobotProgramm != "")
            {
                if (RDK != null)
                {
                    var prog = RDK.getItem(RobotProgramm, RoboDK.ITEM_TYPE_PROGRAM);
                    prog.Stop();
                }
            }

            RDKIsConnected = false;
            if (RDK != null)
                RDK.Disconnect();
        }

        // Update is called once per frame
        void Update()
        {
            //�����RoboDK������״̬����δ�������������
            if (!RDKIsConnected)
                if (RDK != null)
                    if (RDK.Connected())
                        RDKIsConnected = true;

            if (RDKIsConnected)
                IsConnected = true;
            else
                IsConnected = false;


        }




        #region �߳�

        Thread RoboDKThread;//�߳�
        bool runThread = false;


        /// <summary>
        /// �����߳�
        /// </summary>
        void OpenThread()
        {
            //�̴߳���д��1
            RoboDKThread = new Thread(() => RoboDKThreadUpdate());//Lambda���ʽ

            //�̴߳���д��2
            //ThreadStart threadDelegate = new ThreadStart(PLCSiganlThreadUpdate);
            //Thread SignalThread = new Thread(threadDelegate);

            runThread = true;
            RoboDKThread?.Start();
            Debug.Log("�����̣߳�");
        }

        /// <summary>
        /// �ر��߳�
        /// </summary>
        void CloseThread()
        {
            runThread = false;
            RoboDKThread?.Abort("Abort Thread Nomally!");
            Debug.Log("��ֹ�̣߳�");
        }


        /// <summary>
        /// ���߳��и����ź�
        /// </summary>
        protected void RoboDKThreadUpdate()
        {
            if (!RDKIsConnected)
                return;

            try
            {
                do
                {

                    //  UpdateAxisPosition();


                } while (runThread);
            }
            catch (ThreadAbortException abortException)
            {
                Debug.Log($"Exception message: {abortException.Message}");
            }



        }





        #endregion


    }
}