using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RDTS
{
    //�ӿ����������������ķ���
    public interface IConveyor
    {
        Vector3 GetPosition(float position);
        Quaternion GetRotation(float position);
        float GetHeight();
        float GetLength();
        float GetWidth();
        float GetBottomHeight();
        bool GetLegs();
        float GetLeftGuideHeight();
        float GetRightGuideHeight();
        float GetHeightIncrease();
        bool GetIsRollerConveyor();
        float GetRollDistance();

    }
}
