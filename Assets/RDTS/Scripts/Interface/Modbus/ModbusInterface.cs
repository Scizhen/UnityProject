using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using HslCommunication.ModBus;
using NaughtyAttributes;
using RDTS.Utility;
using System;

namespace RDTS.Interface
{
    public class ModbusInterface : BaseInterface
    {
        [Header("Configure")]
        public string IP = "127.0.0.1";
        public int Port = 502;
        public byte Station = 0x01;
        [Tooltip("地址是否从0开始，还是从1开始")]
        public bool AddreStartWith0 = false;

        ///针对于机械臂数据的批量地读/写
        [Header("Robot")][InfoBox("地址偏移量一般为2")]///float类型
        public List<RobotArmJoints> robotArmJoints = new List<RobotArmJoints>();

        ///针对于六类数据的单个/批量地读/写
        [Header("Interactive data")]
        [InfoBox("Bool类型地址偏移量一般为1")]
        [InfoBox("Int类型地址偏移量一般为0")]
        [InfoBox("Float类型地址偏移量一般为2")]
        public List<InputBoolSignal> inputBoolSignals;
        public List<InputIntSignal> inputIntSignals;
        public List<InputFloatSignal> inputFloatSignals;
        public List<OutputBoolSignal> outputBoolSignals;
        public List<OutputIntSignal> outputIntSignals;
        public List<OutputFloatSignal> outputFloatSignals;

        private ModbusTcpNet modbusClient;


        [Button("Connect Modbus")]
        void ButtonConnect()
        {
            ModbusInit();
            ModbusConnect();
        }

        [Button("Disconnect Modbus")]
        void ButtonDisconnect()
        {
            MobudClose();
        }

        [Button("Check Connection")]
        void ButtonCheckConnection()
        {
            CheckConnection();
        }

        [Button("Write")]
        void ButtonWrite()
        {
            if (ConnectionStatus)
            {
                UpdateInputBool();
                UpdateInputInt();
                UpdateInputFloat();
            }
            else
                QM.Warn("Modbus not connect!");

        }

        [Button("Read")]
        void ButtonRead()
        {
            if (ConnectionStatus)
            {
                RobotJointsUpdate();
                UpdateOutputBool();
                UpdateOutputInt();
                UpdateOutputFloat();
            }
            else
                QM.Warn("Modbus not connect!");
            
        }

       

        // Start is called before the first frame update
        void Start()
        {
            if (Active == ActiveOnly.Always)
            {
                ModbusInit();//配置Modbus
                if(ModbusConnect())//连接Modbus服务端
                    OpenThread();//开启线程
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (Active == ActiveOnly.Always)
            {

            }
        }

        private void OnApplicationQuit()
        {
            if (Active == ActiveOnly.Always)
            {
                if (ConnectionStatus) MobudClose();
                if (runThread) CloseThread();
            }
           
        }



        #region Robot

        /// <summary>机械臂的关节数据更新</summary>
        protected void RobotJointsUpdate()
        {
            if (robotArmJoints.Count == 0)
                return;

            robotArmJoints.ForEach(joints => JointsUpdate(joints));//更新每个关节的position
        }
        
        /// <summary>更新一个机械臂中的关节位置</summary>
        protected void JointsUpdate(RobotArmJoints joints)
        {
            if (!joints.isExecute)
                return;

            if (joints.startAddre < 0)
                return;

            //从Modbus中读取地址
            string addr = (joints.startAddre + joints.AddreOffset).ToString();
            var readAxis = modbusClient.ReadFloat(addr, (ushort)(joints.length));//float占两位

            //将从Modbus中读取的数据赋值给Drive.CurrentPosition
            if (joints.lerpSpeed == 0)
                joints.lerpSpeed = 1f;
            for (int i = 0; i < joints.length; i++)
            {
                joints.Values[i] = readAxis.Content[i];
                ///插值—使关节运动更为顺畅但会造成一定延迟
                joints.Axis[i].CurrentPosition = Mathf.Lerp(joints.Axis[i].CurrentPosition, joints.Values[i], joints.lerpSpeed);
            }

        }

        #endregion


        #region 六类信号的更新

        protected void UpdateInputBool()
        {
            if (inputBoolSignals.Count == 0)
                return;

            inputBoolSignals.ForEach(block =>
            {
                if (!block.isExecute)//无需执行信号交互
                    return;

                try
                {
                    if (block.interactType == InteractType.Single)///单个处理
                    {

                        block.signals.ForEach(signal => {
                            string addr = (int.Parse(signal.Address) + block.AddreOffset).ToString();
                            modbusClient.Write(addr, signal.Value);
                        });///向PLC写入数据
                    }
                    else///批量处理
                    {
                        if (block.startAddre == null || block.startAddre == "")
                            return;

                        int addr1 = (int.Parse(block.startAddre) + block.AddreOffset);
                        if (addr1 < 0)
                            return;

                        string addr2 = addr1.ToString();

                        int length = block.signals.Count;//信号个数

                        bool[] write = new bool[length];
                        for (int i = 0; i < length; i++)
                            write[i] = block.signals[i].Value;

                        modbusClient.Write(addr2, write);//依次写入一个数组
                    }
                }
                catch
                {

                }    

            });

        }

        protected void UpdateInputInt()///注意：采用int16
        {
            if (inputIntSignals.Count == 0)
                return;

            inputIntSignals.ForEach(block =>
            {
                if (!block.isExecute)//无需执行信号交互
                    return;

                try
                {
                    if (block.interactType == InteractType.Single)
                    {
                        block.signals.ForEach(signal => {
                            string addr = (int.Parse(signal.Address) + block.AddreOffset).ToString();
                            modbusClient.Write(addr, signal.Value);
                        });///向PLC写入数据
                    }
                    else///批量处理
                    {
                        if (block.startAddre == null || block.startAddre == "")
                            return;

                        int addr1 = (int.Parse(block.startAddre) + block.AddreOffset);
                        if (addr1 < 0)
                            return;

                        string addr2 = addr1.ToString();

                        int length = block.signals.Count;//信号个数

                        int[] write = new int[length];
                        for (int i = 0; i < length; i++)
                            write[i] = block.signals[i].Value;

                        modbusClient.Write(addr2, write);//依次写入一个数组
                    }
                }
                catch
                {

                }

            });

        }

        protected void UpdateInputFloat()
        {
            if (inputFloatSignals.Count == 0)
                return;

            inputFloatSignals.ForEach(block =>
            {
                if (!block.isExecute)//无需执行信号交互
                    return;

                try
                {
                    if (block.interactType == InteractType.Single)
                    {
                        block.signals.ForEach(signal => {
                            string addr = (int.Parse(signal.Address) + block.AddreOffset).ToString();
                            modbusClient.Write(addr, signal.Value);
                        });///向PLC写入数据
                    }
                    else///批量处理
                    {
                        if (block.startAddre == null || block.startAddre == "")
                            return;

                        int addr1 = (int.Parse(block.startAddre) + block.AddreOffset);
                        if (addr1 < 0)
                            return;

                        string addr2 = addr1.ToString();

                        int length = block.signals.Count;//信号个数

                        float[] write = new float[length];
                        for (int i = 0; i < length; i++)
                            write[i] = block.signals[i].Value;

                        modbusClient.Write(addr2, write);//依次写入一个数组
                    }
                }
                catch
                {

                }

            });

        }

        protected void UpdateOutputBool()
        {
            if (outputBoolSignals.Count == 0)
                return;

            outputBoolSignals.ForEach(block =>
            {
                if (!block.isExecute)//无需执行信号交互
                    return;

                try
                {
                    if (block.interactType == InteractType.Single)///单个处理
                    {
                        block.signals.ForEach(signal => {
                            string addr = (int.Parse(signal.Address) + block.AddreOffset).ToString();
                            signal.Value = modbusClient.ReadBool(addr).Content;
                        });///从PLC读取数据
                    }
                    else///批量处理
                    {
                        if (block.startAddre == null || block.startAddre == "")
                            return;

                        int addr1 = (int.Parse(block.startAddre) + block.AddreOffset);
                        if (addr1 < 0)
                            return;

                        string addr2 = addr1.ToString();

                        int length = block.signals.Count;//信号个数

                        var read = modbusClient.ReadBool(addr2, (ushort)length);
                        if (read != null)
                        {
                            for (int i = 0; i < length; i++)
                                block.signals[i].Value = read.Content[i];
                        }

                    }
                }
                catch
                {

                }

                
            });
        }

        protected void UpdateOutputInt()
        {
            if (outputIntSignals.Count == 0)
                return;

            outputIntSignals.ForEach(block =>
            {
                if (!block.isExecute)//无需执行信号交互
                    return;

                try
                {
                    if (block.interactType == InteractType.Single)///单个处理
                    {
                        block.signals.ForEach(signal => {
                            string addr = (int.Parse(signal.Address) + block.AddreOffset).ToString();
                            signal.Value = modbusClient.ReadInt32(addr).Content;
                        });///从PLC读取数据
                    }
                    else///批量处理
                    {
                        if (block.startAddre == null || block.startAddre == "")
                            return;

                        int addr1 = (int.Parse(block.startAddre) + block.AddreOffset);
                        if (addr1 < 0)
                            return;

                        string addr2 = addr1.ToString();

                        int length = block.signals.Count;//信号个数

                        var read = modbusClient.ReadInt32(addr2, (ushort)length);
                        if (read != null)
                        {
                            for (int i = 0; i < length; i++)
                                block.signals[i].Value = read.Content[i];
                        }


                    }
                }
                catch
                {

                }

               
            });
        }

        protected void UpdateOutputFloat()
        {
            if (outputFloatSignals.Count == 0)
                return;

            outputFloatSignals.ForEach(block =>
            {
                if (!block.isExecute)//无需执行信号交互
                    return;

                try
                {
                    if (block.interactType == InteractType.Single)///单个处理
                    {
                        block.signals.ForEach(signal => {
                            string addr = (int.Parse(signal.Address) + block.AddreOffset).ToString();
                            signal.Value = modbusClient.ReadFloat(addr).Content;
                        });///从PLC读取数据
                    }
                    else///批量处理
                    {
                        if (block.startAddre == null || block.startAddre == "")
                            return;

                        int addr1 = (int.Parse(block.startAddre) + block.AddreOffset);
                        if (addr1 < 0)
                            return;

                        string addr2 = addr1.ToString();

                        int length = block.signals.Count;//信号个数

                        var read = modbusClient.ReadFloat(addr2, (ushort)length);
                        if (read != null)
                        {
                            for (int i = 0; i < length; i++)
                                block.signals[i].Value = read.Content[i];
                        }

                    }
                }
                catch
                {

                }

            });
        }

        #endregion


        #region Modbus的连接/断开

        void ModbusInit()
        {
            modbusClient = new ModbusTcpNet(IP, Port, Station);
            modbusClient.AddressStartWithZero = AddreStartWith0;

        }

        bool ModbusConnect()
        {
            try
            {
                var connection = modbusClient.ConnectServer();
                if (connection.IsSuccess)
                {
                    ConnectionStatus = true;
                    QM.Log("Connect Succ!");
                    return true;
                }
                else
                {
                    ConnectionStatus = false;
                    QM.Log("Connect Fail!");
                    return false;
                }
            }
            catch(Exception e)
            {
                QM.Error("Connect Fail！" + e.ToString());
            }

            return false;


        }

        void MobudClose()
        {
            modbusClient.ConnectClose();
            ConnectionStatus = false;
            QM.Log("Connect Close!");
        }

        bool CheckConnection()
        {

            if (ConnectionStatus == true)
            {
                QM.Log("Connect Succ!");
                return true;
            }
            else
            {
                QM.Log("Connect Fail!");
                return false;
            }

        }

        #endregion


        #region 线程

        Thread ModbusThread;//线程
        bool runThread = false;


        /// <summary>
        /// 开启线程
        /// </summary>
        void OpenThread()
        {
            //线程创建写法1
            ModbusThread = new Thread(() => ModbusThreadUpdate());//Lambda表达式

            //线程创建写法2
            //ThreadStart threadDelegate = new ThreadStart(PLCSiganlThreadUpdate);
            //Thread SignalThread = new Thread(threadDelegate);

            runThread = true;
            ModbusThread?.Start();
            QM.Log("开启线程！");
        }

        /// <summary>
        /// 关闭线程
        /// </summary>
        void CloseThread()
        {
            runThread = false;
            ModbusThread?.Abort("Abort Thread Nomally!");
            QM.Log("中止线程！");
        }


        /// <summary>
        /// 在线程中更新信号
        /// </summary>
        protected void ModbusThreadUpdate()
        {
            try
            {
                do
                {
                    lock (robotArmJoints)
                    {
                        RobotJointsUpdate();
                    }

                    lock (inputBoolSignals)
                    {
                        UpdateInputBool();
                    }
                    lock (inputIntSignals)
                    {
                        UpdateInputInt();
                    }
                    lock (inputFloatSignals)
                    {
                        UpdateInputFloat();
                    }

                    lock (outputBoolSignals)
                    {
                        UpdateOutputBool();
                    }
                    lock (outputIntSignals)
                    {
                        UpdateOutputInt();
                    }
                    lock (outputFloatSignals)
                    {
                        UpdateOutputFloat();
                    }

                } while (runThread);
            }
            catch (ThreadAbortException abortException)
            {
                QM.Warn($"Exception message: {abortException.Message}");
            }



        }


        #endregion







    }

   
}
