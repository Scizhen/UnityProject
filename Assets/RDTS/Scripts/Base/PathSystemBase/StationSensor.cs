
using RDTS;
using UnityEngine;

//路径系统中工作站碰撞体相关脚本

public class StationSensor : MonoBehaviour
{
    private BaseStation station;

    private void Start()
    {
        station = GetComponentInParent<BaseStation>();

    }

    private void OnTriggerEnter(Collider other)
    {
        if (station != null)
            station.OnTriggerEnter(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (station != null)
            station.OnTriggerExit(other);

    }


}
