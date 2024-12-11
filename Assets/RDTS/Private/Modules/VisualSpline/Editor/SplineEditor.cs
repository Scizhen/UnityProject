using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;



namespace VisualSpline
{

    [CustomEditor(typeof(Spline))]
    public class SplineEditor : Editor
    {
        Spline _target;

        void OnEnable()
        {
            _target = target as Spline;
        }


        //Inspector�������:
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();//����Ĭ�Ͻ���
            DrawAddButton();//������ư�ť
        }


        //Gizmo Overload:
        [DrawGizmo(GizmoType.Selected)]
        public static void RenderCustomGizmo(Transform objectTransform, GizmoType gizmoType)
        {
            DrawTools(objectTransform);
        }


        void DrawAddButton()
        {
            if (GUILayout.Button("Add line"))
            {
                _target.AddLine();
            }

            if (GUILayout.Button("Clear lines"))
            {
                _target.ClearLines();
            }
        }


        public static void DrawTools(Transform target)
        {
            Spline spline = target.GetComponent<Spline>();//����
            if (spline == null) return;
            if (target.transform.childCount == 0) return;

            spline.ConfigureLineInformation();
            List<SplinePoint> points = spline.Points;//��ȡê��

            Handles.color = spline.color;

            for (int i = 0; i < points.Count; i++)
            {
                Quaternion lookRotation = Quaternion.identity;
                SplinePoint currentPoint = points[i];

                /* //scale
                currentPoint.Point.localScale = Vector3.one * (spline.toolScale * 2.1f);
                */

                //����ê��
                if (spline.toolScale > 0)
                {
                    if (SceneView.currentDrawingSceneView != null)
                    {
                        lookRotation = Quaternion.LookRotation((SceneView.currentDrawingSceneView.camera.transform.position - currentPoint.Point.position).normalized);
                        Handles.SphereHandleCap(0, currentPoint.Point.position, lookRotation, spline.toolScale * .35f, EventType.Repaint);
                    }

                }

            }

            spline.RefreshLinesList();
            //��������
            spline.lines.ForEach(line =>
            {
                SplinePoint startPoint, endPoint;
                if (!line.reverse)
                {
                    startPoint = line.point1;
                    endPoint = line.point2;
                }
                else
                {
                    startPoint = line.point2;
                    endPoint = line.point1;
                }
                SplineTangent sourcePoint, destinationPoint;
                sourcePoint = line.sourceControl;
                destinationPoint = line.destinationControl;

                //����ͨ���������ߵ������յ���������������ߡ�
                if (line.lineType == LineType.Bezier)
                    Handles.DrawBezier(startPoint.Point.position, endPoint.Point.position, sourcePoint.transform.position, destinationPoint.transform.position, line.color, null, line.lineWidth);

                Handles.color = line.color;
                //����ֱ��
                if (line.lineType == LineType.Straight)
                    Handles.DrawLine(startPoint.Point.position, endPoint.Point.position, line.lineWidth);
            });

            ///�ڶ�ά�ռ���
            if (spline.spatialDimension == SpatialDimension.TwoDimensional)
            {
                List<Vector3> positions = new List<Vector3>();//�洢ê���λ������
                List<Vector3> points_2D = new List<Vector3>();//�洢Ͷ�䵽��άƽ���ê������

                //ԭ��
                Vector3 origin = spline.gameObject.transform.position;
                //ê��
                points.ForEach(point =>
                {
                    //��ȡê���λ������
                    Vector3 thisPosition = point.gameObject.transform.position;
                    positions.Add(thisPosition);
                    //��ȡͶ�䵽��άƽ���ê������
                    Vector3 newPoint_2D = new Vector3(thisPosition.x, origin.y, thisPosition.z);
                    points_2D.Add(newPoint_2D);
                    point.gameObject.transform.position = newPoint_2D;
                    point.InTangent.position = new Vector3(point.InTangent.position.x, origin.y, point.InTangent.position.z);
                    point.OutTangent.position = new Vector3(point.OutTangent.position.x, origin.y, point.OutTangent.position.z);
                });

                ///���ƶ�ά����ƽ�棨���ڹ۲죩��
                //�Ƚϵó��������λ����Ϣ
                float maxX, minX, maxZ, minZ;
                maxX = minX = positions[0].x;
                maxZ = minZ = positions[0].z;
                for (int i = 1; i < positions.Count; i++)
                {
                    Vector3 pos = positions[i];
                    if (pos.x > maxX) maxX = pos.x;
                    if (pos.x < minX) minX = pos.x;
                    if (pos.z > maxZ) maxZ = pos.z;
                    if (pos.z < minZ) minZ = pos.z;
                }
                ///Debug.Log($"maxX:{maxX} + minX:{minX} + maxZ:{maxZ} + minZ:{minZ}");
                float redundancy = 0.5f;//�����Ե���
                Vector3[] verts = new Vector3[]
                {
                    new Vector3(minX-redundancy, origin.y, minZ-redundancy),
                    new Vector3(minX-redundancy, origin.y, maxZ+redundancy),
                    new Vector3(maxX+redundancy, origin.y, maxZ+redundancy),
                    new Vector3(maxX+redundancy, origin.y, minZ-redundancy),
                };
                //���Ƴ���ά�ľ���ƽ�棨�ɰ������е�ê�㣩
                Handles.color = new Color(0.8f, 0.8f, 0.8f, 0.5f);
                Handles.DrawSolidRectangleWithOutline(verts, new Color(0.4f, 0.7f, 0.7f, 0.1f), new Color(0.6f, 0.6f, 0.6f, 0.1f));

            }


        }



    }

}
