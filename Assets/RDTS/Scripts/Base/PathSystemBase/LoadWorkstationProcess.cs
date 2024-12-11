using NaughtyAttributes;
using UnityEngine;

namespace RDTS
{
    //路径系统中类定义站点内的加载过程
    //! Class define loading process within a station
    public class LoadWorkstationProcess : MonoBehaviour,IWorkstationProcess
    {
        public SimulationPath LoadFromLine;//!< Simulation path from which MUs can be loaded
        public StraightConveyor LoadFromConveyor;//!< Conveyor from which MUs can be loaded
        public bool MoveToLoadPositon;//!< Moving to a specific load position
        [Foldout("Status")][ReadOnly]public MU  WaitingLoad; //!< information which MU is waiting for loading
        [Foldout("Status")][ReadOnly]public MU  CurrentLoad;//!< information which MU is currently bin loading
        [Foldout("Status")][ReadOnly] public PathMover  AvailablePathMover; //!< currently available pathmover
        [Foldout("Status")][ReadOnly] public bool WaitingForLoad = false; 
        [Foldout("Status")][ReadOnly] public bool IsLoading = false;//!< boolean whether the loading process is currently active

        private WorkStation workstation;
        private Vector3 transportablepos;
        private Vector3 loadpos;
        private float loadtime;
        private float t;
        private Drive Convdrive;
        private Sensor ConvSensor;
        
        public bool AllowEntry(WorkStation station, PathMover pathMover)
        {
            return true;
        }
        
        public bool AllowStart(WorkStation station, PathMover pathMover)
        {
            workstation = station;
            AvailablePathMover = pathMover;
            Invoke("CheckLoading", 0);
            return false;
        }

        // Called when a failure starts (user method)
        public void OnFailureStart(WorkStation station)
        {
            return;
        }
        // Called when a failure ends. (user method)
        public void OnFailureEnd(WorkStation station)
        {
            return;
        }

        // Called when the worktime starts
        public void OnStart(WorkStation station, float time)
        {
            CurrentLoad = WaitingLoad;
            CurrentLoad.Rigidbody.isKinematic = true;
            WaitingLoad = null;
            // Start here the loading with the time
            loadpos = AvailablePathMover.GetComponentInChildren<LoadPosition>().transform.position;
            // var currentpos
            transportablepos = CurrentLoad.transform.position;
            IsLoading = true;
            if (LoadFromLine != null)
                CurrentLoad.GetComponent<PathMover>().RemoveFromPath();
            t = 0;
            loadtime = time;
        }

        public void OnEnd(WorkStation station, PathMover pathMover)
        {
            IsLoading = false;
            CurrentLoad.transform.position = loadpos;
            AvailablePathMover.Mu.LoadMu(CurrentLoad);
            AvailablePathMover = null;
            CurrentLoad = null;
          
        }

        // Called when worktime is finished
        public void OnExit(WorkStation station, PathMover pathMover)
        {
        
        }
        
        private void CheckLoading()
        {
            
            if (AvailablePathMover != null && !IsLoading )
            {
                AvailablePathMover.Stop();
            }
           if (WaitingLoad != null && AvailablePathMover != null && !IsLoading)
            {
                IsLoading = true;
                workstation.StartWorking();
            }
          
        }
        // Start is called before the first frame update
        void Awake()
        {
                if (LoadFromLine!=null)
                    LoadFromLine.OnPathEnd.AddListener(LoadAtPath);
                if (LoadFromConveyor != null)
                {
                    Convdrive = LoadFromConveyor.GetComponentInChildren<Drive>();
                    ConvSensor = LoadFromConveyor.GetComponentInChildren<Sensor>();
                }

                AvailablePathMover = null;
                IsLoading = false;
                CurrentLoad = null;
                WaitingLoad = null;
        }

        private void LoadAtPath(SimulationPath path, PathMover pathMover)
        {
            WaitingLoad = pathMover.GetComponent<MU>();
            CheckLoading();
        }

        void Update()
        {
            if (IsLoading)
            {
                if (MoveToLoadPositon)
                {
                    t +=  Time.deltaTime/loadtime;
                    CurrentLoad.transform.position = Vector3.Lerp(transportablepos, loadpos, t);
                }
            }

        }
        
        // Update is called once per frame
        void FixedUpdate()
        {
            if (LoadFromConveyor != null)
            {
                if (ConvSensor.Occupied == true && Convdrive.IsRunning == true)
                {
                    Convdrive.Stop();
                    WaitingLoad = ConvSensor.CollidingMus[0];
                }

                if (ConvSensor.Occupied == true && Convdrive.IsRunning == false)
                {
                    CheckLoading();
                }
                if (ConvSensor.Occupied == false && Convdrive.IsRunning == false)
                {
                    Convdrive.Forward();
                    WaitingLoad = null;
                }
            }

            if (LoadFromLine != null)
            {
                CheckLoading();
            }
        }

    }
}

