using UnityEngine;

public class Area : MonoBehaviour
{
    //AreaEvents���ӽű�
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
