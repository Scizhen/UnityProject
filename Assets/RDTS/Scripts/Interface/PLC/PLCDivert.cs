using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RDTS.Method
{
    //与运输机系统中的分拣器有关的PLC脚本？
    public class PLCDivert : PLCBase
    {

        public Drive DivertDirectionDrive;
        public Drive DivertedConveyorDrive;
        public Sensor SensorSink;
        public Sink Sink;
        public Sensor SensorEndDivert;
        public Sensor SensorEndStraight;
        public Sensor SensorStartDivert;
        public BoxSource SourceToCreateWhenDelete;
        public Sensor StopDivertedConveyorOnSensor;
        public float DriveStraightAngle = 0;
        public float DriveDivertAngle = -45;
        public float MinTimeWork = 1.0f;
        public float MaxTimeWork = 10.0f;
        
        private bool sensorendinfeedbefore;
        private bool sensorendstraighbefore;
        private bool waitingforremove;

        private Distribution distribution;
        void Start()
        {
            if (!Application.isPlaying)
                return;
            DivertedConveyorDrive.JogForward = true;
            distribution = new Distribution();
            distribution.Min = MinTimeWork;
            distribution.Max = MaxTimeWork;
            distribution.Type = Distribution.DistributionType.Uniform;
            distribution.Init();
            FirstState();
        }

        void RemoveMU()
        {
            
            DivertedConveyorDrive.JogForward = true;
            Sink.DeleteMUs();
            Invoke("Removed",2);
        }

        void Removed()
        {
            waitingforremove = false;
            if (SourceToCreateWhenDelete!=null)
                SourceToCreateWhenDelete.Create();
        }
        // Update is called once per frame
        void FixedUpdate()
        {
           
            if (!Application.isPlaying)
                return;

            var state = State;
            
            if (state == "Empty")
            {
                if (SensorStartDivert.Occupied && !SensorEndDivert.Occupied )
                {
                    DivertDirectionDrive.DriveTo(DriveDivertAngle);
                    State = "Diverting";
                }
                else
                {
                       
                    if (SensorStartDivert.Occupied )
                    {
                        DivertDirectionDrive.DriveTo(DriveStraightAngle);
                        NextState();
                    }
                }
             
            }

            if (state == "Straight")
            {
                if (!SensorEndStraight.Occupied && SensorEndStraight.Occupied != sensorendstraighbefore)
                {
                    FirstState();
                }
            }
            
            if (state == "Diverting")
            {
                if (SensorEndDivert.Occupied)
                {
                    FirstState();
                }
            }

            if (StopDivertedConveyorOnSensor != null)
            {
                if (StopDivertedConveyorOnSensor.Occupied && state != "Diverting")
                {
                    DivertedConveyorDrive.JogForward = false;
                }
                else
                {
                    if (!waitingforremove)
                       DivertedConveyorDrive.JogForward = true;
                }
            }
            
            if (SensorSink.Occupied && !waitingforremove) 
            {
                DivertedConveyorDrive.JogForward = false;
                waitingforremove = true;
                Invoke("RemoveMU",distribution.GetNextRandom());
            }
            sensorendstraighbefore = SensorEndStraight.Occupied;
        }
    }
}