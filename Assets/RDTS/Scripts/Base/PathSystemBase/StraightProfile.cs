using UnityEngine;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEditor;

//直式运输机俩边挡板大小控制脚本

public class StraightProfile : LibraryPart
{

    public enum ScaleDir
    {
        X,
        Y,
        Z
    };

    [OnValueChanged("Modify")] public float Length = 1;
    [OnValueChanged("Modify")] public float Height = 1;

    [OnValueChanged("Modify")] public float MeshLenght = 1;
    [OnValueChanged("Modify")] public ScaleDir LengthDirection;

    [OnValueChanged("Modify")] public float MeshHeight = 1;
    [OnValueChanged("Modify")] public ScaleDir HeihgtDirection;


    [Button("Update")]
    public override void Modify()
    {
        var scalelength = 1 / MeshLenght * Length;
        var scaleheight = 1 / MeshHeight * Height;

        switch (LengthDirection)
        {
            case ScaleDir.X:
                {
                    transform.localScale = new Vector3(scalelength, 1, 1);
                    break;
                }
            case ScaleDir.Y:
                {
                    transform.localScale = new Vector3(1, scalelength, 1);
                    break;
                }
            case ScaleDir.Z:
                {
                    transform.localScale = new Vector3(1, 1, scalelength);
                    break;
                }
        }
        switch (HeihgtDirection)
        {
            case ScaleDir.X:
                {
                    transform.localScale = new Vector3(scaleheight, transform.localScale.y, transform.localScale.z);
                    break;
                }
            case ScaleDir.Y:
                {
                    transform.localScale = new Vector3(transform.localScale.x, scaleheight, transform.localScale.z);
                    break;
                }
            case ScaleDir.Z:
                {
                    transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, scaleheight);
                    break;
                }
        }
    }
}
