//*************************************************************************
//Thanks for the code reference game4automation provides.                 *
//                                                                        *
//*************************************************************************
using System.Collections.Generic;
using UnityEngine;


namespace RDTS
{
    public interface IValueInterface
    {
        public List<BehaviorInterfaceConnection> GetConnections();
        public List<Value> GetValues();
        GameObject gameObject { get; }
    }
}


