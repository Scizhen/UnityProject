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
    /// 在Value的Inspector面板中添加“Value Connection Info”的字段
    /// </summary>
    [CustomEditor(typeof(Value), true)]//CustomEditor：定义自定义编辑器类可以编辑的对象类型
    public class ValueInspectorWindow : UnityEditor.Editor
    {

        bool show = true;

        /// <summary>
        /// 实现此函数以创建自定义检视面板；
        /// 在此函数中，可以为特定对象类的检视面板添加自己的基于 IMGUI 的自定义 GUI。
        /// </summary>
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();//绘制内置检视面板
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
