using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RDTS.Utility
{
    public class BLT_Loader : MonoBehaviour
    {
        public ValueMiddleBool StartLoad;
        public ValueMiddleBool EndLoad;

        private bool _lastStartLoad;
        private bool _lastEndLoad;

        private Sensor Get_sensor;
        private Rigidbody[] Get_Rigidbody;

        private GameObject LoadObject;
        public GameObject AfterLoadObject;
        // Start is called before the first frame update
        void Start()
        {
            _lastStartLoad = false;
            _lastEndLoad = false;
            StartLoad.Value = false;
            EndLoad.Value = false;
        }

        // Update is called once per frame
        void Update()
        {

            GetLoadObject();
            if (StartLoad != null && StartLoad.Value && !_lastStartLoad)
            {
                LoadObject.transform.SetParent(this.gameObject.transform);
                GetLoadObject();
                Get_Rigidbody[6].isKinematic = true;
            }
            if (EndLoad != null && EndLoad.Value && !_lastEndLoad)
            {
                LoadObject.transform.SetParent(AfterLoadObject.transform);
                Get_Rigidbody[6].isKinematic = false;
                LoadObject = null;
            }
            if (StartLoad != null) _lastStartLoad = StartLoad.Value;
            if (EndLoad != null) _lastEndLoad = EndLoad.Value;
        }

        private void GetLoadObject()
        {
            Get_sensor = GetComponentInChildren<Sensor>();
            Get_Rigidbody = GetComponentsInChildren<Rigidbody>();
            if (Get_sensor != null)
            {
                LoadObject = Get_sensor.LastTriggeredBy;
            
            }
        }
    }
}

