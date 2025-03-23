using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEditor;

namespace RDTS
{
    //与力控制模块相关，判断物体碰撞触发事件
    public class ProximitySensor : LibraryPart
    {

        public delegate void
            ProximitySensorEnterExitDelegate(
                Collider other); //!< Delegate function for GameObjects entering the Sensor.

        public event ProximitySensorEnterExitDelegate OnEnter;
        public event ProximitySensorEnterExitDelegate OnExit;


        private void OnTriggerEnter(Collider other)
        {
            if (OnEnter != null)
                OnEnter.Invoke(other);
        }

        private void OnTriggerExit(Collider other)
        {
            if (OnExit != null)
                OnExit.Invoke(other);

        }
    }

}

