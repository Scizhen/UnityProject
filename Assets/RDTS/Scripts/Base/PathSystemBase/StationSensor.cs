
using RDTS;
using UnityEngine;

//·��ϵͳ�й���վ��ײ����ؽű�

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
