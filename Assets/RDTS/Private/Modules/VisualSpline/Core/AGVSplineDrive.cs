using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;



namespace VisualSpline
{

    /// <summary>
    /// 样条线驱动器
    /// </summary>
    public class AGVSplineDrive : MonoBehaviour
    {

        [System.Serializable]
        public class AxisRotate
        {
            public bool isRotate = true;//是否沿着线条前进方向驱动
            public bool yIsUp = true;//y轴是向上还是向下
        }


        public Spline associatedSpline;//关联的样条线
        public List<SplinePoint> motionPath = new List<SplinePoint>();//运动路径的锚点

        [Foldout(("Settings"))] public float Distance = 0.5f;
        [Foldout(("Settings"))] public float DistanceSides = 0.5f;
        [Foldout(("Settings"))] public float AngleSide = 30f;
        [Foldout(("Settings"))] public bool DrawRay = true;

        public LineDetail currentLine;//当前正在驱动的线段

        [Header("Drive Control")]
        public bool isDrive = false;//是否进行驱动
        public bool isAutomaticallyNext = true;//是否自动向下一个点驱动
        public bool isReverse = false;//是否反方向驱动
        public bool isLoop = false;//是否循环驱动
        [HideInInspector] public bool specialCaseHandling = false;//是否开启特殊情况处理


        public float speed = 10f;//速度
        private int _scale = 100;//换算比例（speed / _scale 在 0—1之间，即所占百分比）
        public float acceleration = 1f;//速度

        [Header("Direction Setting")]
        private AxisRotate axisRotate = new AxisRotate();//有关驱动时坐标轴的方向设置
        private bool rotateWithPoint = false;//是否随锚点旋转。若开启，则会关闭axisRotate的设置，驱动对象在线条上运动时的旋转姿态会逐渐趋近于结束点直至对齐

        private Vector3 raydirection;
        private Vector3 pathdirection;
        private int raycastlayermask;


        // Start is called before the first frame update
        void Start()
        {
            axisRotate.isRotate = true;
            axisRotate.yIsUp = true;
            raycastlayermask = 1 << this.gameObject.layer;

            if ( associatedSpline != null && motionPath.Count > 1 && associatedSpline.IsContainedInPonitList(motionPath[0], associatedSpline.Points) && motionPath[0] != null && motionPath[1] != null)
            {
                //是否需要一开始就将对象定位到轨迹的初始点
                if (isDrive)
                {
                    Line line = associatedSpline.GetLineByPoints(motionPath[0], motionPath[1], associatedSpline.lines);
                    //设置初始位置
                    this.transform.position = GetPositionInBezierCurves(line,motionPath[0], motionPath[1], 0);
                    //设置初始朝向
                    if (rotateWithPoint) this.transform.rotation = motionPath[0].transform.rotation;
                    if (axisRotate.isRotate && !rotateWithPoint)
                        this.transform.LookAt(this.transform.position + GetDirectionInBezierCurves(line,motionPath[0], motionPath[1], 0));
                }


                this.currentLine.index = 1;
                this.currentLine.percentage = 0;
                this.currentLine.SetPoints(motionPath[0], motionPath[1]);

            }

        }

        // Update is called once per frame
        void Update()
        {
            CheckCollission();
            if (associatedSpline != null)
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


                //else//特殊情况处理
                //{
                //    SpecialCaseDeal();
                //}

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
                    this.transform.position = GetPositionInBezierCurves(line,lineDetail.startPoint, lineDetail.endPoint, lineDetail.percentage);
                    //UpdateOrientation 更新朝向
                    if (axisRotate.isRotate)
                    {
                        if (percentage != 1)
                        {
                            pathdirection = GetDirectionInBezierCurves(line, lineDetail.startPoint, lineDetail.endPoint, lineDetail.percentage);
                            this.transform.LookAt(this.transform.position + GetDirectionInBezierCurves(line, lineDetail.startPoint, lineDetail.endPoint, lineDetail.percentage),
                            (axisRotate.yIsUp) ? Vector3.up : Vector3.down);
                        }
                        else 
                        {
                            this.transform.eulerAngles = new Vector3(this.transform.eulerAngles.x, 90, this.transform.eulerAngles.z);
                        }
                    }



                    break;
                case LineType.Straight:
                    //UpdatePosition 更新位置
                    this.transform.position = GetPositionInStraightLine(lineDetail.startPoint, lineDetail.endPoint, lineDetail.percentage);

                    //UpdateOrientation 更新朝向
                    if (axisRotate.isRotate)
                    {
                        if (percentage != 1)
                        {
                            pathdirection = GetDirectionInStraightLine(lineDetail.startPoint, lineDetail.endPoint);
                            this.transform.LookAt(this.transform.position + GetDirectionInStraightLine(lineDetail.startPoint, lineDetail.endPoint),
                            (axisRotate.yIsUp) ? Vector3.up : Vector3.down);
                        }
                        else
                        {
                            this.transform.eulerAngles = new Vector3(this.transform.eulerAngles.x, 90, this.transform.eulerAngles.z);
                        }
                    }


                    break;
            }

            //更新rotation
            if (rotateWithPoint)
            {
                if (percentage != 1)
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

                }
                else
                {
                    this.transform.eulerAngles = new Vector3(this.transform.eulerAngles.x,90,this.transform.eulerAngles.z);
                }

                //关闭其他有关方向调整的方法
                axisRotate.isRotate = false;

            }


        }


        /// <summary>
        /// 根据百分比从贝塞尔曲线中获取该值的方向
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        /// <param name="percentage"></param>
        /// <param name="normalized"></param>
        /// <returns></returns>
        public Vector3 GetDirectionInBezierCurves(Line line, SplinePoint startPoint, SplinePoint endPoint, float percentage, bool normalized = true)
        {
            //获取一阶导数

            if (associatedSpline.driveAgainst == false)//是否为反向走
            {
                return BezierCurves.GetFirstDerivative(startPoint.Point.position, endPoint.Point.position, line.sourceControl.transform.position, line.destinationControl.transform.position, percentage).normalized;
            }
            else
            {
                return -1*BezierCurves.GetFirstDerivative(endPoint.Point.position, startPoint.Point.position, line.sourceControl.transform.position, line.destinationControl.transform.position, 1 - percentage).normalized;
            }
        }

        /// <summary>
        /// 根据百分比从贝塞尔曲线中获取该值的位置
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        /// <param name="percentage"></param>
        /// <param name="normalized"></param>
        /// <returns></returns>
        public Vector3 GetPositionInBezierCurves(Line line,SplinePoint startPoint, SplinePoint endPoint, float percentage, bool normalized = true)
        {
            if (associatedSpline.driveAgainst == false)//是否为反向走
            {
                return BezierCurves.GetPoint(startPoint.Point.position, endPoint.Point.position, line.sourceControl.transform.position, line.destinationControl.transform.position, percentage, true, 100);
            }
            else
            {
                return BezierCurves.GetPoint(endPoint.Point.position, startPoint.Point.position, line.sourceControl.transform.position, line.destinationControl.transform.position, 1-percentage, true, 100);
            }
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

        private bool CheckCollission()
        {
            raydirection = pathdirection;
            var colorok = Color.yellow;
            var colorcollide = Color.red;
            var color = colorok;
            var collission = Physics.Raycast(transform.position, raydirection, Distance, raycastlayermask);
            //向场景中的所有碰撞体投射一条射线，该射线起点为 /origin/，朝向 /direction/，长度为 /maxDistance/。如果射线与任何碰撞体相交，返回 true，否则返回 false。
            //raycastlayermask 层遮罩，用于在投射射线时有选择地忽略碰撞体。该脚本代码放置的对象与检测的对象处于同一Layer下才能检测成功
            if (collission)
                color = colorcollide;
            if (DrawRay)
                Debug.DrawRay(transform.position, raydirection * Distance, color);

            if (DistanceSides > 0 && !collission)
            {
                var ray1 = Quaternion.AngleAxis(-AngleSide, Vector3.up) * raydirection;//俩条斜射线判断
                var ray2 = Quaternion.AngleAxis(AngleSide, Vector3.up) * raydirection;

                collission = Physics.Raycast(transform.position, ray1, DistanceSides, raycastlayermask);
                if (collission)
                    color = colorcollide;
                if (DrawRay)
                    Debug.DrawRay(transform.position, ray1 * DistanceSides, color);
                color = colorok;
                if (!collission)
                {
                    color = colorok;
                    collission = Physics.Raycast(transform.position, ray2, DistanceSides, raycastlayermask);
                    if (collission)
                        color = colorcollide;
                    if (DrawRay)
                        Debug.DrawRay(transform.position, ray2 * DistanceSides, color);
                }
            }

            //if (collission)
            //    isDrive = false;//判断有无遮挡
            //if (!collission )//&& IsBlocked)
            //    isDrive = true;
            return collission;
        }






    }


}