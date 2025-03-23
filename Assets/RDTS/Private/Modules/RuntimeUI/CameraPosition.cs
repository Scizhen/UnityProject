//*************************************************************************
//Thanks for the code reference game4automation provides.                 *
//                                                                        *
//*************************************************************************  
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.EventSystems;


namespace RDTS
{
    //! Saves the camera position for game view.  保存游戏视角的摄像机位置。
    //! Each position can get a name and a Hoteky to display the camera position 每个位置都可以获取一个名称和一个Hoteky来显示相机位置
    //! Multiple of this objects can be attachted to the camera for saving multiple positions 可以将多个这些对象附加到相机以保存多个位置
    public class CameraPosition : RDTSBehavior
    {
        public string ViewName; //!< The view name  视图名称，按快捷键后会显示在消息框中
        public string KeyCode; //!< The Key code (hotkey) do display the vidw   切换到此视角的快捷键
        public bool ActivateOnStart; //!< True if view should be activated on start 如果应在启动时激活视图，则为真

        public TouchInteraction //触控交互
            DoubleTapGesture; //!< Reference to TouchInteraction position should be displayed on double tap 双击时应显示对 TouchInteraction 位置的引用

        public CameraPos campos;//相机位姿信息（asset文件）
        
        private bool _display = true;

        private EventSystem _event;

        //! Gets the current position of the game view and saves it to this object
        //获取游戏视图的当前位置并将其保存到此对象
        [Button("Save Camera Position")]
        public void GetCameraPosition()
        {
            SceneMouseNavigation nav = GetComponent<SceneMouseNavigation>();
            campos.SaveCameraPosition(nav);//保存当前相机位姿
        }


        private void SetCameraPositionNoDisplay()
        {
            _display = false;
            SetCameraPosition();
            _display = true;
        }


        void OnEnable()
        {
            _event = GetComponent<EventSystem>();
            if (DoubleTapGesture != null)
                DoubleTapGesture.doubleTouchDelegate += TapGestureHandler;
        }

        private void TapGestureHandler(Vector2 pos)
        {
            SetCameraPosition();
        }

        //! Sets the game view position to the saved positoin and displays a message
        //将游戏视图位置设置为保存的位置，并在相机前显示一条消息
        [Button("Set Position")]
        public void SetCameraPosition()
        {
            // Debug.Log("Switched to View " + ViewName);
            SceneMouseNavigation nav = GetComponent<SceneMouseNavigation>();
            if (campos != null && nav != null)
            {
                nav.SetNewCameraPosition(campos.TargetPos, campos.CameraDistance, campos.CameraRot);
                if (_display)
                {
                    RDTSController.MessageBox("View changed to " + ViewName, true, 2);
                }
            }
        }

        public void Start()
        {
            if (ActivateOnStart)
            {
                Invoke("SetCameraPositionNoDisplay", 0.1f);
            }
        }

        public void Update()
        {
            if (KeyCode != "")
            {
                if (Input.GetKeyDown(KeyCode))
                {
                    SetCameraPosition();
                }
            }
        }
    }
}