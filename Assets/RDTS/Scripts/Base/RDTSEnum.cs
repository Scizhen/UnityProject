///********************************************************************************************
///this is part of Robot Digital Twin System.                                                 *
///@Copyright by Shaw.S                                                                       *
///Do not distribute without authorization,thanks!                                            *
///********************************************************************************************
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;


namespace RDTS
{
    /// <summary>
    /// 层级
    /// </summary>
    public enum RDTSLayer
    {
        None = -1,
        SolidObject = 15,//实心物体，不允许穿模
        Sensor,//传感器，可检测MU或TransportMU或Obstacle或MovableTransport
        Obstacle, //障碍物，阻挡其他物体穿过（包含检测光线）
        Transport,//能承载、运输MU的面
        MovableTransport,//可移动且能承载、运输MU，类似于AGV、旋转台、移动台
        MU,//可移动单元(BoxCollider)
        TransportMU,//运输碰撞体的MU（MeshCollider），更真实的碰撞，也可用于AGV
    }

    /// <summary>
    /// 标签
    /// </summary>
    public enum RDTSTag
    {
        None = -1,
        Controller = 0,//控制器（仅一个）
        Robot,//主要指机械臂
        RobotTool,//机器人的工具，如夹爪、吸盘
        Machine,//各类加工机器
        MachineCraft,//机器工艺，在一台机器中能实现如推进、阻焊、冲孔等加工工艺的部分
        Station,//工作台（指具有加工工序的工位，一般与Station脚本配合使用）
        Transport,//运输模块
        Sensor,//传感器，检测设备
        Workpiece,//工件
        Product,//加工完成的产品
        Container,//容器，可装载其他物体如物料框、包装盒
        AGV,//这里将AGV单独从Transport中提出
        Obstacle,//障碍物
        Ground,//地面（仅一个，用来放置生产线）
        Human,//人（为以后人机工程模块考虑）
        UI,//用户交互模块
        Camera,//相机（主相机unity默认定义）
        Environment,//环境、场景装饰等
        MainLight,//主光线
        Light//光线
    }

    /// <summary>
    /// Drive的方向
    /// </summary>
    public enum DIRECTION
    {
        LinearX,
        LinearY,
        LinearZ,
        RotationX,
        RotationY,
        RotationZ,
        Virtual
    }

    /// <summary>
    /// 脚本的启用状态
    /// </summary>
    public enum ActiveOnly
    {
        Always,
        Connected,
        Disconnected,
        Never,
        DontChange
    }

    /// <summary>
    /// 池对象状态
    /// </summary>
    public enum PoolObjectState
    {
        Silent,//沉默状态
        Active,//激活状态
        Broken //破损状态
    }

    /// <summary>
    /// 孪生模型层次
    /// </summary>
    public enum TwinModelHierarchy
    {
        MainDeviceModel = 1,//主要设备模型
        SecondaryDeviceModel,//次要设备模型
        AuxiliaryDeviceModel//辅助设备模型
    }

    /// <summary>
    /// 孪生模型类型（对应模型库中的模型分类）
    /// </summary>
    public enum TwinModelType
    {
        [Description("机械臂")]
        RobotArm = 1,
        [Description("末端工具")]
        EndTool,
        [Description("机器设备")]
        MechanicalDevice,
        [Description("输送模块")]
        DeliveryModule,
        [Description("传感器")]
        Sensor,
        [Description("AGV")]
        AGV,
        [Description("工件")]
        Workpiece,
        [Description("其他")]
        Other = 8
    }



}
