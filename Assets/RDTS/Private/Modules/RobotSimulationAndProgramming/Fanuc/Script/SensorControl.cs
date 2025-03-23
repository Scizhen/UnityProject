using System.Collections;
using System.Collections.Generic;
using RDTS;
using UnityEngine;

public class SensorControl : MonoBehaviour
{

    public ValueOutputBool conveyerDrive;
    public ValueMiddleBool sensorDetect;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        conveyerDrive.Value = !sensorDetect.Value;

    }
}
