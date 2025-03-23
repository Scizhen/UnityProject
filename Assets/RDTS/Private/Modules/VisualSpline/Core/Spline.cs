using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VisualSpline
{
    /// <summary>������Ϣ�����ݴ���Ϣ������һ������</summary>
    [Serializable]
    public class LineComposition
    {
        public int index = -1;//������
        public SplinePoint startPoint;//��ʼ��
        public SplinePoint endPoint;//������
        public LineType lineType;//��������
    }
    /// <summary>�������ɣ��ɹ����߶ι�ϵ��һ�Ե�</summary>
    [Serializable]
    public class Line
    {
        public int index = -1;//������
        public SplinePoint point1;//ê��1
        public SplinePoint point2;//ê��2
        public bool reverse;//����ʱ�Ƿ����point1��point2��˳��
        public LineType lineType;//��������
        public Color color;//������ɫ
        public float lineWidth;//�������
        public SplineTangent sourceControl;
        public SplineTangent destinationControl ;

    }


    /// <summary>�������ͣ���ѡ��ֱ�߻��������߻���ĳ������ </summary>
    public enum LineType { Straight, Bezier }

    /// <summary>�ռ�ά�ȣ���ѡ��������Ϊ��ά���ά </summary>
    public enum SpatialDimension { ThreeDimensional, TwoDimensional }


    [ExecuteInEditMode]
    public class Spline : MonoBehaviour
    {
        public event Action OnSplineChanged;//�¼�����δ�ã�

        public SpatialDimension spatialDimension = SpatialDimension.ThreeDimensional;
        public Color color = Color.yellow;//��������ɫ
        [Range(0, 1)] public float toolScale = .1f;//ê�����Ŵ�С
        //public bool loop;//�Ƿ�ѭ�����պϣ�
        public int countOfPoints = 0;//��¼���е�ê������
        [HideInInspector] public bool automaticallyAddIfNull = false;//�Ƿ���û������ʱ���Զ����
        public List<Line> lines = new List<Line>();//�������е������߶���Ϣ�����ݴ��б����������������

        protected Dictionary<int, SplinePoint> PointPerID = new Dictionary<int, SplinePoint>();//�洢���������µ�ê�㣬ͨ��ID�ż���
        public bool driveAgainst = false;

        /// <summary>�����ߵ��ܳ��� </summary>
        public float Length
        {
            get;
            private set;
        }


        private SpatialDimension lastSpatialDimension;
        private bool _wasLooping;
        private int _previousPointCount;
        private bool _lengthDirty = true;//�����ߵĳ����Ƿ����仯



        List<SplinePoint> _points;//��¼�м���ê��
        public List<SplinePoint> Points//��¼�������������е�ê��
        {
            get
            {
                _points = GetComponentsInChildren<SplinePoint>().ToList();
                _previousPointCount = countOfPoints = _points.Count;

                return _points;

            }

        }




        void Reset()
        {

        }


        public void AddLine()
        {
            GameObject pointTemplate = Resources.Load("SplinePoint") as GameObject;
            if (pointTemplate == null) return;

            Transform parent = this.gameObject.transform;

            //������һ���µ�ê��
            GameObject newPoint1 = Instantiate<GameObject>(pointTemplate);
            SplinePoint point1 = newPoint1.GetComponent<SplinePoint>();
            point1.parentSpline = this;//������Spline
            point1.ID = this.Points.Count + 1;//����ID
            newPoint1.name = newPoint1.name.Replace("(Clone)", point1.ID.ToString());
            newPoint1.transform.parent = parent;//����transform����������

            //�����ڶ����µ�ê��
            GameObject newPoint2 = Instantiate<GameObject>(pointTemplate);
            SplinePoint point2 = newPoint2.GetComponent<SplinePoint>();
            point2.parentSpline = this;//������Spline
            point2.ID = this.Points.Count + 1;//����ID
            newPoint2.name = newPoint2.name.Replace("(Clone)", point2.ID.ToString());
            newPoint2.transform.parent = parent;

            //��������ê�������
            newPoint1.transform.rotation = newPoint2.transform.rotation = Quaternion.LookRotation(parent.forward);
            newPoint1.transform.position = transform.position + (transform.forward * 1.5f);//��ǰ��ķ����������ã�
            newPoint2.transform.position = newPoint1.transform.position + (transform.forward * 1.5f);//�ӵ�һ��ê�㿪ʼ��ǰ��ķ����������ã�
            point1.InTangent.Translate(Vector3.up * .5f);
            point1.OutTangent.Translate(Vector3.up * -.5f);
            point2.InTangent.Translate(Vector3.up * .5f);
            point2.OutTangent.Translate(Vector3.up * -.5f);


            //����������ê��
            point1.AddConnectedPoint(point2);
            point2.AddConnectedPoint(point1);
            
        }


        public void Addline(Line line)
        {
            if (line == null)
                return;

            if (!lines.Contains(line))
                lines.Add(line);
        }

        public void ClearLines()
        {
            lines.Clear();
        }



        /// <summary>
        /// ˢ��lines�б���ê��Ϊnull������ɾȥ
        /// </summary>
        public void RefreshLinesList()
        {
            if (lines == null || lines.Count == 0)
                return;

            for (int i = 0; i < lines.Count; i++)
            {
                //��ê��Ϊnull������ɾȥ
                if (lines[i].point1 == null || lines[i].point2 == null)
                    lines.RemoveAt(i);
            }


        }


        /// <summary>
        /// ����������Ϣ
        /// </summary>
        public void ConfigureLineInformation()
        {
            //û��ê���ȫ�����
            if (Points.Count == 0)
            {
                lines.Clear();
                return;
            }

            RefreshLinesList();
            List<Line> newlines = lines;


            //����ÿһ��ê��
            Points.ForEach(p =>
            {
                p.RefreshConnectedPointsList();
                //����ÿһ��ê���й����ĵ�ê��
                p.connectedPoints.ForEach(cp =>
                {
                    if (IsNotContainedInLineList(p, cp, newlines))
                    {
                        Line newLine = new Line();
                        newLine.index = newlines.Count + 1;
                        newLine.lineType = LineType.Bezier;
                        newLine.point1 = p;
                        newLine.point2 = cp;
                        newLine.color = Color.yellow;
                        newLine.lineWidth = 2f;
                        if (newLine.sourceControl == null)
                        {
                            newLine.sourceControl = newLine.point1.OutTangent.GetComponent<SplineTangent>();
                        }
                        if (newLine.destinationControl == null)
                        {
                            newLine.destinationControl = newLine.point2.InTangent.GetComponent<SplineTangent>();
                        }
                        newlines.Add(newLine);
                    }

                });



            });


            lines = newlines;


        }



        /// <summary>
        /// �ж�ָ����SplinePoint�б����Ƿ����������ê��
        /// </summary>
        /// <param name="point"></param>
        /// <param name="points"></param>
        /// <returns></returns>
        public bool IsContainedInPonitList(SplinePoint point, List<SplinePoint> points)
        {
            if (point == null || points == null || points.Count == 0)
                return false;

            if (points.Contains(point))
                return true;

            return false;
        }


        /// <summary>
        /// �ж�ָ����Line�б����Ƿ񲻰��������ö�ê��
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <param name="lines"></param>
        /// <returns>true��˵������������Ҫ��ӣ�false��˵���������������</returns>
        public bool IsNotContainedInLineList(SplinePoint point1, SplinePoint point2, List<Line> lines)
        {

            if (lines == null || point1 == null || point2 == null)
                return false;

            if (lines.Count == 0)//���б�Ϊ�գ�ֱ����Ӽ���
                return true;

            //����ÿһ����
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].point1 == null || lines[i].point2 == null)
                {
                    lines.RemoveAt(i);
                    return true;
                }


                //���Ѵ�����ͬ��һ�Կ�����ê��
                if ((lines[i].point1 == point1 && lines[i].point2 == point2) || (lines[i].point1 == point2 && lines[i].point2 == point1))
                    return false;

            }


            return true;
        }

        /// <summary>
        /// �ж�ָ����Line�б����Ƿ���������ö�ê��
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <param name="lines"></param>
        /// <returns></returns>
        public bool IsContainedInLineList(SplinePoint point1, SplinePoint point2, List<Line> lines)
        {
            return !IsNotContainedInLineList(point1, point2, lines);
        }


        /// <summary>
        /// ͨ������ê���ȡ��������
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <param name="lines"></param>
        /// <returns></returns>
        public LineType? GetLineTypeByPoints(SplinePoint point1, SplinePoint point2, List<Line> lines)
        {
            //����ÿһ����
            for (int i = 0; i < lines.Count; i++)
            {
                //���Ѵ�����ͬ��һ�Կ�����ê��
                if ((lines[i].point1 == point1 && lines[i].point2 == point2) || (lines[i].point1 == point2 && lines[i].point2 == point1))
                    return lines[i].lineType;

            }

            return null;
        }


        /// <summary>
        /// ͨ������ê���ȡ������
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <param name="lines"></param>
        /// <returns></returns>
        public Line GetLineByPoints(SplinePoint point1, SplinePoint point2, List<Line> lines)
        {
            //Debug.Log(lines.Count);
            //����ÿһ����
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].point1 == point1 && lines[i].point2 == point2)
                {
                    driveAgainst = false;
                }
                if (lines[i].point1 == point2 && lines[i].point2 == point1)
                {
                    driveAgainst = true;
                }
                //���Ѵ�����ͬ��һ�Կ�����ê��
                if ((lines[i].point1 == point1 && lines[i].point2 == point2) || (lines[i].point1 == point2 && lines[i].point2 == point1))
                {
                    return lines[i];
                }

            }
            return null;
        }


    }

}
