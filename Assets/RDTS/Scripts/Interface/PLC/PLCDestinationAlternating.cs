using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PLCDestinationAlternating : PLCDestinationLogic
{
    //运输机系统中的PLC模块用于配合分拣器，控制货物的转运
    public int NumDestinations = 2;
    public int CurrentDestination = 0;

    public void Start()
    {
        CurrentDestination = 1;
    }

    public override int GetDestination()
    {
        CurrentDestination++;
        if (CurrentDestination > NumDestinations)
            CurrentDestination = 1;
        return CurrentDestination;
    }



}
