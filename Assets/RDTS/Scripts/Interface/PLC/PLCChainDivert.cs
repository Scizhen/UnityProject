using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RDTS.Method
{
    /// <summary>
    /// 链条转移模块中的PLC控制脚本
    /// </summary>
    public class PLCChainDivert : PLCBase
    {

        public StraightConveyor EntryConveyor;
        public StraightConveyor Transfer;
        public ChainTransfer Chain;

        private Drive driveEntryConveyor;
        private Sensor sensorEntryConveyor;
        private Drive driveChainTransfer;
        private Sensor sensorChainTransfer;
        private Drive driveChain;
        private Drive_Cylinder liftChain;

        [RDTS.Utility.ReadOnly] public int NumDivert = 1;

        private int destination;

        void Start()
        {
            if (!Application.isPlaying)
                return;
            driveEntryConveyor = EntryConveyor.GetComponentByName<Drive>("Drive");
            sensorEntryConveyor = EntryConveyor.GetComponentByName<Sensor>("Sensor");
            driveChainTransfer = Transfer.GetComponentByName<Drive>("Drive");
            sensorChainTransfer = Transfer.GetComponentByName<Sensor>("Sensor");
            driveChain = Chain.GetComponentByName<Drive>("Drive");
            liftChain = Chain.GetComponentByName<Drive_Cylinder>("Lift");
        }

        // Update is called once per frame
        void FixedUpdate()
        {

            if (!Application.isPlaying)
                return;

            if (sensorEntryConveyor.Occupied && State == "Empty")
            {
                NextState();
                driveEntryConveyor.JogForward = true;
                driveChainTransfer.JogForward = true;
            }

            if (State == "WaitingForDestination")
            {
                destination = GetDestination();
                if (destination > 0)
                    NextState();
            }


            if (sensorChainTransfer.Occupied && State == "Loading")
            {
                if (destination == 2)
                {
                    driveChainTransfer.JogForward = false;
                    NextState();
                    liftChain._out = true;
                }
                else
                {
                    State = "NoDivert";
                }
            }

            if (sensorChainTransfer.Occupied == false && State == "NoDivert")
            {
                NextState();
                driveChain.JogForward = false;
                driveEntryConveyor.JogForward = true;
            }

            if (State == "Lifting" && liftChain._isOut)
            {
                NextState();
                driveChain.JogForward = true;
            }

            if (State == "Chain" && sensorChainTransfer.Occupied == false)
            {
                NextState();
                liftChain._out = false;
            }

            if (State == "Lowering" && liftChain._isIn == true)
            {
                State = "Empty";
                driveChain.JogForward = false;
                driveEntryConveyor.JogForward = true;
            }

            if (State != "Loading" && State != "Empty" && sensorEntryConveyor.Occupied == true)
            {
                driveEntryConveyor.JogForward = false;
            }

        }
    }
}