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
    //    public bool isOr = false;//<! trueΪ���롱��falseΪ����
    //    public List<ValueOutputBool> pickSignal;
    //}
    //[Serializable]
    //public class PlaceStartSignal
    //{
    //    public string name = "signal";
    //    public bool isOr = false;//<! trueΪ���롱��falseΪ����
    //    public List<ValueOutputBool> placeSignal;
    //}
    [Serializable]
    public class StartSignal
    {
        public string name = "signal";
        public bool isOr = false;//<! trueΪ���롱��falseΪ����
        public List<ValueOutputBool> signals;
    }

    [Serializable]
    public class InputSignal
    {
        public bool isOr = false;//<! trueΪ���롱��falseΪ����
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
    /// ����RoboDK���ʹ�õĻ�е�ۿ��ƽű���
    /// </summary>
    [SelectionBase]
    public class RobotArm : ControlLogic
    {
        [BoxGroup("Status")] public string StatusLoader = "Home";
        [BoxGroup("Status")] [NaughtyAttributes.ReadOnly] public int stepNum = 1;//һ����Ҫִ�м��鶯��
        [BoxGroup("Status")] [NaughtyAttributes.ReadOnly] public int stepNow = 1;//��ǰִ�еĻ�е�۶�������
        [BoxGroup("Status")] [NaughtyAttributes.ReadOnly] public int succCount = 0;//��е�����ץ/�ŵĹ�������
        [BoxGroup("Status")] [NaughtyAttributes.ReadOnly] public int failCount = 0;//��е��δ���ץ/�ŵĹ�������

        /* ��ʵ�ʲ��߽������ź� *////* ����; */
        //�������б���Ӧ��ͬ
        [BoxGroup("Interactive Signal")] public List<RobotArmStep> Steps;

        [BoxGroup("Interactive Signal")] public ValueOutputBool PickFinish;//ץȡ����ź�
        [BoxGroup("Interactive Signal")] public ValueOutputBool PlaceFinish;//��������ź�

        //RoboDK����ź�
        [BoxGroup("RoboDK Signal")] public ValueInputInt MoveToPick;//��Ҫȥץȡ
        [BoxGroup("RoboDK Signal")] public ValueOutputInt MakePick;//ץȡ
        [BoxGroup("RoboDK Signal")] public ValueInputInt PickObjOK;//ץȡ���
        [BoxGroup("RoboDK Signal")] public ValueInputInt MoveToPlace;//��Ҫȥ����
        [BoxGroup("RoboDK Signal")] public ValueOutputInt MakePlace;//����
        [BoxGroup("RoboDK Signal")] public ValueInputInt PlaceObjOK;//�������
        [BoxGroup("RoboDK Signal")] public ValueOutputInt Waitting;//�ȴ�
        [Header("RoboDK Signal use Gripper")]//��צ������Ҫ���ĸ��źţ���ʵ�ּ�צ���ſ����պ�Ч��
        [ShowIf("chooseTool", RobotTool.Gripper)] [BoxGroup("RoboDK Signal")] public ValueOutputInt MakeGripperOut;//ʹ��צ�ſ�
        [ShowIf("chooseTool", RobotTool.Gripper)] [BoxGroup("RoboDK Signal")] public ValueInputInt GripperOutOK;//��צ�ſ����
        [ShowIf("chooseTool", RobotTool.Gripper)] [BoxGroup("RoboDK Signal")] public ValueOutputInt MakeGripperIn;//ʹ��צ�պ�
        [ShowIf("chooseTool", RobotTool.Gripper)] [BoxGroup("RoboDK Signal")] public ValueInputInt GripperInOK;//��צ�պ����

        [BoxGroup("Grip")] public Grip RobotGrip;//������Grip�ű�
        //[BoxGroup("Grip")] public Detector GripperPoint;//ץȡ��ļ����
        [BoxGroup("Grip")] public ValueOutputBool SignalPick;//ץȡ�ź�
        [BoxGroup("Grip")] public ValueOutputBool SignalPlace;//�����ź�

        [BoxGroup("Grip")] public RobotTool chooseTool;//ѡ���е��ĩ�˹��ߣ���צor����
        //ʹ�ü�צ
        [ShowIf("chooseTool", RobotTool.Gripper)]
        [BoxGroup("Gripper")]
        public ValueOutputBool GripperOpen;//���Ƽ�צ�ſ��ź�
        [ShowIf("chooseTool", RobotTool.Gripper)]
        [BoxGroup("Gripper")]
        public ValueOutputBool GripperClose;//���Ƽ�צ�պ��ź�
        [ShowIf("chooseTool", RobotTool.Gripper)]
        [BoxGroup("Gripper")]
        public ValueInputBool GripperOpened;//��צ���ſ��ź�
        [ShowIf("chooseTool", RobotTool.Gripper)]
        [BoxGroup("Gripper")]
        public ValueInputBool GripperClosed;//��צ�ѱպ��ź�



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
            stepNum = Steps.Count;//��������
        }


        /// <summary>
        /// �жϵ�ǰ��е�۲����Ƿ��ܹ����У�����true���У�
        /// </summary>
        /// <param name="steps">�趨��һϵ�в���</param>
        /// <param name="index">������������ڼ�������</param>
        /// <param name="motion">ץȡ/����</param>
        /// <returns></returns>
        bool InteractiveSignalDeal(List<RobotArmStep> steps, int index, RobotMotion motion)
        {
            if (steps.Count == 0) return false;
            if (steps.Count <= index) return false;

            if (motion == RobotMotion.Pick)//<!ץȡ�ź�
            {
                if (steps[index].PickStart.signals.Count == 0) return true;//δ�����źž�Ĭ��ֱ��ץȡ

                if (!steps[index].PickStart.isOr)//"��"
                {
                    var signals = steps[index].PickStart.signals;
                    foreach (var sig in signals)
                    {
                        if (!sig.Value) return false;//��һ���ź�Ϊ���򷵻�false

                    }
                    return true;
                }
                else//"��"
                {
                    var signals = steps[index].PickStart.signals;
                    foreach (var sig in signals)
                    {
                        if (sig.Value) return true;//��һ���ź�Ϊ���򷵻�true

                    }
                    return false;
                }
            }
            else if (motion == RobotMotion.Place)//<!�����ź�
            {
                if (steps[index].PlaceStart.signals.Count == 0) return true;

                if (!steps[index].PlaceStart.isOr)//"��"
                {
                    var signals = steps[index].PlaceStart.signals;
                    foreach (var sig in signals)
                    {
                        if (!sig.Value) return false;//��һ���ź�Ϊ���򷵻�false

                    }
                    return true;
                }
                else//"��"
                {
                    var signals = steps[index].PlaceStart.signals;
                    foreach (var sig in signals)
                    {
                        if (sig.Value) return true;//��һ���ź�Ϊ���򷵻�true

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
                case "Home"://��ʼ״̬

                    if (InteractiveSignalDeal(Steps, index, RobotMotion.Pick))//��Ҫȥץȡ
                    {
                        StatusLoader = "PickObj";
                        MoveToPick.Value = 1;

                        /* ץȡMU�б���λ */
                        MUcount = lastMUcount = 0;
                        lastMUcount = GetGripCount(RobotGrip);//��ȡץȡǰ����������

                        PickFinish.Value = false;//��ץȡ�ɹ��ź���λ
                        PlaceFinish.Value = false;//�����óɹ��ź���λ

                    }



                    break;
                case "Waitting"://�ȴ�״̬
                    if (PickObjOK.Value == 1 && Waitting.Value == 1 && InteractiveSignalDeal(Steps, index, RobotMotion.Place))//ץȡ�������ȴ�ȥ����
                    {
                        StatusLoader = "PlaceObj";
                        MoveToPlace.Value = 1;
                        PickObjOK.Value = 0;
                        if (chooseTool == RobotTool.Gripper)
                        {
                            GripperInOK.Value = 0;
                            GripperOutOK.Value = 0;
                        }

                        PickFinish.Value = false;//��ץȡ�ɹ��ź���λ

                        lastMUcount = MUcount;//��ȡ����ǰ����������


                    }

                    if (PlaceObjOK.Value == 1 && Waitting.Value == 1)//���ú�����󷵻�Home״̬
                    {
                        StatusLoader = "Home";

                        PlaceObjOK.Value = 0;
                        if (chooseTool == RobotTool.Gripper)
                        {
                            GripperInOK.Value = 0;
                            GripperOutOK.Value = 0;
                        }
                        PlaceFinish.Value = false;//�����óɹ��ź���λ

                        stepNow++;
                        if (stepNow > stepNum)
                        {
                            stepNow = 1;
                        }

                    }

                    break;
                case "PickObj"://ץȡ״̬
                    if (chooseTool == RobotTool.Suck)
                    {
                        if (MoveToPick.Value == 1 && MakePick.Value == 1)
                        {


                            MUcount = GetGripCount(RobotGrip);//��ȡ��ǰ������
                            if (MUcount > lastMUcount)//��צץȡ�б��ж��˶�������Ϊץȡ�ɹ���������Ϊδץȡ��ץȡʧ��
                            {
                                PickObjOK.Value = 1;//ץȡ�ɹ�
                                PickFinish.Value = true;//���ץȡ�ɹ��ź�

                                StatusLoader = "Waitting";
                                MoveToPick.Value = 0;

                            }
                            else
                                GriperPickOrPlace(1);//δץȡ��ץȡʧ�ܣ������ץȡ

                        }
                    }
                    else
                    {
                        if (MoveToPick.Value == 1 && MakeGripperOut.Value == 1)
                        {
                            GripperOpen.Value = true;//ʹ��צ�ſ�

                            MoveToPick.Value = 0;
                        }
                        if (GripperOpened.Value == true && MakeGripperOut.Value == 1)//��צ���ſ�
                        {
                            GripperOpen.Value = false;
                            GripperOutOK.Value = 1;//��roboDK�������ſ��ź� ����> ִ��ץȡ����


                        }
                        if (MakeGripperIn.Value == 1)
                        {
                            GripperClose.Value = true;//ʹ��צ�պ�
                            GripperOutOK.Value = 0;
                        }

                        if (GripperClosed.Value == true && MakeGripperIn.Value == 1)//��צ�ѱպ�
                        {
                            //MakeGripperIn.Value = 0;<!��roboDK����λ
                            GripperClose.Value = false;
                            GripperInOK.Value = 1;//��roboDK�������ѱպ��ź� ����> ִ��ץȡ����
                        }

                        if (MakePick.Value == 1 && GripperInOK.Value == 1)
                        {


                            MUcount = GetGripCount(RobotGrip);//��ȡ��ǰ������
                            if (MUcount > lastMUcount)//��צץȡ�б��ж��˶�������Ϊץȡ�ɹ���������Ϊδץȡ��ץȡʧ��
                            {
                                PickObjOK.Value = 1;//ץȡ�ɹ�
                                PickFinish.Value = true;//���ץȡ�ɹ��ź�

                                StatusLoader = "Waitting";

                            }
                            else
                                GriperPickOrPlace(1);//δץȡ��ץȡʧ�ܣ������ץȡ
                        }
                    }


                    break;
                case "PlaceObj"://����״̬
                    if (chooseTool == RobotTool.Suck)
                    {
                        if (MoveToPlace.Value == 1 && MakePlace.Value == 1)
                        {


                            MUcount = GetGripCount(RobotGrip);//��ȡ��ǰ������
                            if (MUcount < lastMUcount)//��צץȡ�б������˶�������Ϊ���óɹ���������Ϊδ���û����ʧ��
                            {
                                PlaceObjOK.Value = 1;//���óɹ�
                                PlaceFinish.Value = true;//������óɹ��ź�

                                StatusLoader = "Waitting";
                                MoveToPlace.Value = 0;

                                succCount++;
                            }
                            else
                                GriperPickOrPlace(2);//δץȡ��ץȡʧ�ܣ������ץȡ

                        }
                    }
                    else
                    {
                        if (MoveToPlace.Value == 1 && MakeGripperOut.Value == 1)
                        {
                            lastMUcount = MUcount;//��ȡ����ǰ����������
                            GripperOpen.Value = true;//ʹ��צ�ſ�

                            MoveToPlace.Value = 0;
                        }
                        if (GripperOpened.Value == true && MakeGripperOut.Value == 1)//��צ���ſ�
                        {
                            GripperOpen.Value = false;
                            GripperOutOK.Value = 1;//��roboDK�������ſ��ź� ����> ִ��ץȡ����
                        }

                        if (MakePlace.Value == 1 && GripperOutOK.Value == 1)
                        {
                            // GripperOutOK.Value = 0;

                            MUcount = GetGripCount(RobotGrip);//��ȡ��ǰ������
                            if (MUcount < lastMUcount)//��צץȡ�б������˶�������Ϊ���óɹ���������Ϊδ���û����ʧ��
                            {
                                PlaceObjOK.Value = 1;//���óɹ�
                                PlaceFinish.Value = true;//������óɹ��ź�

                                succCount++;
                            }
                            else
                                GriperPickOrPlace(2);//δ���û����ʧ�ܣ�����з���
                        }

                        if (PlaceObjOK.Value == 1 && MakePlace.Value == 0)
                        {
                            if (MakeGripperIn.Value == 1)
                            {
                                GripperClose.Value = true;//ʹ��צ�պ�
                                GripperOutOK.Value = 0;
                            }

                            if (GripperClosed.Value == true)//��צ�ѱպ�
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
        /// ��צץȡ�����
        /// </summary>
        /// <param name="index">1:ץȡ  2:����</param>
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
        /// ��ȡ��צ��ץȡ����MU����
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

