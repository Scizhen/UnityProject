using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RDTS.Method
{
    //与运输机系统中分拣器状态配合的PLC脚本
    public class PLCCode : PLCBase
    {
        public Drive DriveEntryConveyor;
        public Sensor SensorEntryConveyor;
        public Drive DriveChainTransfer;
        public Sensor SensorChainTransfer;
        public Drive DriveChain;
        public Drive_Cylinder LiftChain;
        [RDTS.Utility.ReadOnly] public int NumDivert = 1;

        private int currentnum;

        void Start()
        {
            currentnum = 0;
        }

        // Update is called once per frame
        void Update()
        {
            if (SensorEntryConveyor.Occupied && State == "Empty")
            {
                NextState();
                DriveEntryConveyor.JogForward = true;
                DriveChainTransfer.JogForward = true;
            }

            if (SensorChainTransfer.Occupied && State == "Loading")
            {
                if (currentnum < NumDivert)
                // Divert
                {
                    DriveChainTransfer.JogForward = false;
                    State = "Lifting";
                    LiftChain._out = true;
                    currentnum++;
                }
                else
                {
                    currentnum = 0;
                    State = "NoDivert";
                }
            }

            if (SensorChainTransfer.Occupied == false && State == "NoDivert")
            {
                State = "Empty";
                DriveChain.JogForward = false;
                DriveEntryConveyor.JogForward = true;
            }

            if (State == "Lifting" && LiftChain._isOut)
            {
                NextState();
                DriveChain.JogForward = true;
            }

            if (State == "Chain" && SensorChainTransfer.Occupied == false)
            {
                NextState();
                LiftChain._out = false;
            }

            if (State == "Lowering" && LiftChain._isIn == true)
            {
                NextState();
                DriveChain.JogForward = false;
                DriveEntryConveyor.JogForward = true;
            }

            if (State != "Loading" && State != "Empty" && SensorEntryConveyor.Occupied == true)
            {
                DriveEntryConveyor.JogForward = false;
            }

        }
    }
}