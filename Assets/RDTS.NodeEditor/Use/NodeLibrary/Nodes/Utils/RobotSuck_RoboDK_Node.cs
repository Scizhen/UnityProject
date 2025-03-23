using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RDTS;
using System;

namespace RDTS.NodeEditor
{
    [System.Serializable, NodeMenuItem("Utils/RobotSuck_RoboDK")]
    public class RobotSuck_RoboDK_Node : BaseNode
    {
        public override string name => "RobotSuck_RoboDK";
        public override bool isRenamable => true;//��������

        public string status;
        public int stepNum = 1;//һ����Ҫִ�м��鶯��

        [BindObject]
        public Grip RobotGrip;
        public int muCount;
        public int lastMUCount;

        [NonSerialized]
        private string StatusLoader = "Home";
        [NonSerialized]
        public int stepNow = 1;//��ǰִ�еĻ�е�۶�������
        [NonSerialized]
        public int succCount = 0;//��е�����ץ/�ŵĹ�������
        [NonSerialized]
        public int failCount = 0;//��е��δ���ץ/�ŵĹ�������


        Utility.Detector GripperPoint;
        int MUcount = 0;
        int lastMUcount = 0;

        //ץȡ/���ÿ�ʼ�ź�
        [Input("PickStart")][NonSerialized]
        public bool pickStart;
        [Input("PlaceStart")][NonSerialized]
        public bool placeStart;
        //RoboDK�ź�
        [Input("MakePick")][NonSerialized]
        public int makePick;
        [Input("MakePlace")][NonSerialized]
        public int makePlace;
        [Input("Waitting")][NonSerialized]
        public int waitting;

        //RoboDK�ź�
        [Output("MoveToPick")][NonSerialized]
        public int moveToPick;
        [Output("PickObjOK")][NonSerialized]
        public int pickObjOK;
        [Output("MoveToPlace")][NonSerialized]
        public int moveToPlace;
        [Output("PlaceObjOK")][NonSerialized]
        public int placeObjOK;

        //ץȡ/������ɵ�����ź�
        [Output("PickFinish")][NonSerialized]
        public bool pickFinish;
        [Output("PlaceFinish")][NonSerialized]
        public bool placeFinish;
        //���Ƽ�צץ/�ŵ��ź�
        [Output("SignalPick")][NonSerialized]
        public bool signalPick;
        [Output("SignalPlace")][NonSerialized]
        public bool signalPlace;

      
        protected override void Process()
        {
             RobotControl();
        }

        void RobotControl()
        {
            ResetPickAndPlaceValue();

            status = StatusLoader;
            muCount = MUcount;
            lastMUCount = lastMUcount;

            switch (StatusLoader)
            {
                case "Home"://��ʼ״̬

                    if (pickStart)
                    {
                        StatusLoader = "PickObj";
                        moveToPick = 1;

                        /* ץȡMU�б���λ */
                        MUcount = lastMUcount = 0;
                        lastMUcount = GetGripCount(RobotGrip);//��ȡץȡǰ����������

                        pickFinish = false;//��ץȡ�ɹ��ź���λ
                        placeFinish = false;//�����óɹ��ź���λ

                    }



                    break;
                case "Waitting"://�ȴ�״̬
                    if (pickObjOK == 1 && waitting == 1 && placeStart)//ץȡ�������ȴ�ȥ����
                    {
                        StatusLoader = "PlaceObj";
                        moveToPlace = 1;
                        pickObjOK = 0;
                        pickFinish = false;//��ץȡ�ɹ��ź���λ

                        lastMUcount = MUcount;//��ȡ����ǰ����������


                    }

                    if (placeObjOK == 1 && waitting == 1)
                    {
                        StatusLoader = "Home";

                        placeObjOK = 0;
                        placeFinish = false;//�����óɹ��ź���λ

                        stepNow++;
                        if (stepNow > stepNum)
                        {
                            stepNow = 1;
                        }

                    }

                    break;
                case "PickObj"://ץȡ״̬
                    if (moveToPick == 1 && makePick == 1)
                    {


                        MUcount = GetGripCount(RobotGrip);//��ȡ��ǰ������
                        if (MUcount > lastMUcount)//��צץȡ�б��ж��˶�������Ϊץȡ�ɹ���������Ϊδץȡ��ץȡʧ��
                        {
                            pickObjOK = 1;//ץȡ�ɹ�
                            pickFinish = true;//���ץȡ�ɹ��ź�

                            StatusLoader = "Waitting";
                            moveToPick = 0;

                        }
                        else
                            GriperPickOrPlace(1);//δץȡ��ץȡʧ�ܣ������ץȡ

                    }


                    break;
                case "PlaceObj"://����״̬
                    if (moveToPlace == 1 && makePlace == 1)
                    {


                        MUcount = GetGripCount(RobotGrip);//��ȡ��ǰ������
                        if (MUcount < lastMUcount)//��צץȡ�б������˶�������Ϊ���óɹ���������Ϊδ���û����ʧ��
                        {
                            placeObjOK = 1;//���óɹ�
                            placeFinish = true;//������óɹ��ź�

                            StatusLoader = "Waitting";
                            moveToPlace = 0;

                            succCount++;
                        }
                        else
                            GriperPickOrPlace(2);//δץȡ��ץȡʧ�ܣ������ץȡ

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
        /// ��ȡ��צ��ץȡ����MU����
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
