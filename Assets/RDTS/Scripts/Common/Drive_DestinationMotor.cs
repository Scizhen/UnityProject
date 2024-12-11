using NaughtyAttributes;
using UnityEngine;

namespace RDTS
{
    /// <summary>
    /// 目标点驱动行为，与Drive脚本配合使用
    /// </summary>
    [RequireComponent(typeof(Drive))]
    //! Behavior model of an intelligent drive which is getting a destination and moving to the destination.
    //! This component needs to have as a basis a standard Drive.
    public class Drive_DestinationMotor : BehaviorInterface
    {
        private Drive Drive;

        private new void Awake()
        {
            _isIsAtPositionNotNull = IsAtPosition != null;
        }

        [Header("PLC IOs")] public ValueOutputFloat Speed; //!< Current Speed of the drive in millimeters / second
        public ValueOutputBool StartDrive; //!< Start to drive Signal
        public ValueOutputFloat Destination; //!<  Destination position of the drive in millimeters
        public ValueOutputFloat Acceleration; //!< Acceleration of the drive in millimeters / second
        public ValueOutputFloat TargetSpeed; //!< Target (maximum) speed of the drive in mm/ second

        public ValueInputFloat IsAtPosition; //!< Signal is true if Drive is at destination position
        public ValueInputFloat IsAtSpeed; //!<  Signal for current Drive speed in mm / second
        public ValueInputBool IsAtDestination; //!<  Signal if Drive is at Destination
        public ValueInputBool IsDriving; //!<  Signal is true if Drive is currently driving.
        private bool _isStartDriveNotNull;
        private bool _isDestinationNotNull;
        private bool _isTargetSpeedNotNull;
        private bool _isAccelerationNotNull;
        private bool _isIsAtPositionNotNull;
        private bool _isIsAtDestinationNotNull;
        private bool _isIsDrivingNotNull;
        private bool _isIsAtSpeedNotNull;


        // Use this for initialization
        void Start()
        {
            _isIsAtSpeedNotNull = IsAtSpeed != null;
            _isIsDrivingNotNull = IsDriving != null;
            _isIsAtDestinationNotNull = IsAtDestination != null;
            _isAccelerationNotNull = Acceleration != null;
            _isTargetSpeedNotNull = TargetSpeed != null;
            _isDestinationNotNull = Destination != null;
            _isStartDriveNotNull = StartDrive != null;
            Drive = GetComponent<Drive>();
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            // PLC Outputs
            if (_isStartDriveNotNull)
                Drive.TargetStartMove = StartDrive.Value;
            if (_isDestinationNotNull)
                Drive.TargetPosition = Destination.Value;
            if (_isTargetSpeedNotNull)
                Drive.TargetSpeed = TargetSpeed.Value;
            if (_isAccelerationNotNull)
                Drive.Acceleration = Acceleration.Value;

            // PLC Inputs
            if (_isIsAtPositionNotNull)
                IsAtPosition.Value = Drive.CurrentPosition;
            if (_isIsAtDestinationNotNull)
                IsAtDestination.Value = Drive.IsAtTarget;
            if (_isIsDrivingNotNull)
                IsDriving.Value = Drive.IsRunning;
            if (_isIsAtSpeedNotNull)
                IsAtSpeed.Value = Drive.CurrentSpeed;
        }

    }
}
