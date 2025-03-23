using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

namespace RDTS
{
    //路径系统中与传感器相关脚本？
    public class StrategyAreaLimit : RDTSBehavior
    {
        public int Number;
        [ReorderableList] List<MU> ReservedFor = new List<MU>();
        private Sensor sensor;
        // Start is called before the first frame update
        void Start()
        {
            Number = 0;
            sensor = GetComponent<Sensor>();
            sensor.EventEnter += SensorOnEventEnter;
            sensor.EventExit += SensorOnEventExit;
        }

        private void SensorOnEventExit(GameObject obj)
        {
            throw new System.NotImplementedException();
        }

        private void SensorOnEventEnter(GameObject obj)
        {
            throw new System.NotImplementedException();
        }

        // Update is called once per frame
        void Update()
        {
            Number = sensor.CollidingMus.Count;
        }
    }

}
