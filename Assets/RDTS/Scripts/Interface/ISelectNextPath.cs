using System.Collections.Generic;

namespace RDTS
{
    //�ӿ��������ƶ���������ķ���
    public interface ISelectNextPath
    {
        void SelectNextPath(PathMover pathMover, ref List<SimulationPath> Pathes);
    }
}

