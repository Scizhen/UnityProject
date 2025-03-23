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
    ///  Grip抓取脚本，实现夹持工具对MU的抓取/放置，即用于将 MU 固定到由 Drives 移动的组件。MU 可以作为子组件或与刚体一起抓取。
    /// </summary>
    [SelectionBase]
    [RequireComponent(typeof(Rigidbody))]
    //! Grip is used for fixing MUs to components which are moved by Drives.
    //! The MUs can be gripped as Sub-Components or with Rigid Bodies.
    public class Grip : BaseGrip, IFix
    {
        [Header("Kinematic")] public Sensor PartToGrip; //!< Identifies the MU to be gripped.  标识要抓取的 MU（确定要抓取的对象）

        public bool
            DirectlyGrip = false; //!< If set to true the MU is directly gripped when Sensor PartToGrip detects a Part  此标志位为true时，当传感器检测到MU时，MU将会被直接抓取

        ///Pick Align With Object 和 Place Align With Object 可用于将拾取和放置的零件对齐到定义的位置，
        ///可以选择任何游戏对象，MU 的轴心点将与所选游戏对象的轴心点对齐， 
        ///完整的方向对齐，包括枢轴点的中心和所有轴。 对齐发生在附加对象之前或放置对象之后。
        public GameObject PickAlignWithObject; //!<  If not null the MUs are aligned with this object before picking.
        public GameObject PlaceAlignWithObject; //!<  If not null the MUs are aligned with this object after placing.

        [Tooltip("Should be usually kept empty, for very special cases where joint should be used for gripping")]
        public UnityEngine.Joint
            ConnectToJoint; //< Should be usually kept empty, for very special cases where joint should be used for gripping 通常应保持为空，用于非常特殊的情况，其中应使用接头进行夹持

        public Sensor PickBasedOnSensor; //!< Picking is started when this sensor is occupied (optional)  当传感器检测到时，开始抓取
        public Drive_Cylinder PickBasedOnCylinder; //!< Picking is stared when Cylinder is Max or Min (optional)  当Drive_Cylinder达到最大值或最小值时，开始抓取（PickOnCylinderMax为true时在最大值，反之在最小值）
        public bool PickOnCylinderMax; //!< Picking is started when Cylinderis Max  是否当Drive_Cylinder达到最大值时，开始抓取
        public bool NoPhysicsWhenPlaced = false; //!< Object remains kinematic (no phyisics) when placed  物体在放置时保持运动（无物理）

        ///MU可以通过两种方式放置在其他 MU 上。
        ///可以使用物理行为将 MU 放置在另一个 MU 或任何类型的表面上。 这是通过释放选择的 MU 来完成的。 在这种情况下，Place Load On MU 需要为 false。 请注意，在此版本中，MU 附件并不完全稳定。 相反，我们建议使用 Place Load On MU = true 将 MU 放置到其他 MU 上。
        ///若PlaceLoadOnMU为true，则需要定义一个额外的传感器（PlaceLoadOnMUSensor），它标识了应该装载 MU 的 另一个MU。 在这种情况下，放置此MU 时，此MU 将作为 Sub-Component 添加到另一个MU 中。
        public bool
            PlaceLoadOnMU = false; //!<  When placing the components they should be loaded onto an MU as subcomponent. 【未用到？】
        public Sensor PlaceLoadOnMUSensor; //!<  Sensor defining the MU where the picked MUs should be loaded to.  标识了应该装载MU的另一个MU（只要此变量不为空就会进行MU间的装载，未用到“PlaceLoadOnMU==true”）

        [Header("Pick & Place Control")]
        public bool PickObjects = false; //!< true for picking MUs identified by the sensor.  抓取标志位，控制Grip进行抓取
        public bool PlaceObjects = false; //!< //!< true for placing the loaded MUs.  放置标志位，控制Grip进行放置

        [Header("Events")]
        public EventMUGrip
            EventMUGrip; //!<  Unity event which is called for MU grip and ungrip. On grip it passes MU and true. On ungrip it passes MU and false.

        [Header("PLC IOs")] public ValueOutputBool SignalPick;//抓取信号，若存在则信号值会被赋予给PickObjects
        public ValueOutputBool SignalPlace;//放置信号，若存在则信号值会被赋予给PlaceObjects


        private bool _pickobjectsbefore = false;
        private bool _placeobjectsbefore = false;
        private List<FixedJoint> _fixedjoints;

        [NaughtyAttributes.ReadOnly] public List<GameObject> PickedMUs;//已抓取的MU对象

        private bool _issignalpicknotnull;
        private bool _issignalplacenotnull;


        //! Picks the GameObject obj  抓取指定的MU对象
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

                //抓取对齐
                if (PickAlignWithObject != null)
                {
                    obj.transform.position = PickAlignWithObject.transform.position;
                    obj.transform.rotation = PickAlignWithObject.transform.rotation;
                }

                if (ConnectToJoint != null)
                    ConnectToJoint.connectedBody = mu.Rigidbody;

                PickedMUs.Add(obj);//加入列表
                if (EventMUGrip != null)
                    EventMUGrip.Invoke(mu, true);//调用事件
            }
        }

        //! Places the GameObject obj 放置指定的MU对象
        public void Unfix(MU mu)
        {
            var obj = mu.gameObject;
            var tmpfixedjoints = _fixedjoints;
            var rb = mu.GetComponent<Rigidbody>();

            if (EventMUGrip != null)
                EventMUGrip.Invoke(mu, false);//事件调用

            //放置对齐
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
                    rb.isKinematic = false;//关闭运动学
            }

            //装载MU至另一个MU
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

            PickedMUs.Remove(obj);//从列表移除
        }

        //! Picks al objects collding with the Sensor  抓取所有被（指定）Sensor检测到的MU
        public void Pick()
        {
            if (PartToGrip != null)
            {
                // Attach all objects with fixed joint - if not already attached  抓取所有具有固定关节的对象 - 如果尚未抓取
                foreach (GameObject obj in PartToGrip.CollidingObjects)
                {
                    var pickobj = GetTopOfMu(obj);//获取顶层的MU
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

        //! Places all objects  放置所有PickedMUs列表中的MU
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

            ///根据设置来添加对应的事件
            //Sensor的事件
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

            //Drive_Cylinder的事件
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
            if (_issignalpicknotnull)//若抓取信号存在
            {
                PickObjects = SignalPick.Value;//获取抓取信号值
            }

            if (_issignalplacenotnull)//若放置信号存在
            {
                PlaceObjects = SignalPlace.Value;//获取放置信号值
            }

            if (_pickobjectsbefore == false && PickObjects)//抓取标志位从false转为true时，抓取PartToGrip传感器检测到的MU
            {
                Pick();
            }

            if (_placeobjectsbefore == false && PlaceObjects)//放置标志位从false转为true时，将列表PickedMUs中所有的MU放置
            {
                Place();
            }

            //记录上一个值
            _pickobjectsbefore = PickObjects;
            _placeobjectsbefore = PlaceObjects;
        }
    }
}