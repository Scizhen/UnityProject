using UnityEngine;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEditor;

public class LegProfile : LibraryPart
{
    //����������׽Ǵ�С�Ľű�
    [OnValueChanged("Modify")] public float Height = 1;


    [Button("Update")]
    public override void Modify()
    {
        transform.localScale = new Vector3(1, Height, 1);
    }
}
