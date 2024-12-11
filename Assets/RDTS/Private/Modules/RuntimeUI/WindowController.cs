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
    /// 对RuntimeHierarchy和RuntimeInspector的浮动窗口进行控制：
    ///     点击Open/Close图标可打开窗口，
    ///     或按下拖动可拉伸窗口的宽度
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
        public bool WindowClosed = true;//对应窗口是否关闭
        public float OpenCloseSpeed = 1f;

        public float StandardSize;
        private float _lastsize;
        private float _startwindowopen;
        private RectTransform _windowtransform;
        private bool dragging = false;//是否进入指针拖动状态

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
            _lastsize = StandardSize;//第一次打开window时默认标准尺寸
            OpenWindow(!WindowClosed);

            _canvas = GetComponentInParent<CanvasScaler>();
            _thisrecttransform = GetComponent<RectTransform>();
            _transform = Window.GetComponent<RectTransform>();
            if (GetComponent<WindowController>().UpdateLayoutGroup != null)
                _updatelayoutgroup = GetComponent<WindowController>().UpdateLayoutGroup.GetComponent<RectTransform>();
        }

        public void PointerDown(float size)
        {
            if (WindowClosed)//窗口已关闭就打开
            {
                OpenWindow(true);
                _startwindowopen = Time.unscaledTime;
            }
        }

        public void PointerUp(float size)
        {

            // If not dragged and not opened just before
            if (!(System.Math.Abs(size - _startdragpos) > 10))//指针未被拖拽（按下和抬起间的距离不大于10像素）
            {
                if (!WindowClosed)//此前窗口已打开
                {
                    var delta = Time.unscaledTime - _startwindowopen;
                    // Window was lon ooened, no short click - close it 窗口已打开，没有短按，关闭它
                    if (delta > 0.3f)
                    {
                        OpenWindow(false);
                    }

                    if (delta < 0.3f)
                    {
                        // Short click - no dragging - set to size 短按，无拖动，设置大小

                        if (!TranslateInY)
                            _windowtransform.sizeDelta = new Vector2(_lastsize, _windowtransform.sizeDelta.y);
                        else
                            _windowtransform.sizeDelta = new Vector2(_windowtransform.sizeDelta.x, _lastsize);
                    }
                }
            }
        }

        //true打开窗口，false关闭窗口。会记录上一次被拖动的尺寸
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

            //referenceResolution：UI布局设计的分辨率。如果屏幕分辨率较大，则UI将按比例放大，如果屏幕分辨率较小，则UI将按比例缩小。这是根据屏幕匹配模式完成的
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
                    if (Input.mousePosition.x < Screen.width - 30)//Screen.width：Game窗口的宽度
                    {
                        //Debug.Log("Screen.width:"+ Screen.width);
                        var delta = GetMousePos() - _startdragpos;//鼠标拖动的增量值
                        var newpos = _startsize.x + delta;
                        if (newpos < 0)
                            OpenWindow(false);
                        else
                            _transform.sizeDelta = new Vector2(newpos, _transform.sizeDelta.y);//对Window对象的size进行调整
                    }
                }

                if (_updatelayoutgroup != null)
                    LayoutRebuilder.ForceRebuildLayoutImmediate(_updatelayoutgroup);//强制立即重建受计算影响的布局元素和子布局元素。
            }
        }


        ///！对象或其子对象所附加的UI组件含有Raycast Target属性（为true），且鼠标光标进入对象的Rect范围时，才会触发 UnityEngine.EventSystems
        //使用此回调来检测指针向下的事件:鼠标按下
        public void OnPointerDown(PointerEventData eventData)
        {
            dragging = true;
            _startsize = _transform.sizeDelta;
            PointerDown(GetMousePos());
            _startdragpos = GetMousePos();
        }

        //使用此回调来检测指针向上的事件：鼠标抬起
        public void OnPointerUp(PointerEventData eventData)
        {
            dragging = false;
            PointerUp(GetMousePos());
        }
    }
}