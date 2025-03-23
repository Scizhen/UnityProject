using UnityEngine;

namespace RDTS
{
    /// <summary>
    /// ��������Ϊ��������PLC�ź�ʵ����ؿ���
    /// </summary>
    [RequireComponent(typeof(Sensor))]
    //! The Sensor_Standard component is providing the Sensor behavior and connection to the signal inputs and outputs.
    public class Sensor_Standard : BehaviorInterface
    {

        [Header("Settings")] public bool NormallyClosed = false;  //!< Defines if sensor signal is *true* if occupied (*NormallyClosed=false*) of if signal is *false* if occupied (*NormallyClosed=true*) NormallyClosedΪfalseʱ����⵽����ʱ�ź�Occupied���Ϊtrue��NormallyClosedΪtrueʱ����⵽����ʱ�ź�Occupied���Ϊfalse
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
                occupied = !Sensor.Occupied;//ȡ��
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