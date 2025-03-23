using UnityEngine;

namespace RDTS
{
    //路径系统中的工作站事件，控制移动物体反向运行
    public class PathEventRevertDirection : MonoBehaviour
    {
        public void OnEndPath(SimulationPath path, PathMover pathMover)
        {
            var drive = path.Drive;
            if (drive.JogForward)
            {
                drive.JogBackward = true;
                drive.JogForward = false;
            }
            else
            {
                drive.JogBackward = false;
                drive.JogForward = true;
            }
            // Unblock Transportable
            pathMover.LeavePath = false;
            pathMover.ReleaseFromPathEnd();
        }
    }
}
