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
    /// 有关Value的基类。主要提供了一系列值对象相关的方法
    /// </summary>
    public class InterfaceBaseClass : BaseInterface
    {
        [ReadOnly] public bool IsConnected = false;

        public List<InterfaceValue> InterfaceValues = new List<InterfaceValue>();


        //! Creates a new List of InterfaceValues based on the Components under this Interface GameObject
        /// <summary>
        /// 基于此接口对象下的组件创建一个新的接口值列表
        /// </summary>
        /// <param name="inputs">input类型的数量</param>
        /// <param name="outputs">output类型的数量</param>
        public void UpdateInterfaceValues(ref int inputs, ref int outputs)
        {
            InterfaceValues.Clear();
            inputs = 0;
            outputs = 0;
            var values = gameObject.GetComponentsInChildren(typeof(Value), true);//获取gameobject下Value类型的脚本
            foreach (Value value in values)
            {
                var newvalue = value.GetInterfaceValue();//根据信号组件返回一个 InterfaceValue 对象
                InterfaceValues.Add(newvalue);//加入InterfaceValues列表中
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
        /// 根据name、type、direction来创建一个新的值对象，若已存在同名的值对象就修改其Value脚本
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

            valueObj = GetChildByName(name);//获取此对象下名称为name的子对象
            if (valueObj == null)//为空则创建新的对象，作为子对象
            {
                valueObj = new GameObject("name");
                valueObj.transform.parent = this.transform;
                valueObj.name = name;
            }

            //根据type和direction获取相应的Value脚本
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
        /// 根据interfacevalue来添加、修改一个子级的值对象
        /// </summary>
        /// <param name="interfacevalue"></param>
        /// <returns></returns>
        public Value AddValueObject(InterfaceValue interfacevalue)
        {
            GameObject valueobject = null;
            Value newvalue = null;
            InterfaceValues.Add(interfacevalue);
            valueobject = GetValueObject(interfacevalue.Name);//根据Name获取值对象
            if (valueobject == null)//若值对象为空则创建新的
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

            //若旧的Value与新Value不同，就将旧的销毁
            if (oldvalue != newvalue && newvalue != null)
            {
                DestroyImmediate(oldvalue);
            }

            interfacevalue.Value = valueobject.gameObject.GetComponent<Value>();//获取值对象的Value脚本

            if (interfacevalue.Value != null)//不为空就将参数interfacevalue的信息赋予给Value脚本
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

                //激活Value脚本
                interfacevalue.Value.Settings.Active = true;
                interfacevalue.Value.SetStatusConnected(true);
            }

            return interfacevalue.Value;
        }


        /// <summary>
        /// 移除一个指定的值对象
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
        /// 根据给定的名称获取子级中的值对象
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
        /// 设置所有子级值对象的Value脚本的Connected参数
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
        /// 销毁所有子级值对象
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
        /// 清空InterfaceValues列表中的Value信息
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