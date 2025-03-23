//*************************************************************************
//Thanks for the code reference game4automation provides.                 *
//                                                                        *
//*************************************************************************  
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

namespace RDTS
{
    /// <summary>
    /// 第一人称视角
    /// </summary>
    public class FirstPersonController : MonoBehaviour
    {
        public GameObject MainCamera;//主相机对象
        public GameObject Head;
        [OnValueChanged("Setup")] public float Size = 1.6f;
        public float JumpHeight = 2.5f;

        public float SensitivityX = 100f; //灵敏度系数
        public float SensitivityY = 100f;
        public float WalkSpeed = 5f;
        public float RunMultiplier = 2;
        public float Smoothing = 2.0f;
        public float HeadRotationLimits = 190f;
        public GenericButton Button;//点击此UI按钮，切换第一/三人称视角


        private SceneMouseNavigation _sceneMouseNavigation;//鼠标控制
        private Camera _camera;
        private bool _cameranotnull = false;
        private bool _isinknee = false;//是否蹲下
        private Rigidbody _rigidbody;
        private CharacterController _characterController;//角色控制器

        private float xRotation;

        public void SetActive(bool active)
        {
            if (Button != null)
                Button.SetStatus(active);
            this.enabled = active;
        }

        void Setup()//设置角色控制器的高度，head的相对位置
        {
            _characterController.height = Size;
            Head.transform.localPosition = new Vector3(0, Size / 2 - 0.1f, 0);
        }

        void Awake()
        {
            if (MainCamera != null)
            {
                _sceneMouseNavigation = MainCamera.GetComponent<SceneMouseNavigation>();
                _camera = MainCamera.GetComponent<Camera>();
                _cameranotnull = true;
            }

            _rigidbody = GetComponent<Rigidbody>();
            _characterController = GetComponent<CharacterController>();//获取此gameobject下的CharacterController组件
            Setup();
        }

        // Update is called once per frame
        void Update()
        {
            if (!_cameranotnull)
                return;

            //获取鼠标移动的相关偏移量
            float mouseX = Input.GetAxis("Mouse X") * SensitivityX * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * SensitivityY * Time.deltaTime;

            float movex = Input.GetAxis("Horizontal");
            float movez = Input.GetAxis("Vertical");

            if (Input.GetKey(KeyCode.Y))//按“Y”蹲下
            {
                Head.transform.localPosition = new Vector3(0, (Size / 2 - 0.1f) / 2, 0);//相机高度降一半
                _isinknee = true;
            }
            else
            {
                if (_isinknee)
                {
                    Head.transform.localPosition = new Vector3(0, (Size / 2 - 0.1f), 0);
                    _isinknee = false;
                }
            }

            var run = 1f;
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))//按下左、右Shift键进入跑步状态，速度加一倍
            {
                run = RunMultiplier;
            }

            //if (movex > 0 || movez > 0)
            //    Debug.Log(movex + " / " + movez);
            Vector3 move = transform.right * movex + transform.forward * movez;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -HeadRotationLimits, HeadRotationLimits);//返回限幅中的值
            Head.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            transform.Rotate(Vector3.up * mouseX);

            _characterController.Move(move * WalkSpeed * Time.deltaTime * run);

            if (_sceneMouseNavigation.FirstPersonControllerActive)
            {
                MainCamera.transform.position = Head.transform.position;
                MainCamera.transform.rotation = Head.transform.rotation;
            }
        }
    }
}