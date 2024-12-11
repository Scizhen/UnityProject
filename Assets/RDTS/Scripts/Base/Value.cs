//*************************************************************************
//Thanks for the code reference game4automation provides.                 *
//                                                                        *
//*************************************************************************
using System;
using UnityEngine;
using System.Collections.Generic;
using NaughtyAttributes;

namespace RDTS
{
    [Serializable]
    //! Struct for Settings of Values
    //! ֵ״̬
    public struct SettingsValue
    {
        public bool Active;//��Ծ
        public bool Override;//����
    }

    [Serializable]
    //! Struct for current status of a bool value
    public struct StatusBool
    {
        public bool Connected;
        public bool ValueOverride;
        public bool Value;
        [HideInInspector] public bool OldValue;
    }

    [Serializable]
    //! Struct for current status of a float value
    public struct StatusFloat
    {
        public bool Connected;
        public float ValueOverride;
        public float Value;
        [HideInInspector] public float OldValue;
    }


    [Serializable]
    //! Struct for current status of a omt value
    public struct StatusInt
    {
        public bool Connected;
        public int ValueOverride;
        public int Value;
        [HideInInspector] public int OldValue;
    }

    [Serializable]
    //! Class for saving connection information for value - Behavior where value is connected tp and property  where value is connected to
    //! ����ֵ������Ϣ���� - ֵ���ӵ���Ϊ��ֵ���ӵ�����
    public class Connection
    {
        public GameObject Behavior;//�����Ķ���
        public string ConnectionName;//������Value���ͣ��ֶε�����
    }

    [System.Serializable]
    public class ValueEvent : UnityEngine.Events.UnityEvent<Value> { }

    //! The base class for all Values
    public class Value : RDTSBehavior
    {
        public string Comment;//ע��
        public string Address;//��ַ
        public string OriginDataType;//��ԭʼ����������
        public SettingsValue Settings;//ֵ��״̬���ã�Active��Ծ��Override���ǣ�
        protected string Visutext;
        public ValueEvent EventValueChanged;//ֵ�仯ʱ�������¼�


        [HideInInspector] public List<Connection> ConnectionInfo = new List<Connection>();//����������Ϣ��Inspector�����ValueConnectionInfo�۵��˵�����Ϣ��


        protected new bool hidename()
        {
            return false;
        }

        //!  Virtual for getting the text for the Hierarchy View ��ȡֵ�����ֵ��string���ͣ�
        public virtual string GetVisuText()
        {
            return "not implemented";//������о���ֵ
        }

        //! Virtual for getting information if the value is an Input  �ж��Ƿ����������ͣ�inputΪtrue��outputΪfalse���ж�input��output����������������
        public virtual bool IsInput()
        {
            return false;
        }

        //! ��ȡ��ֵ���󡱵������������0ΪĬ�ϣ�1Ϊ����Input��2Ϊ���Output��3Ϊ�м����Middle ���ж�input��output��middle����������������
        public virtual int GetValueDirection()
        {
            return 0;
        }

        //! Virtual for setting the value ���ô�ֵ����Value����ű�����ֵ
        public virtual void SetValue(string value)
        {
        }


        //! Virtual for toogle in hierarhy view  ��Hierarchy�����ͨ��������л�ֵ�����ֵ��ֻ��bool���Ͳ��У����л�true/false��
        public virtual void OnToggleHierarchy()
        {
        }

        //! Virtual for setting the Status to connected  ����ֵ���������״̬��true/false��
        public virtual void SetStatusConnected(bool status)
        {
        }

        //! Sets the value of the Value  
        public virtual void SetValue(object value)
        {
        }

        //! Unforces the value
        public void Unforce()//ǿ�ƽ��
        {
            Settings.Override = false;
            EventValueChanged.Invoke(this);
            ValueChangedEvent(this);
        }

        //! Gets the value of the Value  ��ȡֵ�����ֵ
        public virtual object GetValue()
        {
            return null;
        }

        //! Virtual for getting the connected Status
        public virtual bool GetStatusConnected()
        {
            return false;
        }

        /// <summary>
        /// ɾ��ֵ���������������Ϣ
        /// </summary>
        public void DeleteValueConnectionInfos()
        {
            ConnectionInfo.Clear();
        }


        /// <summary>
        /// ��ӡ�ֵ���ӵ����󡱵���Ϣ
        /// </summary>
        /// <param name="behavior"></param>
        /// <param name="connectionname"></param>
        public void AddValueConnectionInfo(GameObject behavior, string connectionname)
        {
            var element = new Connection();
            element.Behavior = behavior;
            element.ConnectionName = connectionname;
            ConnectionInfo.Add(element);
            if (IsInput())
            {
                if (ConnectionInfo.Count > 1)
                {
                    //   Error("Input Vaule Obj is connected to more than one behavior model, this is not allowed", this);
                }
            }
        }


        //! Returns true if InterfaceValue is connected to any Behavior Script
        /// <summary>
        ///�ж�Value��������Ϣ�Ƿ�Ϊ��
        /// </summary>
        /// <returns></returns>
        public bool IsConnectedToBehavior()
        {
            if (ConnectionInfo.Count > 0)
                return true;
            else
                return false;
        }

        //! Returns an InterfaceValue Object based on the Value Component �����ź��������һ�� InterfaceValue ����
        public InterfaceValue GetInterfaceValue()
        {
            var newvalue = new InterfaceValue();
            newvalue.OriginDataType = OriginDataType;
            newvalue.Name = name;
            newvalue.SymbolName = Name;
            newvalue.Value = this;
            var type = this.GetType().ToString();
           // Debug.Log("type:" + type);
            switch (type)
            {
                case "RDTS.ValueInputBool":
                    newvalue.Type = InterfaceValue.TYPE.BOOL;
                    newvalue.Direction = InterfaceValue.DIRECTION.INPUT;
                    break;
                case "RDTS.ValueOutputBool":
                    newvalue.Type = InterfaceValue.TYPE.BOOL;
                    newvalue.Direction = InterfaceValue.DIRECTION.OUTPUT;
                    break;
                case "RDTS.ValueMiddleBool":
                    newvalue.Type = InterfaceValue.TYPE.BOOL;
                    newvalue.Direction = InterfaceValue.DIRECTION.INPUTOUTPUT;
                    break;
                case "RDTS.ValueInputFloat":
                    newvalue.Type = InterfaceValue.TYPE.REAL;
                    newvalue.Direction = InterfaceValue.DIRECTION.INPUT;
                    break;
                case "RDTS.ValueOutputFloat":
                    newvalue.Type = InterfaceValue.TYPE.REAL;
                    newvalue.Direction = InterfaceValue.DIRECTION.OUTPUT;
                    break;
                case "RDTS.ValueMiddleFloat":
                    newvalue.Type = InterfaceValue.TYPE.REAL;
                    newvalue.Direction = InterfaceValue.DIRECTION.INPUTOUTPUT;
                    break;
                case "RDTS.ValueInputInt":
                    newvalue.Type = InterfaceValue.TYPE.INT;
                    newvalue.Direction = InterfaceValue.DIRECTION.INPUT;
                    break;
                case "RDTS.ValueOutputInt":
                    newvalue.Type = InterfaceValue.TYPE.INT;
                    newvalue.Direction = InterfaceValue.DIRECTION.OUTPUT;
                    break;
                case "RDTS.ValueMiddleInt":
                    newvalue.Type = InterfaceValue.TYPE.INT;
                    newvalue.Direction = InterfaceValue.DIRECTION.INPUTOUTPUT;
                    break;
                case "RDTS.PLCInputTransform":
                    newvalue.Type = InterfaceValue.TYPE.TRANSFORM;
                    newvalue.Direction = InterfaceValue.DIRECTION.INPUT;
                    break;
                case "RDTS.PLCOutputTransform":
                    newvalue.Type = InterfaceValue.TYPE.TRANSFORM;
                    newvalue.Direction = InterfaceValue.DIRECTION.OUTPUT;
                    break;
            }

            return newvalue;
        }

        //! Is called when value is changed in inspector
        private void OnValidate()
        {
            ValueChangedEvent(this);
        }

        private void Start()
        {
            ValueChangedEvent(this);
            if (EventValueChanged != null)
            {
                if (EventValueChanged.GetPersistentEventCount() == 0)
                {
                    enabled = false;
                }
                else
                {
                    enabled = true;
                }
            }
            else
            {
                enabled = false;
            }
        }

        public delegate void OnValueChangedDelegate(Value obj);

        public event OnValueChangedDelegate ValueChanged;

        protected void ValueChangedEvent(Value value)
        {
            if (ValueChanged != null)
                ValueChanged(value);
        }
    }
}