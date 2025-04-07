using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;



namespace VisualSpline
{

    /// <summary>
    /// ������������
    /// </summary>
    public class AGVSplineDrive : MonoBehaviour
    {

        [System.Serializable]
        public class AxisRotate
        {
            public bool isRotate = true;//�Ƿ���������ǰ����������
            public bool yIsUp = true;//y�������ϻ�������
        }


        public Spline associatedSpline;//������������
        public List<SplinePoint> motionPath = new List<SplinePoint>();//�˶�·����ê��

        [Foldout(("Settings"))] public float Distance = 0.5f;
        [Foldout(("Settings"))] public float DistanceSides = 0.5f;
        [Foldout(("Settings"))] public float AngleSide = 30f;
        [Foldout(("Settings"))] public bool DrawRay = true;

        public LineDetail currentLine;//��ǰ�����������߶�

        [Header("Drive Control")]
        public bool isDrive = false;//�Ƿ��������
        public bool isAutomaticallyNext = true;//�Ƿ��Զ�����һ��������
        public bool isReverse = false;//�Ƿ񷴷�������
        public bool isLoop = false;//�Ƿ�ѭ������
        [HideInInspector] public bool specialCaseHandling = false;//�Ƿ��������������


        public float speed = 10f;//�ٶ�
        private int _scale = 100;//���������speed / _scale �� 0��1֮�䣬����ռ�ٷֱȣ�
        public float acceleration = 1f;//�ٶ�

        [Header("Direction Setting")]
        private AxisRotate axisRotate = new AxisRotate();//�й�����ʱ������ķ�������
        private bool rotateWithPoint = false;//�Ƿ���ê����ת�������������ر�axisRotate�����ã������������������˶�ʱ����ת��̬���������ڽ�����ֱ������

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
                //�Ƿ���Ҫһ��ʼ�ͽ�����λ���켣�ĳ�ʼ��
                if (isDrive)
                {
                    Line line = associatedSpline.GetLineByPoints(motionPath[0], motionPath[1], associatedSpline.lines);
                    //���ó�ʼλ��
                    this.transform.position = GetPositionInBezierCurves(line,motionPath[0], motionPath[1], 0);
                    //���ó�ʼ����
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

                //���������ϵ��������
                    DriveInSpline();


                //else//�����������
                //{
                //    SpecialCaseDeal();
                //}

                //������״̬�²��ܼ��������İٷֱ�
                if (isDrive)
                {
                    //�Ƿ�������
                    if (!isReverse)
                        currentLine.percentage += (speed * Time.deltaTime / 100);
                    else
                        currentLine.percentage -= (speed * Time.deltaTime / 100);
                }


                //�޷�
                if (currentLine.percentage < 0) currentLine.percentage = 0;
                if (currentLine.percentage > 1) currentLine.percentage = 1;

                //���ݰٷֱ�����
                UpdateTransform(currentLine.percentage);
            }

        }


        /// <summary>
        /// ˢ���˶�·����ê���б���Ϊ�յ�ê��ɾȥ
        /// </summary>
        public void RefreshMotionPathList()
        {
            if (motionPath == null || motionPath.Count == 0)
                return;

            for (int i = 0; i < motionPath.Count; i++)
            {
                //��Ϊnull��ê��ɾȥ
                if (motionPath[i] == null)
                    motionPath.RemoveAt(i);
            }
        }



        /// <summary>
        /// �����������
        /// </summary>
        public void SpecialCaseDeal()
        {
            //��λ��ر�־λ��������������
            isAutomaticallyNext = false;
            isReverse = false;
            isLoop = false;
            currentLine.index = 0;

            //��ǰ��ʼ�㲻���ڡ�����������ڣ���ӵ�ǰ���ƶ���������
            if (currentLine.startPoint == null && currentLine.endPoint != null)
            {

                if (isDrive)
                {
                    //�ӵ�ǰλ��������������
                    DriveFromNowToPoint(currentLine.endPoint);
                    //���㵱ǰ��ͽ������ֱ�߾���
                    float distance = Vector3.Distance(this.transform.position, currentLine.endPoint.transform.position);
                    //��ֱ�߾���С��0.001ʱ��Ϊ���ע�⣬����ֱ�������������������жϣ���Ϊ����ʱ����0.00001������
                    if (Mathf.Abs(distance) < 0.001)
                        currentLine.isArrived = true;
                    else
                        currentLine.isArrived = false;

                }


            }

            //����ʼ��ͽ�����Ϊͬһ���㣬��Ĭ��ֱ�ӵ���
            if (currentLine.startPoint == currentLine.endPoint && isDrive)
            {
                currentLine.isArrived = true;
                currentLine.percentage = 1;
            }

            //������״̬�£�����Ϊ1ʱ��Ϊ�ѵ���
            if (currentLine.percentage == 1 && isDrive)
            {
                currentLine.isArrived = true;
            }

            //������״̬�£�������0~1��ʱ���������û�е���ĳ����
            if (currentLine.percentage > 0 && currentLine.percentage < 1 && isDrive)
            {
                currentLine.isArrived = false;
            }



        }


        void DriveInSpline()
        {
            if (!isDrive)
                return;

            ///�������˶��켣�ϵ�����״̬
            //�Ƿ��Զ�����һ���߶�����
            if (isAutomaticallyNext)
            {
                //������������һ����
                if (!isReverse && currentLine.percentage == 1 && currentLine.index < motionPath.Count - 1)
                {
                    currentLine.index++;
                    currentLine.isArrived = true;
                    currentLine.percentage = 0;
                    currentLine.SetPoints(motionPath[currentLine.index - 1], motionPath[currentLine.index]);
                }

                //������������һ����
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


            //�Ƿ�ѭ������
            if (isLoop && isAutomaticallyNext)
            {
                //����������ѭ������λ
                if (this.currentLine.index == motionPath.Count - 1 && currentLine.percentage == 1 && !isReverse)
                {
                    this.currentLine.index = 1;
                    this.currentLine.percentage = 0;
                    this.currentLine.SetPoints(motionPath[0], motionPath[1]);
                }

                //����������ѭ������λ
                if (this.currentLine.index == 1 && currentLine.percentage == 0 && isReverse)
                {
                    this.currentLine.index = motionPath.Count - 1;
                    this.currentLine.percentage = 1;
                    this.currentLine.SetPoints(motionPath[motionPath.Count - 2], motionPath[motionPath.Count - 1]);
                }

            }



        }


        /// <summary>
        /// ���ݰٷֱ���ָ�������������˶�
        /// </summary>
        /// <param name="percentage"></param>
        public void UpdateTransform(float percentage)
        {
            if (!isDrive)
                return;

            //��startPoint�����ڻ�endPoint������ʱ
            if (currentLine.startPoint == null || currentLine.endPoint == null)
                return;


            LineType lineType = LineType.Straight;//�߶�����Ϊ�գ���Ĭ��ֱ��
            // LineDetail lineDetail = CalculatePercentageInCurrentLine(percentage);
            // //ˢ��Spline�ű��е������б�
            // associatedSpline.RefreshLinesList();
            // //��ȡ�߶�����
            // Line line = associatedSpline.GetLineByPoints(lineDetail.startPoint, lineDetail.endPoint, associatedSpline.lines);

            LineDetail lineDetail = this.currentLine;
            //ˢ��Spline�ű��е������б�
            associatedSpline.RefreshLinesList();
            //��ȡ�߶�����
            Line line = associatedSpline.GetLineByPoints(lineDetail.startPoint, lineDetail.endPoint, associatedSpline.lines);
            //�߶��Ƿ����
            if (line != null)
            {
                currentLine.isExist = true;
                lineType = line.lineType;
            }
            else
            {
                currentLine.isExist = false;
                //�߶β����ڣ���ǰ�ߵ��߶β�������associatedSpline��lines�б��У�ʱ����Ĭ�ϻ��ƺ�ɫֱ�߲���ֱ������
                Debug.DrawLine(this.transform.position, lineDetail.endPoint.transform.position, Color.red, 0, true);//��DisplayStatusΪtrue�����ƺ�ɫ����
            }


            switch (lineType)
            {
                case LineType.Bezier:
                    //UpdatePosition ����λ��
                    this.transform.position = GetPositionInBezierCurves(line,lineDetail.startPoint, lineDetail.endPoint, lineDetail.percentage);
                    //UpdateOrientation ���³���
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
                    //UpdatePosition ����λ��
                    this.transform.position = GetPositionInStraightLine(lineDetail.startPoint, lineDetail.endPoint, lineDetail.percentage);

                    //UpdateOrientation ���³���
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

            //����rotation
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
                            delta_angle / (1 - percentage) * (speed * Time.deltaTime / 100)//��֡�ʣ�֡��������
                        );
                    }
                    else
                    {
                        float delta_angle = Quaternion.Angle(this.transform.rotation, lineDetail.startPoint.transform.rotation);
                        this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation,
                            lineDetail.startPoint.transform.rotation,
                            //delta_angle*percentage//(speed * Time.deltaTime / 100)
                            delta_angle / percentage * (speed * Time.deltaTime / 100)//��֡�ʼ���
                        );
                    }

                }
                else
                {
                    this.transform.eulerAngles = new Vector3(this.transform.eulerAngles.x,90,this.transform.eulerAngles.z);
                }

                //�ر������йط�������ķ���
                axisRotate.isRotate = false;

            }


        }


        /// <summary>
        /// ���ݰٷֱȴӱ����������л�ȡ��ֵ�ķ���
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        /// <param name="percentage"></param>
        /// <param name="normalized"></param>
        /// <returns></returns>
        public Vector3 GetDirectionInBezierCurves(Line line, SplinePoint startPoint, SplinePoint endPoint, float percentage, bool normalized = true)
        {
            //��ȡһ�׵���

            if (associatedSpline.driveAgainst == false)//�Ƿ�Ϊ������
            {
                return BezierCurves.GetFirstDerivative(startPoint.Point.position, endPoint.Point.position, line.sourceControl.transform.position, line.destinationControl.transform.position, percentage).normalized;
            }
            else
            {
                return -1*BezierCurves.GetFirstDerivative(endPoint.Point.position, startPoint.Point.position, line.sourceControl.transform.position, line.destinationControl.transform.position, 1 - percentage).normalized;
            }
        }

        /// <summary>
        /// ���ݰٷֱȴӱ����������л�ȡ��ֵ��λ��
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        /// <param name="percentage"></param>
        /// <param name="normalized"></param>
        /// <returns></returns>
        public Vector3 GetPositionInBezierCurves(Line line,SplinePoint startPoint, SplinePoint endPoint, float percentage, bool normalized = true)
        {
            if (associatedSpline.driveAgainst == false)//�Ƿ�Ϊ������
            {
                return BezierCurves.GetPoint(startPoint.Point.position, endPoint.Point.position, line.sourceControl.transform.position, line.destinationControl.transform.position, percentage, true, 100);
            }
            else
            {
                return BezierCurves.GetPoint(endPoint.Point.position, startPoint.Point.position, line.sourceControl.transform.position, line.destinationControl.transform.position, 1-percentage, true, 100);
            }
        }

        /// <summary>
        /// ���ݰٷֱȴ�ֱ���л�ȡ��ֵ�ķ���
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
        /// ���ݰٷֱȴ�ֱ���л�ȡ��ֵ��λ��
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
        /// �ӵ�ǰλ��������һ��ê��
        /// </summary>
        public float DriveFromNowToPoint(SplinePoint target, float speedMultiplier = 1f)
        {

            float distance = Vector3.Distance(this.transform.position, target.transform.position);
            //float _speed = distance
            //��֡�ʼ���
            this.transform.position = Vector3.MoveTowards(this.transform.position, target.transform.position, distance / (1 - currentLine.percentage) * (speed * Time.deltaTime / 100) * speedMultiplier);


            float delta_angle = Quaternion.Angle(this.transform.rotation, target.transform.rotation);
            this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation,
                target.transform.rotation,
                delta_angle / (1 - currentLine.percentage) * (speed * Time.deltaTime / 100) * speedMultiplier//��֡�ʼ���
            );

            //���ƺ�ɫֱ�߲���ֱ������
            Debug.DrawLine(this.transform.position, target.transform.position, Color.red, 0, true);//��DisplayStatusΪtrue�����ƺ�ɫ����

            return distance;
        }

        private bool CheckCollission()
        {
            raydirection = pathdirection;
            var colorok = Color.yellow;
            var colorcollide = Color.red;
            var color = colorok;
            var collission = Physics.Raycast(transform.position, raydirection, Distance, raycastlayermask);
            //�򳡾��е�������ײ��Ͷ��һ�����ߣ����������Ϊ /origin/������ /direction/������Ϊ /maxDistance/������������κ���ײ���ཻ������ true�����򷵻� false��
            //raycastlayermask �����֣�������Ͷ������ʱ��ѡ��غ�����ײ�塣�ýű�������õĶ�������Ķ�����ͬһLayer�²��ܼ��ɹ�
            if (collission)
                color = colorcollide;
            if (DrawRay)
                Debug.DrawRay(transform.position, raydirection * Distance, color);

            if (DistanceSides > 0 && !collission)
            {
                var ray1 = Quaternion.AngleAxis(-AngleSide, Vector3.up) * raydirection;//����б�����ж�
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
            //    isDrive = false;//�ж������ڵ�
            //if (!collission )//&& IsBlocked)
            //    isDrive = true;
            return collission;
        }






    }


}