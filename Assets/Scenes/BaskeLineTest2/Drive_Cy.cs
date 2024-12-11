using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RDTS
{
    public class Drive_Cy : MonoBehaviour
    {
        public float Position;
        //public int TimeOut;
        //public int TimeIn;
        public float timeout = 1;
        public float timein = 1;

        public ValueOutputBool ExtractPushOut;
        public ValueOutputBool ExtractPushIn;
        public ValueInputBool ExtractPushOutFinish;
        public ValueInputBool ExtractPushInFinish;

        public ValueInputBool TransportSensor;

        private bool Flag_pushout = false;
        private bool Flag_pushin = false;
        private bool Flag_transport = false;
        private Drive Get_Drive;
        private float  Get_CurrentPosition;
        // Start is called before the first frame update
        void Start()
        {
            Get_Drive = GetComponent<Drive>();
            //初始化PLC信号
            //ExtractPushOut.Value = false;
            //ExtractPushIn.Value = false;
            //ExtractPushOutFinish.Value = false;
            //ExtractPushOutFinish.Value = false;

        }

        // Update is called once per frame
        void Update()
        {
            Get_CurrentPosition = Get_Drive.CurrentPosition;

            //推进信号判断
            if (ExtractPushOut != null && ExtractPushOut.Value == true )
            {
                ExtractPushInFinish.Value = false;
                Flag_pushout = true;
            }
            if (ExtractPushIn != null && ExtractPushIn.Value == true)
            {
                ExtractPushOutFinish.Value = false;
                Flag_pushin = true;
            }
            if (TransportSensor != null && TransportSensor .Value == true)
            {
                Flag_transport = true;
            }
            if(TransportSensor != null && TransportSensor.Value == false)
            {
                Flag_transport = false;
            }

            //推进过程赋值
            if (Flag_pushout == true)
            {
                Get_Drive.TargetSpeed = Math.Abs(Position - 0) / timeout;
                Get_Drive.DriveTo(Position);
            }
            if (Flag_pushin == true)
            {
                Get_Drive.TargetSpeed = Math.Abs(Position - 0) / timein;
                Get_Drive.DriveTo(0);
            }
            if ((Flag_pushout | Flag_pushin) == false)
            {
                Get_Drive.Stop();
            }
            if (TransportSensor != null && Flag_transport == true)
            {
                Get_Drive.Stop();
            }
            if(TransportSensor != null && Flag_transport == false)
            {
                Get_Drive.Forward();
            }

            //推进结束返回
            if (ExtractPushOutFinish != null && Get_CurrentPosition >= Position)
            {
                ExtractPushOutFinish.Value = true;
                Flag_pushout = false;
            }
            if (ExtractPushInFinish != null && Get_CurrentPosition <= 0)
            {
                ExtractPushInFinish.Value = true;
                Flag_pushin = false;
            }

             


        }
    }

}
