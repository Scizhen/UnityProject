//*************************************************************************
//Thanks for the code reference game4automation provides.                 *
//                                                                        *
//*************************************************************************
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using RDTS.Utility;
using RDTS.Interface;

namespace RDTS
{
    /// <summary>
    /// �й�Value�Ļ��ࡣ��Ҫ�ṩ��һϵ��ֵ������صķ���
    /// </summary>
    public class InterfaceBaseClass : BaseInterface
    {
        [ReadOnly] public bool IsConnected = false;

        public List<InterfaceValue> InterfaceValues = new List<InterfaceValue>();


        //! Creates a new List of InterfaceValues based on the Components under this Interface GameObject
        /// <summary>
        /// ���ڴ˽ӿڶ����µ��������һ���µĽӿ�ֵ�б�
        /// </summary>
        /// <param name="inputs">input���͵�����</param>
        /// <param name="outputs">output���͵�����</param>
        public void UpdateInterfaceValues(ref int inputs, ref int outputs)
        {
            InterfaceValues.Clear();
            inputs = 0;
            outputs = 0;
            var values = gameObject.GetComponentsInChildren(typeof(Value), true);//��ȡgameobject��Value���͵Ľű�
            foreach (Value value in values)
            {
                var newvalue = value.GetInterfaceValue();//�����ź��������һ�� InterfaceValue ����
                InterfaceValues.Add(newvalue);//����InterfaceValues�б���
                if (newvalue.Direction == InterfaceValue.DIRECTION.INPUT)
                {
                    inputs++;
                }
                if (newvalue.Direction == InterfaceValue.DIRECTION.OUTPUT)
                {
                    outputs++;
                }
            }
        }
        //! Create a value object as sub gameobject
        /// <summary>
        /// ����name��type��direction������һ���µ�ֵ�������Ѵ���ͬ����ֵ������޸���Value�ű�
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public Value CreateValueObject(string name, VALUETYPE type, VALUEDIRECTION direction)
        {
            GameObject valueObj;
            Value newvalue = null;
            Value valuescript = null;

            valueObj = GetChildByName(name);//��ȡ�˶���������Ϊname���Ӷ���
            if (valueObj == null)//Ϊ���򴴽��µĶ�����Ϊ�Ӷ���
            {
                valueObj = new GameObject("name");
                valueObj.transform.parent = this.transform;
                valueObj.name = name;
            }

            //����type��direction��ȡ��Ӧ��Value�ű�
            if (direction == VALUEDIRECTION.INPUT)
            {
                // Byte and  Input
                switch (type)
                {
                    case VALUETYPE.BOOL:
                        if (valueObj.GetComponent<ValueInputBool>() == null)
                        {
                            newvalue = valueObj.AddComponent<ValueInputBool>();
                        }

                        break;
                    case VALUETYPE.INT:
                        if (valueObj.GetComponent<ValueInputInt>() == null)
                        {
                            newvalue = valueObj.AddComponent<ValueInputInt>();
                        }

                        break;
                    case VALUETYPE.DINT:
                        if (valueObj.GetComponent<ValueInputInt>() == null)
                        {
                            newvalue = valueObj.AddComponent<ValueInputInt>();
                        }

                        break;
                    case VALUETYPE.BYTE:
                        if (valueObj.GetComponent<ValueInputInt>() == null)
                        {
                            newvalue = valueObj.AddComponent<ValueInputInt>();
                        }

                        break;
                    case VALUETYPE.WORD:
                        if (valueObj.GetComponent<ValueInputInt>() == null)
                        {
                            newvalue = valueObj.AddComponent<ValueInputInt>();
                        }

                        break;
                    case VALUETYPE.DWORD:
                        if (valueObj.GetComponent<ValueInputInt>() == null)
                        {
                            newvalue = valueObj.AddComponent<ValueInputInt>();
                        }

                        break;
                    case VALUETYPE.REAL:
                        if (valueObj.GetComponent<ValueInputFloat>() == null)
                        {
                            newvalue = valueObj.AddComponent<ValueInputFloat>();
                        }
                        break;
                }
            }

            if (direction == VALUEDIRECTION.OUTPUT)
            {
                switch (type)
                {
                    case VALUETYPE.BOOL:
                        if (valueObj.GetComponent<ValueOutputBool>() == null)
                        {
                            newvalue = valueObj.AddComponent<ValueOutputBool>();
                        }

                        break;
                    case VALUETYPE.INT:
                        if (valueObj.GetComponent<ValueOutputInt>() == null)
                        {
                            newvalue = valueObj.AddComponent<ValueOutputInt>();
                        }

                        break;
                    case VALUETYPE.DINT:
                        if (valueObj.GetComponent<ValueOutputInt>() == null)
                        {
                            newvalue = valueObj.AddComponent<ValueOutputInt>();
                        }

                        break;
                    case VALUETYPE.BYTE:
                        if (valueObj.GetComponent<ValueOutputInt>() == null)
                        {
                            newvalue = valueObj.AddComponent<ValueOutputInt>();
                        }

                        break;
                    case VALUETYPE.WORD:
                        if (valueObj.GetComponent<ValueOutputInt>() == null)
                        {
                            newvalue = valueObj.AddComponent<ValueOutputInt>();
                        }

                        break;
                    case VALUETYPE.DWORD:
                        if (valueObj.GetComponent<ValueOutputInt>() == null)
                        {
                            newvalue = valueObj.AddComponent<ValueOutputInt>();
                        }

                        break;
                    case VALUETYPE.REAL:
                        if (valueObj.GetComponent<ValueOutputFloat>() == null)
                        {
                            newvalue = valueObj.AddComponent<ValueOutputFloat>();
                        }

                        break;
                }
            }

            if (direction == VALUEDIRECTION.INPUTOUTPUT)
            {
                switch (type)
                {
                    case VALUETYPE.BOOL:
                        if (valueObj.GetComponent<ValueMiddleBool>() == null)
                        {
                            newvalue = valueObj.AddComponent<ValueMiddleBool>();
                        }

                        break;
                    case VALUETYPE.INT:
                        if (valueObj.GetComponent<ValueMiddleInt>() == null)
                        {
                            newvalue = valueObj.AddComponent<ValueMiddleInt>();
                        }

                        break;
                    case VALUETYPE.DINT:
                        if (valueObj.GetComponent<ValueMiddleInt>() == null)
                        {
                            newvalue = valueObj.AddComponent<ValueMiddleInt>();
                        }

                        break;
                    case VALUETYPE.BYTE:
                        if (valueObj.GetComponent<ValueMiddleInt>() == null)
                        {
                            newvalue = valueObj.AddComponent<ValueMiddleInt>();
                        }

                        break;
                    case VALUETYPE.WORD:
                        if (valueObj.GetComponent<ValueMiddleInt>() == null)
                        {
                            newvalue = valueObj.AddComponent<ValueMiddleInt>();
                        }

                        break;
                    case VALUETYPE.DWORD:
                        if (valueObj.GetComponent<ValueMiddleInt>() == null)
                        {
                            newvalue = valueObj.AddComponent<ValueMiddleInt>();
                        }

                        break;
                    case VALUETYPE.REAL:
                        if (valueObj.GetComponent<ValueMiddleFloat>() == null)
                        {
                            newvalue = valueObj.AddComponent<ValueMiddleFloat>();
                        }

                        break;
                }
            }


            if (newvalue != null)
                valuescript = newvalue;
            else
                valuescript = valueObj.gameObject.GetComponent<Value>();

            // delete old signals of wrong typ
            var allssignalscripts = GetComponents<Value>();
            foreach (var sig in allssignalscripts)
            {
                if (sig != valuescript)
                    DestroyImmediate(sig);
            }

            if (valuescript != null)
            {
                valuescript.Settings.Active = true;
                valuescript.SetStatusConnected(true);
            }

            return valuescript;
        }


        /// <summary>
        /// ����interfacevalue����ӡ��޸�һ���Ӽ���ֵ����
        /// </summary>
        /// <param name="interfacevalue"></param>
        /// <returns></returns>
        public Value AddValueObject(InterfaceValue interfacevalue)
        {
            GameObject valueobject = null;
            Value newvalue = null;
            InterfaceValues.Add(interfacevalue);
            valueobject = GetValueObject(interfacevalue.Name);//����Name��ȡֵ����
            if (valueobject == null)//��ֵ����Ϊ���򴴽��µ�
            {
                valueobject = new GameObject("name");
                valueobject.transform.parent = this.transform;
                if (interfacevalue.SymbolName != "")
                {
                    valueobject.name = interfacevalue.SymbolName;
                }
                else
                {
                    valueobject.name = interfacevalue.Name;
                }
            }


            Value oldvalue = valueobject.GetComponent<Value>();
            if (interfacevalue.Direction == InterfaceValue.DIRECTION.INPUT)
            {
                // Byte and  Input
                switch (interfacevalue.Type)
                {
                    case InterfaceValue.TYPE.BOOL:
                        if (valueobject.GetComponent<ValueInputBool>() == null)
                        {
                            newvalue = valueobject.AddComponent<ValueInputBool>();
                        }

                        break;
                    case InterfaceValue.TYPE.INT:
                        if (valueobject.GetComponent<ValueInputInt>() == null)
                        {
                            newvalue = valueobject.AddComponent<ValueInputInt>();
                        }

                        break;

                    case InterfaceValue.TYPE.REAL:
                        if (valueobject.GetComponent<ValueInputFloat>() == null)
                        {
                            newvalue = valueobject.AddComponent<ValueInputFloat>();
                        }

                        break;
                }
            }

            if (interfacevalue.Direction == InterfaceValue.DIRECTION.OUTPUT)
            {
                switch (interfacevalue.Type)
                {
                    case InterfaceValue.TYPE.BOOL:
                        if (valueobject.GetComponent<ValueOutputBool>() == null)
                        {
                            newvalue = valueobject.AddComponent<ValueOutputBool>();
                        }

                        break;
                    case InterfaceValue.TYPE.INT:
                        if (valueobject.GetComponent<ValueOutputInt>() == null)
                        {
                            newvalue = valueobject.AddComponent<ValueOutputInt>();
                        }

                        break;
                    case InterfaceValue.TYPE.REAL:
                        if (valueobject.GetComponent<ValueOutputFloat>() == null)
                        {
                            newvalue = valueobject.AddComponent<ValueOutputFloat>();
                        }

                        break;
                }
            }

            if (interfacevalue.Direction == InterfaceValue.DIRECTION.INPUTOUTPUT)
            {
                switch (interfacevalue.Type)
                {
                    case InterfaceValue.TYPE.BOOL:
                        if (valueobject.GetComponent<ValueMiddleBool>() == null)
                        {
                            newvalue = valueobject.AddComponent<ValueMiddleBool>();
                        }

                        break;
                    case InterfaceValue.TYPE.INT:
                        if (valueobject.GetComponent<ValueMiddleInt>() == null)
                        {
                            newvalue = valueobject.AddComponent<ValueMiddleInt>();
                        }

                        break;
                    case InterfaceValue.TYPE.REAL:
                        if (valueobject.GetComponent<ValueMiddleFloat>() == null)
                        {
                            newvalue = valueobject.AddComponent<ValueMiddleFloat>();
                        }

                        break;
                }
            }

            //���ɵ�Value����Value��ͬ���ͽ��ɵ�����
            if (oldvalue != newvalue && newvalue != null)
            {
                DestroyImmediate(oldvalue);
            }

            interfacevalue.Value = valueobject.gameObject.GetComponent<Value>();//��ȡֵ�����Value�ű�

            if (interfacevalue.Value != null)//��Ϊ�վͽ�����interfacevalue����Ϣ�����Value�ű�
            {
                //if (newvalue)
                //{
                interfacevalue.Value.Comment = interfacevalue.Comment;
                interfacevalue.Value.OriginDataType = interfacevalue.OriginDataType;
                if (interfacevalue.SymbolName != "")
                {
                    interfacevalue.Value.Name = interfacevalue.Name;
                }
                //}

                //����Value�ű�
                interfacevalue.Value.Settings.Active = true;
                interfacevalue.Value.SetStatusConnected(true);
            }

            return interfacevalue.Value;
        }


        /// <summary>
        /// �Ƴ�һ��ָ����ֵ����
        /// </summary>
        /// <param name="interfacevalue"></param>
        public void RemoveValueObject(InterfaceValue interfacevalue)
        {
            if (interfacevalue.Value != null)
            {
                Destroy(interfacevalue.Value.gameObject);
            }

            InterfaceValues.Remove(interfacevalue);
        }


        //! Gets a value object with a name
        /// <summary>
        /// ���ݸ��������ƻ�ȡ�Ӽ��е�ֵ����
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual GameObject GetValueObject(string name)
        {
            Transform[] children = transform.GetComponentsInChildren<Transform>();
            // First check names of values
            foreach (var child in children)
            {
                var value = child.GetComponent<Value>();
                if (value != null && child != gameObject.transform)
                {
                    if (value.Name == name)
                    {
                        return child.gameObject;
                    }
                }
            }

            // Second check names of components
            foreach (var child in children)
            {
                if (child != gameObject.transform)
                {
                    if (child.name == name)
                    {
                        return child.gameObject;
                    }
                }
            }

            return null;
        }

        protected void OnConnected()
        {
            SetAllValueStatus(true);
            IsConnected = true;
            if (RDTSController != null)
                RDTSController.OnConnectionOpened(gameObject);
        }

        protected void OnDisconnected()
        {
            SetAllValueStatus(false);

            IsConnected = false;
            if (RDTSController != null)
                RDTSController.OnConnectionClosed(gameObject);
        }


        public virtual void OpenInterface()
        {
        }

        public virtual void CloseInterface()
        {
        }

        /// <summary>
        /// ���������Ӽ�ֵ�����Value�ű���Connected����
        /// </summary>
        /// <param name="connected"></param>
        public void SetAllValueStatus(bool connected)
        {
            var values = GetComponentsInChildren<Value>();
            foreach (var value in values)
            {
                value.SetStatusConnected(connected);
            }
        }

        /// <summary>
        /// ���������Ӽ�ֵ����
        /// </summary>
        public void DestroyAllValues()
        {
            var values = GetComponentsInChildren<Value>();
            foreach (var value in values.ToArray())
            {
                if (value != null)
                    DestroyImmediate(value.gameObject);
            }
        }

        /// <summary>
        /// ���InterfaceValues�б��е�Value��Ϣ
        /// </summary>
        public void DeleteValues()
        {
            InterfaceValues.Clear();
        }

        void OnEnable()
        {
            OpenInterface();
        }

        void OnDisable()
        {
            CloseInterface();
        }



    }
}