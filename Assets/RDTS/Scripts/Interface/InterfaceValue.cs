//*************************************************************************
//Thanks for the code reference game4automation provides.                 *
//                                                                        *
//*************************************************************************
using UnityEngine;

namespace RDTS
{
    /// <summary>
    /// 定义一个接口值（值对象）的属性
    /// </summary>
    public class InterfaceValue
    {
        public enum TYPE
        {
            BOOL,
            INT,
            REAL,
            TRANSFORM,
            UNDEFINED
        };

        public enum DIRECTION
        {
            NOTDEFINED,
            INPUT,
            OUTPUT,
            INPUTOUTPUT
        };

        public Value Value;//
        public string Name;//
        public TYPE Type;//
        public DIRECTION Direction;//
        public int Mempos;
        public byte Bit;
        public string SymbolName;
        public string Comment;
        public string OriginDataType;

        public InterfaceValue()
        {

        }


        public InterfaceValue(string name, DIRECTION direction, TYPE type)
        {
            Name = name;
            Direction = direction;
            Type = type;
        }

        public void UpdateValue(Value value)
        {

        }
    }

}

