// Game4Automation (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// (c) 2019 in2Sight GmbH - Usage of this source code only allowed based on License conditions see https://game4automation.com/lizenz  


namespace RDTS
{
    [System.Serializable]

    //! BOOL Output Value Object
    public class ValueOutputBool : Value
    {

        public StatusBool Status;

        public bool Value
        {
            get
            {
                if (Settings.Override)
                {
                    return Status.ValueOverride;
                }
                else
                {
                    return Status.Value;
                }
            }
            set
            {
                var oldvalue = Status.Value;
                Status.Value = value;
                if (oldvalue != value)
                    ValueChangedEvent(this);

            }
        }

        public override void OnToggleHierarchy()
        {
            if (Settings.Override == false)
                Settings.Override = true;
            Status.ValueOverride = !Status.ValueOverride;
            EventValueChanged.Invoke(this);
        }

        public override void SetStatusConnected(bool status)
        {
            Status.Connected = status;
        }

        public override bool GetStatusConnected()
        {
            return Status.Connected;
        }

        // When Script is added or reset ist pushed
        private void Reset()
        {
            Settings.Active = true;
            Settings.Override = false;
            Status.Value = false;
        }


        public override string GetVisuText()
        {
            return Value.ToString();
        }


        //若是output类型，返回2
        public override int GetValueDirection()
        {
            return 2;
        }


        // Sets the value as a string
        public override void SetValue(string value)
        {
            if (value != "")
                Value = bool.Parse(value);
            else
                Value = false;
        }

        public override void SetValue(object value)
        {
            if (value != null)
                Value = (bool)value;
        }

        public override object GetValue()
        {
            return Value;
        }

        // Sets the value as a bool
        public void SetValue(bool value)
        {
            Value = value;
        }

        public void Update()
        {
            if (Status.OldValue != Status.Value)
            {
                EventValueChanged.Invoke(this);
                Status.OldValue = Status.Value;
            }
        }

    }
}