using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System;

//��Ϊ��ؿ��ƽű�������ʵ���ڹ���״̬����ʵ��PieceStateMachine
namespace VisualSpline
{
    public class GEN_Control_Piece_Drive : MonoBehaviour
    {
        //�ӹ������嵥
        [Serializable]
        public class Processing_scheme
        { 
            public string Process_name;
            public int step;//����
            public int machine;
            public int AGV;
        }
        public List<Processing_scheme> SchemeList = new List<Processing_scheme>();

        public GEN_Encoed_result Encoed_Scripts;
        //������Ϣ
        public int piece_name;//��������-�ܿظ�
        public int process_max;//�����
        [ReadOnly] public int processStep = 0; //�������
        [ReadOnly] public string processStepName = ""; //��������
        [ReadOnly] public SplinePoint piecePlace; //����λ��-��ʼ�����ܿظ�
        [ReadOnly] public SplinePoint pieceEndPlace; //����λ��-��ʼ�����ܿظ�
        [ReadOnly] public SplinePoint pieceStartPlace; //����λ��-��ʼ�����ܿظ�

        public int currentPieceNum = 0;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
        }
    }
}

