
using System.Collections.Generic;

using UnityEngine;

namespace RDTS
{
    /// <summary>
    /// ����UI��Toolbar�����ţ��趨Ϊ�󴰿ں�С��������״̬
    ///     �󴰿�״̬��UIȫ����ʾ������С
    ///     С����״̬���������DeactivateWhenSmall�б��еĶ������أ�UI�Ŵ󣨱��ڿ��壩
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

        public float SmallIsWidthSmallerThen = 13;//���϶�Ϊ��IsSmall������С���
        public float SmallIsHeightSmallerThen = 13;//���϶�Ϊ��IsSmall������С�߶�

        public bool IsSmall;

        private RectTransform _rect;

        [Tooltip("PS����������Сʱ�����صĶ���")]
        public List<GameObject> DeactivateWhenSmall;//��������Сʱ�����صĶ���

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

                foreach (var obj in DeactivateWhenSmall)//������Сʱ�����ظ��б��еĶ���
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

                foreach (var obj in DeactivateWhenSmall)//���ڷŴ�ʱ������б�Ķ���
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
