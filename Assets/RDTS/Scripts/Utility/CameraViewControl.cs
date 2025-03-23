using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CameraView
{

    /* 若要使用输入来进行任何类型的移动行为，请使用 Input.GetAxis。
             *  要读取轴，请将 Input.GetAxis 与以下默认轴之一配合使用： 
             *   “Horizontal”和“Vertical”映射到游戏杆（D、D、D、D 和箭头键）; 
             *   “Mouse X”和“Mouse Y”映射到鼠标增量;
             *   “Fire1”、“Fire2”、“Fire3”映射到 Cmd、Cmd、Cmd 键和三个鼠标或游戏杆按钮。 
             *    可以添加新输入轴。请参阅输入管理器以了解相关信息。
             * 将 Input.GetButton 仅用于事件等操作。不要将它用于移动操作。Input.GetAxis 将使脚本代码更简洁。
             * 
             * 鼠标：0 表示左按钮，1 表示右按钮，2 表示中间按钮。按下鼠标按钮时返回 true，释放时返回 false
             * 
             */


    public class CameraViewControl : MonoBehaviour
    {
        /* 漫游的相机 */
        public GameObject ViewCamera;
        /* 鼠标右键旋转、中键按下移动及灵敏度 *//* 平移 */
        private float mouseX;
        private float mouseY;
        [Header("MouseControl：")]
        //关于Axis轴的灵敏度
        [Header("Axis轴灵敏度")]
        public float XSensitivity = 50;
        public float YSensitivity = 50;
        //右键鼠标移动的速度
        [Header("右键鼠标移动速度")]
        public float XRotSpeed = 5;
        public float YRotSpeed = 5;
        //插值（间隔量）
        [Header("插值（间隔量）")]
        public float RotLerp = 3;
        public float PosLerp = 3;
        //x,y轴的旋转角度
        [Header("x,y轴的旋转角度")]
        private float xDeg = 0.0f;
        private float yDeg = 0.0f;
        public int yMinLimit = -80;
        public int yMaxLimit = 80;

        private Quaternion currentRotation, targetRotation;
        private Vector3 currentPosition, targetPosition;

        /* 鼠标中键滚动及缩放率 *//* 缩放 */
        [Header(" 鼠标中键滚动缩放率")]
        private float mouseScrollWheel;
        //public float ZoomRate = 300;//缩放率
        public float ZoomSensitivity = 300;//缩放的灵敏度
        public float ZoomLerp = 3;
        [Header(" 鼠标中键平移缩放率")]
        public float TranslateSensitivity = 8;//平移灵敏度

        private float currentDistance, targetDistance;
        public float minDistance = .6f;
        public float maxDistance = 20;

        /* 按键上下左右箭头及灵敏度 */
        private float mouseH;//Horizontal
        private float mouseV;//Vertical
        //public float HVSensitivity = 10;
        [Header(" 按键上下左右箭头及灵敏度（未实现）")]
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
        /// 主要放置鼠标的控制方法：中键-控制平移，右键-控制旋转
        /// </summary>
        /*  */
        void LateUpdate()
        {
            ViewCameraXYMovement();
            ViewCameraZoomInOrOut();
        }

        /// <summary>
        /// 初始化相关操作，获取所用组件
        /// 
        /// </summary>
        void Init()
        {
            currentPosition = targetPosition = ViewCamera.transform.position;
            currentRotation = targetRotation = Quaternion.identity;

        }



        /* 键盘操作类
         *   Vertical:对应键盘上面的上下箭头，当按下上或下箭头时触发
         *   Horizontal:对应键盘上面的左右箭头，当按下左或右箭头时触发
         * 触屏类  
         *   Mouse X:鼠标沿着屏幕X移动时触发
         *   Mouse Y:鼠标沿着屏幕Y移动时触发
         *   Mouse ScrollWheel:当鼠标滚动轮滚动时触发
         */
        /// <summary>
        /// 获取鼠标移动的X，Y，ScrollWheel的增量。
        /// mouseIndex=1，获取平面内的鼠标增量；mouseIndex=2，获取鼠标滚轮值;mouseIndex=3，获取键盘上下左右箭头的值
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
                    mouseH = Input.GetAxis("Horizontal") * HorizontalSensitivity;//左右
                    mouseV = Input.GetAxis("Vertical") * VerticalSensitivity;//前后
                    break;

            }


        }

        /// <summary>
        /// 获取鼠标按键
        /// </summary>
        int GetMouseButton()
        {
            if (Input.GetMouseButton(0))//鼠标左键
            {
                return 0;
            }
            else if (Input.GetMouseButton(1))//鼠标右键
            {
                return 1;
            }
            else if (Input.GetMouseButton(2))//鼠标中键
            {
                return 2;
            }

            return -1;
        }

        /// <summary>
        /// 鼠标右键、中键控制相机移动和旋转
        /// </summary>
        void ViewCameraXYMovement()
        {
            if (GetMouseButton() != -1)//若无鼠标按键输入，则不进入语句，节省运行资源
            {
                GetAxisValue(1);
                switch (GetMouseButton())
                {
                    case 0:/*左键*/

                        break;

                    case 1:/*右键*/
                           //累计的增量
                        xDeg += mouseX;
                        yDeg -= mouseY;
                        // Debug.Log("yDeg"+ yDeg);
                        yDeg = ClampAngle(yDeg, yMinLimit, yMaxLimit);//获取在limit内的值—y轴即上下移动的角度限制在-80°~80°

                        currentRotation = transform.rotation;//获取当前的旋转
                        targetRotation = Quaternion.Euler(yDeg * YRotSpeed, xDeg * XRotSpeed, 0);//目标旋转量

                        var rotation = Quaternion.Slerp(currentRotation, targetRotation, Time.deltaTime * RotLerp);//插值—更平滑
                        this.transform.rotation = rotation;//<! 旋转

                        break;

                    case 2:/*中键*/
                        currentPosition = transform.position;//获取当前的位置
                        var dxdy = transform.right * mouseX + transform.up * mouseY;//计算在x，y轴上的鼠标移动增量
                        targetPosition = currentPosition - dxdy * TranslateSensitivity;//目标位置

                        var position = Vector3.Lerp(currentPosition, targetPosition, Time.deltaTime * PosLerp);//插值—更平滑
                        this.transform.position = position;//<! 平移

                        currentPosition = this.transform.position;

                        break;
                }
            }


        }

        /// <summary>
        /// 鼠标中键滑动控制相机的距离缩放
        /// </summary>
        void ViewCameraZoomInOrOut()
        {
            GetAxisValue(2);
            //Debug.Log("mouseScrollWheel"+ mouseScrollWheel);
            //targetDistance -= mouseScrollWheel;
            //targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
            //currentDistance = Mathf.Lerp(currentDistance, targetDistance,Time.deltaTime* ZoomSensitivity);
            //// For smoothing of the zoom, lerp distance 为了平滑缩放，调整距离
            ////currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * ZoomSensitivity);
            //this.transform.position = currentPosition - Vector3.forward * currentDistance;

            targetDistance = mouseScrollWheel;
            currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * ZoomLerp); //为了平滑缩放，调整距离
            this.transform.position += transform.forward * currentDistance;//使相机向前、后伸缩

        }

        /// <summary>
        /// 键盘上下左右箭头控制对象移动（垂直、水平方向）
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
            return Mathf.Clamp(angle, min, max);//将给定值限制在给定的最小浮点数和最大浮点值之间。 如果它在最小和最大范围内，则返回给定值。
        }

    }
}