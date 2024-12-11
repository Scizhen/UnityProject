using System.Collections.Generic;

namespace RDTS
{
    //接口来定义移动物体所需的方法
    public interface ISelectNextPath
    {
        void SelectNextPath(PathMover pathMover, ref List<SimulationPath> Pathes);
    }
}

