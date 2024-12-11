using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace VisualSpline
{
    [CustomEditor(typeof(SplineTangent))]
    public class SplineTangentEditor : Editor
    {

        void OnSceneGUI()
        {
            //ensure pivot is used so anchor selection has a proper transform origin:
            if (Tools.pivotMode == PivotMode.Center)
            {
                Tools.pivotMode = PivotMode.Pivot;
            }
        }

        //Gizmos:
        [DrawGizmo(GizmoType.Selected)]
        static void RenderCustomGizmo(Transform objectTransform, GizmoType gizmoType)
        {
            if (objectTransform.parent != null)
            {
                SplinePointEditor.RenderCustomGizmo(objectTransform.parent, gizmoType);
            }
        }

    }

}
