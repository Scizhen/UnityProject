using UnityEngine;

public class Area : MonoBehaviour
{
    //AreaEventsµÄ×Ó½Å±¾
    [Header("Status")] public float NumberInArea;


    private void OnTriggerEnter(Collider other)
    {
        NumberInArea++;

    }

    private void OnTriggerExit(Collider other)
    {
        NumberInArea--;
    }


}
