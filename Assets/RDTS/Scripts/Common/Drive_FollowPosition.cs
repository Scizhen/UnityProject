using UnityEngine;

namespace RDTS
{
    [RequireComponent(typeof(Drive))]
    //! Behavior model of a drive where the drive is exactly following the current given position of the PLC
    //! This is special useful for connecting motion controllers and robot controllers to game4automation.
    public class Drive_FollowPosition : BehaviorInterface
    {
        [Header("Settings")]
        public float Offset = 0; //!< Offset in millimeter which is added to the position signal
        public float Scale = 1; //!<  Scale factor which is scaling the position value

        [Header("PLC IOs")]
        public ValueOutputFloat Position; //!< Signal (PLCOutput) for the defined position of the drive
        public ValueInputFloat CurrentPosition; //!< PLCInput for the current position of the drive (without offset and scaling)

        private Drive Drive;
        private bool _isPositionNotNull;
        private bool _isCurrentPositionNotNull;

        // Use this for initialization
        void Start()
        {
            Drive = GetComponent<Drive>();
            _isPositionNotNull = Position != null;
            _isCurrentPositionNotNull = CurrentPosition != null;

        }

        // Update is called once per frame
        void FixedUpdate()
        {
            // Get external Signals
            if (_isPositionNotNull)
                Drive.CurrentPosition = Position.Value * Scale + Offset;

            // Set external Signals
            if (_isCurrentPositionNotNull)
                CurrentPosition.Value = Drive.CurrentPosition;
        }

    }
}
