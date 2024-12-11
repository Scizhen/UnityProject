using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;



namespace VisualSpline
{
    [CustomEditor(typeof(SplinePoint))]
    public class SplinePointEditor : Editor
    {

        SplinePoint _target;

        void OnEnable()
        {
            _target = target as SplinePoint;
        }


        //Gizmo Overload:
        [DrawGizmo(GizmoType.Selected)]
        public static void RenderCustomGizmo(Transform objectTransform, GizmoType gizmoType)
        {
            if (objectTransform.parent != null)
            {
                SplineEditor.RenderCustomGizmo(objectTransform.parent, gizmoType);
            }
            DrawTools(objectTransform);
        }



        //Inspector界面绘制:
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();//绘制默认界面
            DrawAddButton();//额外绘制按钮
        }



        void OnSceneGUI()
        {
            DrawTools((target as SplinePoint).transform);
        }


        void DrawAddButton()
        {
            if (GUILayout.Button("Add Point"))
            {
                _target.AddSplinePoint(1);
            }
        }



        static void DrawTools(Transform target)
        {
            SplinePoint splinePoint = target.GetComponent<SplinePoint>();
            if (splinePoint == null) return;
            if (!splinePoint.isComplete) return;

            if (splinePoint.parentSpline == null || splinePoint.parentSpline.color == null)
                Handles.color = Color.green;
            else
                Handles.color = splinePoint.parentSpline.color;

            float toolScale = 0.1f;
            if (splinePoint.parentSpline != null)
                toolScale = splinePoint.parentSpline.toolScale;


            Quaternion lookRotation = Quaternion.identity;

            /* //scale
            splinePoint.InTangent.localScale = Vector3.one * (toolScale * 1.3f);
            splinePoint.OutTangent.localScale = Vector3.one * (toolScale * 1.3f);
            splinePoint.Point.localScale = Vector3.one * (toolScale * 2.1f);
            */

            //绘制切线
            if (toolScale > 0)
            {
                //绘制面向场景视图相机的持久标识符，并且仅在相应的切线处于活动状态时才绘制：
                if (splinePoint.OutTangent.gameObject.activeSelf)
                {
                    //connection:
                    Handles.DrawDottedLine(splinePoint.Point.position, splinePoint.OutTangent.position, 3);//绘制虚线

                    //indicators:
                    if (SceneView.currentDrawingSceneView != null)//（2个）圆形手柄
                    {
                        lookRotation = Quaternion.LookRotation((SceneView.currentDrawingSceneView.camera.transform.position - splinePoint.OutTangent.position).normalized);
                        Handles.CircleHandleCap(0, splinePoint.OutTangent.position, lookRotation, toolScale * .25f, EventType.Repaint);
                    }
                }
                //绘制面向场景视图相机的持久标识符，并且仅在相应的切线处于活动状态时才绘制：
                if (splinePoint.InTangent.gameObject.activeSelf)
                {
                    //connection:
                    Handles.DrawDottedLine(splinePoint.Point.position, splinePoint.InTangent.position, 3);//绘制虚线

                    //indicators:
                    if (SceneView.currentDrawingSceneView != null)//（2个）圆形手柄
                    {
                        lookRotation = Quaternion.LookRotation((SceneView.currentDrawingSceneView.camera.transform.position - splinePoint.InTangent.position).normalized);
                        Handles.CircleHandleCap(0, splinePoint.InTangent.position, lookRotation, toolScale * .25f, EventType.Repaint);
                    }
                }

                //若锚点无父对象，则需要在锚点处自行绘制
                if (splinePoint.parentSpline == null)
                {
                    if (SceneView.currentDrawingSceneView != null)
                    {
                        lookRotation = Quaternion.LookRotation((SceneView.currentDrawingSceneView.camera.transform.position - splinePoint.Point.position).normalized);
                        Handles.SphereHandleCap(0, splinePoint.Point.position, lookRotation, toolScale * .35f, EventType.Repaint);
                    }
                }


            }


        }


    }

}