using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace RDTS.RobotSimulationProgramming
{
    public class RSP_ParameterEditingWindow : EditorWindow
    {
        RSP_ProgrammingWindow _programmingWindow;//对应的编程窗口
        IOParameter editParameter;//要被编辑的参数
        private string _parameterName = "";//名称
        public string parameterName { get { return _parameterName; } }//供外部读取此对象名称



        void OnGUI()
        {
            GUILayout.Space(5);
            GUILayout.Label("填入以下信息以编辑目标点：", labelStyle);
            GUILayout.Space(5);

            GUILayout.BeginHorizontal(GUILayout.Height(25));
            GUILayout.Space(30);
            GUILayout.Label("目标点名称：", GUILayout.Width(position.width / 3), GUILayout.Height(25));
            GUILayout.FlexibleSpace();
            _parameterName = GUILayout.TextField(_parameterName, GUILayout.Width(position.width / 2), GUILayout.Height(20));
            GUILayout.Space(30);
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Edit", GUILayout.Width(60), GUILayout.Height(28), GUILayout.ExpandWidth(true)))
            {
                this._programmingWindow.EditParameter(editParameter, _parameterName);
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


        public void LinkProgrammingWindow(RSP_ProgrammingWindow programmingWindow, IOParameter editParameter)
        {
            this._programmingWindow = programmingWindow;
            this.editParameter = editParameter;
            this._parameterName = editParameter.name;
        }



    }


}