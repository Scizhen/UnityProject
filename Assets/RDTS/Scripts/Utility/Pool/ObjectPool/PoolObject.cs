///********************************************************************************************
///this is part of Robot Digital Twin System.                                                 *
///@Copyright by Shaw.S , 2022                                                                *
///Do not distribute without authorization,thanks!                                            *
///********************************************************************************************
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RDTS.Utility
{
    /// <summary>
    /// �ض����࣬������ض����������ԡ�������
    /// </summary>
    [Serializable]
    public class PoolObject 
    {
        public ObjectPoolUtility objectPoolUtility;

        public string name;//����
        public int ID;//������(Ӧ��Ψһ��)
        public int recyclingTimes = 0;//���մ���
        public GameObject poolObject;//��Ӧ��gameobject
        public Vector3 position;//��ʼλ��
        public Quaternion rotation;//��ʼ��ת
        public Vector3 localposition;//��ʼ�ֲ�λ��
        public Quaternion localrotation;//��ʼ�ֲ���ת

        public PoolObjectState state;//״̬
        public PoolObjectState lastState;//��һ��״̬

        public GameObject parent;//������
        public GameObject lastParent;//��һ��������

        public List<string> LifecycleInfo = new List<string>();//��¼�����������е���Ϣ
        //public Dictionary<Time, string> LifecycleInfos = new Dictionary<Time, string>();//��¼�����������е���Ϣ
        public Dictionary<DateTime, string> LifecycleInfos = new Dictionary<DateTime, string>();//��¼�����������е���Ϣ

        [NonSerialized][ReadOnly]
        public string adoptInfo = "Adopt";
        [NonSerialized][ReadOnly]
        public string recycleInfo = "Recycle";
        [NonSerialized][ReadOnly]
        public string breakInfo = "Break";


        public PoolObject(ObjectPoolUtility objectPoolUtility, string name, int ID, GameObject poolObject, PoolObjectState state)
        {
            this.objectPoolUtility = objectPoolUtility;
            this.name = name;
            this.ID = ID;
            this.poolObject = poolObject;
            this.poolObject.name = name;
            this.position = poolObject.transform.position;
            this.rotation = poolObject.transform.rotation;
            this.localposition = poolObject.transform.localPosition;
            this.localrotation = poolObject.transform.localRotation;
            this.state = this.lastState = state;
            this.parent = this.lastParent = poolObject.transform.parent.gameObject;
        }



        /// <summary>
        /// ���մ˳ض���
        /// </summary>
        /// <param name="delay"></param>
        public void RecyclePoolObj(float delay = 0f)
        {
            objectPoolUtility?.RecycleGivenPoolObject(this.poolObject, delay);
        }



        public void RecordInfo_adopt()
        {
            RecordInfo(adoptInfo);
        }

        public void RecordInfo_recycle()
        {
            RecordInfo(recycleInfo);
        }

        public void RecordInfo_break()
        {
            RecordInfo(breakInfo);
        }

        public void RecordInfo(string info)
        {
            ///...��¼һ����Ϣ...
            ///...����������Ϣ�ĸ�ʽ���������ƣ����ʱ�䣬״̬�����մ����������󣬼ӹ������...
            if (info == null)
                return;

            DateTime time = DateTime.Now;
            string poolObjInfo = $"{this.name}-{this.ID}-{this.state}-{this.recyclingTimes}-{info}";
            

            LifecycleInfo.Add(poolObjInfo);
            LifecycleInfos[time] = poolObjInfo;

        }

        public void ReportInfo()
        {
            ///...ÿ���������ڽ���ʱ...
            ///...�����˳ض��������������Ϣ...
            ///...���������ʱ�����ض���δ�����գ�ҲӦ��������Ϣ...
            ///...���������ݸ�ʽ��Json��excel��csv��...

        }

        ///(����ʱ)����λ��
        public void ResetPosition()
        {
            this.poolObject.transform.position = this.position;
            this.poolObject.transform.localPosition = this.localposition;
        }
        public void ResetRotation()
        {
            this.poolObject.transform.rotation = this.rotation;
            this.poolObject.transform.localRotation = this.localrotation;
        }

        ///���óض���״̬
        public void SetPoolObjectState(PoolObjectState state)
        {
            this.lastState = this.state;
            this.state = state;
        }

        ///���³ض���ĸ�����
        public void UpdateParent(GameObject parent)
        {
            if (parent == null)
                return;

            this.lastParent = this.parent;
            this.parent = parent;
        }

        #region �ͷ���Դ
        /////Dispose�Ƿ񱻵���
        //private bool disposed = false;

        /////���ⲿ���ã����ͷ�����Դ
        //public void Dispose()
        //{
        //    Dispose(true);

        //    GC.SuppressFinalize(this);//���˶�����ս�������Ƴ�������ֹ�˶�����ս�����ٴ�ִ��
        //}

        //protected virtual void Dispose(bool disposing)
        //{
        //    if(!disposed)//�������ͷ�
        //    {
        //        if(disposing)
        //        {
        //            ///�й���Դ���ͷ�

        //        }

        //        ///���й���Դ���ͷ�


        //        disposed = true;//��ʾ�˶����ѱ��ͷ�
        //    }
        //}

        /////���������������ã��ͷŷ��й���Դ
        //~PoolObject()
        //{
        //    Dispose(false);
        //}

        #endregion

    }

}
