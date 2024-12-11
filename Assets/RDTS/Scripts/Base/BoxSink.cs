using System;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using Random = UnityEngine.Random;


namespace RDTS
{
    /// <summary>
    /// 销毁盒子的基础脚本对LibraryObject覆写，用于改变销毁盒子形状去适应运输机的高度和宽度
    /// </summary>
    [SelectionBase]
    public class BoxSink : LibraryObject
    {

        [OnValueChanged("Modify")] public float ConveyorHeight = 0.8f;
        [OnValueChanged("Modify")] public float Width = 1f;
        [OnValueChanged("Modify")] public float Lenth = 1f;
        [OnValueChanged("Modify")] public float SinkHeight = 0.2f;

        public override void OnParentChanged()
        {
            if (Parent != null)
            {
                var straight = Parent.GetComponent<StraightConveyor>();
                if (straight != null)
                {
                    var endsnap = straight.GetComponentByName<SnapPoint>("SnapOut");
                    var posend = endsnap.transform.localPosition;
                    this.transform.localPosition = new Vector3(posend.x - Lenth / 2, posend.y - SinkHeight / 2, 0);
                    this.transform.localRotation = Quaternion.identity;
                }
            }
            Modify();
        }

        public override void OnParentModify()
        {
            Modify();
        }

        public override void OnModify()
        {

            if (Parent != null)
            {
                var straight = Parent.GetComponent<StraightConveyor>();
                if (straight != null)
                {
                    ConveyorHeight = straight.Height;
                    Width = straight.Width;
                    BottomHeight = straight.BottomHeight;

                }

            }
            this.transform.position =
                new Vector3(this.transform.position.x, BottomHeight + ConveyorHeight + SinkHeight / 2, this.transform.position.z);
            this.transform.localScale = new Vector3(Lenth, SinkHeight, Width);
        }

        public override bool HideAllowed(GameObject gameObject)
        {

            return true;
        }

    }
}