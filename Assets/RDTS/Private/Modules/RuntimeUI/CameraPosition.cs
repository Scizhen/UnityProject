//*************************************************************************
//Thanks for the code reference game4automation provides.                 *
//                                                                        *
//*************************************************************************  
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.EventSystems;


namespace RDTS
{
    //! Saves the camera position for game view.  ������Ϸ�ӽǵ������λ�á�
    //! Each position can get a name and a Hoteky to display the camera position ÿ��λ�ö����Ի�ȡһ�����ƺ�һ��Hoteky����ʾ���λ��
    //! Multiple of this objects can be attachted to the camera for saving multiple positions ���Խ������Щ���󸽼ӵ�����Ա�����λ��
    public class CameraPosition : RDTSBehavior
    {
        public string ViewName; //!< The view name  ��ͼ���ƣ�����ݼ������ʾ����Ϣ����
        public string KeyCode; //!< The Key code (hotkey) do display the vidw   �л������ӽǵĿ�ݼ�
        public bool ActivateOnStart; //!< True if view should be activated on start ���Ӧ������ʱ������ͼ����Ϊ��

        public TouchInteraction //���ؽ���
            DoubleTapGesture; //!< Reference to TouchInteraction position should be displayed on double tap ˫��ʱӦ��ʾ�� TouchInteraction λ�õ�����

        public CameraPos campos;//���λ����Ϣ��asset�ļ���
        
        private bool _display = true;

        private EventSystem _event;

        //! Gets the current position of the game view and saves it to this object
        //��ȡ��Ϸ��ͼ�ĵ�ǰλ�ò����䱣�浽�˶���
        [Button("Save Camera Position")]
        public void GetCameraPosition()
        {
            SceneMouseNavigation nav = GetComponent<SceneMouseNavigation>();
            campos.SaveCameraPosition(nav);//���浱ǰ���λ��
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
        //����Ϸ��ͼλ������Ϊ�����λ�ã��������ǰ��ʾһ����Ϣ
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