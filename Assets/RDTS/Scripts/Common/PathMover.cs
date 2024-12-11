using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

/// <summary>
/// AGV系统中负责控制移动对象、与Drive脚本联动的脚本
/// </summary>

namespace RDTS
{
    [SelectionBase]
    [RequireComponent(typeof(MU))]

    //! An element which is transported by a Path System with connected Drives 由连接驱动器的路径系统传送的一种元件
    public class PathMover : RDTSBehavior, ISelectNextPath, ISourceCreated
    {
        public SimulationPath CreateOnPath;
        [Foldout(("Settings"))] public bool LeavePath = true;
        [Foldout(("Settings"))] public bool AlignWithPath = true; //!< true if the chainelement needs to align with the chain tangent while moving 如果chainelement在移动时需要与链切线对齐，则为True

        [Foldout(("Settings"))] public float Distance = 0.5f;
        [Foldout(("Settings"))] public float DistanceSides = 0.5f;
        [Foldout(("Settings"))] public float AngleSide = 30f;
        [Foldout(("Settings"))] public bool DrawRay = true;

        [Foldout(("Settings"))]
        [ShowIf("AlignWithPath")]
        public Vector3 AlignVector = new Vector3(0, 1, 0); //!< additinal rotation for the alignment 附加旋转对准


        [Foldout(("Status"))]
        [NaughtyAttributes.ReadOnly]
        public BaseStation CurrentStation;//!< current station the path mover is in 当前位置的路径搬运工
        [Foldout(("Status"))]
        [NaughtyAttributes.ReadOnly]
        public BaseStation CurrentStationWorking;//!< contains a value, when the current station starts working on the path mover 包含一个值，当当前站开始对路径移动器工作时

        [Foldout(("Status"))]
        [NaughtyAttributes.ReadOnly]
        public bool IsStopping;//!< true when the path mover slow down to stop 当路径移动器减速到停止时为True

        [Foldout(("Status"))]
        [NaughtyAttributes.ReadOnly]
        public bool IsStarting;//!< true when the path mover accelerate to target speed 当路径移动器加速到目标速度时为True

        [Foldout(("Status"))]
        [NaughtyAttributes.ReadOnly]
        public bool IsStopped;//!< true, if the path mover fully stopped 如果路径移动器完全停止，则为True

        [Foldout(("Status"))]
        [NaughtyAttributes.ReadOnly]
        public bool IsBlocked;//!< true, if the path mover is blocked 如果路径移动器被阻塞，则为True

        [Foldout(("Status"))]
        [NaughtyAttributes.ReadOnly]
        public bool IsOnPathEnd;//!< true, if the path mover freach the end of a path 如果路径移动器到达路径的末端，则为True

        [Foldout(("Status"))]
        [NaughtyAttributes.ReadOnly]
        public SimulationPath Path; //!< Drive where the chain is connected to 驱动链条连接的地方

        [Foldout(("Status"))]
        [NaughtyAttributes.ReadOnly]
        public float Position; //!< Current position of this chain element 该链条元件的当前位置

        [Foldout(("Status"))]
        [ReorderableList]
        public List<SimulationPath> NextPathes;//! < contains the following path of the path mover 包含路径传送器的以下路径


        [Foldout(("Path Events"))] public SimulationPathEvent OnPathEntered;//!< Event when the path mover enter a path
        [Foldout(("Path Events"))] public SimulationPathEvent OnPathEnd;//!< Event when the path mover reach the end a path
        [Foldout(("Path Events"))] public SimulationPathEvent OnPathExit;//!< Event when the path mover leaves a path
        [Foldout(("Path Events"))] public SimulationPathEvent OnStopping;//!< Event when the path mover start slowing down to stop
        [Foldout(("Path Events"))] public SimulationPathEvent OnStopped;//!< Event when the path mover fully stops
        [Foldout(("Path Events"))] public SimulationPathEvent OnStart;//!< Event when the path mover start accelerate
        [Foldout(("Path Events"))] public SimulationPathEvent OnFullyStartd;//!< Event when the path mover reach target speed
        [Foldout(("Path Events"))] public SimulationPathEvent OnBlocked;//!< Event when the path mover is blocked

        [Foldout(("Station Events"))] public SimulationStationEvent OnStationEntered;//!< Event when the path mover enter a station
        [Foldout(("Station Events"))] public SimulationStationEvent OnStationWorkStarting;//!< Event when the path mover start working within a station
        [Foldout(("Station Events"))] public SimulationStationEvent OnStationWorkFinished;//!< Event when the path mover finished working within a station
        [Foldout(("Station Events"))] public SimulationStationEvent OnStationExit;//!< Event when the path mover leave a station

#if GAME4AUTOMATION_DEBUG
        [Foldout(("Debug"))] public int DebugMUID;
        [Foldout(("Debug"))] public SimulationPath DebugPath;
        [Foldout(("Debug"))] public bool DebugBreak = false;
        [Foldout(("Debug"))] public float DebugTime;
#endif

        [HideInInspector] public MU Mu;//!< Loaded MU
        [HideInInspector] public float EnteringStationDistance = 1000000000f;//!< Distance between pre- and main stopper of the current station 当前站的前挡板和主挡板之间的距离

        private Vector3 _targetpos;
        private Rigidbody _rigidbody;
        private Quaternion targetrotation;
        private Vector3 tangentforward;
        private RDTSController game4Automation;

        private bool pathnotnull = false;
        private Vector3 raydirection;
        private Vector3 pathdirection;
        private int raycastlayermask;
        private Drive transportabledrive;
        private bool transportabledrivenotnull;
        private Drive drive;


#if GAME4AUTOMATION_DEBUG
        [Button("Stop")]
        public void ButtonStop()
        {
            Stop(true);
        }

        [Button("UnStop")]
        public void ButtonUnStop()
        {
            Stop(false);
        }
#endif


        // called when a pathmover is created 创建路径移动器时调用
        public void OnSourceCreated()
        {
            OnCreated();
            if (CreateOnPath != null)
                SetToPath(CreateOnPath, 0.0f);
        }

        // called when a path mover stop 当路径移动器停止时调用
        public void Stop()
        {
            Stop(true);
        }
        // called when a path mover start 路径移动器启动时调用
        public void Start()
        {
            Stop(false);
         

        }
        // called to stop a path mover
        public void Stop(bool stop)
        {
            if (stop)
            {
                IsStarting = false;
                IsStopping = true;
                IsStopped = false;
                OnStopping.Invoke(Path, this);
                if (transportabledrivenotnull)
                {
                    // AGVS with Drive 减速
                    transportabledrive.Decelerate();
                }
                else
                {
                    OnIsFulllyStopped();
                }

            }

            if (stop && IsStopped)
                OnIsFulllyStopped();

            if (!stop)
            {
                IsStarting = true;
                IsStopped = false;
                IsStopping = false;
                OnStart.Invoke(Path, this);
                if (!IsBlocked)
                    if (transportabledrivenotnull)
                        transportabledrive.Accelerate();// 启动驱动器-它将加速与sinoide如果打开
            }
        }


        private void OnIsFulllyStopped()
        {
            IsStopping = false;
            IsStopped = true;
            OnStopped.Invoke(Path, this);
        }

        private void OnIsFullyStarted()
        {
            IsStarting = false;
            OnFullyStartd.Invoke(Path, this);
        }


        // called when a path mover is blocked
        public void Block(bool blocked)
        {
            if (blocked && !IsBlocked)
            {
                IsBlocked = blocked;
                OnBlocked.Invoke(Path, this);
                if (transportabledrivenotnull)
                    transportabledrive.Decelerate();
            }

            if (!blocked && IsBlocked)
            {
                IsBlocked = blocked;
                OnBlocked.Invoke(Path, this);
                if (!IsStopped && !IsStopping)
                    if (transportabledrivenotnull)
                        transportabledrive.Accelerate();
            }
        }

        // called to select the next path
        public void SelectNextPath(PathMover pathMover, ref List<SimulationPath> Pathes)
        {
            return;
        }

        // called when path mover enter a station
        public void StationEntered(BaseStation station)
        {
            CurrentStation = station;
            OnStationEntered.Invoke(station, this);
        }
        // called when the worktime of the station start
        public void StationWorkStarting(BaseStation station)
        {
            CurrentStationWorking = station;
            OnStationWorkStarting.Invoke(station, this);
        }
        // called when worktime of a station finished
        public void StationWorkFinished(BaseStation station)
        {

            OnStationWorkFinished.Invoke(station, this);
            CurrentStationWorking = null;
        }
        // called when the path mover leave the station 
        public void StationExit(BaseStation station)
        {
            OnStationExit.Invoke(station, this);
            CurrentStation = null;
        }
        // set the position of the path mover on the path
        public void SetPosition(float Positon)
        {
            if (!pathnotnull)
                return;
            var currentpath = (BasePath)Path;
            var position = Path.GetAbsPosition(Positon, ref currentpath);
            Quaternion rotation = new Quaternion();
            pathdirection = Path.GetAbsDirection(Position);
            rotation = Quaternion.LookRotation(pathdirection, AlignVector);


            _rigidbody.MovePosition(position);
            if (AlignWithPath)
            {
                _rigidbody.MoveRotation(rotation);
            }
        }

        private void InitToPath(SimulationPath NewPath, float SetPositon)
        {
            var mu = GetComponent<MU>();
            mu.EventMuEnterPathSimulation();
            this.enabled = true;
            Path = NewPath;
            Position = SetPositon;
            var currentpath = (BasePath)NewPath;
            pathdirection = Path.GetAbsDirection(Position);
            transform.position = Path.GetAbsPosition(0, ref currentpath);
            var rotation = Quaternion.LookRotation(pathdirection, AlignVector);
            if (AlignWithPath)
            {
                transform.rotation = rotation;
            }
            if (transportabledrivenotnull)
                transportabledrive.Accelerate();
        }

        // called when path mover has left the path
        public void ReleaseFromPathEnd()
        {
            Stop(false);
            IsOnPathEnd = false;
        }

        // set the path mover to a defined path 将路径移动器设置为已定义的路径
        public void SetToPath(SimulationPath NewPath, float SetPositon)
        {
            if (Path == null)
            {
                InitToPath(NewPath, SetPositon);
            }
            Path = NewPath;
            Position = SetPositon;
            OnPathEntered.Invoke(Path, this);
            Path.TransportableEntered(this);
            pathnotnull = true;
            var currentpath = (BasePath)Path;
            SetPosition(SetPositon);
        }

        // set the velocity of the path mover 设置路径移动器的速度
        public void SetVelocity(float Velocity)
        {
            if ((!transportabledrivenotnull && IsStopped == false && IsBlocked == false) ||
                (transportabledrivenotnull))
            {
                pathdirection = Path.GetAbsDirection(Position);//在全局坐标系中获取路径上绝对位置(米)的切线方向
                _rigidbody.velocity = Velocity * pathdirection;

                var rotation = Quaternion.LookRotation(pathdirection, AlignVector);//使用指定的 forward 和 upwards 方向创建旋转。
                _rigidbody.MoveRotation(rotation);//将刚体旋转到 /rotation/。
            }
            else
            {
                _rigidbody.velocity = Vector3.zero;
            }
        }

        void UpdatePosition(float deltaTime)
        {
            if (!pathnotnull)
                return;
            if (transportabledrivenotnull)
                drive = transportabledrive;
            else
            {
                if (!Path.DriveNotNull)
                    return;
                else
                    drive = Path.Drive;
            }

            if (drive.CurrentSpeed == 0)
                return;

            if ((IsStopped || IsBlocked) && !transportabledrive)
                return;
            float speed = 0;

            if (!drive.ReverseDirection)
                speed = drive.CurrentSpeed;
            else
                speed = -drive.CurrentSpeed;
            var deltapos = speed / 1000 * deltaTime;


            Position = Position + deltapos;

            if (Position > Path.Length)
            {
                TransportableOnPathEnd();
            }
            if (Position < 0)
            {
                TransportableOnPathEnd();
            }
        }

        // called when the path mover is created
        public void OnCreated()
        {
            _rigidbody = GetComponent<Rigidbody>();
            raycastlayermask = 1 << this.gameObject.layer;
            transportabledrive = GetComponent<Drive>();
            Mu = GetComponent<MU>();
            transportabledrivenotnull = transportabledrive != null;
            if (transportabledrivenotnull)
            {
                transportabledrive.JogForward = false;
                transportabledrive.JogBackward = false;
            }
            IsStopped = false;
            IsBlocked = false;
            EnteringStationDistance = 1000000000;
        }
        // called when the path mover move on
        public bool TryMoveNext()
        {

            if (!pathnotnull)
                return false;

#if GAME4AUTOMATION_DEBUG
            var mu = GetComponent<MU>();
            if (mu.ID == DebugMUID || DebugMUID == 0)
            {
                if (Path == DebugPath)
                {
                    G4ADebug("TryMoveNext");
                    if (DebugBreak) Debug.Break();
                }
            }
#endif
            if (Position > 0)
                NextPathes = new List<SimulationPath>(Path.Successors);
            if (Position < 0)
                NextPathes = new List<SimulationPath>(Path.Predecessors);


            // First all strategies on Transportable
            var strategies = GetComponents<ISelectNextPath>();
            foreach (var strategy in strategies)
            {
                strategy.SelectNextPath(this, ref NextPathes);
            }

            // Next External  Strategy
            if (Path.PathStrategy != null)
            {
                strategies = Path.PathStrategy.GetComponents<ISelectNextPath>();
                foreach (var strategy in strategies)
                {
                    strategy.SelectNextPath(this, ref NextPathes);
                }
            }

            // Last all strategies added as component to Path (if available)
            strategies = Path.GetComponents<ISelectNextPath>();
            foreach (var strategy in strategies)
            {
                strategy.SelectNextPath(this, ref NextPathes);
            }

            // Take top one next Path
            if (NextPathes.Count > 0)
            {
                var nextPath = NextPathes[0];
                var nextpos = Position - Path.Length;
                if (Position < 0)
                    nextpos = nextPath.Length + Position;
                if (nextpos < 0)
                    Debug.Log("Position < 0");
                OnPathExit.Invoke(Path, this);
                Path.TransportableExit(this);
                SetToPath(nextPath, nextpos);
                ReleaseFromPathEnd();
                return true;
            }
            else
            {
                return false;
            }
        }
        // called when path mover is removed from a path
        public void RemoveFromPath()
        {
            Stop(false);
            OnPathExit.Invoke(Path, this);
            Path = null;
            pathnotnull = false;
            IsOnPathEnd = false;
            var mu = GetComponent<MU>();
            mu.EventMUExitPathSimulation();
            this.enabled = false;
        }

        void TransportableOnPathEnd()
        {
            if (!IsOnPathEnd)
            {
                if (CurrentStationWorking != null)
                {
                    Error($"game4automation Simulation - Transportable at time [{Time.time}] on end of Path [{Path.name}] and still in Station [{CurrentStation.name}], this is not allowed");
                }
                IsOnPathEnd = true;
                Stop(true);
                OnPathEnd.Invoke(Path, this);
                Path.TransportableOnEnd(this);
                // Is there no next path and no Method on Path End - Leave Path?
                if (LeavePath)
                {
                    if (Position > Path.Length)
                    {
                        if (Path.Successors.Count == 0)
                        {
                            RemoveFromPath();
                            return;
                        }
                    }
                    if (Position < 0)
                    {
                        if (Path.Predecessors.Count == 0)
                        {
                            RemoveFromPath();
                            return;
                        }
                    }
                }

                TryMoveNext();
            }
        }


        private bool CheckCollission()
        {
            raydirection = pathdirection;
            var colorok = Color.yellow;
            var colorcollide = Color.red;
            var color = colorok;
            var collission = Physics.Raycast(transform.position, raydirection, Distance, raycastlayermask);
            //向场景中的所有碰撞体投射一条射线，该射线起点为 /origin/，朝向 /direction/，长度为 /maxDistance/。如果射线与任何碰撞体相交，返回 true，否则返回 false。
            //raycastlayermask 层遮罩，用于在投射射线时有选择地忽略碰撞体。该脚本代码放置的对象与检测的对象处于同一Layer下才能检测成功
            if (collission)
                color = colorcollide;
            if (DrawRay)
                Debug.DrawRay(transform.position, raydirection * Distance, color);

            if (DistanceSides > 0 && !collission)
            {
                var ray1 = Quaternion.AngleAxis(-AngleSide, Vector3.up) * raydirection;//俩条斜射线判断
                var ray2 = Quaternion.AngleAxis(AngleSide, Vector3.up) * raydirection;

                collission = Physics.Raycast(transform.position, ray1, DistanceSides, raycastlayermask);
                if (collission)
                    color = colorcollide;
                if (DrawRay)
                    Debug.DrawRay(transform.position, ray1 * DistanceSides, color);
                color = colorok;
                if (!collission)
                {
                    color = colorok;
                    collission = Physics.Raycast(transform.position, ray2, DistanceSides, raycastlayermask);
                    if (collission)
                        color = colorcollide;
                    if (DrawRay)
                       Debug.DrawRay(transform.position, ray2 * DistanceSides, color);
                }
            }

            if (collission)
                Block(true);//判断有无遮挡
            if (!collission && IsBlocked)
                Block(false);
            return collission;
        }

        
        private void FixedUpdate()
        {
#if GAME4AUTOMATION_DEBUG
            var mu = GetComponent<MU>();
            if (mu.ID == DebugMUID && mu.ID != 0)
            {
                if (Time.time >= DebugTime)
                {
                    G4ADebug("Debug Break based on Time");
                    Debug.Break();
                }
            }
#endif

            CheckCollission();

            //Update
            if (transportabledrivenotnull)
            {
                drive = transportabledrive;
            }
            else
            {
                if (!pathnotnull)
                    return;
                if (!Path.DriveNotNull)
                    return;
                drive = Path.Drive;
            }

            float speed = 0;

            if (!drive.ReverseDirection)
                speed = drive.CurrentSpeed;
            else
                speed = -drive.CurrentSpeed;

            SetVelocity(speed / 1000);// set the velocity of the path mover 设置路径移动器的速度
            // Update

            UpdatePosition(Time.deltaTime);

            if (IsStopping && drive.IsStopped)
                OnIsFulllyStopped();
            if (IsStarting && drive.IsAtTargetSpeed)
                OnIsFullyStarted();

        }
    }
}