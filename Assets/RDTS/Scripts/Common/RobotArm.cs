///********************************************************************************************
///this is part of Robot Digital Twin System.                                                 *
///@Copyright by Shaw.S                                                                       *
///Do not distribute without authorization,thanks!                                            *
///********************************************************************************************
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using NaughtyAttributes;
using System.Linq;
using RDTS.Utility;

namespace RDTS
{
    public enum RobotTool
    {
        Gripper,
        Suck
    }

    public enum RobotMotion
    {
        Pick,
        Place
    }

    //[Serializable]
    //public class PickStartSignal
    //{
    //    public string name = "signal";
    //    public bool isOr = false;//<! true为“与”，false为“或”
    //    public List<ValueOutputBool> pickSignal;
    //}
    //[Serializable]
    //public class PlaceStartSignal
    //{
    //    public string name = "signal";
    //    public bool isOr = false;//<! true为“与”，false为“或”
    //    public List<ValueOutputBool> placeSignal;
    //}
    [Serializable]
    public class StartSignal
    {
        public string name = "signal";
        public bool isOr = false;//<! true为“与”，false为“或”
        public List<ValueOutputBool> signals;
    }

    [Serializable]
    public class InputSignal
    {
        public bool isOr = false;//<! true为“与”，false为“或”
        public List<ValueInputBool> signals;
    }

    [Serializable]
    public class RobotArmStep
    {
        public string name = "step";
        public InputSignal PickStart;
        public InputSignal PlaceStart;
    }

    /// <summary>
    /// 搭配RoboDK软件使用的机械臂控制脚本。
    /// </summary>
    [SelectionBase]
    public class RobotArm : ControlLogic
    {
        [BoxGroup("Status")] public string StatusLoader = "Home";
        [BoxGroup("Status")] [NaughtyAttributes.ReadOnly] public int stepNum = 1;//一共需要执行几组动作
        [BoxGroup("Status")] [NaughtyAttributes.ReadOnly] public int stepNow = 1;//当前执行的机械臂动作步骤
        [BoxGroup("Status")] [NaughtyAttributes.ReadOnly] public int succCount = 0;//机械臂完成抓/放的工件数量
        [BoxGroup("Status")] [NaughtyAttributes.ReadOnly] public int failCount = 0;//机械臂未完成抓/放的工件数量

        /* 与实际产线交互的信号 *////* 连博途 */
        //这两个列表长度应相同
        [BoxGroup("Interactive Signal")] public List<RobotArmStep> Steps;

        [BoxGroup("Interactive Signal")] public ValueOutputBool PickFinish;//抓取完成信号
        [BoxGroup("Interactive Signal")] public ValueOutputBool PlaceFinish;//放置完成信号

        //RoboDK相关信号
        [BoxGroup("RoboDK Signal")] public ValueInputInt MoveToPick;//将要去抓取
        [BoxGroup("RoboDK Signal")] public ValueOutputInt MakePick;//抓取
        [BoxGroup("RoboDK Signal")] public ValueInputInt PickObjOK;//抓取完成
        [BoxGroup("RoboDK Signal")] public ValueInputInt MoveToPlace;//将要去放置
        [BoxGroup("RoboDK Signal")] public ValueOutputInt MakePlace;//放置
        [BoxGroup("RoboDK Signal")] public ValueInputInt PlaceObjOK;//放置完成
        [BoxGroup("RoboDK Signal")] public ValueOutputInt Waitting;//等待
        [Header("RoboDK Signal use Gripper")]//夹爪工具需要多四个信号，以实现夹爪的张开、闭合效果
        [ShowIf("chooseTool", RobotTool.Gripper)] [BoxGroup("RoboDK Signal")] public ValueOutputInt MakeGripperOut;//使夹爪张开
        [ShowIf("chooseTool", RobotTool.Gripper)] [BoxGroup("RoboDK Signal")] public ValueInputInt GripperOutOK;//夹爪张开完毕
        [ShowIf("chooseTool", RobotTool.Gripper)] [BoxGroup("RoboDK Signal")] public ValueOutputInt MakeGripperIn;//使夹爪闭合
        [ShowIf("chooseTool", RobotTool.Gripper)] [BoxGroup("RoboDK Signal")] public ValueInputInt GripperInOK;//夹爪闭合完毕

        [BoxGroup("Grip")] public Grip RobotGrip;//关联的Grip脚本
        //[BoxGroup("Grip")] public Detector GripperPoint;//抓取点的检测器
        [BoxGroup("Grip")] public ValueOutputBool SignalPick;//抓取信号
        [BoxGroup("Grip")] public ValueOutputBool SignalPlace;//放置信号

        [BoxGroup("Grip")] public RobotTool chooseTool;//选择机械臂末端工具，夹爪or吸盘
        //使用夹爪
        [ShowIf("chooseTool", RobotTool.Gripper)]
        [BoxGroup("Gripper")]
        public ValueOutputBool GripperOpen;//控制夹爪张开信号
        [ShowIf("chooseTool", RobotTool.Gripper)]
        [BoxGroup("Gripper")]
        public ValueOutputBool GripperClose;//控制夹爪闭合信号
        [ShowIf("chooseTool", RobotTool.Gripper)]
        [BoxGroup("Gripper")]
        public ValueInputBool GripperOpened;//夹爪已张开信号
        [ShowIf("chooseTool", RobotTool.Gripper)]
        [BoxGroup("Gripper")]
        public ValueInputBool GripperClosed;//夹爪已闭合信号



        // Start is called before the first frame update
        void Start()
        {
            MoveToPick.Value = 0;
            PickObjOK.Value = 0;
            MoveToPlace.Value = 0;
            PlaceObjOK.Value = 0;

            if (chooseTool == RobotTool.Gripper)
            {
                GripperOutOK.Value = 0;
                GripperInOK.Value = 0;
            }


            SignalPick.Value = false;
            SignalPlace.Value = false;

            InteractiveSignalNumber();
        }

        // Update is called once per frame
        void Update()
        {
            RobotControl(stepNow - 1);
        }




        void InteractiveSignalNumber()
        {
            stepNum = Steps.Count;//步骤总数
        }


        /// <summary>
        /// 判断当前机械臂步骤是否能够进行（返回true进行）
        /// </summary>
        /// <param name="steps">设定的一系列步骤</param>
        /// <param name="index">步骤的索引，第几个步骤</param>
        /// <param name="motion">抓取/放置</param>
        /// <returns></returns>
        bool InteractiveSignalDeal(List<RobotArmStep> steps, int index, RobotMotion motion)
        {
            if (steps.Count == 0) return false;
            if (steps.Count <= index) return false;

            if (motion == RobotMotion.Pick)//<!抓取信号
            {
                if (steps[index].PickStart.signals.Count == 0) return true;//未设置信号就默认直接抓取

                if (!steps[index].PickStart.isOr)//"与"
                {
                    var signals = steps[index].PickStart.signals;
                    foreach (var sig in signals)
                    {
                        if (!sig.Value) return false;//有一个信号为假则返回false

                    }
                    return true;
                }
                else//"或"
                {
                    var signals = steps[index].PickStart.signals;
                    foreach (var sig in signals)
                    {
                        if (sig.Value) return true;//有一个信号为真则返回true

                    }
                    return false;
                }
            }
            else if (motion == RobotMotion.Place)//<!放置信号
            {
                if (steps[index].PlaceStart.signals.Count == 0) return true;

                if (!steps[index].PlaceStart.isOr)//"与"
                {
                    var signals = steps[index].PlaceStart.signals;
                    foreach (var sig in signals)
                    {
                        if (!sig.Value) return false;//有一个信号为假则返回false

                    }
                    return true;
                }
                else//"或"
                {
                    var signals = steps[index].PlaceStart.signals;
                    foreach (var sig in signals)
                    {
                        if (sig.Value) return true;//有一个信号为真则返回true

                    }
                    return false;
                }
            }
            else return false;





        }



        void RobotControl(int index)
        {

            switch (StatusLoader)
            {
                case "Home"://初始状态

                    if (InteractiveSignalDeal(Steps, index, RobotMotion.Pick))//将要去抓取
                    {
                        StatusLoader = "PickObj";
                        MoveToPick.Value = 1;

                        /* 抓取MU列表置位 */
                        MUcount = lastMUcount = 0;
                        lastMUcount = GetGripCount(RobotGrip);//获取抓取前的物体数量

                        PickFinish.Value = false;//将抓取成功信号置位
                        PlaceFinish.Value = false;//将放置成功信号置位

                    }



                    break;
                case "Waitting"://等待状态
                    if (PickObjOK.Value == 1 && Waitting.Value == 1 && InteractiveSignalDeal(Steps, index, RobotMotion.Place))//抓取好物体后等待去放置
                    {
                        StatusLoader = "PlaceObj";
                        MoveToPlace.Value = 1;
                        PickObjOK.Value = 0;
                        if (chooseTool == RobotTool.Gripper)
                        {
                            GripperInOK.Value = 0;
                            GripperOutOK.Value = 0;
                        }

                        PickFinish.Value = false;//将抓取成功信号置位

                        lastMUcount = MUcount;//获取放置前的物体数量


                    }

                    if (PlaceObjOK.Value == 1 && Waitting.Value == 1)//放置好物体后返回Home状态
                    {
                        StatusLoader = "Home";

                        PlaceObjOK.Value = 0;
                        if (chooseTool == RobotTool.Gripper)
                        {
                            GripperInOK.Value = 0;
                            GripperOutOK.Value = 0;
                        }
                        PlaceFinish.Value = false;//将放置成功信号置位

                        stepNow++;
                        if (stepNow > stepNum)
                        {
                            stepNow = 1;
                        }

                    }

                    break;
                case "PickObj"://抓取状态
                    if (chooseTool == RobotTool.Suck)
                    {
                        if (MoveToPick.Value == 1 && MakePick.Value == 1)
                        {


                            MUcount = GetGripCount(RobotGrip);//获取当前的数量
                            if (MUcount > lastMUcount)//夹爪抓取列表中多了对象，则认为抓取成功，否则认为未抓取或抓取失败
                            {
                                PickObjOK.Value = 1;//抓取成功
                                PickFinish.Value = true;//输出抓取成功信号

                                StatusLoader = "Waitting";
                                MoveToPick.Value = 0;

                            }
                            else
                                GriperPickOrPlace(1);//未抓取或抓取失败，则进行抓取

                        }
                    }
                    else
                    {
                        if (MoveToPick.Value == 1 && MakeGripperOut.Value == 1)
                        {
                            GripperOpen.Value = true;//使夹爪张开

                            MoveToPick.Value = 0;
                        }
                        if (GripperOpened.Value == true && MakeGripperOut.Value == 1)//夹爪已张开
                        {
                            GripperOpen.Value = false;
                            GripperOutOK.Value = 1;//向roboDK发送已张开信号 ——> 执行抓取动作


                        }
                        if (MakeGripperIn.Value == 1)
                        {
                            GripperClose.Value = true;//使夹爪闭合
                            GripperOutOK.Value = 0;
                        }

                        if (GripperClosed.Value == true && MakeGripperIn.Value == 1)//夹爪已闭合
                        {
                            //MakeGripperIn.Value = 0;<!在roboDK里置位
                            GripperClose.Value = false;
                            GripperInOK.Value = 1;//向roboDK发送已已闭合信号 ——> 执行抓取动作
                        }

                        if (MakePick.Value == 1 && GripperInOK.Value == 1)
                        {


                            MUcount = GetGripCount(RobotGrip);//获取当前的数量
                            if (MUcount > lastMUcount)//夹爪抓取列表中多了对象，则认为抓取成功，否则认为未抓取或抓取失败
                            {
                                PickObjOK.Value = 1;//抓取成功
                                PickFinish.Value = true;//输出抓取成功信号

                                StatusLoader = "Waitting";

                            }
                            else
                                GriperPickOrPlace(1);//未抓取或抓取失败，则进行抓取
                        }
                    }


                    break;
                case "PlaceObj"://放置状态
                    if (chooseTool == RobotTool.Suck)
                    {
                        if (MoveToPlace.Value == 1 && MakePlace.Value == 1)
                        {


                            MUcount = GetGripCount(RobotGrip);//获取当前的数量
                            if (MUcount < lastMUcount)//夹爪抓取列表中少了对象，则认为放置成功，否则认为未放置或放置失败
                            {
                                PlaceObjOK.Value = 1;//放置成功
                                PlaceFinish.Value = true;//输出放置成功信号

                                StatusLoader = "Waitting";
                                MoveToPlace.Value = 0;

                                succCount++;
                            }
                            else
                                GriperPickOrPlace(2);//未抓取或抓取失败，则进行抓取

                        }
                    }
                    else
                    {
                        if (MoveToPlace.Value == 1 && MakeGripperOut.Value == 1)
                        {
                            lastMUcount = MUcount;//获取放置前的物体数量
                            GripperOpen.Value = true;//使夹爪张开

                            MoveToPlace.Value = 0;
                        }
                        if (GripperOpened.Value == true && MakeGripperOut.Value == 1)//夹爪已张开
                        {
                            GripperOpen.Value = false;
                            GripperOutOK.Value = 1;//向roboDK发送已张开信号 ——> 执行抓取动作
                        }

                        if (MakePlace.Value == 1 && GripperOutOK.Value == 1)
                        {
                            // GripperOutOK.Value = 0;

                            MUcount = GetGripCount(RobotGrip);//获取当前的数量
                            if (MUcount < lastMUcount)//夹爪抓取列表中少了对象，则认为放置成功，否则认为未放置或放置失败
                            {
                                PlaceObjOK.Value = 1;//放置成功
                                PlaceFinish.Value = true;//输出放置成功信号

                                succCount++;
                            }
                            else
                                GriperPickOrPlace(2);//未放置或放置失败，则进行放置
                        }

                        if (PlaceObjOK.Value == 1 && MakePlace.Value == 0)
                        {
                            if (MakeGripperIn.Value == 1)
                            {
                                GripperClose.Value = true;//使夹爪闭合
                                GripperOutOK.Value = 0;
                            }

                            if (GripperClosed.Value == true)//夹爪已闭合
                            {
                                GripperClose.Value = false;
                                GripperInOK.Value = 1;

                                StatusLoader = "Waitting";

                            }
                        }

                    }



                    break;


            }
        }



        /// <summary>
        /// 夹爪抓取或放置
        /// </summary>
        /// <param name="index">1:抓取  2:放置</param>
        /// <returns></returns>
        int GriperPickOrPlace(int index = 0)
        {
            //SuckPick.Value = false;
            //SuckPlace.Value = false;

            if (index == 1)
            {
                SignalPick.Value = true;
                SignalPlace.Value = false;
                Invoke("ResetPickAndPlaceValue", 2f);
                return 1;
            }
            else if (index == 2)
            {
                SignalPick.Value = false;
                SignalPlace.Value = true;
                Invoke("ResetPickAndPlaceValue", 2f);
                return 2;
            }

            return 0;

        }

        void ResetPickAndPlaceValue()
        {
            SignalPick.Value = false;
            SignalPlace.Value = false;
        }


        int MUcount = 0;
        int lastMUcount = 0;

        /// <summary>
        /// 获取夹爪中抓取到的MU数量
        /// </summary>
        /// <param name="grip"></param>
        /// <returns></returns>
        int GetGripCount(Grip grip)
        {
            if (grip != null)
                return grip.PickedMUs.Count;
            //if (GripperPoint != null)
            //    return GripperPoint.EffectObjects.Count;
            else
                return 0;
        }


    }
}

