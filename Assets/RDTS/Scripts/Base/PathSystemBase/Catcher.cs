using UnityEngine;



namespace RDTS
{
    //基于工作站对PathMover的控制
    [SelectionBase]
    public class Catcher : BaseStation
    {
        public SimulationPath SetToPath;

        public SimulationStationEvent OnCatching;

        protected override bool AllowEntry(PathMover pathMover)
        {
            return true;
        }

        protected override void OnAtPositon(PathMover pathMover)
        { 
            OnCatching.Invoke(this,pathMover);
           pathMover.SetToPath(SetToPath,0);
           Release();
        }
    }
}