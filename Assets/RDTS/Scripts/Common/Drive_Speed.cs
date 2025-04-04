// Game4Automation (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// (c) 2019 in2Sight GmbH - Usage of this source code only allowed based on License conditions see https://game4automation.com/lizenz  

using NaughtyAttributes;
using UnityEngine;

namespace RDTS
{
    [RequireComponent(typeof(Drive))]
    //! Behavior model of an intelligent drive which is getting a destination and moving to the destination.智能驾驶到达目的地并向目的地移动的行为模型。
    //! This component needs to have as a basis a standard Drive.
    [HelpURL("https://game4automation.com/documentation/current/drivebehaviour.html")]
    public class Drive_Speed : BehaviorInterface
    {
        private Drive Drive;

        private new void Awake()
        {

        }

        [Header("Continous Destination IO's")]
        public float TargetSpeed = 100;
        public float Acceleration = 100;


        [Header("PLC IO's")]
        public ValueOutputFloat SignalAcceleration; //!< Acceleration of the drive in millimeters / second
        public ValueOutputFloat SignalTargetSpeed; //!< Target (maximum) speed of the drive in mm/ second

        public ValueInputFloat SignalCurrentSpeed; //!<  Signal for current Drive speed in mm / second
        public ValueInputFloat SignalCurrentPosition;  //!<  Signal for current Drive positon in mm 
        public ValueInputBool SignalIsDriving; //!<  Signal is true if Drive is currently driving.

        private bool _isStartDriveNotNull;
        private bool _isDestinationNotNull;
        private bool _isTargetSpeedNotNull;
        private bool _isAccelerationNotNull;
        private bool _isIsAtPositionNotNull;
        private bool _isIsAtDestinationNotNull;
        private bool _isCurrentPositionNotNull;
        private bool _isIsDrivingNotNull;
        private bool _isCurrentSpeedNotNull;

        // Use this for initialization
        void Start()
        {
            _isCurrentSpeedNotNull = SignalCurrentSpeed != null;
            _isIsDrivingNotNull = SignalIsDriving != null;
            _isCurrentPositionNotNull = SignalCurrentPosition != null;
            _isAccelerationNotNull = SignalAcceleration != null;
            _isTargetSpeedNotNull = SignalTargetSpeed != null;
            Drive = GetComponent<Drive>();
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            // PLC Outputs

            if (_isTargetSpeedNotNull)
                TargetSpeed = SignalTargetSpeed.Value;
            if (_isAccelerationNotNull)
                Acceleration = SignalAcceleration.Value;


            Drive.TargetSpeed = Mathf.Abs(TargetSpeed);
            if (TargetSpeed > 0)
            {
                Drive.JogForward = true;
                Drive.JogBackward = false;
            }

            if (TargetSpeed == 0)
            {
                Drive.JogForward = false;
                Drive.JogBackward = false;
            }

            if (TargetSpeed < 0)
            {
                Drive.JogForward = false;
                Drive.JogBackward = true;
            }

            Drive.Acceleration = Acceleration;


            // PLC Inputs
            if (_isIsDrivingNotNull)
                SignalIsDriving.Value = Drive.IsRunning;
            if (_isCurrentSpeedNotNull)
                SignalCurrentSpeed.Value = Drive.CurrentSpeed;
            if (_isCurrentPositionNotNull)
                SignalCurrentPosition.Value = Drive.CurrentPosition;

        }

    }
}
