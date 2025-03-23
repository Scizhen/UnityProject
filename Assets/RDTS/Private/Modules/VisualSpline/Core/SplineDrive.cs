using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace VisualSpline
{

    /// <summary>
    /// 样条线驱动器
    /// </summary>
    public class SplineDrive : MonoBehaviour
    {

        [System.Serializable]
        public class AxisRotate
        {
            public bool isRotate = false;//是否沿着线条前进方向驱动
            public bool yIsUp = true;//y轴是向上还是向下
        }


        public Spline associatedSpline;//关联的样条线
        public List<SplinePoint> motionPath = new List<SplinePoint>();//运动路径的锚点


        [Header("Drive Status")]
        [HideInInspector]
        [Range(0, 1)]
        public float percentage = 0f;//已弃用
        public LineDetail currentLine;//当前正在驱动的线段

        [Header("Drive Control")]
        public bool isDrive = false;//是否进行驱动
        public bool isAutomaticallyNext = true;//是否自动向下一个点驱动
        public bool isReverse = false;//是否反方向驱动
        public bool isLoop = false;//是否循环驱动
        [HideInInspector] public bool specialCaseHandling = false;//是否开启特殊情况处理


        [Range(0, 100)]
        public float speed = 10f;//速度
        private int _scale = 100;//换算比例（speed / _scale 在 0—1之间，即所占百分比）


        [Header("Direction Setting")]
        public AxisRotate axisRotate;//有关驱动时坐标轴的方向设置
        public bool rotateWithPoint = false;//是否随锚点旋转。若开启，则会关闭axisRotate的设置，驱动对象在线条上运动时的旋转姿态会逐渐趋近于结束点直至对齐


        [Header("Special Case")]
        public bool isP2P = false;//是否开启“点到点”的驱动模式，该模式下驱动的路径只与currentLine有关，而与motionPath无关

        private int _slicesPerLine = 100;//每条线段的切片，数量越多越精细准确。这里将样条线中的每条线段切分成100段
        private int _totalslices//总共的切片数量
        {
            get
            {
                return _slicesPerLine * (motionPath.Count - 1);
            }
        }

        Coroutine c;
        private bool _driveInSplineStart = false;
        private bool _driveInSplineFinish = false;



        // Start is called before the first frame update
        void Start()
        {

            if (!isP2P && associatedSpline != null && motionPath.Count > 1 && associatedSpline.IsContainedInPonitList(motionPath[0], associatedSpline.Points) && motionPath[0] != null && motionPath[1] != null)
            {
                //是否需要一开始就将对象定位到轨迹的初始点
                if (isDrive)
                {
                    //设置初始位置
                    this.transform.position = GetPositionInBezierCurves(motionPath[0], motionPath[1], 0);
                    //设置初始朝向
                    if (rotateWithPoint) this.transform.rotation = motionPath[0].transform.rotation;
                    if (axisRotate.isRotate && !rotateWithPoint)
                        this.transform.LookAt(this.transform.position + GetDirectionInBezierCurves(motionPath[0], motionPath[1], 0));
                }


                this.currentLine.index = 1;
                this.currentLine.percentage = 0;
                this.currentLine.SetPoints(motionPath[0], motionPath[1]);

            }

        }

        // Update is called once per frame
        void Update()
        {
            if (associatedSpline != null)
            {
                if (!isP2P)//在P2P模式下，无需依赖于motionPath
                {
                    if (motionPath == null || motionPath.Count == 0)
                        return;

                    if (motionPath.Count == 1)
                    {
                        if (isDrive)
                        {
                            this.transform.position = motionPath[0].transform.position;
                            if (axisRotate.isRotate || rotateWithPoint) this.transform.rotation = motionPath[0].transform.rotation;
                        }

                        return;
                    }

                    //在样条线上的驱动情况
                    DriveInSpline();

                }
                else//特殊情况处理
                {
                    SpecialCaseDeal();
                }

                //在驱动状态下才能计算驱动的百分比
                if (isDrive)
                {
                    //是否反向驱动
                    if (!isReverse)
                        currentLine.percentage += (speed * Time.deltaTime / 100);
                    else
                        currentLine.percentage -= (speed * Time.deltaTime / 100);
                }


                //限幅
                if (currentLine.percentage < 0) currentLine.percentage = 0;
                if (currentLine.percentage > 1) currentLine.percentage = 1;

                //根据百分比驱动
                UpdateTransform(currentLine.percentage);
            }

        }


        /// <summary>
        /// 刷新运动路径的锚点列表，将为空的锚点删去
        /// </summary>
        public void RefreshMotionPathList()
        {
            if (motionPath == null || motionPath.Count == 0)
                return;

            for (int i = 0; i < motionPath.Count; i++)
            {
                //将为null的锚点删去
                if (motionPath[i] == null)
                    motionPath.RemoveAt(i);
            }
        }



        /// <summary>
        /// 特殊情况处理
        /// </summary>
        public void SpecialCaseDeal()
        {
            //置位相关标志位，避免驱动出错
            isAutomaticallyNext = false;
            isReverse = false;
            isLoop = false;
            currentLine.index = 0;

            //当前起始点不存在、但结束点存在，则从当前点移动到结束点
            if (currentLine.startPoint == null && currentLine.endPoint != null)
            {

                if (isDrive)
                {
                    //从当前位姿驱动至结束点
                    DriveFromNowToPoint(currentLine.endPoint);
                    //计算当前点和结束点的直线距离
                    float distance = Vector3.Distance(this.transform.position, currentLine.endPoint.transform.position);
                    //当直线距离小于0.001时认为到达（注意，不能直接用两点的坐标相等来判断，因为驱动时存在0.00001级的误差）
                    if (Mathf.Abs(distance) < 0.001)
                        currentLine.isArrived = true;
                    else
                        currentLine.isArrived = false;

                }


            }

            //若起始点和结束点为同一个点，则默认直接到达
            if (currentLine.startPoint == currentLine.endPoint && isDrive)
            {
                currentLine.isArrived = true;
                currentLine.percentage = 1;
            }

            //在驱动状态下，比例为1时认为已到达
            if (currentLine.percentage == 1 && isDrive)
            {
                currentLine.isArrived = true;
            }

            //在驱动状态下，比例在0~1间时驱动对象必没有到达某个点
            if (currentLine.percentage > 0 && currentLine.percentage < 1 && isDrive)
            {
                currentLine.isArrived = false;
            }



        }


        void DriveInSpline()
        {
            if (!isDrive)
                return;

            ///更新在运动轨迹上的驱动状态
            //是否自动沿下一条线段驱动
            if (isAutomaticallyNext)
            {
                //正向驱动至下一个点
                if (!isReverse && currentLine.percentage == 1 && currentLine.index < motionPath.Count - 1)
                {
                    currentLine.index++;
                    currentLine.isArrived = true;
                    currentLine.percentage = 0;
                    currentLine.SetPoints(motionPath[currentLine.index - 1], motionPath[currentLine.index]);
                }

                //反向驱动至上一个点
                if (isReverse && currentLine.percentage == 0 && currentLine.index > 1)
                {
                    currentLine.index--;
                    currentLine.isArrived = true;
                    currentLine.percentage = 1;
                    currentLine.SetPoints(motionPath[currentLine.index - 1], motionPath[currentLine.index]);
                }


                if (currentLine.percentage > 0 && currentLine.percentage < 1)
                {
                    currentLine.isArrived = false;
                }


            }


            //是否循环驱动
            if (isLoop && isAutomaticallyNext)
            {
                //正向驱动的循环重置位
                if (this.currentLine.index == motionPath.Count - 1 && currentLine.percentage == 1 && !isReverse)
                {
                    this.currentLine.index = 1;
                    this.currentLine.percentage = 0;
                    this.currentLine.SetPoints(motionPath[0], motionPath[1]);
                }

                //反向驱动的循环重置位
                if (this.currentLine.index == 1 && currentLine.percentage == 0 && isReverse)
                {
                    this.currentLine.index = motionPath.Count - 1;
                    this.currentLine.percentage = 1;
                    this.currentLine.SetPoints(motionPath[motionPath.Count - 2], motionPath[motionPath.Count - 1]);
                }

            }



        }


        /// <summary>
        /// 协程：以一定速率自动控制percentage值得变化，以此实现对象在样条线上驱动
        /// </summary>
        /// <param name="speed"></param>
        /// <returns></returns>
        public IEnumerator DriveInSpline(float speed = 10f / 100f)
        {
            float startTime = Time.time;
            float length = 1;
            float frac = 0;
            while (frac < 1.0f)
            {
                float dist = (Time.time - startTime) * speed;
                frac = dist / length;
                if (!isReverse)
                    percentage = Mathf.Lerp(0, 1, frac);
                else
                    percentage = Mathf.Lerp(1, 0, frac);

                if (!isReverse && percentage == 1)
                {
                    _driveInSplineFinish = true;
                }
                if (isReverse && percentage == 0)
                {
                    _driveInSplineFinish = true;
                }

                yield return null;
            }
        }


        /// <summary>
        /// 根据百分比在指定的样条线上运动
        /// </summary>
        /// <param name="percentage"></param>
        public void UpdateTransform(float percentage)
        {
            if (!isDrive)
                return;

            //当startPoint不存在或endPoint不存在时
            if (currentLine.startPoint == null || currentLine.endPoint == null)
                return;


            LineType lineType = LineType.Straight;//线段类型为空，就默认直线
            // LineDetail lineDetail = CalculatePercentageInCurrentLine(percentage);
            // //刷新Spline脚本中的线条列表
            // associatedSpline.RefreshLinesList();
            // //获取线段类型
            // Line line = associatedSpline.GetLineByPoints(lineDetail.startPoint, lineDetail.endPoint, associatedSpline.lines);

            LineDetail lineDetail = this.currentLine;
            //刷新Spline脚本中的线条列表
            associatedSpline.RefreshLinesList();
            //获取线段类型
            Line line = associatedSpline.GetLineByPoints(lineDetail.startPoint, lineDetail.endPoint, associatedSpline.lines);

            //线段是否存在
            if (line != null)
            {
                currentLine.isExist = true;
                lineType = line.lineType;
            }
            else
            {
                currentLine.isExist = false;
                //线段不存在（当前走的线段不包含于associatedSpline的lines列表中）时，会默认绘制红色直线并沿直线驱动
                Debug.DrawLine(this.transform.position, lineDetail.endPoint.transform.position, Color.red, 0, true);//若DisplayStatus为true，绘制红色射线
            }


            switch (lineType)
            {
                case LineType.Bezier:
                    //UpdatePosition 更新位置
                    this.transform.position = GetPositionInBezierCurves(lineDetail.startPoint, lineDetail.endPoint, lineDetail.percentage);

                    //UpdateOrientation 更新朝向
                    if (axisRotate.isRotate)
                    {
                        this.transform.LookAt(this.transform.position + GetDirectionInBezierCurves(lineDetail.startPoint, lineDetail.endPoint, lineDetail.percentage),
                        (axisRotate.yIsUp) ? Vector3.up : Vector3.down);
                    }



                    break;
                case LineType.Straight:
                    //UpdatePosition 更新位置
                    this.transform.position = GetPositionInStraightLine(lineDetail.startPoint, lineDetail.endPoint, lineDetail.percentage);

                    //UpdateOrientation 更新朝向
                    if (axisRotate.isRotate)
                        this.transform.LookAt(this.transform.position + GetDirectionInStraightLine(lineDetail.startPoint, lineDetail.endPoint),
                        (axisRotate.yIsUp) ? Vector3.up : Vector3.down);

                    break;
            }

            //更新rotation
            if (rotateWithPoint)
            {

                if (!isReverse)
                {
                    float delta_angle = Quaternion.Angle(this.transform.rotation, lineDetail.endPoint.transform.rotation);
                    this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation,
                        lineDetail.endPoint.transform.rotation,
                        //delta_angle*percentage//(speed * Time.deltaTime / 100)
                        delta_angle / (1 - percentage) * (speed * Time.deltaTime / 100)//按帧率（帧数）计算
                    );
                }
                else
                {
                    float delta_angle = Quaternion.Angle(this.transform.rotation, lineDetail.startPoint.transform.rotation);
                    this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation,
                        lineDetail.startPoint.transform.rotation,
                        //delta_angle*percentage//(speed * Time.deltaTime / 100)
                        delta_angle / percentage * (speed * Time.deltaTime / 100)//按帧率计算
                    );
                }

                //关闭其他有关方向调整的方法
                axisRotate.isRotate = false;

            }


        }


        /// <summary>
        /// 计算给定的percentage值对应于那一段线条，并获取相关信息
        /// </summary>
        /// <param name="percentage"></param>
        /// <returns></returns>
        public LineDetail CalculatePercentageInWhichLine(float percentage)
        {
            RefreshMotionPathList();

            int indexOfLine = 0;
            if (percentage == 1)
                indexOfLine = (int)(percentage * (motionPath.Count - 1)) - 1;
            else
                indexOfLine = (int)(percentage * (motionPath.Count - 1));
            SplinePoint startPoint = motionPath[indexOfLine];
            SplinePoint endPoint = motionPath[indexOfLine + 1];
            float percentageInCurrentLine = percentage * _totalslices / _slicesPerLine - indexOfLine;

            return new LineDetail(indexOfLine, percentageInCurrentLine, startPoint, endPoint);
        }



        /// <summary>
        /// 在当前线段上的线条信息
        /// </summary>
        /// <param name="percentage"></param>
        /// <returns></returns>
        public LineDetail CalculatePercentageInCurrentLine(float percentage)
        {
            int indexOfLine = 0;
            SplinePoint startPoint = this.currentLine.startPoint;
            SplinePoint endPoint = this.currentLine.endPoint;

            return new LineDetail(indexOfLine, percentage, startPoint, endPoint);
        }


        /// <summary>
        /// 根据百分比从贝塞尔曲线中获取该值的方向
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        /// <param name="percentage"></param>
        /// <param name="normalized"></param>
        /// <returns></returns>
        public Vector3 GetDirectionInBezierCurves(SplinePoint startPoint, SplinePoint endPoint, float percentage, bool normalized = true)
        {
            //获取一阶导数
            return BezierCurves.GetFirstDerivative(startPoint.Point.position, endPoint.Point.position, startPoint.OutTangent.position, endPoint.InTangent.position, percentage).normalized;

        }

        /// <summary>
        /// 根据百分比从贝塞尔曲线中获取该值的位置
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        /// <param name="percentage"></param>
        /// <param name="normalized"></param>
        /// <returns></returns>
        public Vector3 GetPositionInBezierCurves(SplinePoint startPoint, SplinePoint endPoint, float percentage, bool normalized = true)
        {
            return BezierCurves.GetPoint(startPoint.Point.position, endPoint.Point.position, startPoint.OutTangent.position, endPoint.InTangent.position, percentage, true, 100);
        }

        /// <summary>
        /// 根据百分比从直线中获取该值的方向
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        /// <param name="normalized"></param>
        /// <returns></returns>
        public Vector3 GetDirectionInStraightLine(SplinePoint startPoint, SplinePoint endPoint, bool normalized = true)
        {
            return StraightLines.GetDirection(startPoint.Point.position, endPoint.Point.position).normalized;

        }

        /// <summary>
        /// 根据百分比从直线中获取该值的位置
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        /// <param name="percentage"></param>
        /// <param name="normalized"></param>
        /// <returns></returns>
        public Vector3 GetPositionInStraightLine(SplinePoint startPoint, SplinePoint endPoint, float percentage, bool normalized = true)
        {
            return StraightLines.GetPoint(startPoint.Point.position, endPoint.Point.position, percentage);
        }




        /// <summary>
        /// 用某个轴去朝向物体
        /// </summary>
        /// <param name="tr_self">朝向的本体</param>
        /// <param name="lookPos">朝向的目标</param>
        /// <param name="directionAxis">方向轴，取决于你用那个方向去朝向</param>
        void AxisLookAt(Transform tr_self, Vector3 lookPos, Vector3 directionAxis)
        {
            var rotation = tr_self.rotation;
            var targetDir = lookPos - tr_self.position;
            //指定哪根轴朝向目标,自行修改Vector3的方向
            var fromDir = tr_self.rotation * directionAxis;
            //计算垂直于当前方向和目标方向的轴
            var axis = Vector3.Cross(fromDir, targetDir).normalized;
            //计算当前方向和目标方向的夹角
            var angle = Vector3.Angle(fromDir, targetDir);
            //将当前朝向向目标方向旋转一定角度，这个角度值可以做插值
            tr_self.rotation = Quaternion.AngleAxis(angle, axis) * rotation;
            tr_self.localEulerAngles = new Vector3(0, tr_self.localEulerAngles.y, 90);//后来调试增加的，因为我想让x，z轴向不会有任何变化
        }


        /// <summary>
        /// 从当前位姿驱动至一个锚点
        /// </summary>
        public float DriveFromNowToPoint(SplinePoint target, float speedMultiplier = 1f)
        {

            float distance = Vector3.Distance(this.transform.position, target.transform.position);
            //float _speed = distance
            //按帧率计算
            this.transform.position = Vector3.MoveTowards(this.transform.position, target.transform.position, distance / (1 - currentLine.percentage) * (speed * Time.deltaTime / 100) * speedMultiplier);


            float delta_angle = Quaternion.Angle(this.transform.rotation, target.transform.rotation);
            this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation,
                target.transform.rotation,
                delta_angle / (1 - currentLine.percentage) * (speed * Time.deltaTime / 100) * speedMultiplier//按帧率计算
            );

            //绘制红色直线并沿直线驱动
            Debug.DrawLine(this.transform.position, target.transform.position, Color.red, 0, true);//若DisplayStatus为true，绘制红色射线

            return distance;
        }






    }


}