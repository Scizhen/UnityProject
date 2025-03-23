using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace VisualSpline
{
    /// <summary>
    /// ����ģʽ
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
        public int ID = -1;//ID�š�Ĭ��Ϊ-1�������������󽫴�1��ʼ
        public List<SplinePoint> connectedPoints = new List<SplinePoint>();//��¼���ê����Ҫ���ӵ�����ê��

        /// <summary>ê���Ƿ�ı���ƶ�</summary>
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
        Transform _point;//ê�������
        Transform _inTangent;//��������ߵ�����
        Transform _outTangent;//��ȥ�����ߵ�����

        [SerializeField][HideInInspector] Transform _masterTangent;//������
        [SerializeField][HideInInspector] Transform _slaveTangent;//������

        TangentMode _previousTangentMode;//��һ������ģʽ
        Vector3 _previousInPosition;//��һ��InTangent�����λ��
        Vector3 _previousOutPosition;//��һ��OutTangent�����λ��
        Vector3 _previousPointPosition;///��һ��Point�����λ��




        void Awake()
        {
            Initialize();
        }


        /// <summary>
        /// ��ʼ������ȡ���������������¼��ʼλ����Ϣ
        /// </summary>
        void Initialize()
        {
            initialized = true;//�ѳ�ʼ����λ

            //��ȡ�������Spline
            parentSpline = transform.GetComponentInParent<Spline>();

            //��ȡһ��ê���µ�3������
            InTangent = transform.Find("InTangent");
            OutTangent = transform.Find("OutTangent");
            Point = transform.Find("Point");//<! �ö����롰SplinePoint��������ͬ

            if (InTangent == null || OutTangent == null || Point == null)
            {
                isComplete = false;
                return;
            }
            else isComplete = true;


            //Debug.Log(InTangent.gameObject.name + "-" +
            //    OutTangent.gameObject.name + "-" +
            //    Point.gameObject.name);

            //��������������
            _masterTangent = InTangent;
            _slaveTangent = OutTangent;
            //��"Point"��������
            // Point.hideFlags = HideFlags.HideInHierarchy;
            //����һЩ���
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


            //scale:��Ҫ�á�SplinePoint�����ţ�
            transform.localScale = Vector3.one;


            if (!initialized)
            {
                Initialize();
            }

            Point.localPosition = Vector3.zero;

            //ê���Ƿ��ƶ�
            if (_previousPointPosition != transform.position)
            {
                Changed = true;
                _previousPointPosition = transform.position;
            }

            // ���ģʽ�Ѹ��ģ����������߲�����
            if (_previousTangentMode != tangentMode)
            {
                Changed = true;
                TangentChanged();
                _previousTangentMode = tangentMode;
            }

            //��������˶���
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
        /// ˢ�¹���ê���б���Ϊ�յ�ê��ɾȥ
        /// </summary>
        public void RefreshConnectedPointsList()
        {
            if (connectedPoints == null || connectedPoints.Count == 0)
                return;

            for (int i = 0; i < connectedPoints.Count; i++)
            {
                //��Ϊnull��ê��ɾȥ
                if (connectedPoints[i] == null)
                    connectedPoints.RemoveAt(i);
            }
        }



        /// <summary>
        /// ���߱仯�Ĵ�����
        /// </summary>
        void TangentChanged()
        {
            //calculate tangent positions:��������λ�ã�
            switch (tangentMode)
            {
                case TangentMode.Free:
                    break;

                case TangentMode.Mirrored://���񣨷���;��룩
                    Vector3 mirroredOffset = _masterTangent.position - transform.position;
                    _slaveTangent.position = transform.position - mirroredOffset;
                    break;

                case TangentMode.Aligned://���������
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
        /// �������ߵ�״̬
        /// </summary>
        /// <param name="inStatus">������</param>
        /// <param name="outStatus">������</param>
        public void SetTangentStatus(bool inStatus, bool outStatus)
        {
            InTangent.gameObject.SetActive(inStatus);
            OutTangent.gameObject.SetActive(outStatus);
        }


        /// <summary>
        /// ��ӹ�����ê��
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
        /// ���ָ��������ê��
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

                //����һ���µ�ê��
                GameObject newPoint = Instantiate<GameObject>(pointTemplate);
                SplinePoint point = newPoint.GetComponent<SplinePoint>();
                point.tangentMode = tangentMode;//������ͬ����ģʽ
                point.parentSpline = parentSpline;//������Spline
                point.ID = parentSpline.Points.Count + 1;//����ID
                Transform parent = parentSpline.gameObject.transform;
                newPoint.name = newPoint.name.Replace("(Clone)", point.ID.ToString());
                newPoint.transform.parent = parent;
                newPoint.transform.rotation = Quaternion.LookRotation(parent.forward);
                //�Ӵ�ê�㿪ʼ����ȷ�ķ����������ã�
                newPoint.transform.position = transform.position + (transform.forward * 1.5f);

                point.InTangent.Translate(Vector3.up * .5f);
                point.OutTangent.Translate(Vector3.up * -.5f);

                point.AddConnectedPoint(this);
                this.AddConnectedPoint(point);

                //�洢����ê���Թ����أ�
                returnGObj.Add(newPoint);

            }


            return returnGObj;
        }
        public List<GameObject> AddSplinePointAsXml(int id,string pointName,float xPosition,float yPosition, Spline parentSpline)
        {
            GameObject pointTemplate = Resources.Load("SplinePoint") as GameObject;
            if (pointTemplate == null) return null;

            List<GameObject> returnGObj = new List<GameObject>();

            //����һ���µ�ê��
            GameObject newPoint = Instantiate<GameObject>(pointTemplate);
            SplinePoint point = newPoint.GetComponent<SplinePoint>();
            point.tangentMode = tangentMode;//������ͬ����ģʽ
            point.parentSpline = parentSpline;//������Spline
            point.ID =id;//����ID
            Transform parent = parentSpline.gameObject.transform;
            newPoint.name = pointName;//newPoint.name.Replace("(Clone)", point.ID.ToString());
            newPoint.transform.parent = parent;
            newPoint.transform.rotation = Quaternion.LookRotation(parent.forward);
            Vector3 pointTransform = new Vector3(xPosition, 0f , yPosition);
            //�Ӵ�ê�㿪ʼ����ȷ�ķ����������ã�
            newPoint.transform.position =  pointTransform;//(transform.forward * 1.5f);
             
            point.InTangent.transform.localPosition = new Vector3(0.5f, 0f, 0.5f);
            point.OutTangent.transform.localPosition = new Vector3(-0.5f, 0f,-0.5f);

            //�洢����ê���Թ����أ�
            returnGObj.Add(newPoint);

            return returnGObj;
        }

    }






}