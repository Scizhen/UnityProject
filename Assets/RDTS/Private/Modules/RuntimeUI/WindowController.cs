//*************************************************************************
//Thanks for the code reference game4automation provides.                 *
//                                                                        *
//*************************************************************************
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#pragma warning disable 0168
#pragma warning disable 0649

namespace RDTS
{
    /// <summary>
    /// ��RuntimeHierarchy��RuntimeInspector�ĸ������ڽ��п��ƣ�
    ///     ���Open/Closeͼ��ɴ򿪴��ڣ�
    ///     �����϶������촰�ڵĿ��
    /// </summary>
    //! Controls floating windows during simulation / gamemode for hierarchy / inspector and automation UI display
    public class WindowController : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
    {
        public GameObject Window;
        public GameObject UpdateLayoutGroup;
        public GameObject OpenIcon;
        public GameObject CloseIcon;


        public bool TranslateInY = false;
        public bool HideIconWhenClosed = false;
        public bool SetPassiveWhenClosed = true;
        public bool WindowClosed = true;//��Ӧ�����Ƿ�ر�
        public float OpenCloseSpeed = 1f;

        public float StandardSize;
        private float _lastsize;
        private float _startwindowopen;
        private RectTransform _windowtransform;
        private bool dragging = false;//�Ƿ����ָ���϶�״̬

        private float _startdragpos;
        private Vector2 _startsize;
        private RectTransform _transform;
        private RectTransform _updatelayoutgroup;
        private RectTransform _thisrecttransform;
        private CanvasScaler _canvas;

        public delegate void WindowControllerOnWindowOpen(WindowController windowcontroller);

        public event WindowControllerOnWindowOpen OnWindowOpen;

        public delegate void WindowControllerOnWindowClose(WindowController windowcontroller);

        public event WindowControllerOnWindowClose OnWindowClose;

        public void Start()
        {
            _windowtransform = Window.GetComponent<RectTransform>();
            /* if (TranslateInY)
                StandardSize = _windowtransform.sizeDelta.y;
            else
                StandardSize = _windowtransform.sizeDelta.x; */
            _lastsize = StandardSize;//��һ�δ�windowʱĬ�ϱ�׼�ߴ�
            OpenWindow(!WindowClosed);

            _canvas = GetComponentInParent<CanvasScaler>();
            _thisrecttransform = GetComponent<RectTransform>();
            _transform = Window.GetComponent<RectTransform>();
            if (GetComponent<WindowController>().UpdateLayoutGroup != null)
                _updatelayoutgroup = GetComponent<WindowController>().UpdateLayoutGroup.GetComponent<RectTransform>();
        }

        public void PointerDown(float size)
        {
            if (WindowClosed)//�����ѹرվʹ�
            {
                OpenWindow(true);
                _startwindowopen = Time.unscaledTime;
            }
        }

        public void PointerUp(float size)
        {

            // If not dragged and not opened just before
            if (!(System.Math.Abs(size - _startdragpos) > 10))//ָ��δ����ק�����º�̧���ľ��벻����10���أ�
            {
                if (!WindowClosed)//��ǰ�����Ѵ�
                {
                    var delta = Time.unscaledTime - _startwindowopen;
                    // Window was lon ooened, no short click - close it �����Ѵ򿪣�û�ж̰����ر���
                    if (delta > 0.3f)
                    {
                        OpenWindow(false);
                    }

                    if (delta < 0.3f)
                    {
                        // Short click - no dragging - set to size �̰������϶������ô�С

                        if (!TranslateInY)
                            _windowtransform.sizeDelta = new Vector2(_lastsize, _windowtransform.sizeDelta.y);
                        else
                            _windowtransform.sizeDelta = new Vector2(_windowtransform.sizeDelta.x, _lastsize);
                    }
                }
            }
        }

        //true�򿪴��ڣ�false�رմ��ڡ����¼��һ�α��϶��ĳߴ�
        public void OpenWindow(bool open)
        {
            if (open)
            {
                var max = 0;
                if (TranslateInY)
                    max = Screen.height - 30;
                else
                    max = Screen.width - 30;

                if (_lastsize < 30)
                {
                    _lastsize = StandardSize;
                }

                if (_lastsize > Screen.width - 30)
                {
                    _lastsize = StandardSize;
                }

                Window.SetActive(true);
                if (!TranslateInY)
                    _windowtransform.sizeDelta = new Vector2(_lastsize, _windowtransform.sizeDelta.y);
                else
                    _windowtransform.sizeDelta = new Vector2(_windowtransform.sizeDelta.x, _lastsize);
                WindowClosed = false;
                OpenIcon.SetActive(false);
                CloseIcon.SetActive(true);
            }
            else
            {
                if (!TranslateInY)
                    _lastsize = _windowtransform.sizeDelta.x;
                else
                    _lastsize = _windowtransform.sizeDelta.y;
                if (SetPassiveWhenClosed)
                {
                    Window.SetActive(false);
                }

                if (!TranslateInY)
                    _windowtransform.sizeDelta = new Vector2(0, _windowtransform.sizeDelta.y);
                else
                    _windowtransform.sizeDelta = new Vector2(_windowtransform.sizeDelta.x, 0);


                WindowClosed = true;
                OpenIcon.SetActive(true);
                CloseIcon.SetActive(false);
                if (HideIconWhenClosed)
                    gameObject.SetActive(false);

                if (OnWindowClose != null)
                    OnWindowClose(this);
            }
        }

        public void ToggleWindow()
        {
            if (WindowClosed)
            {
                if (OnWindowOpen != null)
                    OnWindowOpen(this);
                OpenWindow(true);
            }
            else
            {
                if (OnWindowClose != null)
                    OnWindowClose(this);
                OpenWindow(false);
            }
        }

        private float GetMousePos()
        {
            Vector3 screen;

            var posi = Input.mousePosition;

            //referenceResolution��UI������Ƶķֱ��ʡ������Ļ�ֱ��ʽϴ���UI���������Ŵ������Ļ�ֱ��ʽ�С����UI����������С�����Ǹ�����Ļƥ��ģʽ��ɵ�
            if (TranslateInY)
                return posi.y * _canvas.referenceResolution.y / Screen.height;
            else
                return posi.x * _canvas.referenceResolution.x / Screen.width;
        }


        public void Update()
        {
            if (dragging)
            {
                if (TranslateInY)
                {
                    var delta = GetMousePos() - _startdragpos;
                    var newpos = _startsize.y + delta;
                    if (newpos < 0)
                        OpenWindow(false);
                    else
                        _transform.sizeDelta = new Vector2(_transform.sizeDelta.x, newpos);
                }
                else
                {
                    if (Input.mousePosition.x < Screen.width - 30)//Screen.width��Game���ڵĿ��
                    {
                        //Debug.Log("Screen.width:"+ Screen.width);
                        var delta = GetMousePos() - _startdragpos;//����϶�������ֵ
                        var newpos = _startsize.x + delta;
                        if (newpos < 0)
                            OpenWindow(false);
                        else
                            _transform.sizeDelta = new Vector2(newpos, _transform.sizeDelta.y);//��Window�����size���е���
                    }
                }

                if (_updatelayoutgroup != null)
                    LayoutRebuilder.ForceRebuildLayoutImmediate(_updatelayoutgroup);//ǿ�������ؽ��ܼ���Ӱ��Ĳ���Ԫ�غ��Ӳ���Ԫ�ء�
            }
        }


        ///����������Ӷ��������ӵ�UI�������Raycast Target���ԣ�Ϊtrue������������������Rect��Χʱ���Żᴥ�� UnityEngine.EventSystems
        //ʹ�ô˻ص������ָ�����µ��¼�:��갴��
        public void OnPointerDown(PointerEventData eventData)
        {
            dragging = true;
            _startsize = _transform.sizeDelta;
            PointerDown(GetMousePos());
            _startdragpos = GetMousePos();
        }

        //ʹ�ô˻ص������ָ�����ϵ��¼������̧��
        public void OnPointerUp(PointerEventData eventData)
        {
            dragging = false;
            PointerUp(GetMousePos());
        }
    }
}