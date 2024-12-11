using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;
using UnityEditor;



namespace RDTS.RobotSimulationProgramming
{
    public class RSP_InstructionEditingWindow : EditorWindow
    {
        RSP_ProgrammingWindow _programmingWindow;//对应的编程窗口
        BaseInstruction _instruction;//要编辑的指令
        int _index = -1;//要编辑指令的索引
        Instruction _instructionType = Instruction.MoveJoint;//指令类型
        string _parameterName = "";//名称
        List<string> _targetNames = new List<string>();
        int _indexOfTarget = 0;
        //List<string> _parameterNames = new List<string>();
        List<string> _inParameterNames = new List<string>();
        List<string> _outParameterNames = new List<string>();
        int _indexOfParameter = 0;
        int _paraValue = 0;//参数值
        float _setSpeed = 1f;//速度
        float _pauseTime = 0f;//暂停秒数


        void OnGUI()
        {
            GUILayout.Space(5);
            GUILayout.Label("填入以下信息以编辑指令：", labelStyle);
            GUILayout.Space(5);

            GUILayout.BeginHorizontal(GUILayout.Height(25));
            GUILayout.Space(30);
            GUILayout.Label("指令类型", GUILayout.Width(position.width / 3), GUILayout.Height(25));
            GUILayout.FlexibleSpace();
            _instructionType = (Instruction)EditorGUILayout.EnumPopup(_instructionType, GUILayout.Width(position.width / 2), GUILayout.Height(20));
            GUILayout.Space(30);
            GUILayout.EndHorizontal();


            switch (_instructionType)
            {
                case Instruction.MoveJoint:
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(30);
                    GUILayout.Label("目标点名称：", GUILayout.Width(position.width / 3), GUILayout.Height(25));
                    GUILayout.FlexibleSpace();
                    _indexOfTarget = EditorGUI.Popup(new Rect(170, 60, position.width / 2, 20), _indexOfTarget, _targetNames.ToArray());
                    GUILayout.Space(30);
                    GUILayout.EndHorizontal();
                    break;

                case Instruction.MoveCurve:
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(30);
                    GUILayout.Label("目标点名称：", GUILayout.Width(position.width / 3), GUILayout.Height(25));
                    GUILayout.FlexibleSpace();
                    _indexOfTarget = EditorGUI.Popup(new Rect(170, 60, position.width / 2, 20), _indexOfTarget, _targetNames.ToArray());
                    GUILayout.Space(30);
                    GUILayout.EndHorizontal();
                    break;

                case Instruction.MoveStraightLine:
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(30);
                    GUILayout.Label("目标点名称：", GUILayout.Width(position.width / 3), GUILayout.Height(25));
                    GUILayout.FlexibleSpace();
                    _indexOfTarget = EditorGUI.Popup(new Rect(170, 60, position.width / 2, 20), _indexOfTarget, _targetNames.ToArray());
                    GUILayout.Space(30);
                    GUILayout.EndHorizontal();
                    break;

                case Instruction.SetParameter:
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(30);
                    GUILayout.Label("参数名称：", GUILayout.Width(position.width / 3), GUILayout.Height(25));
                    GUILayout.FlexibleSpace();
                    _indexOfParameter = EditorGUI.Popup(new Rect(170, 60, position.width / 2, 20), _indexOfParameter, _outParameterNames.ToArray());
                    GUILayout.Space(30);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(30);
                    GUILayout.Label("参数值：", GUILayout.Width(position.width / 3), GUILayout.Height(25));
                    GUILayout.FlexibleSpace();
                    _paraValue = EditorGUI.IntField(new Rect(170, 85, position.width / 2, 20), _paraValue);
                    GUILayout.Space(30);
                    GUILayout.EndHorizontal();
                    break;

                case Instruction.WaitParameter:
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(30);
                    GUILayout.Label("参数名称：", GUILayout.Width(position.width / 3), GUILayout.Height(25));
                    GUILayout.FlexibleSpace();
                    _indexOfParameter = EditorGUI.Popup(new Rect(170, 60, position.width / 2, 20), _indexOfParameter, _inParameterNames.ToArray());
                    GUILayout.Space(30);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(30);
                    GUILayout.Label("参数值：", GUILayout.Width(position.width / 3), GUILayout.Height(25));
                    GUILayout.FlexibleSpace();
                    _paraValue = EditorGUI.IntField(new Rect(170, 85, position.width / 2, 20), _paraValue);
                    GUILayout.Space(30);
                    GUILayout.EndHorizontal();
                    break;

                case Instruction.SetSpeed:
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(30);
                    GUILayout.Label("设置速度：", GUILayout.Width(position.width / 3), GUILayout.Height(25));
                    GUILayout.FlexibleSpace();
                    _setSpeed = EditorGUI.FloatField(new Rect(170, 60, position.width / 2, 20), _setSpeed);
                    GUILayout.Space(30);
                    GUILayout.EndHorizontal();
                    break;

                case Instruction.PauseTime:
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(30);
                    GUILayout.Label("暂停秒数：", GUILayout.Width(position.width / 3), GUILayout.Height(25));
                    GUILayout.FlexibleSpace();
                    _pauseTime = EditorGUI.FloatField(new Rect(170, 60, position.width / 2, 20), _pauseTime);
                    GUILayout.Space(30);
                    GUILayout.EndHorizontal();
                    break;

            }



            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Edit", GUILayout.Width(60), GUILayout.Height(28), GUILayout.ExpandWidth(true)))
            {
                if (this._programmingWindow != null)
                {
                    switch (_instructionType)
                    {
                        case Instruction.MoveJoint:
                            //var newInstruction = BaseInstruction.CreateFromType(typeof(RSP_SetParameter));
                            RSP_MoveJoint moveJ = new RSP_MoveJoint();
                            moveJ.inTarget = _targetNames[_indexOfTarget];
                            moveJ.icon = icon_moveJ;
                            moveJ.program = _programmingWindow.program;
                            _programmingWindow.program.EditInstruction(moveJ, _index);
                            break;

                        case Instruction.MoveCurve:
                            RSP_MoveCurve moveC = new RSP_MoveCurve();
                            moveC.inTarget = _targetNames[_indexOfTarget];
                            moveC.icon = icon_moveC;
                            moveC.program = _programmingWindow.program;
                            _programmingWindow.program.EditInstruction(moveC, _index);
                            break;

                        case Instruction.MoveStraightLine:
                            RSP_MoveStraightLine moveS = new RSP_MoveStraightLine();
                            moveS.inTarget = _targetNames[_indexOfTarget];
                            moveS.icon = icon_moveS;
                            moveS.program = _programmingWindow.program;
                            _programmingWindow.program.EditInstruction(moveS, _index);
                            break;

                        case Instruction.SetParameter:
                            RSP_SetParameter setParameter = new RSP_SetParameter();
                            setParameter.inParameter = _outParameterNames[_indexOfParameter];
                            setParameter.inParaValue = _paraValue;
                            setParameter.icon = icon_parameter;
                            setParameter.program = _programmingWindow.program;
                            _programmingWindow.program.EditInstruction(setParameter, _index);
                            break;

                        case Instruction.WaitParameter:
                            RSP_WaitParameter waitParameter = new RSP_WaitParameter();
                            waitParameter.inParameter = _inParameterNames[_indexOfParameter];
                            waitParameter.inParaValue = _paraValue;
                            waitParameter.icon = icon_parameter;
                            waitParameter.program = _programmingWindow.program;
                            _programmingWindow.program.EditInstruction(waitParameter, _index);
                            break;

                        case Instruction.SetSpeed:
                            RSP_SetSpeed speed = new RSP_SetSpeed();
                            speed.inValue = _setSpeed;
                            speed.icon = icon_setSpeed;
                            speed.program = _programmingWindow.program;
                            _programmingWindow.program.EditInstruction(speed, _index);
                            break;

                        case Instruction.PauseTime:
                            RSP_PauseTime pause = new RSP_PauseTime();
                            pause.inValue = _pauseTime;
                            pause.icon = icon_pause;
                            pause.program = _programmingWindow.program;
                            _programmingWindow.program.EditInstruction(pause, _index);
                            break;

                    }

                }


                this.Close();

            }

        }


        private GUISkin skin_rsp;
        private GUIStyle labelStyle;//label元素的GUI style
        private Texture2D icon_moveJ;
        private Texture2D icon_moveC;
        private Texture2D icon_moveS;
        private Texture2D icon_setSpeed;
        private Texture2D icon_parameter;
        private Texture2D icon_pause;
        void OnEnable()
        {
            skin_rsp = Resources.Load<GUISkin>("GUISkinRSP");

            labelStyle = skin_rsp.customStyles[6];

            icon_moveJ = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/RDTS/Private/Modules/RobotSimulationAndProgramming/Resources/JointMovement.png");
            icon_moveC = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/RDTS/Private/Modules/RobotSimulationAndProgramming/Resources/CurvedMovement.png");
            icon_moveS = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/RDTS/Private/Modules/RobotSimulationAndProgramming/Resources/LinearMovement.png");
            icon_setSpeed = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/RDTS/Private/Modules/RobotSimulationAndProgramming/Resources/Speed.png");
            icon_parameter = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/RDTS/Private/Modules/RobotSimulationAndProgramming/Resources/IOParameter.png");
            icon_pause = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/RDTS/Private/Modules/RobotSimulationAndProgramming/Resources/Pause.png");

        }


        public void LinkProgrammingWindow(RSP_ProgrammingWindow programmingWindow, BaseInstruction instruction)
        {
            this._programmingWindow = programmingWindow;
            this._instruction = instruction;
            this._index = programmingWindow.program.instructions.IndexOf(instruction);

            _targetNames.Clear();
            //_parameterNames.Clear();
            programmingWindow.targets.ForEach(tar =>
            {

                if (!_targetNames.Contains(tar.name))
                {
                    _targetNames.Add(tar.name);
                }

            });

            programmingWindow.ioParameters.ForEach(para =>
            {
                if (!_inParameterNames.Contains(para.name))
                {
                    if (para.type == ParameterType.Input)
                        _inParameterNames.Add(para.name);
                    else
                        _outParameterNames.Add(para.name);

                }


            });

            var atrribute = instruction.GetType().GetCustomAttributes<InstructionTypeAttribute>().ToArray();
            _instructionType = atrribute[0].instruction;

            if (atrribute.Length > 0)
            {
                switch (atrribute[0].instruction)
                {
                    case Instruction.MoveJoint:
                        var inst1 = instruction as RSP_MoveJoint;
                        _indexOfTarget = _targetNames.IndexOf(inst1?.inTarget);
                        break;

                    case Instruction.MoveCurve:
                        var inst2 = instruction as RSP_MoveJoint;
                        _indexOfTarget = _targetNames.IndexOf(inst2?.inTarget);
                        break;

                    case Instruction.MoveStraightLine:
                        var inst3 = instruction as RSP_MoveJoint;
                        _indexOfTarget = _targetNames.IndexOf(inst3?.inTarget);
                        break;

                    case Instruction.SetParameter:
                        var inst4 = instruction as RSP_SetParameter;
                        _indexOfParameter = _outParameterNames.IndexOf(inst4?.inParameter);
                        _paraValue = inst4.inParaValue;
                        break;

                    case Instruction.WaitParameter:
                        var inst5 = instruction as RSP_WaitParameter;
                        _indexOfParameter = _inParameterNames.IndexOf(inst5?.inParameter);
                        _paraValue = inst5.inParaValue;
                        break;

                    case Instruction.SetSpeed:
                        var inst6 = instruction as RSP_SetSpeed;
                        _setSpeed = inst6.inValue;
                        break;

                    case Instruction.PauseTime:
                        var inst7 = instruction as RSP_PauseTime;
                        _pauseTime = inst7.inValue;
                        break;
                }
            }


        }



    }

}
