//*************************************************************************
//Thanks for the code reference game4automation provides.                 *
//                                                                        *
//*************************************************************************  
using UnityEngine;

namespace RDTS
{
    /// <summary>
    /// ��ͨ������˵�������Ĭ����Ϊ��CameraPos����asset�ļ��������洢���λ�˵���Ϣ
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

