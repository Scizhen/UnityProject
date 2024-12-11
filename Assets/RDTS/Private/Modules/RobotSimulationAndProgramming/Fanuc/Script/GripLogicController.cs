using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RDTS;


/// <summary>
/// 机械臂抓取/放置的逻辑控制器
/// </summary>
public class GripLogicController : MonoBehaviour
{
    [Header("Conveyer Control")]
    public ValueOutputBool conveyerDrive;
    public ValueMiddleBool sensorDetect;

    [Header("Object Pool")]
    public ValueMiddleBool AdjustSignal;

    [Header("Robot Control")]
    public ValueOutputBool PickSignal;
    public ValueOutputBool PlaceSignal;
    public ValueMiddleInt PickCountOfGripper;
    public ValueOutputInt Refresh;
    public ValueInputInt MoveToPick;
    public ValueOutputInt PickObject;
    public ValueInputInt PickOK;
    public ValueInputInt MoveToPlace;
    public ValueOutputInt PlaceObject;
    public ValueInputInt PlaceOK;




    // Start is called before the first frame update
    void Start()
    {
        PickSignal.Value = false;
        PlaceSignal.Value = false;
    }

    // Update is called once per frame
    void Update()
    {
        ///输送带控制
        conveyerDrive.Value = !sensorDetect.Value;

        ///机械臂控制
        //移动到抓取点
        if (sensorDetect.Value)
            MoveToPick.Value = 1;
        else
            MoveToPick.Value = 0;
        //去抓取对象
        if (PickObject.Value == 1)
            PickSignal.Value = true;
        //抓取完成
        if (PickCountOfGripper.Value == 1)
        {
            PickOK.Value = 1;
            AdjustSignal.Value = false;
        }
        //移动到放置点
        MoveToPlace.Value = 1;//该场景默认直接放置
        //去放置对象
        if (PlaceObject.Value == 1)
            PlaceSignal.Value = true;
        //放置完成
        if (PickCountOfGripper.Value == 0)
        {
            PlaceOK.Value = 1;
            AdjustSignal.Value = true;
        }

        //刷新信号值
        if (Refresh.Value == 1)
        {
            MoveToPick.Value = 0;
            PickOK.Value = 0;
            MoveToPlace.Value = 0;
            PlaceOK.Value = 0;

            PickSignal.Value = false;
            PlaceSignal.Value = false;
            AdjustSignal.Value = false;
        }



    }
}
