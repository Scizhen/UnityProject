using UnityEngine;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEditor;

public class LegProfile : LibraryPart
{
    //控制运输机底角大小的脚本
    [OnValueChanged("Modify")] public float Height = 1;


    [Button("Update")]
    public override void Modify()
    {
        transform.localScale = new Vector3(1, Height, 1);
    }
}
