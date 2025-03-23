using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using UnityEditor;
using log4net.Util;
using Transform = UnityEngine.Transform;

namespace RDTS
{
    //运输机表面的传感器控制脚本
    public class SensorConveyor : LibraryObject
    {
        [Header("Sensor Conveyor")]
        [OnValueChanged("Modify")]
        public bool InheritFromParent;

        [OnValueChanged("Modify")] public float SensorHeight;
        [OnValueChanged("Modify")] public float SensorPosOnConveyor;

        [OnValueChanged("Modify")]
        [HideIf("InheritFromParent")]
        public float SensorLength;

        [OnValueChanged("Modify")] public float SensorAngle;

        [Header("Display Options")]
        [OnValueChanged("Modify")]
        public bool HideSensor;

        [OnValueChanged("Modify")] public bool HousingLeft;
        [OnValueChanged("Modify")] public bool HousingRight;

        [HideInInspector] public Vector3 SensorEnd;

        [Header("Simulation")]
        [OnValueChanged("Modify")]
        public bool SetTag;

        const float housingdeltay = -0.074f;
        const float housingdeltax = 0.031f;


        public override void OnParentModify()
        {
            if (InheritFromParent)
                Modify();
        }

        public override void OnParentChanged()
        {
            InheritFromParent = false;
            if (Parent != null)
                if (Parent.GetComponent<IConveyor>() != null)
                    InheritFromParent = true;

            Modify();
        }

        public override void OnModify()
        {
            var height = 0.0f;
            IConveyor conv = null;
            if (Parent != null)
                conv = Parent.GetComponent<IConveyor>();

            if (conv != null && InheritFromParent)
            {
                BottomHeight = conv.GetBottomHeight();
                SensorLength = conv.GetWidth();
                height = SensorHeight;
            }

            if (Parent == null)
                height = BottomHeight + SensorHeight;


            if (Parent != null && InheritFromParent)
            {
                if (conv != null)
                {
                    var poss = SensorPosOnConveyor;
                    if (poss < 0)
                        poss = conv.GetLength() + poss;
                    var pos = conv.GetPosition(poss);
                    var rot = conv.GetRotation(poss).eulerAngles;
                    transform.localRotation = Quaternion.Euler(-rot.z, rot.y + 90 + SensorAngle, rot.x);
                    var sensordir = transform.right;
                    var localsensordir = Parent.transform.InverseTransformDirection(sensordir);
                    var posmid = new Vector3(pos.x, pos.y + SensorHeight, pos.z);
                    SensorLength = SensorLength / Mathf.Cos((Mathf.PI / 180) * SensorAngle);
                    transform.localPosition = (posmid - localsensordir * SensorLength / 2);
                }
            }
            else
            {
                transform.localPosition = new Vector3(transform.localPosition.x, BottomHeight + SensorHeight,
                transform.localPosition.z);
            }

            SensorEnd = transform.localPosition + new Vector3(SensorLength, 0, 0);

            var sensor = GetComponent<Sensor>();
            sensor.RayCastLength = SensorLength * 1000;


            // Housing
            var housingleft = GetComponentByName<Transform>("HousingLeft");
            var housingright = GetComponentByName<Transform>("HousingRight");
            housingleft.gameObject.SetActive(HousingLeft);
            housingright.gameObject.SetActive(HousingRight);
            housingleft.transform.localPosition = new Vector3(-housingdeltax, housingdeltay, 0);
            housingleft.transform.localRotation = Quaternion.Euler(0, -180 - SensorAngle, 0);
            housingright.transform.localPosition = new Vector3(housingdeltax + SensorLength, housingdeltay, 0);
            housingright.transform.localRotation = Quaternion.Euler(0, -SensorAngle, 0);

#if UNITY_EDITOR
            EditorUtility.SetDirty(sensor);
#endif
        }

        public override bool HideAllowed(GameObject gameObject)
        {
            return false;
        }

        private void SensorOnEventEnter(GameObject obj)
        {
            if (SetTag)
                obj.tag = this.tag;
        }

        public void Start()
        {
            var sensor = GetComponent<Sensor>();
            sensor.EventEnter += SensorOnEventEnter;
        }
    }
}