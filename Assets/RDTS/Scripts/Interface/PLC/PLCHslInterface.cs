using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System.Threading;
using RDTS.Utility;
using HslCommunication;
using HslCommunication.Profinet.Siemens;

namespace RDTS.Interface
{
    public class PLCHslInterface : BaseInterface
    {
        [Header("Configure")]
        public string IP = "192.168.0.1";//"169.168.0.1";
        public PLCType PLCType;

        [Header("Interactive signals")][InfoBox("PLC�����ַƫ��")]
        public List<InputBoolSignal> inputBoolSignals;
        public List<InputIntSignal> inputIntSignals;
        public List<InputFloatSignal> inputFloatSignals;
        public List<OutputBoolSignal> outputBoolSignals;
        public List<OutputIntSignal> outputIntSignals;
        public List<OutputFloatSignal> outputFloatSignals;

        [Button("Connect PLC")]
        void ButtonConnect()
        {
            PLCInit();
            ConnectPLC(plc);
        }

        [Button("Disconnect PLC")]
        void ButtonDisconnect()
        {
            DisconnectPLC(plc);
        }

        SiemensS7Net plc;
        SiemensPLCS CPUType;
        OperateResult connect;

        // Start is called before the first frame update
        void Start()
        {
            if(Active == ActiveOnly.Always)
            {
                PLCInit();
                if(ConnectPLC(plc))///����plc
                    OpenThread();//�����̣߳��������ݶ�д
            }
           
        }

        // Update is called once per frame
        void Update()
        {
            if(Active == ActiveOnly.Always)
            {

            }

        }

        void OnApplicationQuit()//�˳�ʱ�Ͽ���plc������
        {
            if(Active == ActiveOnly.Always)
            {
                if(ConnectionStatus) DisconnectPLC(plc);
                if(runThread) CloseThread();
            }
            

        }


        #region �����źŵĸ���

        protected void UpdateInputBool()
        {
            if (inputBoolSignals.Count == 0)
                return;

            inputBoolSignals.ForEach(block =>
            {
                if (!block.isExecute)//����ִ���źŽ���
                    return;

                if(block.interactType == InteractType.Single)///��������
                {
                    block.signals.ForEach(signal => plc.Write(signal.Address, signal.Value));///��PLCд������
                }
                else///��������
                {
                    if (block.startAddre == null || block.startAddre == "")
                        return;

                    try
                    {
                        int length = block.signals.Count;//�źŸ���

                        bool[] write = new bool[length];
                        for (int i = 0; i < length; i++)
                            write[i] = block.signals[i].Value;

                        plc.Write(block.startAddre, write);//����д��һ������
                    }
                    catch
                    {

                    }

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

                if (block.interactType == InteractType.Single)
                {
                    block.signals.ForEach(signal => plc.Write(signal.Address, signal.Value));///��PLCд������
                }
                else///��������
                {
                    if (block.startAddre == null || block.startAddre == "")
                        return;

                    try
                    {
                        int length = block.signals.Count;//�źŸ���

                        Int16[] write = new Int16[length];
                        for (int i = 0; i < length; i++)
                            write[i] = (short)block.signals[i].Value;

                        plc.Write(block.startAddre, write);//����д��һ������
                    }
                    catch
                    {

                    }  

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

                if (block.interactType == InteractType.Single)
                {
                    block.signals.ForEach(signal => plc.Write(signal.Address, signal.Value));///��PLCд������
                }
                else///��������
                {
                    if (block.startAddre == null || block.startAddre == "")
                        return;

                    try
                    {
                        int length = block.signals.Count;//�źŸ���

                        float[] write = new float[length];
                        for (int i = 0; i < length; i++)
                            write[i] = block.signals[i].Value;

                        plc.Write(block.startAddre, write);//����д��һ������
                    }
                    catch
                    {

                    }

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

                if (block.interactType == InteractType.Single)///��������
                {
                    block.signals.ForEach(signal => signal.Value = plc.ReadBool(signal.Address).Content);///��PLC��ȡ����
                }
                else///��������
                {
                    if (block.startAddre == null || block.startAddre == "")
                        return;

                    try
                    {
                        int length = block.signals.Count;//�źŸ���
                        var read = plc.ReadBool(block.startAddre, (ushort)length);
                        if (read != null)
                        {
                            for (int i = 0; i < length; i++)
                                block.signals[i].Value = read.Content[i];
                        }
                    }
                    catch
                    {
                        
                    }
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

                if (block.interactType == InteractType.Single)///��������
                {
                    block.signals.ForEach(signal => signal.Value = plc.ReadInt16(signal.Address).Content);///��PLC��ȡ����
                }
                else///��������
                {
                    if (block.startAddre == null || block.startAddre == "")
                        return;

                    try
                    {
                        int length = block.signals.Count;//�źŸ���
                        var read = plc.ReadInt16(block.startAddre, (ushort)length);
                        if (read != null)
                        {
                            for (int i = 0; i < length; i++)
                                block.signals[i].Value = read.Content[i];
                        }
                    }
                    catch
                    {

                    }
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

                if (block.interactType == InteractType.Single)///��������
                {
                    block.signals.ForEach(signal => signal.Value = plc.ReadFloat(signal.Address).Content);///��PLC��ȡ����
                }
                else///��������
                {
                    if (block.startAddre == null || block.startAddre == "")
                        return;

                    try
                    {
                        int length = block.signals.Count;//�źŸ���
                        var read = plc.ReadFloat(block.startAddre, (ushort)length);
                        if (read != null)
                        {
                            for (int i = 0; i < length; i++)
                                block.signals[i].Value = read.Content[i];
                        }
                    }
                    catch
                    {
                       
                    }      

                }
            });
        }

        #endregion


        ///ʵ��������ָ��PLC��ip��ַ�Ͷ˿ں�
        void PLCInit()
        {
            CPUType = (PLCType == PLCType.CPU1200) ? SiemensPLCS.S1200 : SiemensPLCS.S1500;
            plc = new SiemensS7Net(CPUType, IP) {ConnectTimeOut = 5000 };
        }



        /// <summary>����siemens plc</summary>
        bool ConnectPLC(SiemensS7Net plc)
        {
            try
            {
                connect = plc.ConnectServer();
                if (connect.IsSuccess)
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
            catch (Exception e)
            {
                QM.Error("Connect Fail��" + e.ToString());
            }

            return false;

        }

        ///�Ͽ�����
        void DisconnectPLC(SiemensS7Net plc)
        {
            plc.ConnectClose();
            ConnectionStatus = false;
            QM.Log("Connect Close!");
        }

        #region �߳�

        Thread PLCHslThread;//�߳�
        bool runThread = false;

        /// <summary>�����߳�</summary>
        void OpenThread()
        {
            //�̴߳���д��1
            PLCHslThread = new Thread(() => PLCHslThreadUpdate());//Lambda���ʽ

            //�̴߳���д��2
            //ThreadStart threadDelegate = new ThreadStart(PLCSiganlThreadUpdate);
            //Thread SignalThread = new Thread(threadDelegate);

            runThread = true;
            PLCHslThread?.Start();
            QM.Log("�����̣߳�");
        }

        /// <summary>�ر��߳�</summary>
        void CloseThread()
        {
            runThread = false;
            PLCHslThread?.Abort("Abort Thread Nomally!");
            QM.Log("��ֹ�̣߳�");
        }


        /// <summary>���߳��и����ź�</summary>
        protected void PLCHslThreadUpdate()
        {
            try
            {
                do
                {
                    lock (inputBoolSignals)
                    {
                        UpdateInputBool();
                    }
                    lock(inputIntSignals)
                    {
                        UpdateInputInt();
                    }
                    lock(inputFloatSignals)
                    {
                        UpdateInputFloat();
                    }

                    lock(outputBoolSignals)
                    {
                        UpdateOutputBool();
                    }
                    lock(outputIntSignals)
                    {
                        UpdateOutputInt();
                    }
                    lock(outputFloatSignals)
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
