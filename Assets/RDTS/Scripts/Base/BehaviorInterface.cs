//*************************************************************************
//Thanks for the code reference game4automation provides.                 *
//                                                                        *
//*************************************************************************  
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;

namespace RDTS
{
    //! Class for saving the connection data - the value and the name of the property where the value is attached to
    //用于保存连接数据的类 - 信号附加到的属性的信号和名称
    public class BehaviorInterfaceConnection
    {
        public Value Value;
        public string Name;
    }

    //! Base class for all behavior models with connection to value object. 
    //连接到"值对象"的所有行为模型的基类。
    public class BehaviorInterface : RDTSBehavior, IValueInterface
    {
        public List<BehaviorInterfaceConnection> ConnectionInfo = new List<BehaviorInterfaceConnection>();

        public new List<BehaviorInterfaceConnection> GetConnections()
        {
            ConnectionInfo = UpdateConnectionInfo(); // 更新、获取所有附带Value类型字段的信息（BehaviorInterfaceConnection）
            return ConnectionInfo;
        }

        public List<Value> GetValues()
        {
            return GetConnectedValues();
        }
    }
}
