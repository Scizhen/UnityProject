using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//运输机特殊控制脚本：当传感器检测到物体时运输机停下？

namespace RDTS
{
    public class StopIfSuccessorOccupied : MonoBehaviour
    {
        public StraightConveyor Successor;

        private Sensor sensorsuccessor;
        private Sensor mysensor;
        private Drive drive;
        
        void Start()
        {
            var straight = GetComponent<StraightConveyor>();
            if (Successor == null)
            {
            
                var snap = straight.GetComponentByName<SnapPoint>("SnapOut");
                if (snap.mate!=null)
                    Successor = snap.mate.GetComponentInParent<StraightConveyor>();
                else
                {
                    Debug.LogError($"{this.name} no mate at Out Snappoint");
                }
            }

            sensorsuccessor = Successor.GetComponentByName<Sensor>("Sensor");
            mysensor = straight.GetComponentByName<Sensor>("Sensor");
            drive = straight.GetComponentByName<Drive>("Drive");
        }
        // Update is called once per frame
        void FixedUpdate()
        {
            if (mysensor.Occupied)
            {
                if (sensorsuccessor.Occupied)
                {
                    drive.JogForward = false;
                    return;
                }
            }
            drive.JogForward = true;
        }
    }
}