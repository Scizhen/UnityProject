using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;



namespace RDTS.RobotSimulationProgramming
{
    /// <summary>
    /// 识别目标点对象
    /// </summary>
    [ExecuteInEditMode]
    public class RSP_Target : MonoBehaviour
    {
        // public RSP_RobotController robotController;
        //[RDTS.Utility.ReadOnly] public string targetName = "new target";//目标点名称

        [HideInInspector] public string Guid = "";//唯一的GUID
        public bool isShowDetail = false;
        public Vector3 scale = new Vector3(1, 1, 1);//目标点缩放倍数
        [ResizableTextArea] public string remark = "";//备注信息

        private Transform targetObj;//要隐藏的目标点

        [Button]
        [Tooltip("设置目标点缩放大小")]
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
            //     tagetObject.hideFlags = HideFlags.HideInHierarchy;//将子对象隐藏

            foreach (Transform child in this.transform)
            {
                if (child.name == "TargetObject")
                    targetObj = child;
            }



            /// GetGuid();//获取唯一的GUID
        }


        // void SetGameObjName2TargetName()
        // {
        //     //this.name = targetName;
        // }

        // /// <summary>
        // /// 设置目标的名称
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