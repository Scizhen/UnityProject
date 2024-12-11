
using UnityEditor;
using UnityEngine;

namespace RDTS.Utility.Editor
{
#if UNITY_EDITOR
    [CustomEditor(typeof(CameraPosition))]
    //! Editor class for Get Position and SetPosition of CameraPosition
    //CameraPosition 的 Get Position 和 SetPosition 的编辑器类
    public class CameraPositionEditor : UnityEditor.Editor
    {

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();//在 OnInspectorGUI 方法中调用此方法可自动绘制检视面板。 如果要添加一些按钮而不必重新绘制整个检视面板

            CameraPosition myScript = (CameraPosition)target;
            //将CameraPosition脚本中的按钮名称替代
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