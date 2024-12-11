using System;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using Random = UnityEngine.Random;


namespace RDTS
{
    [SelectionBase]

    public class WorkStation : BaseStation
    {
        /// <summary>
        /// AGV路径中的工作站脚本，用于改变AGV的运动
        /// </summary>
        //! The class describe the main station functionss
        public enum StationStatus { Entering, Working, Leaving, Empty, Failure, Waiting }; //!< Array of possible station status
        //public PathMover pathMover => this.CurrentPathMover;
        public string StatusToString;
        
        [Foldout("Status")]
        [ReadOnly] public StationStatus Status = StationStatus.Empty;
        [Foldout("Status")][ReadOnly] public float CurrentWorktime = 0;//!< current work time (read only)
        [Foldout("Status")][ReadOnly] public float RemainingWorktime = 0;//!< current remaining time (read only)
        [Foldout("Status")][ReadOnly] public bool EntryBlocked = false;//!< boolean blocked entrance (read only)
        [Foldout("Worktime")] public Distribution.DistributionType Distribution;//!< distribution used for work time duration 分配用于工作时间持续时间
        [Foldout("Worktime")][HideIf("DistConst")] public int RandomSeed = 1;//!< seed value of used distribution
        [Foldout("Worktime")][ShowIf("DistConst")] public float Worktime = 10f;//!< worktime in seconds
        [Foldout("Worktime")][ShowIf("DistUniform")] public float MinTime = 5f;//!< minimum worktime according to uniform distribution
        [Foldout("Worktime")][ShowIf("DistUniform")] public float MaxTime = 10f;//!< maximum worktime according to uniform distribution
        [Foldout("Worktime")][ShowIf("DistNormal")] public float MeanTime = 10f;//!< mean worktime according to normal distribution
        [Foldout("Worktime")][ShowIf("DistNormal")] public float StandardDeviation = 2f;//!< standard deviation of the worktime according to normal distribution
        [Foldout("Worktime")][ShowIf("DistErlang")] public int Shape = 2;//!< shape of working time according to erlang distribution
        [Foldout("Worktime")][ShowIf("DistErlang")] public float Rate = 1.0f;//!< rate of working time according to erlang distribution

        [Foldout("Failure")] public float Availability = 100f;//!< Station availability in %
        [Foldout("Failure")] public float MTTR = 0;//!< mean time to repair in seconds
        [Foldout("Failure")] public int RandomSeedFailureDistance = 10;//!< Seed value distribution failure distance
        [Foldout("Failure")] public int RandomSeedFailureDuration = 11;//!< Seed value distribution failure duration

        [Foldout("Colors")] public Color Entering = Color.yellow; //!< color definition status "entering"
        [Foldout("Colors")] public Color Working = Color.green;//!< color definition status "working"
        [Foldout("Colors")] public Color Leaving = Color.gray;//!< color definition status "leaving"
        [Foldout("Colors")] public Color Empty = Color.blue;//!< color definition status "empty"
        [Foldout("Colors")] public Color Failure = Color.red;//!< color definition status "failure"

        [Foldout("Station Events")] public SimulationStationEvent OnStationEntered;//!< events called when a MUs enter the station
        [Foldout("Station Events")] public SimulationStationEvent OnStationWorkStarting;//!< events called when the working process starts
        [Foldout("Station Events")] public SimulationStationEvent OnStationWorkFinished;//!< events called when the working process finish
        [Foldout("Station Events")] public SimulationStationEvent OnStationExit;//!< events called when a MUs leave the station

        private float timestartworking;
        private Distribution worktimeDistribution;
        private IWorkstationProcess process;
        private bool processnotnull;
        private PathMover waitingentrytransportable;
        private PathMover workingtransportable;
        private float failurestart;
        private float failureduration;
        private float laststarttime;
        private float remainingworktime;
        private StationStatus statbeforefailure;
        private Distribution FailureDistantce;
        private Distribution FailureDuration;



        // Return value of constant distribution
        bool DistConst()
        {
            return Distribution == RDTS.Distribution.DistributionType.Const;
        }

        // Return value of uniform distribution
        bool DistUniform()
        {
            return Distribution == RDTS.Distribution.DistributionType.Uniform;
        }

        // Return value of normal distribution
        bool DistNormal()
        {
            return Distribution == RDTS.Distribution.DistributionType.Normal;
        }

        // Return value of erlang distribution
        bool DistErlang()
        {
            return Distribution == RDTS.Distribution.DistributionType.Erlang;
        }

        bool DistExponential()
        {
            return Distribution == RDTS.Distribution.DistributionType.Exponential;
        }

        // Methods to override in BaseStation 要在基站中重写的方法
        protected override bool AllowEntry(PathMover pathMover)
        {
            bool allow = false;
            waitingentrytransportable = pathMover;
            if (Status == StationStatus.Failure)
            {
                allow = false;
            }
            else
            {
                // Check if Start Is Allowed
                if (processnotnull)
                {
                    if (process.AllowEntry(this, pathMover))
                        allow = true;
                }
                else
                {
                    allow = true;
                }

            }

            EntryBlocked = !allow;
            if (allow)
                OpenWorkstationEntry();
            return allow;
        }

        protected override void OnExit(PathMover pathMover)
        {
            if (PathMoverMovingOut.Count == 0)
                SetStatus(StationStatus.Empty);
            OnStationExit.Invoke(this, pathMover);
            if (processnotnull)
                process.OnExit(this, pathMover);
        }

        protected override void OnAtPositon(PathMover pathMover)
        {
            // Check if Start Is Allowed
            var directstart = false;
            if (processnotnull)
            {
                if (process.AllowStart(this, pathMover))
                {
                    directstart = true;
                }
            }
            else
            {
                directstart = true;
            }

            if (directstart) StartWorking();
            else SetStatus(StationStatus.Waiting);
        }

        protected override void OnFixedUpdate()
        {
            // Update Worktime 更新工作时间
            if (Status == StationStatus.Working)
                RemainingWorktime = CurrentWorktime - (Time.time - timestartworking);
            else
                RemainingWorktime = 0;
        }


        // Public Methods
        public void OpenWorkstationEntry()
        {

            waitingentrytransportable.StationEntered(this);
            if (PathMoverMovingIn.Count == 1)
                SetStatus(StationStatus.Entering);
            OnStationEntered.Invoke(this, waitingentrytransportable);
            if (EntryBlocked)
            {
                EntryBlocked = false;
                OpenEntry(waitingentrytransportable);
            }
            waitingentrytransportable = null;
        }
        // Start working process at a station
        public void StartWorking()
        {
            if (workingtransportable != null)
                Error($"Already working on Transportable [{workingtransportable}], Start working is called twice!");
            workingtransportable = CurrentPathMover;
            SetStatus(StationStatus.Working);
            CurrentPathMover.StationWorkStarting(this);
            OnStationWorkStarting.Invoke(this, CurrentPathMover);
            timestartworking = Time.time;

            // First check if time is set from Component on Transportable
            float time = 0;
            var timegetter = CurrentPathMover.GetComponent<IGetStationTime>();
            if (timegetter != null)
                time = timegetter.GetStationTime(CurrentPathMover);
            else
            {
                // nect check it time can be get from Component on Station
                timegetter = GetComponent<IGetStationTime>();
                if (timegetter != null)
                    time = timegetter.GetStationTime(CurrentPathMover);
                else
                {
                    // last get tine from standard Method in Station
                    time = GetStationTime(CurrentPathMover);
                }
            }
            CurrentWorktime = time;
            laststarttime = Time.time;
            if (time < 0)
            {
                Debug.Log("Time lower 0 - setting time to 0");
                time = 0;
            }
            if (processnotnull)
                process.OnStart(this, time);
            if (Status == StationStatus.Failure)
            {
                if (time > 0)
                    remainingworktime = time;
            }
            else
            {
                if (time > 0)
                    Invoke("ReleaseWorkStation", time);
            }

        }

        // working time finished at a station 工作时间在车站结束
        public void StopWorking()
        {
            workingtransportable = null;
            ReleaseWorkStation();
        }

        // Private Methods
        private void ReleaseWorkStation()
        {
            if (Status == StationStatus.Failure || CurrentPathMover == null)
            {
                return;
            }
            workingtransportable = null;
            SetStatus(StationStatus.Leaving);
            CurrentPathMover.StationWorkFinished(this);
            OnStationWorkFinished.Invoke(this, CurrentPathMover);
            if (processnotnull)
                process.OnEnd(this, CurrentPathMover);
            CurrentWorktime = 0;
            laststarttime = 0;
            Release();
        }


        private float GetStationTime(PathMover pathMover)
        {
            if (Distribution == RDTS.Distribution.DistributionType.Const)
            {
                return Worktime;
            }
            else
            {
                return worktimeDistribution.GetNextRandom();
            }

        }
        // Called when a failure starts 
        public void FailureStart()
        {
            failureduration = FailureDuration.GetNextRandom();
            failureduration = failureduration * 100;
            Invoke("FailureEnd", failureduration);
            if (laststarttime > 0)
            {
                remainingworktime = Time.time - laststarttime;

            }
            statbeforefailure = Status;
            SetStatus(StationStatus.Failure);
        }

        // Called when a failure ends
        public void FailureEnd()
        {
            SetStatus(statbeforefailure);
            if (remainingworktime > 0)
            {
                Invoke("ReleaseWorkStation", remainingworktime);
                remainingworktime = 0;
            }
            else
            {
                if (PathMoverWaitingForEntry.Count > 0)
                    OpenEntry(PathMoverWaitingForEntry[0]);
            }
            failurestart = FailureDistantce.GetNextRandom();
            Invoke("FailureStart", failurestart);
        }

        new void Awake()
        {
            LimitToTransportablesonPath = true;
            process = GetComponent<IWorkstationProcess>();
            processnotnull = process != null;
            areameshrenderer = GetComponentInChildren<StationSensor>().gameObject.GetComponent<MeshRenderer>();
            SetStatus(StationStatus.Empty);
            if (Distribution != RDTS.Distribution.DistributionType.Const)
            {
                worktimeDistribution = new Distribution();
                worktimeDistribution.Seed = RandomSeed;
                worktimeDistribution.Min = MaxTime;
                worktimeDistribution.Max = MaxTime;
                worktimeDistribution.Mean = MeanTime;
                worktimeDistribution.Rate = Rate;
                worktimeDistribution.Shape = Shape;
                worktimeDistribution.StandardDeviation = StandardDeviation;
                worktimeDistribution.Type = Distribution;
                worktimeDistribution.Init();
            }

            // Start Failures 启动失败
            if (Availability < 100)
            {
                FailureDistantce = new Distribution();
                FailureDistantce.Type = RDTS.Distribution.DistributionType.Exponential;
                FailureDistantce.Rate = (Availability / (100 * MTTR)) / (1 - (Availability / 100));
                FailureDistantce.Seed = RandomSeedFailureDistance;
                FailureDistantce.Init();

                failurestart = FailureDistantce.GetNextRandom();
                Invoke("FailureStart", failurestart);
            }

            FailureDuration = new Distribution();
            FailureDuration.Type = RDTS.Distribution.DistributionType.Erlang;
            FailureDuration.Shape = 2;
            FailureDuration.Rate = MTTR;
            FailureDuration.Seed = RandomSeedFailureDuration;
            FailureDuration.Init();

            workingtransportable = null;
            CurrentPathMover = null;
            CurrentWorktime = 0;

            base.Awake();

        }

        public void SetStatus(StationStatus status)
        {
            Status = status;
            StatusToString = Status.ToString();
            switch (status)
            {
                case StationStatus.Empty:
                    areameshrenderer.material.color = Empty;
                    break;
                case StationStatus.Entering:
                    areameshrenderer.material.color = Entering;
                    break;
                case StationStatus.Failure:
                    areameshrenderer.material.color = Failure;
                    break;
                case StationStatus.Leaving:
                    areameshrenderer.material.color = Leaving;
                    break;
                case StationStatus.Working:
                    areameshrenderer.material.color = Working;
                    break;
            }
        }
    }
}