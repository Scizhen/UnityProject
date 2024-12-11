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
        [Tooltip("��ַ�Ƿ��0��ʼ�����Ǵ�1��ʼ")]
        public bool AddreStartWith0 = false;

        ///����ڻ�е�����ݵ������ض�/д
        [Header("Robot")][InfoBox("��ַƫ����һ��Ϊ2")]///float����
        public List<RobotArmJoints> robotArmJoints = new List<RobotArmJoints>();

        ///������������ݵĵ���/�����ض�/д
        [Header("Interactive data")]
        [InfoBox("Bool���͵�ַƫ����һ��Ϊ1")]
        [InfoBox("Int���͵�ַƫ����һ��Ϊ0")]
        [InfoBox("Float���͵�ַƫ����һ��Ϊ2")]
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
                ModbusInit();//����Modbus
                if(ModbusConnect())//����Modbus�����
                    OpenThread();//�����߳�
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

        /// <summary>��е�۵Ĺؽ����ݸ���</summary>
        protected void RobotJointsUpdate()
        {
            if (robotArmJoints.Count == 0)
                return;

            robotArmJoints.ForEach(joints => JointsUpdate(joints));//����ÿ���ؽڵ�position
        }
        
        /// <summary>����һ����е���еĹؽ�λ��</summary>
        protected void JointsUpdate(RobotArmJoints joints)
        {
            if (!joints.isExecute)
                return;

            if (joints.startAddre < 0)
                return;

            //��Modbus�ж�ȡ��ַ
            string addr = (joints.startAddre + joints.AddreOffset).ToString();
            var readAxis = modbusClient.ReadFloat(addr, (ushort)(joints.length));//floatռ��λ

            //����Modbus�ж�ȡ�����ݸ�ֵ��Drive.CurrentPosition
            if (joints.lerpSpeed == 0)
                joints.lerpSpeed = 1f;
            for (int i = 0; i < joints.length; i++)
            {
                joints.Values[i] = readAxis.Content[i];
                ///��ֵ��ʹ�ؽ��˶���Ϊ˳���������һ���ӳ�
                joints.Axis[i].CurrentPosition = Mathf.Lerp(joints.Axis[i].CurrentPosition, joints.Values[i], joints.lerpSpeed);
            }

        }

        #endregion


        #region �����źŵĸ���

        protected void UpdateInputBool()
        {
            if (inputBoolSignals.Count == 0)
                return;

            inputBoolSignals.ForEach(block =>
            {
                if (!block.isExecute)//����ִ���źŽ���
                    return;

                try
                {
                    if (block.interactType == InteractType.Single)///��������
                    {

                        block.signals.ForEach(signal => {
                            string addr = (int.Parse(signal.Address) + block.AddreOffset).ToString();
                            modbusClient.Write(addr, signal.Value);
                        });///��PLCд������
                    }
                    else///��������
                    {
                        if (block.startAddre == null || block.startAddre == "")
                            return;

                        int addr1 = (int.Parse(block.startAddre) + block.AddreOffset);
                        if (addr1 < 0)
                            return;

                        string addr2 = addr1.ToString();

                        int length = block.signals.Count;//�źŸ���

                        bool[] write = new bool[length];
                        for (int i = 0; i < length; i++)
                            write[i] = block.signals[i].Value;

                        modbusClient.Write(addr2, write);//����д��һ������
                    }
                }
                catch
                {

                }    

            });

        }

        protected void UpdateInputInt()///ע�⣺����int16
        {
            if (inputIntSignals.Count == 0)
                return;

            inputIntSignals.ForEach(block =>
            {
                if (!block.isExecute)//����ִ���źŽ���
                    return;

                try
                {
                    if (block.interactType == InteractType.Single)
                    {
                        block.signals.ForEach(signal => {
                            string addr = (int.Parse(signal.Address) + block.AddreOffset).ToString();
                            modbusClient.Write(addr, signal.Value);
                        });///��PLCд������
                    }
                    else///��������
                    {
                        if (block.startAddre == null || block.startAddre == "")
                            return;

                        int addr1 = (int.Parse(block.startAddre) + block.AddreOffset);
                        if (addr1 < 0)
                            return;

                        string addr2 = addr1.ToString();

                        int length = block.signals.Count;//�źŸ���

                        int[] write = new int[length];
                        for (int i = 0; i < length; i++)
                            write[i] = block.signals[i].Value;

                        modbusClient.Write(addr2, write);//����д��һ������
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
                if (!block.isExecute)//����ִ���źŽ���
                    return;

                try
                {
                    if (block.interactType == InteractType.Single)
                    {
                        block.signals.ForEach(signal => {
                            string addr = (int.Parse(signal.Address) + block.AddreOffset).ToString();
                            modbusClient.Write(addr, signal.Value);
                        });///��PLCд������
                    }
                    else///��������
                    {
                        if (block.startAddre == null || block.startAddre == "")
                            return;

                        int addr1 = (int.Parse(block.startAddre) + block.AddreOffset);
                        if (addr1 < 0)
                            return;

                        string addr2 = addr1.ToString();

                        int length = block.signals.Count;//�źŸ���

                        float[] write = new float[length];
                        for (int i = 0; i < length; i++)
                            write[i] = block.signals[i].Value;

                        modbusClient.Write(addr2, write);//����д��һ������
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
                if (!block.isExecute)//����ִ���źŽ���
                    return;

                try
                {
                    if (block.interactType == InteractType.Single)///��������
                    {
                        block.signals.ForEach(signal => {
                            string addr = (int.Parse(signal.Address) + block.AddreOffset).ToString();
                            signal.Value = modbusClient.ReadBool(addr).Content;
                        });///��PLC��ȡ����
                    }
                    else///��������
                    {
                        if (block.startAddre == null || block.startAddre == "")
                            return;

                        int addr1 = (int.Parse(block.startAddre) + block.AddreOffset);
                        if (addr1 < 0)
                            return;

                        string addr2 = addr1.ToString();

                        int length = block.signals.Count;//�źŸ���

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
                if (!block.isExecute)//����ִ���źŽ���
                    return;

                try
                {
                    if (block.interactType == InteractType.Single)///��������
                    {
                        block.signals.ForEach(signal => {
                            string addr = (int.Parse(signal.Address) + block.AddreOffset).ToString();
                            signal.Value = modbusClient.ReadInt32(addr).Content;
                        });///��PLC��ȡ����
                    }
                    else///��������
                    {
                        if (block.startAddre == null || block.startAddre == "")
                            return;

                        int addr1 = (int.Parse(block.startAddre) + block.AddreOffset);
                        if (addr1 < 0)
                            return;

                        string addr2 = addr1.ToString();

                        int length = block.signals.Count;//�źŸ���

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
                if (!block.isExecute)//����ִ���źŽ���
                    return;

                try
                {
                    if (block.interactType == InteractType.Single)///��������
                    {
                        block.signals.ForEach(signal => {
                            string addr = (int.Parse(signal.Address) + block.AddreOffset).ToString();
                            signal.Value = modbusClient.ReadFloat(addr).Content;
                        });///��PLC��ȡ����
                    }
                    else///��������
                    {
                        if (block.startAddre == null || block.startAddre == "")
                            return;

                        int addr1 = (int.Parse(block.startAddre) + block.AddreOffset);
                        if (addr1 < 0)
                            return;

                        string addr2 = addr1.ToString();

                        int length = block.signals.Count;//�źŸ���

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


        #region Modbus������/�Ͽ�

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
                QM.Error("Connect Fail��" + e.ToString());
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


        #region �߳�

        Thread ModbusThread;//�߳�
        bool runThread = false;


        /// <summary>
        /// �����߳�
        /// </summary>
        void OpenThread()
        {
            //�̴߳���д��1
            ModbusThread = new Thread(() => ModbusThreadUpdate());//Lambda���ʽ

            //�̴߳���д��2
            //ThreadStart threadDelegate = new ThreadStart(PLCSiganlThreadUpdate);
            //Thread SignalThread = new Thread(threadDelegate);

            runThread = true;
            ModbusThread?.Start();
            QM.Log("�����̣߳�");
        }

        /// <summary>
        /// �ر��߳�
        /// </summary>
        void CloseThread()
        {
            runThread = false;
            ModbusThread?.Abort("Abort Thread Nomally!");
            QM.Log("��ֹ�̣߳�");
        }


        /// <summary>
        /// ���߳��и����ź�
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
