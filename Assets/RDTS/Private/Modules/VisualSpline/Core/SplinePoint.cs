using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace VisualSpline
{
    /// <summary>
    /// 切线模式
    /// </summary>
    public enum TangentMode
    {
        Mirrored,
        Aligned,
        Free
    }


    [ExecuteInEditMode]
    public class SplinePoint : MonoBehaviour
    {

        public TangentMode tangentMode = TangentMode.Mirrored;
        public Spline parentSpline;
        public int ID = -1;//ID号。默认为-1，被正常创建后将从1开始
        public List<SplinePoint> connectedPoints = new List<SplinePoint>();//记录与此锚点需要连接的其他锚点

        /// <summary>锚点是否改变或移动</summary>
        public bool Changed
        {
            get; set;
        }


        public Transform InTangent
        {
            get
            {
                if (!initialized) Initialize();
                return _inTangent;
            }

            private set
            {
                _inTangent = value;
            }
        }

        public Transform OutTangent
        {
            get
            {
                if (!initialized) Initialize();
                return _outTangent;
            }

            private set
            {
                _outTangent = value;
            }
        }

        public Transform Point
        {
            get
            {
                if (!initialized) Initialize();
                return _point;
            }

            private set
            {
                _point = value;
            }
        }



        bool initialized;
        [HideInInspector] public bool isComplete;
        Transform _point;//锚点的坐标
        Transform _inTangent;//进入的切线的坐标
        Transform _outTangent;//出去的切线的坐标

        [SerializeField][HideInInspector] Transform _masterTangent;//主切线
        [SerializeField][HideInInspector] Transform _slaveTangent;//从切线

        TangentMode _previousTangentMode;//上一种切线模式
        Vector3 _previousInPosition;//上一个InTangent对象的位置
        Vector3 _previousOutPosition;//上一个OutTangent对象的位置
        Vector3 _previousPointPosition;///上一个Point对象的位置




        void Awake()
        {
            Initialize();
        }


        /// <summary>
        /// 初始化：获取对象、隐藏组件、记录初始位置信息
        /// </summary>
        void Initialize()
        {
            initialized = true;//已初始化置位

            //获取父对象的Spline
            parentSpline = transform.GetComponentInParent<Spline>();

            //获取一个锚点下的3个对象
            InTangent = transform.Find("InTangent");
            OutTangent = transform.Find("OutTangent");
            Point = transform.Find("Point");//<! 该对象与“SplinePoint”坐标相同

            if (InTangent == null || OutTangent == null || Point == null)
            {
                isComplete = false;
                return;
            }
            else isComplete = true;


            //Debug.Log(InTangent.gameObject.name + "-" +
            //    OutTangent.gameObject.name + "-" +
            //    Point.gameObject.name);

            //设置主、从切线
            _masterTangent = InTangent;
            _slaveTangent = OutTangent;
            //将"Point"对象隐藏
            // Point.hideFlags = HideFlags.HideInHierarchy;
            //隐藏一些组件
            foreach (var item in GetComponentsInChildren<Renderer>())
            {
                item.sharedMaterial.hideFlags = HideFlags.HideInInspector;
            }
            foreach (var item in GetComponentsInChildren<MeshFilter>())
            {
                item.hideFlags = HideFlags.HideInInspector;
            }
            foreach (var item in GetComponentsInChildren<MeshRenderer>())
            {
                item.hideFlags = HideFlags.HideInInspector;
            }
            //foreach (var item in GetComponentsInChildren<SkinnedMeshRenderer>())
            //{
            //    item.hideFlags = HideFlags.HideInInspector;
            //}


            _previousTangentMode = tangentMode;
            _previousInPosition = InTangent.localPosition;
            _previousOutPosition = OutTangent.localPosition;
            _previousPointPosition = transform.position;


        }


        void Update()
        {
            if (!isComplete) return;


            //scale:不要让“SplinePoint”缩放：
            transform.localScale = Vector3.one;


            if (!initialized)
            {
                Initialize();
            }

            Point.localPosition = Vector3.zero;

            //锚点是否移动
            if (_previousPointPosition != transform.position)
            {
                Changed = true;
                _previousPointPosition = transform.position;
            }

            // 如果模式已更改，则运行切线操作：
            if (_previousTangentMode != tangentMode)
            {
                Changed = true;
                TangentChanged();
                _previousTangentMode = tangentMode;
            }

            //检测切线运动：
            if (InTangent.localPosition != _previousInPosition)
            {
                Changed = true;
                _previousInPosition = InTangent.localPosition;
                _masterTangent = InTangent;
                _slaveTangent = OutTangent;
                TangentChanged();
                return;
            }
            if (OutTangent.localPosition != _previousOutPosition)
            {
                Changed = true;
                _previousOutPosition = OutTangent.localPosition;
                _masterTangent = OutTangent;
                _slaveTangent = InTangent;
                TangentChanged();
                return;
            }

        }




        /// <summary>
        /// 刷新关联锚点列表，将为空的锚点删去
        /// </summary>
        public void RefreshConnectedPointsList()
        {
            if (connectedPoints == null || connectedPoints.Count == 0)
                return;

            for (int i = 0; i < connectedPoints.Count; i++)
            {
                //将为null的锚点删去
                if (connectedPoints[i] == null)
                    connectedPoints.RemoveAt(i);
            }
        }



        /// <summary>
        /// 切线变化的处理方法
        /// </summary>
        void TangentChanged()
        {
            //calculate tangent positions:计算切线位置：
            switch (tangentMode)
            {
                case TangentMode.Free:
                    break;

                case TangentMode.Mirrored://镜像（方向和距离）
                    Vector3 mirroredOffset = _masterTangent.position - transform.position;
                    _slaveTangent.position = transform.position - mirroredOffset;
                    break;

                case TangentMode.Aligned://仅方向对齐
                    float distance = Vector3.Distance(_slaveTangent.position, transform.position);
                    Vector3 alignedOffset = (_masterTangent.position - transform.position).normalized;
                    _slaveTangent.position = transform.position - (alignedOffset * distance);
                    break;
            }

            //cache tangent positions:
            _previousInPosition = InTangent.localPosition;
            _previousOutPosition = OutTangent.localPosition;
        }



        /// <summary>
        /// 设置切线的状态
        /// </summary>
        /// <param name="inStatus">主切线</param>
        /// <param name="outStatus">从切线</param>
        public void SetTangentStatus(bool inStatus, bool outStatus)
        {
            InTangent.gameObject.SetActive(inStatus);
            OutTangent.gameObject.SetActive(outStatus);
        }


        /// <summary>
        /// 添加关联的锚点
        /// </summary>
        /// <param name="newpoint"></param>
        public void AddConnectedPoint(SplinePoint newpoint)
        {
            RefreshConnectedPointsList();

            if (!this.connectedPoints.Contains(newpoint))
                this.connectedPoints.Add(newpoint);
            else
                Debug.Log("Notice: The point has been added!");


        }



        /// <summary>
        /// 添加指定数量的锚点
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<GameObject> AddSplinePoint(int count)
        {
            GameObject pointTemplate = Resources.Load("SplinePoint") as GameObject;
            if (pointTemplate == null) return null;

            List<GameObject> returnGObj = new List<GameObject>();

            for (int i = 0; i < count; i++)
            {

                //创建一个新的锚点
                GameObject newPoint = Instantiate<GameObject>(pointTemplate);
                SplinePoint point = newPoint.GetComponent<SplinePoint>();
                point.tangentMode = tangentMode;//设置相同切线模式
                point.parentSpline = parentSpline;//关联父Spline
                point.ID = parentSpline.Points.Count + 1;//设置ID
                Transform parent = parentSpline.gameObject.transform;
                newPoint.name = newPoint.name.Replace("(Clone)", point.ID.ToString());
                newPoint.transform.parent = parent;
                newPoint.transform.rotation = Quaternion.LookRotation(parent.forward);
                //从此锚点开始以正确的方向与距离放置：
                newPoint.transform.position = transform.position + (transform.forward * 1.5f);

                point.InTangent.Translate(Vector3.up * .5f);
                point.OutTangent.Translate(Vector3.up * -.5f);

                point.AddConnectedPoint(this);
                this.AddConnectedPoint(point);

                //存储该新锚点以供返回：
                returnGObj.Add(newPoint);

            }


            return returnGObj;
        }
        public List<GameObject> AddSplinePointAsXml(int id,string pointName,float xPosition,float yPosition, Spline parentSpline)
        {
            GameObject pointTemplate = Resources.Load("SplinePoint") as GameObject;
            if (pointTemplate == null) return null;

            List<GameObject> returnGObj = new List<GameObject>();

            //创建一个新的锚点
            GameObject newPoint = Instantiate<GameObject>(pointTemplate);
            SplinePoint point = newPoint.GetComponent<SplinePoint>();
            point.tangentMode = tangentMode;//设置相同切线模式
            point.parentSpline = parentSpline;//关联父Spline
            point.ID =id;//设置ID
            Transform parent = parentSpline.gameObject.transform;
            newPoint.name = pointName;//newPoint.name.Replace("(Clone)", point.ID.ToString());
            newPoint.transform.parent = parent;
            newPoint.transform.rotation = Quaternion.LookRotation(parent.forward);
            Vector3 pointTransform = new Vector3(xPosition, 0f , yPosition);
            //从此锚点开始以正确的方向与距离放置：
            newPoint.transform.position =  pointTransform;//(transform.forward * 1.5f);
             
            point.InTangent.transform.localPosition = new Vector3(0.5f, 0f, 0.5f);
            point.OutTangent.transform.localPosition = new Vector3(-0.5f, 0f,-0.5f);

            //存储该新锚点以供返回：
            returnGObj.Add(newPoint);

            return returnGObj;
        }

    }






}