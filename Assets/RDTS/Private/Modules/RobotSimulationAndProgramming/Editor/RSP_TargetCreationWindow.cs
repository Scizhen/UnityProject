using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RDTS.Utility;


namespace RDTS.RobotSimulationProgramming
{
    public class RSP_TargetCreationWindow : EditorWindow
    {
        RSP_ProgrammingWindow _programmingWindow;//对应的编程窗口
        private string _targetName = "";//名称
        public string targetName { get { return _targetName; } }//供外部读取此对象名称
        private RSP_Target _script;//目标点关联的对象
        private RSP_RobotController _robotController;//关联的机器人控制器组件
        private GameObject _posture;//目标点创建时的位姿（position、rotation）


        void OnGUI()
        {
            GUILayout.Space(5);
            GUILayout.Label("填入以下信息以添加目标点：", labelStyle);
            GUILayout.Space(5);

            GUILayout.BeginHorizontal(GUILayout.Height(25));
            GUILayout.Space(30);
            GUILayout.Label("目标点名称：", GUILayout.Width(position.width / 3), GUILayout.Height(25));
            GUILayout.FlexibleSpace();
            _targetName = GUILayout.TextField(_targetName, GUILayout.Width(position.width / 2), GUILayout.Height(20));
            GUILayout.Space(30);
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            GUILayout.BeginHorizontal(GUILayout.Height(25));
            GUILayout.Space(30);
            GUILayout.Label("关联的对象：", GUILayout.Width(position.width / 3), GUILayout.Height(25));
            GUILayout.FlexibleSpace();
            _script = (RSP_Target)EditorGUILayout.ObjectField(_script, typeof(RSP_Target), true, GUILayout.Width(position.width / 2), GUILayout.Height(20));
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
            GUILayout.Label("创建的位姿：", GUILayout.Width(position.width / 3), GUILayout.Height(25));
            GUILayout.FlexibleSpace();
            _posture = (GameObject)EditorGUILayout.ObjectField(_posture, typeof(GameObject), true, GUILayout.Width(position.width / 2), GUILayout.Height(20));
            GUILayout.Space(30);
            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Create", GUILayout.Width(60), GUILayout.Height(28), GUILayout.ExpandWidth(true)))
            {
                if (this._programmingWindow != null)
                    this._programmingWindow.CreateNewTarget(_targetName, _script, _robotController, _posture?.transform, true);

                //QM.Log($"create new target: {_targetName}");
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
                this._posture = programmingWindow.robotController.endPoint ?? null;
            }

        }



    }

}
