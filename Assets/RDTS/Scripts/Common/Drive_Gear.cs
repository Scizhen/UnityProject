using UnityEngine;

namespace RDTS
{
    /// <summary>
    /// 齿轮驱动行为，与Drive脚本配合使用，需要一个主驱动器来跟随其驱动
    /// </summary>
    [RequireComponent(typeof(Drive))]
    //! Behavior model of a drive which is connected to another drive with a gear.
    //! All positions of the master drive are directly transfered with offset and gear factor to this drive.
    //! The formula is CurrentPosition = MasterDrive.CurrentPosition * GearFactor+Offset;
    public class Drive_Gear : BehaviorInterface
    {
        [Header("Settings")]
        public Drive MasterDrive; //!< Master drive which is defining the position of this drive
        public float GearFactor = 1; //!< Gear factor of the gear
        public float Offset = 0; //!< Offset of the gear in millimeter

        private Drive _thisdrive;
        private Drive Drive;

        // Use this for initialization
        void Start()
        {
            _thisdrive = GetComponent<Drive>();
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            _thisdrive.CurrentPosition = MasterDrive.CurrentPosition * GearFactor + Offset;
            _thisdrive.CurrentSpeed = MasterDrive.CurrentSpeed * GearFactor;

        }
    }
}
