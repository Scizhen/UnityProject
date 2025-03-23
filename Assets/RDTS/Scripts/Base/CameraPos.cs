//*************************************************************************
//Thanks for the code reference game4automation provides.                 *
//                                                                        *
//*************************************************************************  
using UnityEngine;

namespace RDTS
{
    /// <summary>
    /// 可通过点击菜单来创建默认名为“CameraPos”的asset文件，用来存储相机位姿的信息
    /// </summary>
    [CreateAssetMenu(fileName = "CameraPos", menuName = "Parallel-RDTS/Add Camera Position", order = 1)]
    //! Scriptable object for saving camera positions (user views)
    public class CameraPos : ScriptableObject
    {
        public string Description;
        public Vector3 CameraRot;
        public Vector3 TargetPos;
        public float CameraDistance;


        public void SaveCameraPosition(SceneMouseNavigation mousenav)
        {
            CameraRot = mousenav.currentRotation.eulerAngles;
            CameraDistance = mousenav.currentDistance;
            TargetPos = mousenav.target.position;
        }
    }

}

