using System;
using UnityEngine;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEditor;

namespace RDTS
{
    //运输机中分拣机的模型缩放
    public class TransferChain : LibraryPart
    {
        [OnValueChanged("Modify")] public float Diameter = 1;
        [OnValueChanged("Modify")] public float Length = 1;

        const float meshdiameter = 1.0f;
        const float meshlength = 5.0f;
        const float meshwidth = 0.2f;

        [Button("Update")]
        public override void Modify()
        {
            transform.localScale = new Vector3(Length * 1 / meshlength, Diameter * 1 / meshdiameter, meshwidth);

        }


    }
}

