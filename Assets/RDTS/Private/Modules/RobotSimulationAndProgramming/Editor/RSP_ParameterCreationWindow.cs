using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;



namespace RDTS.RobotSimulationProgramming
{
    public class RSP_ParameterCreationWindow : EditorWindow
    {
        RSP_ProgrammingWindow _programmingWindow;//对应的编程窗口
        private string _parameterName = "";//名称
        private RSP_RobotController _robotController;//关联的机器人控制器组件

        private ParameterType _parameterType;//参数类型


        void OnGUI()
        {
            GUILayout.Space(5);
            GUILayout.Label("填入以下信息以添加目标点：", labelStyle);
            GUILayout.Space(5);

            GUILayout.BeginHorizontal(GUILayout.Height(25));
            GUILayout.Space(30);
            GUILayout.Label("目标点名称：", GUILayout.Width(position.width / 3), GUILayout.Height(25));
            GUILayout.FlexibleSpace();
            _parameterName = GUILayout.TextField(_parameterName, GUILayout.Width(position.width / 2), GUILayout.Height(20));
            GUILayout.Space(30);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.Height(25));
            GUILayout.Space(30);
            GUILayout.Label("关联的控制器：", GUILayout.Width(position.width / 3), GUILayout.Height(25));
            GUILayout.FlexibleSpace();
            _robotController = (RSP_RobotController)EditorGUILayout.ObjectField(_robotController, typeof(RSP_RobotController), true, GUILayout.Width(position.width / 2), GUILayout.Height(20));
            GUILayout.Space(30);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.Height(25));
            GUILayout.Space(30);
            GUILayout.Label("参数类型", GUILayout.Width(position.width / 3), GUILayout.Height(25));
            GUILayout.FlexibleSpace();
            _parameterType = (ParameterType)EditorGUILayout.EnumPopup(_parameterType, GUILayout.Width(position.width / 2), GUILayout.Height(20));
            GUILayout.Space(30);
            GUILayout.EndHorizontal();


            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Create", GUILayout.Width(60), GUILayout.Height(28), GUILayout.ExpandWidth(true)))
            {
                if (this._programmingWindow != null)
                {
                    if (_robotController != null)
                    {
                        this._programmingWindow.CreateNewParameter(_parameterName, _robotController, _parameterType, true);
                    }
                    else
                        EditorUtility.DisplayDialog("关联警告", "未关联到有效的控制器组件", "yes");


                }


                this.Close();

            }


        }


        private GUISkin skin_rsp;
        private GUIStyle labelStyle;//label元素的GUI style
        void OnEnable()
        {
            skin_rsp = Resources.Load<GUISkin>("GUISkinRSP");

            labelStyle = skin_rsp.customStyles[6];

        }


        public void LinkProgrammingWindow(RSP_ProgrammingWindow programmingWindow)
        {
            this._programmingWindow = programmingWindow;
            if (programmingWindow.robotController != null)
            {
                this._robotController = programmingWindow.robotController;

            }

        }


    }

}
