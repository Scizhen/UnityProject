using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using RDTS.Utility;


namespace RDTS
{
    [System.Serializable]
    public class EventMUGrip : UnityEvent<MU, bool>
    {
    }

    /// <summary>
    ///  Gripץȡ�ű���ʵ�ּгֹ��߶�MU��ץȡ/���ã������ڽ� MU �̶����� Drives �ƶ��������MU ������Ϊ������������һ��ץȡ��
    /// </summary>
    [SelectionBase]
    [RequireComponent(typeof(Rigidbody))]
    //! Grip is used for fixing MUs to components which are moved by Drives.
    //! The MUs can be gripped as Sub-Components or with Rigid Bodies.
    public class Grip : BaseGrip, IFix
    {
        [Header("Kinematic")] public Sensor PartToGrip; //!< Identifies the MU to be gripped.  ��ʶҪץȡ�� MU��ȷ��Ҫץȡ�Ķ���

        public bool
            DirectlyGrip = false; //!< If set to true the MU is directly gripped when Sensor PartToGrip detects a Part  �˱�־λΪtrueʱ������������⵽MUʱ��MU���ᱻֱ��ץȡ

        ///Pick Align With Object �� Place Align With Object �����ڽ�ʰȡ�ͷ��õ�������뵽�����λ�ã�
        ///����ѡ���κ���Ϸ����MU �����ĵ㽫����ѡ��Ϸ��������ĵ���룬 
        ///�����ķ�����룬�������������ĺ������ᡣ ���뷢���ڸ��Ӷ���֮ǰ����ö���֮��
        public GameObject PickAlignWithObject; //!<  If not null the MUs are aligned with this object before picking.
        public GameObject PlaceAlignWithObject; //!<  If not null the MUs are aligned with this object after placing.

        [Tooltip("Should be usually kept empty, for very special cases where joint should be used for gripping")]
        public UnityEngine.Joint
            ConnectToJoint; //< Should be usually kept empty, for very special cases where joint should be used for gripping ͨ��Ӧ����Ϊ�գ����ڷǳ���������������Ӧʹ�ý�ͷ���мг�

        public Sensor PickBasedOnSensor; //!< Picking is started when this sensor is occupied (optional)  ����������⵽ʱ����ʼץȡ
        public Drive_Cylinder PickBasedOnCylinder; //!< Picking is stared when Cylinder is Max or Min (optional)  ��Drive_Cylinder�ﵽ���ֵ����Сֵʱ����ʼץȡ��PickOnCylinderMaxΪtrueʱ�����ֵ����֮����Сֵ��
        public bool PickOnCylinderMax; //!< Picking is started when Cylinderis Max  �Ƿ�Drive_Cylinder�ﵽ���ֵʱ����ʼץȡ
        public bool NoPhysicsWhenPlaced = false; //!< Object remains kinematic (no phyisics) when placed  �����ڷ���ʱ�����˶���������

        ///MU����ͨ�����ַ�ʽ���������� MU �ϡ�
        ///����ʹ��������Ϊ�� MU ��������һ�� MU ���κ����͵ı����ϡ� ����ͨ���ͷ�ѡ��� MU ����ɵġ� ����������£�Place Load On MU ��ҪΪ false�� ��ע�⣬�ڴ˰汾�У�MU ����������ȫ�ȶ��� �෴�����ǽ���ʹ�� Place Load On MU = true �� MU ���õ����� MU �ϡ�
        ///��PlaceLoadOnMUΪtrue������Ҫ����һ������Ĵ�������PlaceLoadOnMUSensor��������ʶ��Ӧ��װ�� MU �� ��һ��MU�� ����������£����ô�MU ʱ����MU ����Ϊ Sub-Component ��ӵ���һ��MU �С�
        public bool
            PlaceLoadOnMU = false; //!<  When placing the components they should be loaded onto an MU as subcomponent. ��δ�õ�����
        public Sensor PlaceLoadOnMUSensor; //!<  Sensor defining the MU where the picked MUs should be loaded to.  ��ʶ��Ӧ��װ��MU����һ��MU��ֻҪ�˱�����Ϊ�վͻ����MU���װ�أ�δ�õ���PlaceLoadOnMU==true����

        [Header("Pick & Place Control")]
        public bool PickObjects = false; //!< true for picking MUs identified by the sensor.  ץȡ��־λ������Grip����ץȡ
        public bool PlaceObjects = false; //!< //!< true for placing the loaded MUs.  ���ñ�־λ������Grip���з���

        [Header("Events")]
        public EventMUGrip
            EventMUGrip; //!<  Unity event which is called for MU grip and ungrip. On grip it passes MU and true. On ungrip it passes MU and false.

        [Header("PLC IOs")] public ValueOutputBool SignalPick;//ץȡ�źţ����������ź�ֵ�ᱻ�����PickObjects
        public ValueOutputBool SignalPlace;//�����źţ����������ź�ֵ�ᱻ�����PlaceObjects


        private bool _pickobjectsbefore = false;
        private bool _placeobjectsbefore = false;
        private List<FixedJoint> _fixedjoints;

        [NaughtyAttributes.ReadOnly] public List<GameObject> PickedMUs;//��ץȡ��MU����

        private bool _issignalpicknotnull;
        private bool _issignalplacenotnull;


        //! Picks the GameObject obj  ץȡָ����MU����
        public void Fix(MU mu)
        {
            var obj = mu.gameObject;
            if (PickedMUs.Contains(obj) == false)
            {
                if (mu == null)
                {
                    ErrorMessage("MUs which should be picked need to have the MU script attached!");
                    return;
                }

                if (ConnectToJoint == null)
                    mu.Fix(this.gameObject);

                //ץȡ����
                if (PickAlignWithObject != null)
                {
                    obj.transform.position = PickAlignWithObject.transform.position;
                    obj.transform.rotation = PickAlignWithObject.transform.rotation;
                }

                if (ConnectToJoint != null)
                    ConnectToJoint.connectedBody = mu.Rigidbody;

                PickedMUs.Add(obj);//�����б�
                if (EventMUGrip != null)
                    EventMUGrip.Invoke(mu, true);//�����¼�
            }
        }

        //! Places the GameObject obj ����ָ����MU����
        public void Unfix(MU mu)
        {
            var obj = mu.gameObject;
            var tmpfixedjoints = _fixedjoints;
            var rb = mu.GetComponent<Rigidbody>();

            if (EventMUGrip != null)
                EventMUGrip.Invoke(mu, false);//�¼�����

            //���ö���
            if (PlaceAlignWithObject != null)
            {
                obj.transform.position = PlaceAlignWithObject.transform.position;
                obj.transform.rotation = PlaceAlignWithObject.transform.rotation;
            }

            if (ConnectToJoint == null)
                mu.Unfix();

            if (ConnectToJoint != null)
                ConnectToJoint.connectedBody = null;

            if (PlaceLoadOnMUSensor == null)
            {
                if (!NoPhysicsWhenPlaced)
                    rb.isKinematic = false;//�ر��˶�ѧ
            }

            //װ��MU����һ��MU
            if (PlaceLoadOnMUSensor != null)
            {
                var loadmu = PlaceLoadOnMUSensor.LastTriggeredBy.GetComponent<MU>();
                if (loadmu == null)
                {
                    ErrorMessage("You can only load parts on parts which are of type MU, please add to part [" +
                                 PlaceLoadOnMUSensor.LastTriggeredBy.name + "] MU script");
                }

                loadmu.LoadMu(mu);
            }

            PickedMUs.Remove(obj);//���б��Ƴ�
        }

        //! Picks al objects collding with the Sensor  ץȡ���б���ָ����Sensor��⵽��MU
        public void Pick()
        {
            if (PartToGrip != null)
            {
                // Attach all objects with fixed joint - if not already attached  ץȡ���о��й̶��ؽڵĶ��� - �����δץȡ
                foreach (GameObject obj in PartToGrip.CollidingObjects)
                {
                    var pickobj = GetTopOfMu(obj);//��ȡ�����MU
                    if (pickobj == null)
                        Warning("No MU on object for gripping detected", obj);
                    else
                        Fix(pickobj);
                }
            }
            else
            {
                ErrorMessage(
                    "Grip needs to define with a Sensor which parts to grip - no [Part to Grip] Sensor is defined");
            }
        }

        //! Places all objects  ��������PickedMUs�б��е�MU
        public void Place()
        {
            var tmppicked = PickedMUs.ToArray();
            foreach (var mu in tmppicked)
            {
                Unfix(mu.GetComponent<MU>());
            }
        }

        private void Reset()
        {
            GetComponent<Rigidbody>().isKinematic = true;
        }

        // Use this for initialization
        private void Start()
        {
            PickedMUs = new List<GameObject>();
            _issignalpicknotnull = SignalPick != null;
            _issignalplacenotnull = SignalPlace != null;
            if (PartToGrip == null)
            {
                Error("Grip Object needs to be connected with a sensor to identify objects to pick", this);
            }

            _fixedjoints = new List<FixedJoint>();
            GetComponent<Rigidbody>().isKinematic = true;

            ///������������Ӷ�Ӧ���¼�
            //Sensor���¼�
            if (PickBasedOnSensor != null)
            {
                PickBasedOnSensor.EventEnter += PickBasedOnSensorOnEventEnter;
            }

            if (DirectlyGrip == true)
            {
                PartToGrip.EventEnter += PickBasedOnSensorOnEventEnter;
            }

            if (PickBasedOnSensor != null)
            {
                PickBasedOnSensor.EventExit += PickBasedOnSensorOnEventExit;
            }

            //Drive_Cylinder���¼�
            if (PickBasedOnCylinder != null)
            {
                if (PickOnCylinderMax)
                {
                    PickBasedOnCylinder.EventOnMin += Place;
                    PickBasedOnCylinder.EventOnMax += Pick;
                }
                else
                {
                    PickBasedOnCylinder.EventOnMin += Pick;
                    PickBasedOnCylinder.EventOnMax += Place;
                }
            }
        }

        private void PickBasedOnSensorOnEventExit(GameObject obj)
        {
            var mu = obj.GetComponent<MU>();
            if (mu != null)
                Unfix(mu);
        }

        private void PickBasedOnSensorOnEventEnter(GameObject obj)
        {
            var mu = obj.GetComponent<MU>();
            if (mu != null)
                Fix(mu);
        }


        private void FixedUpdate()
        {
            if (_issignalpicknotnull)//��ץȡ�źŴ���
            {
                PickObjects = SignalPick.Value;//��ȡץȡ�ź�ֵ
            }

            if (_issignalplacenotnull)//�������źŴ���
            {
                PlaceObjects = SignalPlace.Value;//��ȡ�����ź�ֵ
            }

            if (_pickobjectsbefore == false && PickObjects)//ץȡ��־λ��falseתΪtrueʱ��ץȡPartToGrip��������⵽��MU
            {
                Pick();
            }

            if (_placeobjectsbefore == false && PlaceObjects)//���ñ�־λ��falseתΪtrueʱ�����б�PickedMUs�����е�MU����
            {
                Place();
            }

            //��¼��һ��ֵ
            _pickobjectsbefore = PickObjects;
            _placeobjectsbefore = PlaceObjects;
        }
    }
}