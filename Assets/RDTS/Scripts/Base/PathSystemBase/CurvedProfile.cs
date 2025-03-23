using UnityEngine;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEditor;

public class CurvedProfile : LibraryPart
{
    //可能是生成弯曲运输机表面的绘制脚本
    [OnValueChanged("Modify")] public float StartAngle = 0;


    [OnValueChanged("Modify")] public float EndAngle = 180;

    [OnValueChanged("Modify")] public bool Clockwise = false;

    [OnValueChanged("Modify")] public float Radius = 0.1f;

    [OnValueChanged("Modify")] public float DeltaHeight = 0.0f;

    [OnValueChanged("Modify")] public bool Flip;

    [OnValueChanged("Modify")] public float XScale = 1f;
    [OnValueChanged("Modify")] public float YScale = 1f;


    [OnValueChanged("Modify")] public int IntermediatePoints = 1;

    [OnValueChanged("Modify")] public Vector3 ProfileRotation = new Vector3(0, 0, 0);

    public void setStartAngle(float pStartAngle)
    {
        StartAngle = pStartAngle;
    }

    public void setEndAngle(float pEndAngle)
    {
        EndAngle = pEndAngle;
    }

    public void setRadius(float pRadius)
    {
        Radius = pRadius;
    }

    private List<Vector3> positions;
    private List<Vector3> rotations;

    [Button("Update")]
    public override void Modify()
    {
        positions = LibraryUtils.CalculateArcPos(Vector3.zero, Radius, StartAngle, EndAngle, DeltaHeight,
            IntermediatePoints + 2, Flip, Clockwise);
        rotations = LibraryUtils.CalculateArcRot(StartAngle, EndAngle, Radius, DeltaHeight, IntermediatePoints + 2,
            ProfileRotation, Flip, Clockwise);
        var splineMesh = gameObject.GetComponent<SplineMesh>();
        var spline = gameObject.GetComponent<SuperSpline>();

        // Update if different number of nodes
        if (spline.SplineNodes.Length != IntermediatePoints + 2)
        {
            UnpackPrefab();

            var allnodes = GetComponentsInChildren<SplineNode>();
            foreach (var node in allnodes)
            {
                spline.RemoveSplineNode(node);
                DestroyImmediate(node.gameObject);
            }

            var inp = spline.AddSplineNode();
            inp.transform.parent = this.transform;
            inp.name = "In";
            for (int i = 0; i < IntermediatePoints; i++)
            {
                var newpoint = spline.AddSplineNode();
                newpoint.transform.parent = this.transform;
                newpoint.name = "Middle";
            }

            var outp = spline.AddSplineNode();
            outp.transform.parent = this.transform;
            outp.name = "Out";
        }

        splineMesh.xyScale.x = XScale;
        splineMesh.xyScale.y = YScale;
        LibraryUtils.SetArc(gameObject, positions, rotations, Radius, StartAngle, EndAngle);
    }
}
