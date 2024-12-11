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


        //Inspector界面绘制:
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();//绘制默认界面
            DrawAddButton();//额外绘制按钮
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
            Spline spline = target.GetComponent<Spline>();//查找
            if (spline == null) return;
            if (target.transform.childCount == 0) return;

            spline.ConfigureLineInformation();
            List<SplinePoint> points = spline.Points;//获取锚点

            Handles.color = spline.color;

            for (int i = 0; i < points.Count; i++)
            {
                Quaternion lookRotation = Quaternion.identity;
                SplinePoint currentPoint = points[i];

                /* //scale
                currentPoint.Point.localScale = Vector3.one * (spline.toolScale * 2.1f);
                */

                //绘制锚点
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
            //绘制线条
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

                //绘制通过给定切线的起点和终点的纹理化贝塞尔曲线。
                if (line.lineType == LineType.Bezier)
                    Handles.DrawBezier(startPoint.Point.position, endPoint.Point.position, sourcePoint.transform.position, destinationPoint.transform.position, line.color, null, line.lineWidth);

                Handles.color = line.color;
                //绘制直线
                if (line.lineType == LineType.Straight)
                    Handles.DrawLine(startPoint.Point.position, endPoint.Point.position, line.lineWidth);
            });

            ///在二维空间下
            if (spline.spatialDimension == SpatialDimension.TwoDimensional)
            {
                List<Vector3> positions = new List<Vector3>();//存储锚点的位置坐标
                List<Vector3> points_2D = new List<Vector3>();//存储投射到二维平面的锚点坐标

                //原点
                Vector3 origin = spline.gameObject.transform.position;
                //锚点
                points.ForEach(point =>
                {
                    //获取锚点的位置坐标
                    Vector3 thisPosition = point.gameObject.transform.position;
                    positions.Add(thisPosition);
                    //获取投射到二维平面的锚点坐标
                    Vector3 newPoint_2D = new Vector3(thisPosition.x, origin.y, thisPosition.z);
                    points_2D.Add(newPoint_2D);
                    point.gameObject.transform.position = newPoint_2D;
                    point.InTangent.position = new Vector3(point.InTangent.position.x, origin.y, point.InTangent.position.z);
                    point.OutTangent.position = new Vector3(point.OutTangent.position.x, origin.y, point.OutTangent.position.z);
                });

                ///绘制二维矩形平面（便于观察）：
                //比较得出最外面的位置信息
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
                float redundancy = 0.5f;//冗余边缘宽度
                Vector3[] verts = new Vector3[]
                {
                    new Vector3(minX-redundancy, origin.y, minZ-redundancy),
                    new Vector3(minX-redundancy, origin.y, maxZ+redundancy),
                    new Vector3(maxX+redundancy, origin.y, maxZ+redundancy),
                    new Vector3(maxX+redundancy, origin.y, minZ-redundancy),
                };
                //绘制出二维的矩形平面（可包含所有的锚点）
                Handles.color = new Color(0.8f, 0.8f, 0.8f, 0.5f);
                Handles.DrawSolidRectangleWithOutline(verts, new Color(0.4f, 0.7f, 0.7f, 0.1f), new Color(0.6f, 0.6f, 0.6f, 0.1f));

            }


        }



    }

}
