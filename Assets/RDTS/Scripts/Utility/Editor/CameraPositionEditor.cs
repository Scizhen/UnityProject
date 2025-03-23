
using UnityEditor;
using UnityEngine;

namespace RDTS.Utility.Editor
{
#if UNITY_EDITOR
    [CustomEditor(typeof(CameraPosition))]
    //! Editor class for Get Position and SetPosition of CameraPosition
    //CameraPosition �� Get Position �� SetPosition �ı༭����
    public class CameraPositionEditor : UnityEditor.Editor
    {

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();//�� OnInspectorGUI �����е��ô˷������Զ����Ƽ�����塣 ���Ҫ���һЩ��ť���������»��������������

            CameraPosition myScript = (CameraPosition)target;
            //��CameraPosition�ű��еİ�ť�������
            if (GUILayout.Button("Get Position"))
            {
                myScript.GetCameraPosition();
            }
            if (GUILayout.Button("Set Position"))
            {
                myScript.SetCameraPosition();
            }
        }

    }
#endif
}