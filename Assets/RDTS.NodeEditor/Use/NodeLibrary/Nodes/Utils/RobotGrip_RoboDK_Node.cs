using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RDTS;
using System;

namespace RDTS.NodeEditor
{
    [System.Serializable, NodeMenuItem("Utils/RobotGrip_RoboDK")]
    public class RobotGrip_RoboDK_Node : BaseNode
    {
        public override string name => "RobotGrip_RoboDK";
        public override bool isRenamable => true;//可重命名

        public string status;
        public int stepNum = 1;//一共需要执行几组动作

        [BindObject]
        public Grip RobotGrip;
        public int muCount;
        public int lastMUCount;

        [NonSerialized]
        private string StatusLoader = "Home";
        [NonSerialized]
        public int stepNow = 1;//当前执行的机械臂动作步骤
        [NonSerialized]
        public int succCount = 0;//机械臂完成抓/放的工件数量
        [NonSerialized]
        public int failCount = 0;//机械臂未完成抓/放的工件数量


        Utility.Detector GripperPoint;
        int MUcount = 0;
        int lastMUcount = 0;

        //抓取/放置开始信号
        [Input("PickStart")][NonSerialized]
        public bool pickStart;
        [Input("PlaceStart")][NonSerialized]
        public bool placeStart;
        //RoboDK信号
        [Input("MakePick")][NonSerialized]
        public int makePick;
        [Input("MakePlace")][NonSerialized]
        public int makePlace;
        [Input("Waitting")][NonSerialized]
        public int waitting;
        [Input("MakeGripperOut")][NonSerialized]
        public int makeGripperOut;
        [Input("MakeGripperIn")][NonSerialized]
        public int makeGripperIn;
        //接受夹爪是否已张开/闭合的信号
        [Input("GripperOpened")][NonSerialized]
        public bool gripperOpened;
        [Input("GripperClosed")][NonSerialized]
        public bool gripperClosed;

        //RoboDK信号
        [Output("MoveToPick")][NonSerialized]
        public int moveToPick;
        [Output("PickObjOK")][NonSerialized]
        public int pickObjOK;
        [Output("MoveToPlace")][NonSerialized]
        public int moveToPlace;
        [Output("PlaceObjOK")][NonSerialized]
        public int placeObjOK;
        [Output("GripperOutOK")][NonSerialized]
        public int gripperOutOK;
        [Output("GripperInOK")][NonSerialized]
        public int gripperInOK;

        //抓取/放置完成的输出信号
        [Output("PickFinish")][NonSerialized]
        public bool pickFinish;
        [Output("PlaceFinish")][NonSerialized]
        public bool placeFinish;
        //控制夹爪抓/放的信号
        [Output("SignalPick")][NonSerialized]
        public bool signalPick;
        [Output("SignalPlace")][NonSerialized]
        public bool signalPlace;
        //控制夹爪张开/闭合的信号
        [Output("GripperOpen")][NonSerialized]
        public bool gripperOpen;
        [Output("GripperClose")][NonSerialized]
        public bool gripperClose;
        


        //// Start is called before the first frame update
        //void Start()
        //{
        //    moveToPick = 0;
        //    pickObjOK = 0;
        //    moveToPlace = 0;
        //    placeObjOK = 0;

        //    pickStart = false;
        //    placeStart = false;

        //    //if (chooseTool == RobotTool.Gripper)
        //    //{
        //    //    GripperOutOK.Value = 0;
        //    //    GripperInOK.Value = 0;
        //    //}


        //    signalPick = false;
        //    signalPlace = false;

        //    StatusLoader = "Home";
        //}

        //// Update is called once per frame
        //void Update()
        //{

        //}

        protected override void Process()
        {
            RobotControl();

        }

        void RobotControl()
        {
            if (!signalPick || !signalPlace)
                ResetPickAndPlaceValue();

            status = StatusLoader;
            muCount = MUcount;
            lastMUCount = lastMUcount;

            switch (StatusLoader)
            {
                case "Home"://初始状态

                    if (pickStart)
                    {
                        StatusLoader = "PickObj";
                        moveToPick = 1;

                        /* 抓取MU列表置位 */
                        MUcount = lastMUcount = 0;
                        lastMUcount = GetGripCount(RobotGrip);//获取抓取前的物体数量

                        pickFinish = false;//将抓取成功信号置位
                        placeFinish = false;//将放置成功信号置位

                    }



                    break;
                case "Waitting"://等待状态
                    if (pickObjOK == 1 && waitting == 1 && placeStart)//抓取好物体后等待去放置
                    {
                        StatusLoader = "PlaceObj";
                        moveToPlace = 1;
                        pickObjOK = 0;

                        gripperInOK = 0;
                        gripperOutOK = 0;


                        pickFinish = false;//将抓取成功信号置位

                        lastMUcount = MUcount;//获取放置前的物体数量


                    }

                    if (placeObjOK == 1 && waitting == 1)
                    {
                        StatusLoader = "Home";

                        placeObjOK = 0;
                        gripperInOK = 0;
                        gripperOutOK = 0;
                        placeFinish = false;//将放置成功信号置位

                        stepNow++;
                        if (stepNow > stepNum)
                        {
                            stepNow = 1;
                        }

                    }

                    break;
                case "PickObj"://抓取状态

                    if (moveToPick == 1 && makeGripperOut == 1)
                    {
                        gripperOpen = true;//使夹爪张开

                        moveToPick = 0;
                    }
                    if (gripperOpened == true && makeGripperOut == 1)//夹爪已张开
                    {
                        gripperOpen = false;
                        gripperOutOK = 1;//向roboDK发送已张开信号 ——> 执行抓取动作


                    }
                    if (makeGripperIn == 1)
                    {
                        gripperClose = true;//使夹爪闭合
                        gripperOutOK = 0;
                    }

                    if (gripperClosed == true && makeGripperIn == 1)//夹爪已闭合
                    {
                        //MakeGripperIn.Value = 0;<!在roboDK里置位
                        gripperClose = false;
                        gripperInOK = 1;//向roboDK发送已已闭合信号 ——> 执行抓取动作
                    }

                    if (makePick == 1 && gripperInOK == 1)
                    {


                        MUcount = GetGripCount(RobotGrip);//获取当前的数量
                        if (MUcount > lastMUcount)//夹爪抓取列表中多了对象，则认为抓取成功，否则认为未抓取或抓取失败
                        {
                            pickObjOK = 1;//抓取成功
                            pickFinish = true;//输出抓取成功信号

                            StatusLoader = "Waitting";

                        }
                        else
                            GriperPickOrPlace(1);//未抓取或抓取失败，则进行抓取
                    }



                    break;
                case "PlaceObj"://放置状态


                    if (moveToPlace == 1 && makeGripperOut == 1)
                    {
                        lastMUcount = MUcount;//获取放置前的物体数量
                        gripperOpen = true;//使夹爪张开

                        moveToPlace = 0;
                    }
                    if (gripperOpened == true && makeGripperOut == 1)//夹爪已张开
                    {
                        gripperOpen = false;
                        gripperOutOK = 1;//向roboDK发送已张开信号 ——> 执行抓取动作
                    }

                    if (makePlace == 1 && gripperOutOK == 1)
                    {
                        // GripperOutOK.Value = 0;

                        MUcount = GetGripCount(RobotGrip);//获取当前的数量
                        if (MUcount < lastMUcount)//夹爪抓取列表中少了对象，则认为放置成功，否则认为未放置或放置失败
                        {
                            placeObjOK = 1;//放置成功
                            placeFinish = true;//输出放置成功信号

                            succCount++;
                        }
                        else
                            GriperPickOrPlace(2);//未放置或放置失败，则进行放置
                    }

                    if (placeObjOK == 1 && makePlace == 0)
                    {
                        if (makeGripperIn == 1)
                        {
                            gripperClose = true;//使夹爪闭合
                            gripperOutOK = 0;
                        }

                        if (gripperClosed == true)//夹爪已闭合
                        {
                            gripperClose = false;
                            gripperInOK = 1;

                            StatusLoader = "Waitting";

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

            if (index == 1)
            {
                signalPick = true;
                signalPlace = false;

                return 1;
            }
            else if (index == 2)
            {
                signalPick = false;
                signalPlace = true;

                return 2;
            }

            return 0;

        }

        void ResetPickAndPlaceValue()
        {
            signalPick = false;
            signalPlace = false;
        }




        /// <summary>
        /// 获取夹爪中抓取到的MU数量
        /// </summary>
        /// <param name="grip"></param>
        /// <returns></returns>
        int GetGripCount(Grip grip)
        {
            if (grip != null)
                return grip.PickedMUs.Count;
            if (GripperPoint != null)
                return GripperPoint.EffectObjects.Count;
            else
                return 0;
        }


    }

}

