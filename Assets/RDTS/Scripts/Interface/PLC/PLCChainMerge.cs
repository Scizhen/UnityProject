using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RDTS;

namespace RDTS.Method
{
    public class PLCChainMerge: PLCBase
    {
        //与运输机系统中的分拣器有关的PLC脚本？
        public StraightConveyor EntryMergeConveyor;
        public StraightConveyor EntryStraightConveyor;
        public StraightConveyor MergeConveyor;
        public ChainTransfer Chain;
        public float TimeWaitEntry = 0.5f;

        private Drive driveEntryMergeConveyor;
        private Drive driveEntryStraightConveyor;
        private Sensor sensorEntryMergeConveyor;
        private Sensor sensorEntryStraightConveyor;
        private Drive driveChainTransfer;
        private Sensor sensorExitMerge;
        private Drive driveChain;
        private Drive_Cylinder liftChain;

        private int destination;
        private bool lastsensorentrymerge;
        private bool lastsensorexitmerge;

        void Start()
        {
            if (!Application.isPlaying)
                return;
            driveEntryMergeConveyor = EntryMergeConveyor.GetComponentByName<Drive>("Drive");
            driveEntryStraightConveyor = EntryStraightConveyor.GetComponentByName<Drive>("Drive");
            sensorEntryMergeConveyor = EntryMergeConveyor.GetComponentByName<Sensor>("Sensor");
            sensorEntryStraightConveyor= EntryStraightConveyor.GetComponentByName<Sensor>("Sensor");  
            driveChainTransfer = MergeConveyor.GetComponentByName<Drive>("Drive");
            sensorExitMerge = MergeConveyor.GetComponentByName<Sensor>("Sensor");
            driveChain = Chain.GetComponentByName<Drive>("Drive");
            liftChain = Chain.GetComponentByName<Drive_Cylinder>("Lift");
            FirstState();
        }

        // Update is called once per frame
        void FixedUpdate()
        {
           
            if (!Application.isPlaying)
                return;

            var state = State;

            if (state == "Empty")
            {
                driveEntryMergeConveyor.JogForward = !sensorEntryMergeConveyor.Occupied;

                    driveEntryStraightConveyor.JogForward = !sensorEntryStraightConveyor.Occupied;
            }
            
            if (sensorEntryMergeConveyor.Occupied && state == "Empty" && sensorEntryStraightConveyor.Occupied == false)
            {
                driveChain.JogForward = true;
                driveEntryMergeConveyor.JogForward = true;
                liftChain._out = true;
                driveChainTransfer.JogForward = true;
                driveEntryStraightConveyor.JogForward = false;
                NextState();
            }
            
            if (sensorEntryStraightConveyor.Occupied && state == "Empty")
            {
                driveChainTransfer.JogForward = true;
                driveEntryMergeConveyor.JogForward = false;
                liftChain._out = false;
                driveEntryStraightConveyor.JogForward = true;
                State = "EntryStraight";
            }

            if (state == "EntryStraight") 
            {
                if (sensorEntryStraightConveyor.Occupied == false)
                {
                    NextState();
                }
            }
            
            if (state == "ExitStraight") 
            {
                if (sensorExitMerge.Occupied == false && lastsensorexitmerge != sensorExitMerge.Occupied)
                {
                    NextState();
                }
            }
            
            if (state == "EntryMerge")
            {
                if (!sensorEntryMergeConveyor.Occupied && sensorEntryMergeConveyor.Occupied != lastsensorentrymerge)
                {
                    NextState();
                    Invoke("NextState",TimeWaitEntry);
                }
            }
            
            if (state == "LowerChain")
            {
                driveEntryMergeConveyor.JogForward = false;
                driveChain.JogForward = false;
                driveChainTransfer.JogForward = true;
                liftChain._out = false;
                NextState();
            }
            
            if (state == "ExitMerge")
            {
                driveChainTransfer.JogForward = true;
                if (sensorExitMerge.Occupied == false && lastsensorexitmerge != sensorExitMerge.Occupied)
                {
                    NextState();
                }
            }

            lastsensorentrymerge = sensorEntryMergeConveyor.Occupied;
            lastsensorexitmerge = sensorExitMerge.Occupied;

        }
    }
}