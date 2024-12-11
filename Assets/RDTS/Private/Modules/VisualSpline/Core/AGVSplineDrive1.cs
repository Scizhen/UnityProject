using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

namespace VisualSpline
{
    [RequireComponent(typeof(SplineDrive))]
    //AGV����������������
    public class AGVSplineDrive1 : MonoBehaviour
    {
        public SplinePoint nextPoint;//��������ʼ��
        public bool goToNextPoint;
        private SplineDrive splinedrive;
        // Start is called before the first frame update
        void Start()
        {
            splinedrive = this.GetComponent<SplineDrive>();
            splinedrive.axisRotate.isRotate = true;
        }
         private float timer = 0f;
        // Update is called once per frame
        void Update()
        {

            if (splinedrive.currentLine.isArrived == true)
            {
                if (goToNextPoint == true)
                {
                    goToNextPoint = false;
                    splinedrive.isDrive = false;
                    splinedrive.currentLine.percentage = 0.001f;
                    splinedrive.currentLine.startPoint = splinedrive.currentLine.endPoint;
                    splinedrive.currentLine.endPoint = nextPoint;
                    splinedrive.isDrive = true;

                }
            }
        }
    }
}

