using UnityEngine;

namespace RDTS
{
    //·��ϵͳ�еĹ���վ�¼��������ƶ����巴������
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
