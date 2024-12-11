using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RDTS.Utility
{
    public class BLT_Catcher : MonoBehaviour
    {
        public ValueOutputBool PickSignal;
        public ValueOutputBool PlaceSignal;

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
            PickSignal.Value = false;
            PlaceSignal.Value = false;
        }

        // Update is called once per frame
        void Update()
        {

            GetLoadObject();
            if (PickSignal != null && PickSignal.Value && !_lastStartLoad)
            {
                LoadObject.transform.SetParent(this.gameObject.transform);
                GetLoadObject();
                Get_Rigidbody[16].isKinematic = true;
            }
            if (PlaceSignal != null && PlaceSignal.Value && !_lastEndLoad)
            {
                LoadObject.transform.SetParent(AfterLoadObject.transform);
                Get_Rigidbody[16].isKinematic = false;
                LoadObject = null;
            }
            if (PickSignal != null) _lastStartLoad = PickSignal.Value;
            if (PlaceSignal != null) _lastEndLoad = PlaceSignal.Value;
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

