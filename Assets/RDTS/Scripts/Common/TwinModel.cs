using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System;

namespace RDTS
{
    public class TwinModel : BaseTwinModel
    {
        [Serializable]
        public class ModelSize
        {
            public float Length;//长
            public float Width;//宽
            public float Height;//高

        }

        [Serializable]
        public class DHParameter
        {
            public string link;
            public float alpha_i;
            public float d;
            public float theta;
            public JointRange jointRange;
        }

        [Serializable]
        public class JointRange
        {
            public float MinValue;
            public float MaxValue;
        }

        public TwinModelHierarchy Level;//模型层次
        public TwinModelType Type;//模型类型 

        [ResizableTextArea]
        public string FunctionAndUse;//功能与用途
        [ResizableTextArea]
        public string DeviceBrand;//设备品牌或型号

        public ModelSize modelSize;//模型尺寸

        [ShowIf("Type", TwinModelType.RobotArm)]
        public List<DHParameter> RobotDHParameter;

        [ShowIf("Type", TwinModelType.RobotArm)]
        public List<Drive> RobotAxis;

    }
}