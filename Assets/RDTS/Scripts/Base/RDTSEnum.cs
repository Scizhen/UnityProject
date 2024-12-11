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
    /// �㼶
    /// </summary>
    public enum RDTSLayer
    {
        None = -1,
        SolidObject = 15,//ʵ�����壬������ģ
        Sensor,//���������ɼ��MU��TransportMU��Obstacle��MovableTransport
        Obstacle, //�ϰ���赲�������崩�������������ߣ�
        Transport,//�ܳ��ء�����MU����
        MovableTransport,//���ƶ����ܳ��ء�����MU��������AGV����ת̨���ƶ�̨
        MU,//���ƶ���Ԫ(BoxCollider)
        TransportMU,//������ײ���MU��MeshCollider��������ʵ����ײ��Ҳ������AGV
    }

    /// <summary>
    /// ��ǩ
    /// </summary>
    public enum RDTSTag
    {
        None = -1,
        Controller = 0,//����������һ����
        Robot,//��Ҫָ��е��
        RobotTool,//�����˵Ĺ��ߣ����צ������
        Machine,//����ӹ�����
        MachineCraft,//�������գ���һ̨��������ʵ�����ƽ����躸����׵ȼӹ����յĲ���
        Station,//����̨��ָ���мӹ�����Ĺ�λ��һ����Station�ű����ʹ�ã�
        Transport,//����ģ��
        Sensor,//������������豸
        Workpiece,//����
        Product,//�ӹ���ɵĲ�Ʒ
        Container,//��������װ���������������Ͽ򡢰�װ��
        AGV,//���ｫAGV������Transport�����
        Obstacle,//�ϰ���
        Ground,//���棨��һ�����������������ߣ�
        Human,//�ˣ�Ϊ�Ժ��˻�����ģ�鿼�ǣ�
        UI,//�û�����ģ��
        Camera,//����������unityĬ�϶��壩
        Environment,//����������װ�ε�
        MainLight,//������
        Light//����
    }

    /// <summary>
    /// Drive�ķ���
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
    /// �ű�������״̬
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
    /// �ض���״̬
    /// </summary>
    public enum PoolObjectState
    {
        Silent,//��Ĭ״̬
        Active,//����״̬
        Broken //����״̬
    }

    /// <summary>
    /// ����ģ�Ͳ��
    /// </summary>
    public enum TwinModelHierarchy
    {
        MainDeviceModel = 1,//��Ҫ�豸ģ��
        SecondaryDeviceModel,//��Ҫ�豸ģ��
        AuxiliaryDeviceModel//�����豸ģ��
    }

    /// <summary>
    /// ����ģ�����ͣ���Ӧģ�Ϳ��е�ģ�ͷ��ࣩ
    /// </summary>
    public enum TwinModelType
    {
        [Description("��е��")]
        RobotArm = 1,
        [Description("ĩ�˹���")]
        EndTool,
        [Description("�����豸")]
        MechanicalDevice,
        [Description("����ģ��")]
        DeliveryModule,
        [Description("������")]
        Sensor,
        [Description("AGV")]
        AGV,
        [Description("����")]
        Workpiece,
        [Description("����")]
        Other = 8
    }



}
