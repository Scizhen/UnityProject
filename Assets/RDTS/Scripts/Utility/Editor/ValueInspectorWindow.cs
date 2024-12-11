//*************************************************************************
//Thanks for the code reference game4automation provides.                 *
//                                                                        *
//*************************************************************************
using UnityEditor;
using UnityEngine;


namespace RDTS.Utility.Editor
{
#if UNITY_EDITOR
    /// <summary>
    /// ��Value��Inspector�������ӡ�Value Connection Info�����ֶ�
    /// </summary>
    [CustomEditor(typeof(Value), true)]//CustomEditor�������Զ���༭������Ա༭�Ķ�������
    public class ValueInspectorWindow : UnityEditor.Editor
    {

        bool show = true;

        /// <summary>
        /// ʵ�ִ˺����Դ����Զ��������壻
        /// �ڴ˺����У�����Ϊ�ض�������ļ����������Լ��Ļ��� IMGUI ���Զ��� GUI��
        /// </summary>
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();//�������ü������
            Value value = (Value)target;
            float win = Screen.width;
            float w1 = win * 0.5f;
            float w2 = win * 0.5f;

            show = EditorGUILayout.Foldout(show, "Value Connection Info");
            if (show)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(15);
                GUILayout.Label("Behavior", GUILayout.Width(w1));
                GUILayout.Label("Connection", GUILayout.Width(w2));
                GUILayout.EndHorizontal();
                foreach (var valueinfo in value.ConnectionInfo)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(15);
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.ObjectField(valueinfo.Behavior, typeof(Object), false, GUILayout.Width(w1));
                    EditorGUI.EndDisabledGroup();
                    GUILayout.Label(valueinfo.ConnectionName, GUILayout.Width(w2));
                    GUILayout.EndHorizontal();
                }



            }
        }


    }



#endif
}
