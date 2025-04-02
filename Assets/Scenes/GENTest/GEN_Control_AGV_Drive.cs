using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System;

namespace VisualSpline
{
    [RequireComponent(typeof(AGVSplineDrive))]
    //�ýű���Ե��Ⱥ�ķ�������·�߹滮
    public class GEN_Control_AGV_Drive : MonoBehaviour
    {
        public enum StationStatus { Entering, Working, Leaving, Empty, Failure, Waiting };
        [ReadOnly] public StationStatus AGVStatus = StationStatus.Empty;
        //public int loadedPiece;
        public SplinePoint targetPoint;
        public PieceStateMachine carriedPiece;
        //public SplinePoint currentPoint;
        [ReadOnly] public AGVSplineDrive targetAGVDrive;
        private Spline map;

        //����AGV��״̬
        void Set_AGV_Status()
        {
            targetAGVDrive.isDrive = false;
            //targetAGVDrive.currentLine.startPoint = targetAGVDrive.currentLine.endPoint;
            targetAGVDrive.motionPath.Clear();
            targetAGVDrive.currentLine.index = 0;
        }
        //���ݵ����������ƶ����Ƚ���x�᷽���ϵ��ƶ����ٽ���y�᷽���ϵ��ƶ�
        void Cal_AGV_Motion_path(SplinePoint startPoint, SplinePoint endPoint)
        {
            string[] startPoint_name = startPoint.name.Split('-');//�ַ��ָx��y
            string[] endPoint_name = endPoint.name.Split('-');//

            int x_cal = int.Parse(startPoint_name[0]) - int.Parse(endPoint_name[0]);
            int y_cal = int.Parse(startPoint_name[1]) - int.Parse(endPoint_name[1]);
            for (int i = 0; i <= Mathf.Abs(x_cal); i++)
            {
                int step = x_cal > 0 ? -1 : 1;

                string name = (int.Parse(startPoint_name[0]) + i * step).ToString() + "-" + startPoint_name[1];
                SplinePoint thisPath = map.transform.Find(name).GetComponent<SplinePoint>();
                targetAGVDrive.motionPath.Add(thisPath);
            }
            for (int i = 1; i <= Mathf.Abs(y_cal); i++)
            {
                int step = y_cal > 0 ? -1 : 1;

                string name = endPoint_name[0] + "-" + (int.Parse(startPoint_name[1]) + i * step).ToString();
                SplinePoint thisPath = map.transform.Find(name).GetComponent<SplinePoint>();
                targetAGVDrive.motionPath.Add(thisPath);
            }
            targetAGVDrive.isDrive = true;

        }
        void Cal_AGV_Motion_path_straight(SplinePoint startPoint, SplinePoint endPoint)
        {
            targetAGVDrive.motionPath.Add(startPoint);
            targetAGVDrive.motionPath.Add(endPoint);
            targetAGVDrive.isDrive = true;
        }
        // Start is called before the first frame update
        void Start()
        {
            targetAGVDrive = this.GetComponent<AGVSplineDrive>();
            map = targetAGVDrive.associatedSpline;
            
        }
        // Update is called once per frame
        void Update()
        {
            if (targetAGVDrive.motionPath.Count == 0 || targetAGVDrive.motionPath[targetAGVDrive.motionPath.Count - 1] != targetPoint)
            {
                if (targetPoint is not null)
                {
                    Set_AGV_Status();
                    //Cal_AGV_Motion_path(targetAGVDrive.currentLine.endPoint, targetPoint);
                    Cal_AGV_Motion_path_straight(targetAGVDrive.currentLine.endPoint, targetPoint);

                }
            }
            //if ((AGVStatus == StationStatus.Leaving && carriedPiece != null )|| AGVStatus == StationStatus.Entering)
            //{
            //    if ( targetPoint == targetAGVDrive.currentLine.endPoint && targetAGVDrive.currentLine.percentage == 1)
            //    {
            //        AGVStatus = StationStatus.Waiting;
            //    }
            //}

        }
    }
}


