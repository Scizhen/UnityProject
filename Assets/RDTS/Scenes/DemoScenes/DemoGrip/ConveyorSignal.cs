using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RDTS;
using RDTS.Utility;

public class ConveyorSignal : MonoBehaviour
{

    public ValueInputBool sensorSignal;
    public ValueOutputBool conveySignal;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        SignalControl();
    }

    void SignalControl()
    {
        if (sensorSignal != null && conveySignal != null)
        {
            conveySignal.Value = !sensorSignal.Value;
        }
    }
}
