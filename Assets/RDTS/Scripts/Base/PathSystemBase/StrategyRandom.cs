using System.Collections.Generic;
using UnityEngine;

namespace RDTS
{
    //路径系统中AGV的寻路策略脚本
    public class StrategyRandom : RDTSBehavior,ISelectNextPath
    {
        public int Seed = 1;
        
        public void SelectNextPath(PathMover pathMover,ref List<SimulationPath> Pathes)
        {
      
            var max = Pathes.Count-1;
            int random = (int)Mathf.Round(Random.Range(-0.4999f, max+0.4999f));
            var path = Pathes[random];
            Pathes.Remove(path);
            Pathes.Insert(0,path);
        }

        private void Start()
        {
            Random.InitState(Seed);
        }
    }

}
