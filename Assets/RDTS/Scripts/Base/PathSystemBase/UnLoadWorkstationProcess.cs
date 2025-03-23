using NaughtyAttributes;

using UnityEngine;

namespace RDTS
{
    //路径系统中类定义站点内的卸载过程
    //! Class define unloading process for a station 
    public class UnLoadWorkstationProcess : MonoBehaviour, IWorkstationProcess
    {
        public SimulationPath LoadToPath;
        public LibraryObject LoadToConveyor;
        public bool MoveToLinesSmoothly;
        public Sensor SensorUnloadPosition;
        [Foldout("Status")][ReadOnly] public MU  AvailableLoad;
        [Foldout("Status")][ReadOnly] public PathMover AvailablePathMover;
        [Foldout("Status")][ReadOnly] public bool WaitingForUnloadLoad = false; 
        [Foldout("Status")][ReadOnly] public bool IsUnLoading = false;

        private WorkStation workstation;
        private Vector3 loadpos;
        private Vector3 unloadpos;
        private float unloadtime;
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
            // Get first loaded Transportable
            if (pathMover.Mu.LoadedMus.Count>=1)
                 AvailableLoad = pathMover.Mu.LoadedMus[0].GetComponent<MU>();
            else
                AvailableLoad = null;
            Invoke("CheckUnloading", 0.0001f);
            return false;
        }
        // Called when a failure starts (user method)
        public void OnFailureStart(WorkStation station)
        {
            
        }
        // Called when a failure ends. (user method)
        public void OnFailureEnd(WorkStation station)
        {
            
        }

        // Called when the worktime starts
        public void OnStart(WorkStation station, float time)
        {
            // Start here the loading with the time
            if(LoadToPath!=null)
            unloadpos = LoadToPath.StartPos;
            if (LoadToConveyor != null)
            {
                unloadpos = SensorUnloadPosition.transform.position;
            }
            loadpos = AvailableLoad.transform.position;
            station.CurrentPathMover.Mu.UnloadOneMu(AvailableLoad);
            AvailableLoad.Rigidbody.isKinematic = true;
            IsUnLoading = true;
            t = 0;
            unloadtime = time;
        }
        // Called when worktime is finished
        public void OnEnd(WorkStation station, PathMover pathMover)
        {
            IsUnLoading = false;
            if (LoadToPath != null && AvailableLoad != null)
            {
                AvailableLoad.transform.position = LoadToPath.StartPos;
                var pm = AvailableLoad.GetComponent<PathMover>();
                if (pm!=null)
                       pm.SetToPath(LoadToPath,0);
            }

            if (LoadToConveyor != null)
            {
                AvailableLoad.Rigidbody.isKinematic = false;
                AvailableLoad.transform.position = SensorUnloadPosition.transform.position;
            }
            
            AvailablePathMover = null;
            AvailableLoad = null;
        }
        // Called when a MU leave the station
        public void OnExit(WorkStation station, PathMover pathMover)
        {
        
        }
        
        private void CheckUnloading()
        {
            
           if (AvailableLoad != null && AvailablePathMover != null && !SensorUnloadPosition.Occupied)
            {
                IsUnLoading = true;
                workstation.StartWorking();
            }
           
        }
        // Start is called before the first frame update
        void Awake()
        {
                SensorUnloadPosition.EventMUSensor.AddListener(SensorUnloadPOs);
                if (LoadToConveyor != null)
                {
                    Convdrive = LoadToConveyor.GetComponentInChildren<Drive>();
                    ConvSensor =  LoadToConveyor.GetComponentInChildren<Sensor>();
                }

                AvailablePathMover = null;
                AvailableLoad = null;
                IsUnLoading = false;
        }

        private void SensorUnloadPOs(MU mu, bool occupied)
        {
            if (!occupied)
                CheckUnloading();
        }
        

        // Update is called once per frame
        void FixedUpdate()
        {
            if (IsUnLoading)
            {
                if (MoveToLinesSmoothly)
                {
                    t +=  Time.deltaTime/unloadtime;
                    AvailableLoad.transform.position = Vector3.Lerp(loadpos, unloadpos, t);
                }
            }
        }

    }
}

