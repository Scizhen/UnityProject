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
            public float Length;//��
            public float Width;//��
            public float Height;//��

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

        public TwinModelHierarchy Level;//ģ�Ͳ��
        public TwinModelType Type;//ģ������ 

        [ResizableTextArea]
        public string FunctionAndUse;//��������;
        [ResizableTextArea]
        public string DeviceBrand;//�豸Ʒ�ƻ��ͺ�

        public ModelSize modelSize;//ģ�ͳߴ�

        [ShowIf("Type", TwinModelType.RobotArm)]
        public List<DHParameter> RobotDHParameter;

        [ShowIf("Type", TwinModelType.RobotArm)]
        public List<Drive> RobotAxis;

    }
}