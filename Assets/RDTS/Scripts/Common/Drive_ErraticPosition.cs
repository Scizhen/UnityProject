using UnityEngine;

namespace RDTS
{
    /// <summary>
    /// 不规律驱动行为，与Drive脚本配合使用，一般用于测试
    /// </summary>
    [RequireComponent(typeof(Drive))]
    //! This drive is only for test purposes. It is moving constantly two erratic positions between MinPos and MaxPos. 
    public class Drive_ErraticPosition : BehaviorInterface
    {
        public float MinPos = 0; //!< Minimum position of the range where the drive is allowed to move to.
        public float MaxPos = 100; //!< Maximum position of the range where the drive is allowed to move to.
        public float Speed = 100; //!< Speed of the drive in millimeter / second.
        public bool Driving = false; //!< Set to true if Drive should drive to erratic positions.

        private Drive Drive;
        private float _destpos;

        void Reset()
        {
            Drive = GetComponent<Drive>();
            if (Drive.UseLimits)
            {
                MinPos = Drive.LowerLimit;
                MaxPos = Drive.UpperLimit;
            }
        }

        // Use this for initialization
        void Start()
        {
            Drive = GetComponent<Drive>();
            Drive.CurrentPosition = MinPos;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (Driving && !Drive.IsRunning && Drive.CurrentPosition != _destpos)
            {
                Drive.TargetPosition = _destpos;
                Drive.TargetStartMove = true;
            }

            if (Driving == false)
            {
                Drive.TargetSpeed = Speed;
                Drive.TargetPosition = Random.Range(MinPos, MaxPos);
                Drive.TargetStartMove = true;
                Driving = true;
                _destpos = Drive.TargetPosition;
            }
            else
            if (Drive.IsRunning && Driving == true)
            {
                Drive.TargetStartMove = false;
            }

            if (Drive.CurrentPosition == _destpos && Driving == true)
            {
                Driving = false;
            }

        }
    }
}
