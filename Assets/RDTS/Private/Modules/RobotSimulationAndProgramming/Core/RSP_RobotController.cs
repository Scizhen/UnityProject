using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using Codice.Client.BaseCommands.CheckIn.Progress;
using NaughtyAttributes;
using RDTS.Utility;
using UnityEditorInternal;
using UnityEngine;
using VisualSpline;


namespace RDTS.RobotSimulationProgramming
{
    /// <summary>
    /// 目标点变量
    /// </summary>
    [System.Serializable]
    public class TargetVariable
    {
        public string targetName;
        public RSP_Target targetScript;
    }

    /// <summary>
    /// 参数变量
    /// </summary>
    [System.Serializable]
    public class IOParameterVariable
    {
        public string variableName;
        public Value variableObj;
    }

    /// <summary>
    /// 输入类型参数变量
    /// </summary>
    [System.Serializable]
    public class InParameterVariable
    {
        public string variableName;
        public ValueInputInt variableObj;
    }

    /// <summary>
    /// 输出类型参数变量
    /// </summary>
    [System.Serializable]
    public class OutParameterVariable
    {
        public string variableName;
        public ValueOutputInt variableObj;
    }


    /// <summary>
    /// Move类型的目标点变量
    /// </summary>
    public class MoveInstructionsVariable
    {
        public string instructionName;
        public string targetName;
        public SplinePoint targetPoint;
    }



    /// <summary>
    /// 机器人控制器脚本
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(Spline))]
    public class RSP_RobotController : MonoBehaviour
    {


        public RSP_RobotProgram program;//关联的asset文件
        public GameObject endPoint;//末端点对象

        [Header("Execute Status")]
        public bool isExecute = false;//是否执行程序指令
        public bool isExecuteLoop = false;//是否循环执行，为true时，会设置isExecute = true
        [Utility.ReadOnly] public int whichInstruction = -1;//当前执行的指令
        [Utility.ReadOnly] public bool finishOneProcess = false;//是否执行完成一次流程

        [Range(0, 100)] public float speed = 35f;//执行时关于机器人运动的速度
        public float pauseTime = 0;//执行时暂停时间

        [Utility.ReadOnly] public SplinePoint targetArrived;//最近已到达过的目标点
        [Utility.ReadOnly] public SplinePoint targetGiven;//当前给定要到达的目标点

        private Spline _spline => GetComponent<Spline>();
        [HideInInspector]
        public Spline spline
        {
            get { return _spline; }
        }


        private SplineDrive splineDrive;

        private Coroutine pauseTimeCoro;//暂停时间的协程
        private bool pauseFinish = false;//是否已完成暂停时间

        [Header("Related Objects")]
        public float scaleOfTargets = 1f;//目标点的缩放大小
        public List<TargetVariable> targets = new List<TargetVariable>();//目标对象列表
        private List<string> _targetNames = new List<string>();
        private List<RSP_Target> _targetScripts = new List<RSP_Target>();
        private Dictionary<string, RSP_Target> _targetScriptPerName = new Dictionary<string, RSP_Target>();//
        public List<InParameterVariable> inputParameters = new List<InParameterVariable>();
        public List<OutParameterVariable> outputParameters = new List<OutParameterVariable>();
        private List<string> _inParameterNames = new List<string>();//存储输入两个列表的名称
        private List<string> _outParameterNames = new List<string>();//存储输出两个列表的名称
        [HideInInspector] public List<MoveInstructionsVariable> _moveInstructions = new List<MoveInstructionsVariable>();//存储Move类型指令



        [Button]
        [Tooltip("打开机器人仿真与编程窗口")]
        public void OpenWindow()
        {
            RSP_ProgrammingWindow.ShowWindow();
            RSP_ProgrammingWindow window = (RSP_ProgrammingWindow)UnityEditor.EditorWindow.GetWindow(typeof(RSP_ProgrammingWindow));

            if (program != null)
            {
                UpdateProgram();
                window.InitializeProgram(this.program);
            }
            window.robotController = this;
        }


        [Button]
        public void ResetExecuteIndex()
        {
            indexOfExecute = 0;
        }

        [Button]
        [Tooltip("设置所有目标点缩放大小")]
        public void SetAllTargetScaleBuutton()
        {
            if (targets == null)
                return;

            targets.ForEach(tar =>
            {
                tar.targetScript.scale = new Vector3(1, 1, 1) * scaleOfTargets;
                tar.targetScript.SetScaleOfTargetObj(tar.targetScript.scale);
            });
        }


        void OnEnable()
        {
            // splineDrive = endPoint?.GetComponent<SplineDrive>();
            // splineDrive?.motionPath?.Clear();
            // DrawLineBetweenTargets();
            indexOfExecute = 0;
        }

        // Start is called before the first frame update
        void Start()
        {
            splineDrive = endPoint?.GetComponent<SplineDrive>();
            splineDrive?.motionPath?.Clear();
            if (splineDrive != null) splineDrive.isDrive = false;
            DrawLineBetweenTargets();
            indexOfExecute = 0;

            targetArrived = targetGiven = null;
        }

        // Update is called once per frame
        void Update()
        {

            InstructionProcess(program);
            whichInstruction = indexOfExecute + 1;
        }

        void OnDisable()
        {
            //StopCoroutine(pauseTimeCoro);///关闭协程
            whichInstruction = 0;
        }


        /// <summary>
        /// 向目标点列表中添加元素
        /// </summary>
        /// <param name="targetVariable"></param>
        /// <returns></returns>
        public void AddElement2TargetVariableList(TargetVariable targetVariable)
        {
            if (targetVariable == null)
                return;

            //先更新一下名称列表
            UpdateTargetNameList();

            while (_targetNames.Contains(targetVariable.targetName))
            {
                name += "(1)";
            }

            //存入列表和字典中
            targets.Add(targetVariable);
            //_targetScriptPerName[targetVariable.targetName] = targetVariable.targetScript;

        }


        /// <summary>
        /// 根据给定名称移除目标点变量
        /// </summary>
        /// <param name="name"></param>
        public void RemoveElementInTargetVariableListByName(string name)
        {

            UpdateTargetNameList();
            if (_targetNames.Contains(name))
            {
                for (int i = 0; i < targets.Count; i++)
                {
                    if (targets[i].targetName == name)
                    {
                        targets.Remove(targets[i]);
                        break;
                    }
                }
                _targetNames.Remove(name);
            }
        }


        /// <summary>
        /// 清楚所有的目标点变量
        /// </summary>
        public void ClearAllTargetVariables()
        {
            targets.Clear();
        }



        /// <summary>
        /// 更新asset文件的内容
        /// </summary>
        public void UpdateProgram()
        {
            if (program == null || targets == null)
                return;

            if (program.targets == null)
                return;

            /* 目标点处理 */
            var list = GetTargetNameList();
            List<string> originTargets_name = new List<string>();
            List<TargetObject> originTargets = new List<TargetObject>();
            bool isDifferent = false;

            //判断控制器组件中目标点元素与program的asset文件中存储的元素是否相同
            if (list.Count != program.targets.Count)
            {
                isDifferent = true;
            }
            foreach (var tarP in program.targets)
            {
                if (!list.Contains(tarP.name))
                {
                    isDifferent = true;
                    //break;
                }

                if (list.Contains(tarP.name))
                {
                    originTargets_name.Add(tarP.name);
                }
            }

            //若不同则需要将asset文件中的数据覆盖
            if (isDifferent)
            {
                isDifferent = false;

                List<TargetObject> newTargets = new List<TargetObject>();

                list?.ForEach(tar =>
                {
                    if (!originTargets_name.Contains(tar))
                    {
                        TargetObject tarObj = new TargetObject();
                        tarObj.name = tar;
                        newTargets.Add(tarObj);
                    }

                });

                originTargets_name.ForEach(tar =>
                {
                    TargetObject tarObj = new TargetObject();
                    tarObj.name = tar;
                    originTargets.Add(tarObj);
                });

                originTargets.AddRange(newTargets);
                program.targets = originTargets;
            }

            /* 参数处理 */
            List<IOParameter> iOParameters = new List<IOParameter>();
            inputParameters.ForEach(inPara =>
            {
                IOParameter iOPara = new IOParameter();
                iOPara.name = inPara.variableName;
                iOPara.type = ParameterType.Input;
                iOParameters.Add(iOPara);
            });
            outputParameters.ForEach(outPara =>
            {
                IOParameter iOPara = new IOParameter();
                iOPara.name = outPara.variableName;
                iOPara.type = ParameterType.Output;
                iOParameters.Add(iOPara);
            });
            program.ioParameters = iOParameters;


        }


        /// <summary>
        /// 更新目标点名称列表
        /// </summary>
        /// <returns></returns>
        List<string> UpdateTargetNameList()
        {
            _targetNames.Clear();

            targets.ForEach(tar =>
            {
                string name = tar.targetName;
                if (name == "" || name == null)
                {
                    name = "new Target";
                    if (_targetNames.Contains(name))
                        name += "(1)";
                }

                if (!_targetNames.Contains(name))
                    _targetNames.Add(name);
            });

            //QM.Log($"target name number: {_targetNames.Count}");
            return _targetNames;
        }

        /// <summary>
        /// 更新目标点组件列表
        /// </summary>
        /// <returns></returns>
        List<RSP_Target> UpdateTargetScriptList()
        {
            _targetScripts.Clear();

            targets.ForEach(tar =>
            {
                RSP_Target script = tar.targetScript;
                _targetScripts.Add(script);
            });

            return _targetScripts;
        }

        /// <summary>
        /// 更新字典内容
        /// </summary>
        /// <param name="isUpdateList"></param>
        /// <returns></returns>
        Dictionary<string, RSP_Target> UpdateTargetScriptPerNameDictionary(bool isUpdateList = false)
        {
            _targetScriptPerName.Clear();

            if (isUpdateList)
            {
                UpdateTargetNameList();
                UpdateTargetScriptList();
            }

            try
            {
                for (int index = 0; index < _targetNames.Count; index++)
                {
                    _targetScriptPerName[_targetNames[index]] = _targetScripts[index];
                }
            }
            catch (System.Exception e)
            {
                QM.Log(e.ToString());
            }


            return _targetScriptPerName;
        }


        /// <summary>
        /// 通过名称获取对应的目标点变量
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public TargetVariable GetTargetVariableByName(string name)
        {
            UpdateTargetNameList();

            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i].targetName == name)
                    return targets[i];
            }

            return null;
        }



        /// <summary>
        /// 获取targets目标点列表
        /// </summary>
        /// <returns></returns>
        public List<string> GetTargetNameList()
        {
            return UpdateTargetNameList();
        }


        /// <summary>
        /// 通过目标点名称获取对应的目标点组件
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public RSP_Target GetTargetScriptByName(string name)
        {
            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i].targetName == name)
                {
                    return targets[i].targetScript;
                }

            }

            return null;

        }


        /// <summary>
        /// 通过目标点名称设置对应的目标点组件
        /// </summary>
        /// <param name="name"></param>
        /// <param name="newTarget"></param>
        /// <param name="isUpdate"></param>
        /// <returns></returns>
        public RSP_Target SetTargetScriptByName(string name, RSP_Target newScript, bool isUpdate = false)
        {
            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i].targetName == name)
                {
                    return targets[i].targetScript = newScript;
                }

            }

            return null;

        }





        /// <summary>
        /// 向targets目标点列表添加一个新的元素
        /// </summary>
        /// <param name="targetScript"></param>
        // public void AddElement2TargetList(string targetName)
        // {
        //     if (targetName == "")
        //         return;

        //     if (targets.Contains(targetScript))
        //         return;

        //     targets.Add(targetScript);
        // }

        /// <summary>
        /// 清空targets列表
        /// </summary>
        public void ClearElementsInTargetList()
        {
            targets?.Clear();
        }

        /// <summary>
        /// 从targets列表中移除指定的目标点（若包含的话）
        /// </summary>
        /// <param name="targetScript"></param>
        // public void RemoveElementInTargetList(RSP_Target targetScript)
        // {
        //     if (targetScript == null)
        //         return;

        //     if (!targets.Contains(targetScript))
        //         return;

        //     targets.Remove(targetScript);
        // }


        /// <summary>
        /// 获取末端点对象
        /// </summary>
        /// <returns></returns>
        public GameObject GetEndPoint()
        {
            return endPoint ?? null;
        }



        #region 参数列表处理方法


        /// <summary>
        /// 更新输入参数列名称表
        /// </summary>
        /// <returns></returns>
        List<string> UpdateInParameterNameList()
        {
            _inParameterNames.Clear();

            inputParameters.ForEach(ipara =>
            {
                string name = ipara.variableName;
                if (name == "" || name == null)
                {
                    name = "new Parameter";
                    while (_inParameterNames.Contains(name))
                        name += "(1)";
                }



                if (!_inParameterNames.Contains(name))
                    _inParameterNames.Add(name);
            });

            //QM.Log($"target name number: {_targetNames.Count}");
            return _inParameterNames;
        }

        /// <summary>
        /// 向输入参数列表中添加元素
        /// </summary>
        /// <param name="targetVariable"></param>
        /// <returns></returns>
        public void AddElement2InParameterVariableList(InParameterVariable parameterVariable)
        {
            if (parameterVariable == null)
                return;

            //先更新一下名称列表
            UpdateInParameterNameList();

            while (_inParameterNames.Contains(parameterVariable.variableName))
            {
                name += "(1)";
            }

            inputParameters.Add(parameterVariable);

        }


        /// <summary>
        /// 更新输出参数列名称表
        /// </summary>
        /// <returns></returns>
        List<string> UpdateOutParameterNameList()
        {
            _outParameterNames.Clear();

            outputParameters.ForEach(ipara =>
            {
                string name = ipara.variableName;
                if (name == "" || name == null)
                {
                    name = "new Parameter";
                    while (_outParameterNames.Contains(name))
                        name += "(1)";
                }



                if (!_outParameterNames.Contains(name))
                    _outParameterNames.Add(name);
            });

            //QM.Log($"target name number: {_targetNames.Count}");
            return _outParameterNames;
        }

        /// <summary>
        /// 向输出参数列表中添加元素
        /// </summary>
        /// <param name="targetVariable"></param>
        /// <returns></returns>
        public void AddElement2OutParameterVariableList(OutParameterVariable parameterVariable)
        {
            if (parameterVariable == null)
                return;

            //先更新一下名称列表
            UpdateOutParameterNameList();

            while (_outParameterNames.Contains(parameterVariable.variableName))
            {
                name += "(1)";
            }

            outputParameters.Add(parameterVariable);

        }


        /// <summary>
        /// 通过参数名称获取对应的参数组件
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Value GetParameterScriptByName(IOParameter parameter)
        {
            if (parameter.type == ParameterType.Input)
            {
                for (int i = 0; i < inputParameters.Count; i++)
                {
                    if (inputParameters[i].variableName == parameter.name)
                    {
                        return inputParameters[i].variableObj;
                    }

                }
            }
            else
            {
                for (int i = 0; i < outputParameters.Count; i++)
                {
                    if (outputParameters[i].variableName == parameter.name)
                    {
                        return outputParameters[i].variableObj;
                    }

                }
            }


            return null;

        }


        /// <summary>
        /// 根据给定名称移除参数变量
        /// </summary>
        /// <param name="name"></param>
        public void RemoveElementInParameterVariableList(IOParameter parameter)
        {

            if (parameter.type == ParameterType.Input)
            {
                UpdateInParameterNameList();
                if (_inParameterNames.Contains(parameter.name))
                {
                    for (int i = 0; i < inputParameters.Count; i++)
                    {
                        if (inputParameters[i].variableName == parameter.name)
                        {
                            inputParameters.Remove(inputParameters[i]);
                            break;
                        }
                    }
                    _inParameterNames.Remove(parameter.name);
                }
            }
            else
            {
                UpdateOutParameterNameList();
                if (_outParameterNames.Contains(parameter.name))
                {
                    for (int i = 0; i < outputParameters.Count; i++)
                    {
                        if (outputParameters[i].variableName == parameter.name)
                        {
                            outputParameters.Remove(outputParameters[i]);
                            break;
                        }
                    }
                    _outParameterNames.Remove(parameter.name);
                }
            }


        }


        /// <summary>
        /// 通过名称获取对应的输入型参数变量
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public InParameterVariable GetInParameterVariableByName(string name)
        {
            UpdateInParameterNameList();

            for (int i = 0; i < inputParameters.Count; i++)
            {
                if (inputParameters[i].variableName == name)
                    return inputParameters[i];
            }

            return null;
        }


        /// <summary>
        /// 通过名称获取对应的输出型参数变量
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public OutParameterVariable GetOutParameterVariableByName(string name)
        {
            UpdateOutParameterNameList();

            for (int i = 0; i < outputParameters.Count; i++)
            {
                if (outputParameters[i].variableName == name)
                    return outputParameters[i];
            }

            return null;
        }


        /// <summary>
        /// 清楚所有的参数变量
        /// </summary>
        public void ClearAllParameterVariables()
        {
            inputParameters.Clear();
            outputParameters.Clear();
        }


        #endregion


        #region 目标点间的轨迹绘制与运动


        public void DrawLineBetweenTargets()
        {
            if (this.program == null)
                return;

            ///获取Move类型的指令
            _moveInstructions.Clear();
            var instructions = this.program.GetInstructionList();
            instructions.ForEach(inst =>
            {
                if (inst.GetType().IsSubclassOf(typeof(RSP_BaseMoveInstruction)))
                {
                    MoveInstructionsVariable newMoveInstruction = new MoveInstructionsVariable();
                    //Move指令名称
                    newMoveInstruction.instructionName = inst.name;
                    //Move指令对应目标点上的SplinePoint组件
                    string targetName = ((RSP_BaseMoveInstruction)inst).inTarget;
                    newMoveInstruction.targetName = targetName;
                    newMoveInstruction.targetPoint = GetTargetScriptByName(targetName).gameObject.GetComponent<SplinePoint>();
                    newMoveInstruction.targetPoint.connectedPoints.Clear();//<! 清空连接点列表
                    //存储至列表
                    if (!_moveInstructions.Contains(newMoveInstruction))
                        _moveInstructions.Add(newMoveInstruction);
                }
            });

            ///QM.Log("draw line between targets: " + _moveInstructions.Count);
            //相关列表清空
            _spline.lines.Clear();
            splineDrive.motionPath?.Clear();
            //添加第一个目标点
            splineDrive.motionPath?.Add(_moveInstructions[0].targetPoint);
            //绘制运动轨迹和后续目标点
            if (_moveInstructions.Count > 1)
            {
                ///按照先后顺序绘制轨迹
                for (int i = 1; i < _moveInstructions.Count; i++)
                {
                    var nowMoveInst = _moveInstructions[i];
                    var preMoveInst = _moveInstructions[i - 1];
                    VisualSpline.LineType lineType = LineType.Straight;

                    switch (nowMoveInst.instructionName)
                    {
                        case "MoveJ":
                            lineType = LineType.Bezier;
                            break;
                        case "MoveC":
                            lineType = LineType.Bezier;
                            break;
                        case "MoveS":
                            lineType = LineType.Straight;
                            break;
                    }



                    _spline?.Addline(new VisualSpline.Line()
                    {
                        point1 = preMoveInst.targetPoint,
                        point2 = nowMoveInst.targetPoint,
                        index = _spline.lines.Count,
                        reverse = false,
                        lineType = lineType,
                        color = Color.yellow,
                        lineWidth = 2f
                    });

                    //向前后两个目标点分别添加连接关系

                    if (!nowMoveInst.targetPoint.connectedPoints.Contains(preMoveInst.targetPoint))
                        nowMoveInst.targetPoint.AddConnectedPoint(preMoveInst.targetPoint);
                    if (!preMoveInst.targetPoint.connectedPoints.Contains(nowMoveInst.targetPoint))
                        preMoveInst.targetPoint.AddConnectedPoint(nowMoveInst.targetPoint);


                    //在目标点的motionpath中添加对应的目标点
                    splineDrive.motionPath?.Add(_moveInstructions[i].targetPoint);

                }
            }




        }



        #endregion

        #region 程序指令处理方法


        /// <summary>
        /// 设置参数的值
        /// </summary>
        /// <param name="robotController"></param>
        /// <param name="parameterName"></param>
        public static void SetValueOfOutParameter(RSP_RobotController robotController, string parameterName, int value)
        {
            if (robotController == null || robotController.outputParameters == null)
                return;

            robotController.outputParameters.ForEach(outPara =>
            {
                if (outPara.variableName == parameterName)
                    outPara.variableObj.SetValue(value);
            });
        }

        /// <summary>
        /// 等待参数值是否为期望值
        /// </summary>
        /// <param name="robotController"></param>
        /// <param name="parameterName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool WaitValueOfOutParameter(RSP_RobotController robotController, string parameterName, int value)
        {
            if (robotController == null || robotController.inputParameters == null)
                return false;

            for (int i = 0; i < robotController.inputParameters.Count; i++)
            {
                var inPara = robotController.inputParameters[i];
                if (inPara.variableName == parameterName)
                {
                    if (inPara.variableObj.Value == value)
                    {
                        return true;
                    }
                }
            }

            return false;
        }



        /// <summary>
        /// 设置速度值
        /// </summary>
        /// <param name="robotController"></param>
        /// <param name="value"></param>
        public static void SetValueOfSpeed(RSP_RobotController robotController, float value)
        {
            if (robotController == null)
                return;

            if (value > 100)
                value = 100;
            if (value < 0)
                value = 0;

            robotController.speed = value;
        }


        bool startPause = true;//是否开始暂停时间
        /// <summary>
        /// 设置暂停的时间值（秒数）
        /// </summary>
        /// <param name="robotController"></param>
        /// <param name="value"></param>
        public static bool SetValueOfPauseTime(RSP_RobotController robotController, float value)
        {
            if (robotController == null)
                return false;

            //协程仅开启一次即可
            if (robotController.startPause)
            {
                robotController.pauseTime = value;
                robotController.pauseTimeCoro = robotController.StartCoroutine(robotController.PauseTimeInExecuteInstructions(value));///开启协程
                robotController.startPause = false;
                QM.Log("PauseTime: start!");
            }

            //在指定时间和置位
            if (robotController.pauseFinish)
            {
                robotController.StopCoroutine(robotController.pauseTimeCoro);///关闭协程

                robotController.startPause = true;
                robotController.pauseFinish = false;

                return true;
            }
            return false;
        }


        /// <summary>
        /// 暂停时间执行指令的协程方法
        /// </summary>
        /// <param name="interval"></param>s
        /// <returns></returns>
        protected IEnumerator PauseTimeInExecuteInstructions(float time)
        {
            for (; ; )
            {
                yield return new WaitForSeconds(time);//延迟时间      
                pauseFinish = true;
                QM.Log("PauseTime: End!");
            }


        }


        /// <summary>
        /// 移动到指定的目标点(旧方法，不适用于新的样条线驱动方式)
        /// </summary>
        /// <param name="robotController"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool MoveToTarget(RSP_RobotController robotController, string target)
        {
            if (robotController == null)
                return false;

            if (robotController.splineDrive == null)
                return false;

            SplinePoint splinePoint = null;


            foreach (var moveInst in robotController._moveInstructions)
            {
                if (target == moveInst.targetName)
                {
                    splinePoint = moveInst.targetPoint;
                    break;
                }
            }


            if (splinePoint != null)
            {
                if (robotController.splineDrive.motionPath != null &&
                     (robotController.splineDrive.motionPath.Count == 0 ||
                        (robotController.splineDrive.motionPath.Count >= 0 && robotController.splineDrive.motionPath[robotController.splineDrive.motionPath.Count - 1] != splinePoint)))//&& !robotController.splineDrive.motionPath.Contains(splinePoint))
                    robotController.splineDrive.motionPath.Add(splinePoint);

                ///QM.Log(splinePoint.gameObject.name);
            }


            //若个数大于三，则清除第一个（即保证motionPath中最多只有两个目标点，便于驱动）
            if (robotController.splineDrive?.motionPath.Count > 2)
            {
                robotController.splineDrive?.motionPath.RemoveAt(0);
            }

            if (robotController.splineDrive.percentage == 1)
            {
                robotController.splineDrive.percentage = 0;
            }

            robotController.splineDrive.speed = robotController.speed;
            robotController.splineDrive.isDrive = true;
            robotController.splineDrive.isLoop = false;
            ///splineDrive.speed = speed;
            if (splinePoint != null && robotController.endPoint.transform.position == splinePoint.transform.position)
            {
                robotController.splineDrive.isDrive = false;
                //robotController.splineDrive.percentage = 0;


                return true;
            }

            return false;


        }


        public static bool MoveToGivenTarget(RSP_RobotController robotController, string target)
        {
            if (robotController == null)
                return false;

            if (robotController.splineDrive == null)
                return false;

            //获取名称对应的目标点
            SplinePoint splinePoint = null;
            foreach (var moveInst in robotController._moveInstructions)
            {
                if (target == moveInst.targetName)
                {
                    splinePoint = moveInst.targetPoint;
                    break;
                }
            }
            //如哦给定的目标点不存在，就停止
            if (splinePoint == null)
                return false;




            //从当前目标点移动到指定的目标点
            robotController.targetGiven = splinePoint;//设置要到达的目标点
            robotController.splineDrive.currentLine.startPoint = robotController.targetArrived;//将已到达的目标点赋予splineDrive的startPoint
            robotController.splineDrive.currentLine.endPoint = robotController.targetGiven;//将要到达的目标点赋予splineDrive的startPoint

            // if (robotController.splineDrive.currentLine.startPoint == null)
            //     robotController.splineDrive.specialCaseHandling = true;//开启特殊情况处理
            // else
            //     robotController.splineDrive.specialCaseHandling = false;//关闭特殊情况处理


            robotController.splineDrive.speed = robotController.speed;//设置速度
            // robotController.splineDrive.isAutomaticallyNext = false;//关闭自动向后驱动
            robotController.splineDrive.isP2P = true;//开启P2P驱动模式
            robotController.splineDrive.rotateWithPoint = true;//开启跟随锚点旋转
            robotController.splineDrive.isDrive = true;//开启驱动

            //若已到达目标点
            if (robotController.splineDrive.currentLine.isArrived)
            {
                robotController.splineDrive.currentLine.isArrived = false;
                robotController.splineDrive.isDrive = false;//停止驱动
                robotController.targetArrived = splinePoint;//设置此目标点为新的已到达的目标点
                robotController.splineDrive.currentLine.percentage = 0;

                return true;
            }

            return false;
        }


        /// <summary>
        /// 按照顺序对程序指令进行处理
        /// </summary>
        int indexOfExecute = 0;
        public void InstructionProcess(RSP_RobotProgram program)
        {
            if (program == null)
                return;

            //只执行一次
            if (!isExecuteLoop && finishOneProcess && !isExecute)
            {
                indexOfExecute = program.instructions.Count - 1;
                isExecute = false;

            }

            //(结束一次流程后)再次执行
            if (!isExecuteLoop && isExecute && finishOneProcess)
            {
                indexOfExecute = 0;
                finishOneProcess = false; ;
            }

            //是否循环执行指令
            if (isExecuteLoop && finishOneProcess)
            {
                isExecute = true;
                indexOfExecute = 0;
                finishOneProcess = false;
            }

            //举止指令执行
            if (isExecute)
            {
                bool isFinish = false;
                var inst = program.instructions[indexOfExecute];
                inst.Process(this, out isFinish);
                if (isFinish)
                {
                    indexOfExecute++;
                    isFinish = false;

                    //判断是否完成一次流程执行
                    if (!finishOneProcess && indexOfExecute == program.instructions.Count)
                    {
                        finishOneProcess = true;
                        isExecute = false;
                    }

                }
            }






        }


        #endregion



    }



}