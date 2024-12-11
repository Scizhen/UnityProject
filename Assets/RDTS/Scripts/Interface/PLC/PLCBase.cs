using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NaughtyAttributes;

namespace RDTS.Method
{
    //运输机系统中的PLC基类脚本？
    public class PLCBase : LibraryObject
    {
        [ReadOnly] public string CurrentState;
        [ReorderableList] public List<string> States;
        public bool DebugLog;
        private TextMesh statustextmesh;

        public string State
        {
            get { return CurrentState; }
            set
            {
                if (DebugLog)
                    Debug.Log($"PLC Debug Log Time[{Time.time}] - [{this.name}] changed state from [{CurrentState}] to [{value}]");

                CurrentState = value;
                statustextmesh.text = CurrentState;
            }
        }

        void Awake()
        {
            statustextmesh = Global.GetComponentByName<TextMesh>(this.gameObject, "Status");
            State = States[0];
        }

        public int GetDestination()
        {
            var currentdestionation = 0; // no destination
            var logics = GetComponents<PLCDestinationLogic>();
            foreach (var logic in logics)
            {
                var dest = logic.GetDestination();
                if (dest == -1)// block no other destinations
                {
                    currentdestionation = 0;
                    return currentdestionation;
                }

                if (dest != 0)
                {
                    currentdestionation = dest;
                }
            }

            return currentdestionation;
        }

        public string NextState()
        {
            // get current State
            int num = States.IndexOf(State);
            if (num >= States.Count - 1)
                State = States[0];
            else
            {
                State = States[num + 1];
            }

            if (State == "FIRST")
            {
                FirstState();
                State = States[0];
            }
            return State;
        }


        public string FirstState()
        {
            State = States[0];
            return State;
        }

    }

}
