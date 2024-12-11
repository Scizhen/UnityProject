using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;



namespace RDTS.RobotSimulationProgramming
{
    /// <summary>
    /// ʶ��Ŀ������
    /// </summary>
    [ExecuteInEditMode]
    public class RSP_Target : MonoBehaviour
    {
        // public RSP_RobotController robotController;
        //[RDTS.Utility.ReadOnly] public string targetName = "new target";//Ŀ�������

        [HideInInspector] public string Guid = "";//Ψһ��GUID
        public bool isShowDetail = false;
        public Vector3 scale = new Vector3(1, 1, 1);//Ŀ������ű���
        [ResizableTextArea] public string remark = "";//��ע��Ϣ

        private Transform targetObj;//Ҫ���ص�Ŀ���

        [Button]
        [Tooltip("����Ŀ������Ŵ�С")]
        public void SetTargetScaleBuutton()
        {
            SetScaleOfTargetObj(scale);
        }


        void Reset()
        {
            Initialize();
        }

        void OnEnable()
        {
            Initialize();
        }

        void Update()
        {
            //if (this.name != targetName) SetGameObjName2TargetName();
            // if (isShowDetail) transform.GetChild(0).hideFlags = HideFlags.None;
            // else transform.GetChild(0).hideFlags = HideFlags.HideInHierarchy;

            if (isShowDetail)
            {
                if (targetObj != null) targetObj.hideFlags = HideFlags.None;
            }
            else
            {
                if (targetObj != null) targetObj.hideFlags = HideFlags.HideInHierarchy;
            }


        }


        void Initialize()
        {
            // Transform tagetObject = transform.GetChild(0);
            // if (tagetObject != null)
            //     tagetObject.hideFlags = HideFlags.HideInHierarchy;//���Ӷ�������

            foreach (Transform child in this.transform)
            {
                if (child.name == "TargetObject")
                    targetObj = child;
            }



            /// GetGuid();//��ȡΨһ��GUID
        }


        // void SetGameObjName2TargetName()
        // {
        //     //this.name = targetName;
        // }

        // /// <summary>
        // /// ����Ŀ�������
        // /// </summary>
        // /// <param name="name"></param>
        // public void SetTargetName(string name)
        // {
        //     if (name == "" || name == null)
        //         return;

        //     targetName = name;
        // }



        void GetGuid()
        {
            if (this.Guid == "" || this.Guid == null)
            {
                this.Guid = System.Guid.NewGuid().ToString();
            }

        }


        public void SetScaleOfTargetObj(Vector3 scale)
        {
            if (targetObj == null || scale.x <= 0f || scale.y <= 0f || scale.z <= 0f || scale == null)
                return;

            targetObj.localScale = scale;
        }






    }


}