using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CameraView
{

    /* ��Ҫʹ�������������κ����͵��ƶ���Ϊ����ʹ�� Input.GetAxis��
             *  Ҫ��ȡ�ᣬ�뽫 Input.GetAxis ������Ĭ����֮һ���ʹ�ã� 
             *   ��Horizontal���͡�Vertical��ӳ�䵽��Ϸ�ˣ�D��D��D��D �ͼ�ͷ����; 
             *   ��Mouse X���͡�Mouse Y��ӳ�䵽�������;
             *   ��Fire1������Fire2������Fire3��ӳ�䵽 Cmd��Cmd��Cmd ��������������Ϸ�˰�ť�� 
             *    ��������������ᡣ�����������������˽������Ϣ��
             * �� Input.GetButton �������¼��Ȳ�������Ҫ���������ƶ�������Input.GetAxis ��ʹ�ű��������ࡣ
             * 
             * ��꣺0 ��ʾ��ť��1 ��ʾ�Ұ�ť��2 ��ʾ�м䰴ť��������갴ťʱ���� true���ͷ�ʱ���� false
             * 
             */


    public class CameraViewControl : MonoBehaviour
    {
        /* ���ε���� */
        public GameObject ViewCamera;
        /* ����Ҽ���ת���м������ƶ��������� *//* ƽ�� */
        private float mouseX;
        private float mouseY;
        [Header("MouseControl��")]
        //����Axis���������
        [Header("Axis��������")]
        public float XSensitivity = 50;
        public float YSensitivity = 50;
        //�Ҽ�����ƶ����ٶ�
        [Header("�Ҽ�����ƶ��ٶ�")]
        public float XRotSpeed = 5;
        public float YRotSpeed = 5;
        //��ֵ���������
        [Header("��ֵ���������")]
        public float RotLerp = 3;
        public float PosLerp = 3;
        //x,y�����ת�Ƕ�
        [Header("x,y�����ת�Ƕ�")]
        private float xDeg = 0.0f;
        private float yDeg = 0.0f;
        public int yMinLimit = -80;
        public int yMaxLimit = 80;

        private Quaternion currentRotation, targetRotation;
        private Vector3 currentPosition, targetPosition;

        /* ����м������������� *//* ���� */
        [Header(" ����м�����������")]
        private float mouseScrollWheel;
        //public float ZoomRate = 300;//������
        public float ZoomSensitivity = 300;//���ŵ�������
        public float ZoomLerp = 3;
        [Header(" ����м�ƽ��������")]
        public float TranslateSensitivity = 8;//ƽ��������

        private float currentDistance, targetDistance;
        public float minDistance = .6f;
        public float maxDistance = 20;

        /* �����������Ҽ�ͷ�������� */
        private float mouseH;//Horizontal
        private float mouseV;//Vertical
        //public float HVSensitivity = 10;
        [Header(" �����������Ҽ�ͷ�������ȣ�δʵ�֣�")]
        public float HorizontalSensitivity = 10;
        public float VerticalSensitivity = 10;



        // Start is called before the first frame update
        void Start()
        {
            Init();
        }


        // Update is called once per frame
        void Update()
        {

        }

        /// <summary>
        /// ��Ҫ�������Ŀ��Ʒ������м�-����ƽ�ƣ��Ҽ�-������ת
        /// </summary>
        /*  */
        void LateUpdate()
        {
            ViewCameraXYMovement();
            ViewCameraZoomInOrOut();
        }

        /// <summary>
        /// ��ʼ����ز�������ȡ�������
        /// 
        /// </summary>
        void Init()
        {
            currentPosition = targetPosition = ViewCamera.transform.position;
            currentRotation = targetRotation = Quaternion.identity;

        }



        /* ���̲�����
         *   Vertical:��Ӧ������������¼�ͷ���������ϻ��¼�ͷʱ����
         *   Horizontal:��Ӧ������������Ҽ�ͷ������������Ҽ�ͷʱ����
         * ������  
         *   Mouse X:���������ĻX�ƶ�ʱ����
         *   Mouse Y:���������ĻY�ƶ�ʱ����
         *   Mouse ScrollWheel:���������ֹ���ʱ����
         */
        /// <summary>
        /// ��ȡ����ƶ���X��Y��ScrollWheel��������
        /// mouseIndex=1����ȡƽ���ڵ����������mouseIndex=2����ȡ������ֵ;mouseIndex=3����ȡ�����������Ҽ�ͷ��ֵ
        /// </summary>
        void GetAxisValue(int mouseIndex)
        {
            switch (mouseIndex)
            {
                case 0:
                    break;
                case 1:
                    mouseX = Input.GetAxis("Mouse X") * XSensitivity * Time.deltaTime;
                    mouseY = Input.GetAxis("Mouse Y") * YSensitivity * Time.deltaTime;
                    break;
                case 2:
                    mouseScrollWheel = Input.GetAxis("Mouse ScrollWheel") * ZoomSensitivity * Time.deltaTime;

                    break;
                case 3:
                    mouseH = Input.GetAxis("Horizontal") * HorizontalSensitivity;//����
                    mouseV = Input.GetAxis("Vertical") * VerticalSensitivity;//ǰ��
                    break;

            }


        }

        /// <summary>
        /// ��ȡ��갴��
        /// </summary>
        int GetMouseButton()
        {
            if (Input.GetMouseButton(0))//������
            {
                return 0;
            }
            else if (Input.GetMouseButton(1))//����Ҽ�
            {
                return 1;
            }
            else if (Input.GetMouseButton(2))//����м�
            {
                return 2;
            }

            return -1;
        }

        /// <summary>
        /// ����Ҽ����м���������ƶ�����ת
        /// </summary>
        void ViewCameraXYMovement()
        {
            if (GetMouseButton() != -1)//������갴�����룬�򲻽�����䣬��ʡ������Դ
            {
                GetAxisValue(1);
                switch (GetMouseButton())
                {
                    case 0:/*���*/

                        break;

                    case 1:/*�Ҽ�*/
                           //�ۼƵ�����
                        xDeg += mouseX;
                        yDeg -= mouseY;
                        // Debug.Log("yDeg"+ yDeg);
                        yDeg = ClampAngle(yDeg, yMinLimit, yMaxLimit);//��ȡ��limit�ڵ�ֵ��y�ἴ�����ƶ��ĽǶ�������-80��~80��

                        currentRotation = transform.rotation;//��ȡ��ǰ����ת
                        targetRotation = Quaternion.Euler(yDeg * YRotSpeed, xDeg * XRotSpeed, 0);//Ŀ����ת��

                        var rotation = Quaternion.Slerp(currentRotation, targetRotation, Time.deltaTime * RotLerp);//��ֵ����ƽ��
                        this.transform.rotation = rotation;//<! ��ת

                        break;

                    case 2:/*�м�*/
                        currentPosition = transform.position;//��ȡ��ǰ��λ��
                        var dxdy = transform.right * mouseX + transform.up * mouseY;//������x��y���ϵ�����ƶ�����
                        targetPosition = currentPosition - dxdy * TranslateSensitivity;//Ŀ��λ��

                        var position = Vector3.Lerp(currentPosition, targetPosition, Time.deltaTime * PosLerp);//��ֵ����ƽ��
                        this.transform.position = position;//<! ƽ��

                        currentPosition = this.transform.position;

                        break;
                }
            }


        }

        /// <summary>
        /// ����м�������������ľ�������
        /// </summary>
        void ViewCameraZoomInOrOut()
        {
            GetAxisValue(2);
            //Debug.Log("mouseScrollWheel"+ mouseScrollWheel);
            //targetDistance -= mouseScrollWheel;
            //targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
            //currentDistance = Mathf.Lerp(currentDistance, targetDistance,Time.deltaTime* ZoomSensitivity);
            //// For smoothing of the zoom, lerp distance Ϊ��ƽ�����ţ���������
            ////currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * ZoomSensitivity);
            //this.transform.position = currentPosition - Vector3.forward * currentDistance;

            targetDistance = mouseScrollWheel;
            currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * ZoomLerp); //Ϊ��ƽ�����ţ���������
            this.transform.position += transform.forward * currentDistance;//ʹ�����ǰ��������

        }

        /// <summary>
        /// �����������Ҽ�ͷ���ƶ����ƶ�����ֱ��ˮƽ����
        /// </summary>
        void KeyboardHVControl()
        {

        }

        private static float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360)
                angle += 360;
            if (angle > 360)
                angle -= 360;
            return Mathf.Clamp(angle, min, max);//������ֵ�����ڸ�������С����������󸡵�ֵ֮�䡣 ���������С�����Χ�ڣ��򷵻ظ���ֵ��
        }

    }
}