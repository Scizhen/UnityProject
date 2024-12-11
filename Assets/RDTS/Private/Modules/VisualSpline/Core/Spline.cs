using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VisualSpline
{
    /// <summary>线条信息，根据此信息来绘制一段线条</summary>
    [Serializable]
    public class LineComposition
    {
        public int index = -1;//索引号
        public SplinePoint startPoint;//起始点
        public SplinePoint endPoint;//结束点
        public LineType lineType;//线条类型
    }
    /// <summary>线条构成，可构成线段关系的一对点</summary>
    [Serializable]
    public class Line
    {
        public int index = -1;//索引号
        public SplinePoint point1;//锚点1
        public SplinePoint point2;//锚点2
        public bool reverse;//绘制时是否调换point1和point2的顺序
        public LineType lineType;//线条类型
        public Color color;//线条颜色
        public float lineWidth;//线条宽度
        public SplineTangent sourceControl;
        public SplineTangent destinationControl ;

    }


    /// <summary>线条类型，可选择直线或贝塞尔曲线绘制某段线条 </summary>
    public enum LineType { Straight, Bezier }

    /// <summary>空间维度，可选择样条线为三维或二维 </summary>
    public enum SpatialDimension { ThreeDimensional, TwoDimensional }


    [ExecuteInEditMode]
    public class Spline : MonoBehaviour
    {
        public event Action OnSplineChanged;//事件（暂未用）

        public SpatialDimension spatialDimension = SpatialDimension.ThreeDimensional;
        public Color color = Color.yellow;//样条线颜色
        [Range(0, 1)] public float toolScale = .1f;//锚点缩放大小
        //public bool loop;//是否循环（闭合）
        public int countOfPoints = 0;//记录已有的锚点数量
        [HideInInspector] public bool automaticallyAddIfNull = false;//是否在没有线条时，自动添加
        public List<Line> lines = new List<Line>();//样条线中的所有线段信息，根据此列表绘制完整的样条线

        protected Dictionary<int, SplinePoint> PointPerID = new Dictionary<int, SplinePoint>();//存储该样条线下的锚点，通过ID号检索
        public bool driveAgainst = false;

        /// <summary>样条线的总长度 </summary>
        public float Length
        {
            get;
            private set;
        }


        private SpatialDimension lastSpatialDimension;
        private bool _wasLooping;
        private int _previousPointCount;
        private bool _lengthDirty = true;//样条线的长度是否发生变化



        List<SplinePoint> _points;//记录有几个锚点
        public List<SplinePoint> Points//记录该样条线下所有的锚点
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

            //创建第一个新的锚点
            GameObject newPoint1 = Instantiate<GameObject>(pointTemplate);
            SplinePoint point1 = newPoint1.GetComponent<SplinePoint>();
            point1.parentSpline = this;//关联父Spline
            point1.ID = this.Points.Count + 1;//设置ID
            newPoint1.name = newPoint1.name.Replace("(Clone)", point1.ID.ToString());
            newPoint1.transform.parent = parent;//设置transform到父对象下

            //创建第二个新的锚点
            GameObject newPoint2 = Instantiate<GameObject>(pointTemplate);
            SplinePoint point2 = newPoint2.GetComponent<SplinePoint>();
            point2.parentSpline = this;//关联父Spline
            point2.ID = this.Points.Count + 1;//设置ID
            newPoint2.name = newPoint2.name.Replace("(Clone)", point2.ID.ToString());
            newPoint2.transform.parent = parent;

            //设置两个锚点的属性
            newPoint1.transform.rotation = newPoint2.transform.rotation = Quaternion.LookRotation(parent.forward);
            newPoint1.transform.position = transform.position + (transform.forward * 1.5f);//以前向的方向与距离放置：
            newPoint2.transform.position = newPoint1.transform.position + (transform.forward * 1.5f);//从第一个锚点开始以前向的方向与距离放置：
            point1.InTangent.Translate(Vector3.up * .5f);
            point1.OutTangent.Translate(Vector3.up * -.5f);
            point2.InTangent.Translate(Vector3.up * .5f);
            point2.OutTangent.Translate(Vector3.up * -.5f);


            //连接这两个锚点
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
        /// 刷新lines列表，将锚点为null的线条删去
        /// </summary>
        public void RefreshLinesList()
        {
            if (lines == null || lines.Count == 0)
                return;

            for (int i = 0; i < lines.Count; i++)
            {
                //将锚点为null的线条删去
                if (lines[i].point1 == null || lines[i].point2 == null)
                    lines.RemoveAt(i);
            }


        }


        /// <summary>
        /// 配置线条信息
        /// </summary>
        public void ConfigureLineInformation()
        {
            //没有锚点就全部清空
            if (Points.Count == 0)
            {
                lines.Clear();
                return;
            }

            RefreshLinesList();
            List<Line> newlines = lines;


            //遍历每一个锚点
            Points.ForEach(p =>
            {
                p.RefreshConnectedPointsList();
                //遍历每一个锚点中关联的的锚点
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
        /// 判断指定的SplinePoint列表中是否包含给定的锚点
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
        /// 判断指定的Line列表中是否不包含给定该对锚点
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <param name="lines"></param>
        /// <returns>true，说明不包含，需要添加；false，说明包含，无需添加</returns>
        public bool IsNotContainedInLineList(SplinePoint point1, SplinePoint point2, List<Line> lines)
        {

            if (lines == null || point1 == null || point2 == null)
                return false;

            if (lines.Count == 0)//若列表为空，直接添加即可
                return true;

            //遍历每一条线
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].point1 == null || lines[i].point2 == null)
                {
                    lines.RemoveAt(i);
                    return true;
                }


                //若已存在相同的一对可连接锚点
                if ((lines[i].point1 == point1 && lines[i].point2 == point2) || (lines[i].point1 == point2 && lines[i].point2 == point1))
                    return false;

            }


            return true;
        }

        /// <summary>
        /// 判断指定的Line列表中是否包含给定该对锚点
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
        /// 通过两个锚点获取线条类型
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <param name="lines"></param>
        /// <returns></returns>
        public LineType? GetLineTypeByPoints(SplinePoint point1, SplinePoint point2, List<Line> lines)
        {
            //遍历每一条线
            for (int i = 0; i < lines.Count; i++)
            {
                //若已存在相同的一对可连接锚点
                if ((lines[i].point1 == point1 && lines[i].point2 == point2) || (lines[i].point1 == point2 && lines[i].point2 == point1))
                    return lines[i].lineType;

            }

            return null;
        }


        /// <summary>
        /// 通过两个锚点获取线条类
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <param name="lines"></param>
        /// <returns></returns>
        public Line GetLineByPoints(SplinePoint point1, SplinePoint point2, List<Line> lines)
        {
            //Debug.Log(lines.Count);
            //遍历每一条线
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
                //若已存在相同的一对可连接锚点
                if ((lines[i].point1 == point1 && lines[i].point2 == point2) || (lines[i].point1 == point2 && lines[i].point2 == point1))
                {
                    return lines[i];
                }

            }
            return null;
        }


    }

}
