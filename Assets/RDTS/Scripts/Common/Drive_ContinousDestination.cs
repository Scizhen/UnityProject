
using NaughtyAttributes;
using UnityEngine;

namespace RDTS
{
    /// <summary>
    /// 连续目标点驱动行为，与Drive脚本配合使用
    /// </summary>
    [RequireComponent(typeof(Drive))]
    //! Behavior model of an intelligent drive which is getting a destination and moving to the destination.
    //! This component needs to have as a basis a standard Drive.
    public class Drive_ContinousDestination : BehaviorInterface
    {
        private Drive Drive;

        private new void Awake()
        {
            _isIsAtPositionNotNull = SignalIsAtPosition != null;
        }

        [Header("Continous Destination IO's")] public float Destination = 0;
        public float Acceleration = 100;
        public float TargetSpeed = 100;

        [Header("PLC IO's")]
        public ValueOutputFloat SignalDestination; //!<  Destination position of the drive in millimeters
        public ValueOutputFloat SignalAcceleration; //!< Acceleration of the drive in millimeters / second
        public ValueOutputFloat SignalTargetSpeed; //!< Target (maximum) speed of the drive in mm/ second

        public ValueInputFloat SignalIsAtPosition; //!< Signal is true if Drive is at destination position
        public ValueInputFloat SignalIsAtSpeed; //!<  Signal for current Drive speed in mm / second
        public ValueInputBool SignalIsAtDestination; //!<  Signal if Drive is at Destination
        public ValueInputBool SignalIsDriving; //!<  Signal is true if Drive is currently driving.

        private bool _isStartDriveNotNull;
        private bool _isDestinationNotNull;
        private bool _isTargetSpeedNotNull;
        private bool _isAccelerationNotNull;
        private bool _isIsAtPositionNotNull;
        private bool _isIsAtDestinationNotNull;
        private bool _isIsDrivingNotNull;
        private bool _isIsAtSpeedNotNull;
        private float destinationbefore = 0;

        // Use this for initialization
        void Start()
        {
            _isIsAtSpeedNotNull = SignalIsAtSpeed != null;
            _isIsDrivingNotNull = SignalIsDriving != null;
            _isIsAtDestinationNotNull = SignalIsAtDestination != null;
            _isAccelerationNotNull = SignalAcceleration != null;
            _isTargetSpeedNotNull = SignalTargetSpeed != null;
            _isDestinationNotNull = SignalDestination != null;
            Drive = GetComponent<Drive>();
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            // PLC Outputs
            if (_isDestinationNotNull)
                Destination = SignalDestination.Value;
            if (_isTargetSpeedNotNull)
                TargetSpeed = SignalTargetSpeed.Value;
            if (_isAccelerationNotNull)
                Acceleration = SignalAcceleration.Value;


            if (Destination != destinationbefore)
                Drive.DriveTo(Destination);
            Drive.TargetSpeed = TargetSpeed;
            Drive.Acceleration = Acceleration;


            // PLC Inputs
            if (_isIsAtPositionNotNull)
                SignalIsAtPosition.Value = Drive.CurrentPosition;
            if (_isIsAtDestinationNotNull)
                SignalIsAtDestination.Value = Drive.IsAtTarget;
            if (_isIsDrivingNotNull)
                SignalIsDriving.Value = Drive.IsRunning;
            if (_isIsAtSpeedNotNull)
                SignalIsAtSpeed.Value = Drive.CurrentSpeed;

            destinationbefore = Destination;
        }

    }
}
