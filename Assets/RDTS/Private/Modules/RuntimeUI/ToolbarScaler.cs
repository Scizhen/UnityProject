
using System.Collections.Generic;

using UnityEngine;

namespace RDTS
{
    /// <summary>
    /// 调整UI的Toolbar的缩放：设定为大窗口和小窗口两种状态
    ///     大窗口状态：UI全部显示，但缩小
    ///     小窗口状态：被添加至DeactivateWhenSmall列表中的对象被隐藏，UI放大（便于看清）
    /// </summary>
    public class ToolbarScaler : MonoBehaviour
    {
        public bool AdjustHeight;
        public float SmallScreenHeight = 120;
        public float BigScreenHeight = 60;

        public bool Scale;
        public float SmallScreenScale = 2f;
        public float BigScreenScale = 1f;
        public float CurrentScreenWidth;

        public float CurrentScreenHeight;

        public float SmallIsWidthSmallerThen = 13;//被认定为“IsSmall”的最小宽度
        public float SmallIsHeightSmallerThen = 13;//被认定为“IsSmall”的最小高度

        public bool IsSmall;

        private RectTransform _rect;

        [Tooltip("PS：当窗口缩小时被隐藏的对象")]
        public List<GameObject> DeactivateWhenSmall;//当窗口缩小时被隐藏的对象

        private bool _issmallbefore;

        public void SizeChanged()
        {
            // Settings for small screen sizes
            if (IsSmall)
            {
                if (AdjustHeight)
                {
                    _rect.sizeDelta = new Vector2(_rect.sizeDelta.x, SmallScreenHeight);
                }

                foreach (var obj in DeactivateWhenSmall)//窗口缩小时的隐藏该列表中的对象
                {
                    obj.SetActive(false);
                }

                if (Scale)
                {
                    _rect.localScale = new Vector3(SmallScreenScale, SmallScreenScale, 1);
                }
            }
            // Settings for big screen sizes
            else
            {
                if (AdjustHeight)
                {
                    _rect.sizeDelta = new Vector2(_rect.sizeDelta.x, BigScreenHeight);
                }

                foreach (var obj in DeactivateWhenSmall)//窗口放大时激活该列表的对象
                {
                    obj.SetActive(true);
                }

                if (Scale)
                {
                    _rect.localScale = new Vector3(BigScreenScale, BigScreenScale, 1);
                }
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            _rect = GetComponent<RectTransform>();
        }

        // Update is called once per frame
        void Update()
        {
            CurrentScreenWidth = Screen.width / Screen.dpi * 2.54f;
            CurrentScreenHeight = Screen.height / Screen.dpi * 2.54f;

            if ((CurrentScreenWidth < SmallIsWidthSmallerThen) || (CurrentScreenHeight < SmallIsHeightSmallerThen))
            {
                IsSmall = true;
            }
            else
            {
                IsSmall = false;
            }

            if (IsSmall != _issmallbefore)
                SizeChanged();

            _issmallbefore = IsSmall;

        }
    }
}
