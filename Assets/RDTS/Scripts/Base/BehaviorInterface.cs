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
    //���ڱ����������ݵ��� - �źŸ��ӵ������Ե��źź�����
    public class BehaviorInterfaceConnection
    {
        public Value Value;
        public string Name;
    }

    //! Base class for all behavior models with connection to value object. 
    //���ӵ�"ֵ����"��������Ϊģ�͵Ļ��ࡣ
    public class BehaviorInterface : RDTSBehavior, IValueInterface
    {
        public List<BehaviorInterfaceConnection> ConnectionInfo = new List<BehaviorInterfaceConnection>();

        public new List<BehaviorInterfaceConnection> GetConnections()
        {
            ConnectionInfo = UpdateConnectionInfo(); // ���¡���ȡ���и���Value�����ֶε���Ϣ��BehaviorInterfaceConnection��
            return ConnectionInfo;
        }

        public List<Value> GetValues()
        {
            return GetConnectedValues();
        }
    }
}
