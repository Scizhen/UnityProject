
using UnityEngine;
using NaughtyAttributes;

namespace RDTS
{
    //���ƻ������ɵ����ű���
    public class Carton : LibraryPart
    {
        [OnValueChanged("Modify")] public float Length;
        [OnValueChanged("Modify")] public float Width;
        [OnValueChanged("Modify")] public float Height;

        [Button("Update")]
        public override void Modify()
        {

            transform.localScale = new Vector3(Length, Height, Width);

        }
    }
}

