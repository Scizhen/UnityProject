using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PLCDestinationAlternating : PLCDestinationLogic
{
    //�����ϵͳ�е�PLCģ��������Ϸּ��������ƻ����ת��
    public int NumDestinations = 2;
    public int CurrentDestination = 0;

    public void Start()
    {
        CurrentDestination = 1;
    }

    public override int GetDestination()
    {
        CurrentDestination++;
        if (CurrentDestination > NumDestinations)
            CurrentDestination = 1;
        return CurrentDestination;
    }



}
