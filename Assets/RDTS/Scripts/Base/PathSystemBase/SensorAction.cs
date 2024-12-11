using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

namespace RDTS
{
    //运输机系统中与传感器控制有关
    public class SensorAction : MonoBehaviour
    {
        public enum ActionOn
        {
            OnSensorExit, OnSensorEnter, Never
        }

        public ActionOn Action;
        public bool ChangePhysicMaterial = false;
        [ShowIf("ChangePhysicMaterial")] public PhysicMaterial NewMaterial;
        public bool WriteMULog = false;
        
        private void SenEvent(GameObject obj)
        {
            if (Action == ActionOn.Never)
                return;

            if (WriteMULog)
            {
                Debug.Log($"Sensor Debug Log Time[{Time.time}] - [{this.name}] MU [{obj.name}] ");
            }
            if (ChangePhysicMaterial)
            {
                if (NewMaterial != null)
                {
                    var collider = obj.GetComponentInChildren<Collider>();
                    collider.material = NewMaterial;
                }
            }
        }
        
        
        void Start()
        {
            var sen = GetComponent<Sensor>();
            if (Action == ActionOn.OnSensorEnter)
                   sen.EventEnter += SenEvent;
            if (Action == ActionOn.OnSensorExit)
                sen.EventExit += SenEvent;
        }

     

    }
}

