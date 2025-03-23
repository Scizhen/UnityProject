using UnityEngine;

//·��ϵͳ�й���վ������ƽű�������AGV�����ƶ�

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
