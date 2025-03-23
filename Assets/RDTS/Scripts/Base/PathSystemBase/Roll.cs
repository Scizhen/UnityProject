using System;
using UnityEngine;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEditor;
using RDTS;

//运输机上的滚轮基类脚本

namespace RDTS
{
    public class Roll : LibraryPart
    {

        [OnValueChanged("Modify")] public float Diameter = 1;
        [OnValueChanged("Modify")] public float Width = 1;
        public Drive DirectionDrive;
        public Drive RotationDrive;

        private Vector3 StartRotation;
        private bool directiondrivenotnull;
        private bool rotationdrivenotnull;
        private float rollumfang;

        [Button("Update")]
        public override void Modify()
        {
            transform.localScale = new Vector3(Diameter, Width / 2, Diameter);

        }

        private void Start()
        {
            StartRotation = transform.localRotation.eulerAngles;
            directiondrivenotnull = DirectionDrive != null;
            rotationdrivenotnull = RotationDrive != null;

            if (!directiondrivenotnull && !rotationdrivenotnull)
                gameObject.SetActive(false);
            rollumfang = Mathf.PI * Diameter * 1000;
        }

        public void Update()
        {
            if (directiondrivenotnull)
                transform.localRotation = Quaternion.Euler(StartRotation.x, StartRotation.y, StartRotation.z - DirectionDrive.CurrentPosition);
            if (rotationdrivenotnull)
            {
                var length = Time.deltaTime * RotationDrive.CurrentSpeed / RotationDrive.SpeedScaleTransportSurface;
                var deltaangle = length / rollumfang * 360;
                transform.Rotate(0, -deltaangle, 0);
            }
        }
    }
}

