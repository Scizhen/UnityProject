using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEditor;

namespace RDTS
{
    //��������ģ����أ��ж�������ײ�����¼�
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

