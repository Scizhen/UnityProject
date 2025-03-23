using UnityEngine;

//路径系统中工作站特殊控制脚本：控制AGV反向移动

namespace RDTS
{
    public class StationEventRevertDirection : MonoBehaviour
    {
        public void OnStationWork(BaseStation station, PathMover pathMover)
        {
            var drive = pathMover.GetComponent<Drive>();
            drive.ReverseDirection = !drive.ReverseDirection;
        }
    }

}
