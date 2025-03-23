using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace RDTS.RobotSimulationProgramming
{

    [System.Serializable]
    /// <summary>
    /// 目标点对象类
    /// </summary>
    public class TargetObject
    {
        [SerializeField] public string name;//名称
        [SerializeField] public string Guid;//id
        [System.NonSerialized] public GameObject targetObj;//目标点对象
        [System.NonSerialized] public RSP_Target targetScript;//目标点脚本
        ///public Rect rect;//目标点所在行的矩形（参数）


        public void OnEnableTarget()//目标点创建时的相关处理
        {

        }

        public void OnDisableTarget()//目标点移除时的相关处理
        {

        }


    }


    public enum ParameterType
    {
        Input,
        Output
    }


    [System.Serializable]
    /// <summary>
    /// IO参数类
    /// </summary>
    public class IOParameter
    {
        [SerializeField] public string name;//名称
        [SerializeField] public string Guid;//id
        [SerializeField] public ParameterType type;//类型
        [SerializeField] public int value;//值
        [System.NonSerialized] public GameObject parameterObj;//参数对象


        public void OnEnableIOParameter()
        {

        }


        public void OnDisableIOParameter()
        {

        }



    }

    public class InParameter : IOParameter
    {

    }

    public class OutParameter : IOParameter
    {

    }



    [System.Serializable]
    public class RSP_RobotProgram : ScriptableObject
    {
        ///https://docs.unity.cn/cn/2019.4/ScriptReference/SceneManagement.Scene.html
        ///https://docs.unity.cn/cn/2019.4/ScriptReference/SceneManagement.SceneManager.html
        ///https://docs.unity.cn/cn/2019.4/ScriptReference/SceneManagement.EditorSceneManager.html
        public string scene;//场景名称
        public int sceneIndex;//场景索引
        public string scenePath;//场景路径
        [SerializeField] public List<TargetObject> targets = new List<TargetObject>();//目标点列表
        [SerializeField] public List<IOParameter> ioParameters = new List<IOParameter>();//IO参数列表
        [SerializeField, SerializeReference] public List<BaseInstruction> instructions = new List<BaseInstruction>();//程序指令集




        public event Action<ProgramChanges> onProgramChanges;




        protected virtual void OnEnable()
        {

        }


        protected virtual void OnDisable()
        {
            // Save the graph to the disk 将图表保存到磁盘
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();

        }


        #region  目标点操作方法

        /// <summary>
        /// 获取目标点类列表
        /// </summary>
        /// <returns></returns>
        public List<TargetObject> GetTargetList()
        {
            return targets;
        }


        /// <summary>
        /// 获取目标点名称列表
        /// </summary>
        /// <returns></returns>
        public List<string> GetTargetNameList()
        {
            if (targets == null)
                return null;

            List<string> names = new List<string>();
            targets.ForEach(tar =>
            {
                names.Add(tar.name);
            });

            return names;
        }


        /// <summary>
        /// 添加一个目标点
        /// </summary>
        /// <param name="target"></param>
        public void AddTarget(TargetObject target)
        {
            if (target == null)
                return;

            if (targets.Contains(target))
                return;

            target.OnEnableTarget();

            targets.Add(target);

            onProgramChanges?.Invoke(new ProgramChanges { addTarget = target });
        }


        /// <summary>
        /// 移除一个目标点
        /// </summary>
        /// <param name="target"></param>
        public void RemoveTarget(TargetObject target)
        {
            if (target == null)
                return;

            if (!targets.Contains(target))
                return;

            target.OnDisableTarget();

            targets.Remove(target);

            onProgramChanges?.Invoke(new ProgramChanges { removeTarget = target });

        }


        /// <summary>
        /// 清除所有目标点
        /// </summary>
        public void ClearAllTargets()
        {
            targets.Clear();
        }


        #endregion


        #region  IO参数操作方法
        /// <summary>
        /// 获取IO参数列表
        /// </summary>
        /// <returns></returns>
        public List<IOParameter> GetIOParameterList()
        {
            return ioParameters;
        }


        /// <summary>
        /// 获取参数名称的列表
        /// </summary>
        /// <returns></returns>
        public List<string> GetIOParameterNameList()
        {
            if (ioParameters == null)
                return null;

            List<string> names = new List<string>();
            ioParameters.ForEach(para =>
            {
                names.Add(para.name);
            });

            return names;
        }


        /// <summary>
        /// 添加一个参数
        /// </summary>
        /// <param name="target"></param>
        public void AddIOParameter(IOParameter ioParameter)
        {
            if (ioParameter == null)
                return;

            if (ioParameters.Contains(ioParameter))
                return;

            ioParameter.OnEnableIOParameter();

            ioParameters.Add(ioParameter);

            onProgramChanges?.Invoke(new ProgramChanges { addIOParameter = ioParameter });
        }


        /// <summary>
        /// 移除一个参数
        /// </summary>
        /// <param name="target"></param>
        public void RemoveIOParameter(IOParameter ioParameter)
        {
            if (ioParameter == null)
                return;

            if (!ioParameters.Contains(ioParameter))
                return;

            ioParameter.OnDisableIOParameter();

            ioParameters.Remove(ioParameter);

            onProgramChanges?.Invoke(new ProgramChanges { removeIOParameter = ioParameter });

        }


        /// <summary>
        /// 清除所有参数
        /// </summary>
        public void ClearAllIOParameters()
        {
            ioParameters.Clear();
        }


        #endregion


        #region 指令操作方法

        public List<BaseInstruction> GetInstructionList()
        {
            return instructions;
        }

        public BaseInstruction AddInstruction(BaseInstruction instruction)
        {

            instructions.Add(instruction);//加入节点列表
            //node.Initialize(this);

            onProgramChanges?.Invoke(new ProgramChanges { addInstruction = instruction });

            return instruction;
        }


        public void EditInstruction(BaseInstruction instruction, int index)
        {
            if (index < instructions.Count && index >= 0)
            {
                instructions[index] = instruction;
            }


        }



        /// <summary>
        /// 移除一条指令
        /// </summary>
        /// <param name="target"></param>
        public void RemoveInstruction(BaseInstruction instruction)
        {
            if (instruction == null)
                return;

            if (!instructions.Contains(instruction))
                return;


            instructions.Remove(instruction);

            onProgramChanges?.Invoke(new ProgramChanges { removeInstruction = instruction });

        }


        /// <summary>
        /// 清除所有指令
        /// </summary>
        public void ClearAllInstructions()
        {
            instructions.Clear();
        }


        #endregion



    }




    public class ProgramChanges
    {

        public TargetObject addTarget;
        public TargetObject removeTarget;

        public IOParameter addIOParameter;
        public IOParameter removeIOParameter;

        public BaseInstruction addInstruction;
        public BaseInstruction removeInstruction;


    }










}
