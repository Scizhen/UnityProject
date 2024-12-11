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
    /// 池对象类，定义类池对象的相关属性、方法。
    /// </summary>
    [Serializable]
    public class PoolObject 
    {
        public ObjectPoolUtility objectPoolUtility;

        public string name;//名称
        public int ID;//索引号(应是唯一的)
        public int recyclingTimes = 0;//回收次数
        public GameObject poolObject;//对应的gameobject
        public Vector3 position;//起始位置
        public Quaternion rotation;//起始旋转
        public Vector3 localposition;//起始局部位置
        public Quaternion localrotation;//起始局部旋转

        public PoolObjectState state;//状态
        public PoolObjectState lastState;//上一次状态

        public GameObject parent;//父对象
        public GameObject lastParent;//上一个父对象

        public List<string> LifecycleInfo = new List<string>();//记录其生命周期中的信息
        //public Dictionary<Time, string> LifecycleInfos = new Dictionary<Time, string>();//记录其生命周期中的信息
        public Dictionary<DateTime, string> LifecycleInfos = new Dictionary<DateTime, string>();//记录其生命周期中的信息

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
        /// 回收此池对象
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
            ///...记录一条信息...
            ///...生命周期信息的格式：包含名称，类别，时间，状态，回收次数，父对象，加工工序等...
            if (info == null)
                return;

            DateTime time = DateTime.Now;
            string poolObjInfo = $"{this.name}-{this.ID}-{this.state}-{this.recyclingTimes}-{info}";
            

            LifecycleInfo.Add(poolObjInfo);
            LifecycleInfos[time] = poolObjInfo;

        }

        public void ReportInfo()
        {
            ///...每次生命周期结束时...
            ///...导出此池对象的生命周期信息...
            ///...当程序结束时，若池对象未被回收，也应导出其信息...
            ///...导出的数据格式：Json，excel，csv等...

        }

        ///(回收时)重置位姿
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

        ///设置池对象状态
        public void SetPoolObjectState(PoolObjectState state)
        {
            this.lastState = this.state;
            this.state = state;
        }

        ///更新池对象的父对象
        public void UpdateParent(GameObject parent)
        {
            if (parent == null)
                return;

            this.lastParent = this.parent;
            this.parent = parent;
        }

        #region 释放资源
        /////Dispose是否被调用
        //private bool disposed = false;

        /////由外部调用，以释放类资源
        //public void Dispose()
        //{
        //    Dispose(true);

        //    GC.SuppressFinalize(this);//将此对象从终结队列中移除，并防止此对象的终结代码再次执行
        //}

        //protected virtual void Dispose(bool disposing)
        //{
        //    if(!disposed)//避免多次释放
        //    {
        //        if(disposing)
        //        {
        //            ///托管资源的释放

        //        }

        //        ///非托管资源的释放


        //        disposed = true;//标示此对象已被释放
        //    }
        //}

        /////由垃圾回收器调用，释放非托管资源
        //~PoolObject()
        //{
        //    Dispose(false);
        //}

        #endregion

    }

}
