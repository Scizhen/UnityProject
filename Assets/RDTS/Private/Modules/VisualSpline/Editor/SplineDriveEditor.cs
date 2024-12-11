using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;



namespace VisualSpline
{
    [CustomEditor(typeof(SplineDrive))]
    public class SplineDriveEditor : Editor
    {
        SplineDrive _target;

        void OnEnable()
        {
            _target = target as SplineDrive;
        }


        void OnSceneGUI()
        {
            if (_target != null && _target.associatedSpline != null)
                SplineEditor.DrawTools(_target.associatedSpline.transform);
        }



    }


}