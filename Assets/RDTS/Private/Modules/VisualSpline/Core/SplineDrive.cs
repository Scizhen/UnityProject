using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace VisualSpline
{

    /// <summary>
    /// ������������
    /// </summary>
    public class SplineDrive : MonoBehaviour
    {

        [System.Serializable]
        public class AxisRotate
        {
            public bool isRotate = false;//�Ƿ���������ǰ����������
            public bool yIsUp = true;//y�������ϻ�������
        }


        public Spline associatedSpline;//������������
        public List<SplinePoint> motionPath = new List<SplinePoint>();//�˶�·����ê��


        [Header("Drive Status")]
        [HideInInspector]
        [Range(0, 1)]
        public float percentage = 0f;//������
        public LineDetail currentLine;//��ǰ�����������߶�

        [Header("Drive Control")]
        public bool isDrive = false;//�Ƿ��������
        public bool isAutomaticallyNext = true;//�Ƿ��Զ�����һ��������
        public bool isReverse = false;//�Ƿ񷴷�������
        public bool isLoop = false;//�Ƿ�ѭ������
        [HideInInspector] public bool specialCaseHandling = false;//�Ƿ��������������


        [Range(0, 100)]
        public float speed = 10f;//�ٶ�
        private int _scale = 100;//���������speed / _scale �� 0��1֮�䣬����ռ�ٷֱȣ�


        [Header("Direction Setting")]
        public AxisRotate axisRotate;//�й�����ʱ������ķ�������
        public bool rotateWithPoint = false;//�Ƿ���ê����ת�������������ر�axisRotate�����ã������������������˶�ʱ����ת��̬���������ڽ�����ֱ������


        [Header("Special Case")]
        public bool isP2P = false;//�Ƿ������㵽�㡱������ģʽ����ģʽ��������·��ֻ��currentLine�йأ�����motionPath�޹�

        private int _slicesPerLine = 100;//ÿ���߶ε���Ƭ������Խ��Խ��ϸ׼ȷ�����ｫ�������е�ÿ���߶��зֳ�100��
        private int _totalslices//�ܹ�����Ƭ����
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
                //�Ƿ���Ҫһ��ʼ�ͽ�����λ���켣�ĳ�ʼ��
                if (isDrive)
                {
                    //���ó�ʼλ��
                    this.transform.position = GetPositionInBezierCurves(motionPath[0], motionPath[1], 0);
                    //���ó�ʼ����
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
                if (!isP2P)//��P2Pģʽ�£�����������motionPath
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

                }
                else//�����������
                {
                    SpecialCaseDeal();
                }

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
        /// Э�̣���һ�������Զ�����percentageֵ�ñ仯���Դ�ʵ�ֶ�����������������
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
                    this.transform.position = GetPositionInBezierCurves(lineDetail.startPoint, lineDetail.endPoint, lineDetail.percentage);

                    //UpdateOrientation ���³���
                    if (axisRotate.isRotate)
                    {
                        this.transform.LookAt(this.transform.position + GetDirectionInBezierCurves(lineDetail.startPoint, lineDetail.endPoint, lineDetail.percentage),
                        (axisRotate.yIsUp) ? Vector3.up : Vector3.down);
                    }



                    break;
                case LineType.Straight:
                    //UpdatePosition ����λ��
                    this.transform.position = GetPositionInStraightLine(lineDetail.startPoint, lineDetail.endPoint, lineDetail.percentage);

                    //UpdateOrientation ���³���
                    if (axisRotate.isRotate)
                        this.transform.LookAt(this.transform.position + GetDirectionInStraightLine(lineDetail.startPoint, lineDetail.endPoint),
                        (axisRotate.yIsUp) ? Vector3.up : Vector3.down);

                    break;
            }

            //����rotation
            if (rotateWithPoint)
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

                //�ر������йط�������ķ���
                axisRotate.isRotate = false;

            }


        }


        /// <summary>
        /// ���������percentageֵ��Ӧ����һ������������ȡ�����Ϣ
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
        /// �ڵ�ǰ�߶��ϵ�������Ϣ
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
        /// ���ݰٷֱȴӱ����������л�ȡ��ֵ�ķ���
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        /// <param name="percentage"></param>
        /// <param name="normalized"></param>
        /// <returns></returns>
        public Vector3 GetDirectionInBezierCurves(SplinePoint startPoint, SplinePoint endPoint, float percentage, bool normalized = true)
        {
            //��ȡһ�׵���
            return BezierCurves.GetFirstDerivative(startPoint.Point.position, endPoint.Point.position, startPoint.OutTangent.position, endPoint.InTangent.position, percentage).normalized;

        }

        /// <summary>
        /// ���ݰٷֱȴӱ����������л�ȡ��ֵ��λ��
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
        /// ��ĳ����ȥ��������
        /// </summary>
        /// <param name="tr_self">����ı���</param>
        /// <param name="lookPos">�����Ŀ��</param>
        /// <param name="directionAxis">�����ᣬȡ���������Ǹ�����ȥ����</param>
        void AxisLookAt(Transform tr_self, Vector3 lookPos, Vector3 directionAxis)
        {
            var rotation = tr_self.rotation;
            var targetDir = lookPos - tr_self.position;
            //ָ���ĸ��ᳯ��Ŀ��,�����޸�Vector3�ķ���
            var fromDir = tr_self.rotation * directionAxis;
            //���㴹ֱ�ڵ�ǰ�����Ŀ�귽�����
            var axis = Vector3.Cross(fromDir, targetDir).normalized;
            //���㵱ǰ�����Ŀ�귽��ļн�
            var angle = Vector3.Angle(fromDir, targetDir);
            //����ǰ������Ŀ�귽����תһ���Ƕȣ�����Ƕ�ֵ��������ֵ
            tr_self.rotation = Quaternion.AngleAxis(angle, axis) * rotation;
            tr_self.localEulerAngles = new Vector3(0, tr_self.localEulerAngles.y, 90);//�����������ӵģ���Ϊ������x��z���򲻻����κα仯
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






    }


}