using UnityEngine;

namespace RDTS
{
    /// <summary>
    /// 传感器行为，可连接PLC信号实现相关控制
    /// </summary>
    [RequireComponent(typeof(Sensor))]
    //! The Sensor_Standard component is providing the Sensor behavior and connection to the signal inputs and outputs.
    public class Sensor_Standard : BehaviorInterface
    {

        [Header("Settings")] public bool NormallyClosed = false;  //!< Defines if sensor signal is *true* if occupied (*NormallyClosed=false*) of if signal is *false* if occupied (*NormallyClosed=true*) NormallyClosed为false时，检测到对象时信号Occupied输出为true；NormallyClosed为true时，检测到对象时信号Occupied输出为false
        [Header("Interface Connection")] public ValueInputBool Occupied; //! Boolean Value input for the Sensor signal.

        private Sensor Sensor;
        private bool _isOccupiedNotNull;

        // Use this for initialization
        void Start()
        {
            _isOccupiedNotNull = Occupied != null;
            Sensor = GetComponent<Sensor>();
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            bool occupied = false;

            // Set Behavior Outputs
            if (NormallyClosed)
            {
                occupied = !Sensor.Occupied;//取反
            }
            else
            {
                occupied = Sensor.Occupied;
            }

            // Set external Value Outputs
            if (_isOccupiedNotNull)
                Occupied.Value = occupied;

        }
    }
}