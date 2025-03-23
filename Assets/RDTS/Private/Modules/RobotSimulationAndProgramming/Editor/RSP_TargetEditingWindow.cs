
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RDTS.Utility;


namespace RDTS.RobotSimulationProgramming
{
    public class RSP_TargetEditingWindow : EditorWindow
    {
        RSP_ProgrammingWindow _programmingWindow;//对应的编程窗口
        TargetObject editTarget;//要被编辑的目标点
        private string _targetName = "";//名称
        public string targetName { get { return _targetName; } }//供外部读取此对象名称
        private RSP_Target _targetScript;//目标点关联的对象


        void OnGUI()
        {
            GUILayout.Space(5);
            GUILayout.Label("填入以下信息以编辑目标点：", labelStyle);
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
            _targetScript = (RSP_Target)EditorGUILayout.ObjectField(_targetScript, typeof(RSP_Target), true, GUILayout.Width(position.width / 2), GUILayout.Height(20));
            GUILayout.Space(30);
            GUILayout.EndHorizontal();

            GUILayout.Space(30);

            if (GUILayout.Button("Edit", GUILayout.Width(60), GUILayout.Height(28), GUILayout.ExpandWidth(true)))
            {
                this._programmingWindow.EditTarget(editTarget, _targetName, _targetScript);
                //QM.Log($"Edit new target: {_targetName}");
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


        public void LinkProgrammingWindow(RSP_ProgrammingWindow programmingWindow, TargetObject editTarget)
        {
            this._programmingWindow = programmingWindow;
            this.editTarget = editTarget;
            this._targetName = editTarget.name;
            this._targetScript = editTarget.targetScript;
        }



    }

}

