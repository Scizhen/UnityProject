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
    //! 值状态
    public struct SettingsValue
    {
        public bool Active;//活跃
        public bool Override;//覆盖
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
    //! 保存值连接信息的类 - 值连接的行为和值连接的属性
    public class Connection
    {
        public GameObject Behavior;//关联的对象
        public string ConnectionName;//关联（Value类型）字段的名称
    }

    [System.Serializable]
    public class ValueEvent : UnityEngine.Events.UnityEvent<Value> { }

    //! The base class for all Values
    public class Value : RDTSBehavior
    {
        public string Comment;//注解
        public string Address;//地址
        public string OriginDataType;//（原始）数据类型
        public SettingsValue Settings;//值的状态设置（Active活跃、Override覆盖）
        protected string Visutext;
        public ValueEvent EventValueChanged;//值变化时触发的事件


        [HideInInspector] public List<Connection> ConnectionInfo = new List<Connection>();//数据连接信息（Inspector面板中ValueConnectionInfo折叠菜单的信息）


        protected new bool hidename()
        {
            return false;
        }

        //!  Virtual for getting the text for the Hierarchy View 获取值对象的值（string类型）
        public virtual string GetVisuText()
        {
            return "not implemented";//子类才有具体值
        }

        //! Virtual for getting information if the value is an Input  判断是否是输入类型，input为true，output为false（判断input、output两类的输入输出方向）
        public virtual bool IsInput()
        {
            return false;
        }

        //! 获取“值对象”的输入输出方向：0为默认，1为输入Input，2为输出Output，3为中间变量Middle （判断input、output、middle三类的输入输出方向）
        public virtual int GetValueDirection()
        {
            return 0;
        }

        //! Virtual for setting the value 设置此值对象（Value子类脚本）的值
        public virtual void SetValue(string value)
        {
        }


        //! Virtual for toogle in hierarhy view  在Hierarchy面板中通过鼠标点击切换值对象的值（只有bool类型才有，可切换true/false）
        public virtual void OnToggleHierarchy()
        {
        }

        //! Virtual for setting the Status to connected  设置值对象的连接状态（true/false）
        public virtual void SetStatusConnected(bool status)
        {
        }

        //! Sets the value of the Value  
        public virtual void SetValue(object value)
        {
        }

        //! Unforces the value
        public void Unforce()//强制解除
        {
            Settings.Override = false;
            EventValueChanged.Invoke(this);
            ValueChangedEvent(this);
        }

        //! Gets the value of the Value  获取值对象的值
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
        /// 删除值对象得所有连接信息
        /// </summary>
        public void DeleteValueConnectionInfos()
        {
            ConnectionInfo.Clear();
        }


        /// <summary>
        /// 添加“值连接到对象”的信息
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
        ///判断Value的连接信息是否为空
        /// </summary>
        /// <returns></returns>
        public bool IsConnectedToBehavior()
        {
            if (ConnectionInfo.Count > 0)
                return true;
            else
                return false;
        }

        //! Returns an InterfaceValue Object based on the Value Component 根据信号组件返回一个 InterfaceValue 对象
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