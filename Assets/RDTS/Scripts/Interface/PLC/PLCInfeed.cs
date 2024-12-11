using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RDTS.Method
{
    //运输机系统中的PLC控制脚本与某些状态有关？
    public class PLCInfeed : PLCBase
    {

        public Drive FeedConveyorDrive;
        public Drive BeforeFeedConveyorDrive;
        public Sensor SensorEndInfeed;
        public Sensor SensorStartInfeed;
        public Sensor SensorArea;
 
        public float LowSpeed;
        public float LowAcceleration;
        public float HighSpeed;
        public float HighAcceleration;
        public float WaitBeforeInfeed = 1.0f;

        private bool sensorendinfeedbefore;
      
        void Start()
        {
            if (!Application.isPlaying)
                return;
            FeedConveyorDrive.JogForward = true;
            FeedConveyorDrive.TargetSpeed = LowSpeed;
            BeforeFeedConveyorDrive.JogForward = true;
        }


        // Update is called once per frame
        void FixedUpdate()
        {
           
            if (!Application.isPlaying)
                return;

            var state = State;
            
            if (state == "Empty")
            {
                if (SensorStartInfeed.Occupied )
                {
                    FeedConveyorDrive.TargetSpeed = 0;
                    FeedConveyorDrive.JogForward = false;
                    BeforeFeedConveyorDrive.JogForward = false;
                    NextState();  // WaitingPacket
                    Invoke("NextState", WaitBeforeInfeed); // Ready to Feed
                }
            }
            
            if (state == "WaitingPacket")
            {
                if (SensorStartInfeed.Occupied )
                {
                    FeedConveyorDrive.TargetSpeed = 0;
                    FeedConveyorDrive.JogForward = false;
                    BeforeFeedConveyorDrive.JogForward = false;
                    NextState();  // WaitBeforeFee
                }
            }

            if (state == "WaitBeforeFeet")
            {
                Invoke("NextState", WaitBeforeInfeed); // Ready to Feed
                return;
            }
            
            if (state == "ReadyToFeed")
            {
                if (!SensorArea.Occupied)
                {
                    FeedConveyorDrive.JogForward = true;
                    FeedConveyorDrive.Acceleration = LowAcceleration;
                    FeedConveyorDrive.TargetSpeed =  HighSpeed;
                    NextState();
                }
            }
            if (state == "Feed")
            {
                if (SensorEndInfeed.Occupied!=sensorendinfeedbefore) // negative Flank
                {
                    NextState(); // Waiting Feed End
                    Invoke("NextState", 0.5f);
           
                }
            }

            if (state == "EndFeed")
            {
                FeedConveyorDrive.Acceleration = HighAcceleration;
                FeedConveyorDrive.TargetSpeed = LowSpeed;
                BeforeFeedConveyorDrive.JogForward = true;
                NextState();
            }

            sensorendinfeedbefore = SensorEndInfeed.Occupied;
        }
    }
}