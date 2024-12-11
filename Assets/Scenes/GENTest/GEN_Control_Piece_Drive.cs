using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System;

//作为监控控制脚本，具体实现在工件状态机中实现PieceStateMachine
namespace VisualSpline
{
    public class GEN_Control_Piece_Drive : MonoBehaviour
    {
        //加工方案清单
        [Serializable]
        public class Processing_scheme
        { 
            public string Process_name;
            public int step;//次序
            public int machine;
            public int AGV;
        }
        public List<Processing_scheme> SchemeList = new List<Processing_scheme>();

        public GEN_Encoed_result Encoed_Scripts;
        //工件信息
        public int piece_name;//工件名称-总控给
        public int process_max;//最大工序
        [ReadOnly] public int processStep = 0; //工序进度
        [ReadOnly] public string processStepName = ""; //工序名称
        [ReadOnly] public SplinePoint piecePlace; //工件位置-初始化由总控给
        [ReadOnly] public SplinePoint pieceEndPlace; //工件位置-初始化由总控给
        [ReadOnly] public SplinePoint pieceStartPlace; //工件位置-初始化由总控给

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

