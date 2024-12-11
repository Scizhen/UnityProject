using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using Random = UnityEngine.Random;


namespace RDTS
{
    [SelectionBase]
    public class Aligner : LibraryObject
    {

        [Header("Operation Mode")]
        public bool AllignPivotPoint = true;
        public bool AddForces = true;

        [Header("Force Settings")]
        [ShowIf("AddForces")]
        public bool OnlyMUsWithSameTag = false;
        [ShowIf("AddForces")] public bool ForcesProportionalToMass;
        [ShowIf("AddForces")] public float MassFactor = 1f;
        [ShowIf("AddForces")] public float Torque = 1f;
        [ShowIf("AddForces")] public bool AddLinearForce = false;
        [ShowIf("AddForces")] public Vector3 LinearForce = new Vector3(0, 0, 0);
        [ShowIf("AddForces")] public bool DebugMode = false;

        Hashtable distances = new Hashtable();
        [HideInInspector] public List<MU> AlignMUs;
        [HideInInspector] public List<MU> ForceMUs;


        public override void OnParentModify()
        {
            Modify();
        }



        public void OnEnter(Collider other)
        {

            var mu = other.gameObject.GetComponent<MU>();
            if (mu == null)
                mu = other.gameObject.GetComponentInParent<MU>();
            if (mu == null)
                return;
            if (OnlyMUsWithSameTag && mu.tag != this.tag)
                return;
            if (AllignPivotPoint)
            {
                AlignMUs.Add(mu);
                distances.Add(mu, Vector3.Distance(mu.transform.position, this.transform.position));
            }

            if (AddLinearForce || AddForces)
            {
                ForceMUs.Add(mu);
            }

        }

        public void OnExit(Collider other)
        {
            var mu = other.gameObject.GetComponent<MU>();
            if (mu == null)
                mu = other.gameObject.GetComponentInParent<MU>();
            if (mu == null)
                return;
            if (OnlyMUsWithSameTag && mu.tag != this.tag)
                return;
            distances.Remove(mu);
            if (AllignPivotPoint)
                AlignMUs.Remove(mu);
            if (AddLinearForce || AddForces)
                ForceMUs.Remove(mu);
        }

        public override void OnModify()

        {
            this.transform.position = new Vector3(this.transform.position.x, BottomHeight, this.transform.position.z);

            if (Parent != null)
            {

                if (Parent.GetType() == typeof(StraightConveyor))
                {
                    var par = (StraightConveyor)Parent;
                    BottomHeight = par.BottomHeight;

                }
                var parentcirc = GetComponentInParent<CurvedConveyor>();
                if (Parent.GetType() == typeof(CurvedConveyor))
                {
                    var par = (CurvedConveyor)Parent;
                    BottomHeight = par.BottomHeight;
                }
            }

        }


        void Align(MU mu)
        {
            if (AllignPivotPoint)
            {
                mu.transform.position = this.transform.position;
                mu.transform.rotation = this.transform.rotation;
            }



            distances.Remove(mu);
            AlignMUs.Remove(mu);
        }

        void AddForce()
        {
            float mass = 1;
            Vector3 globallinear = transform.TransformDirection(LinearForce);
            foreach (var mu in ForceMUs)
            {
                var rb = mu.GetComponent<Rigidbody>();
                if (ForcesProportionalToMass)
                {
                    mass = rb.mass * rb.mass;
                };
                if (AddForces)
                {

                    rb.AddTorque(rb.transform.up * Torque * mass, ForceMode.Force);
                    if (DebugMode)
                        Method.Global.DebugArrow(rb.position, rb.transform.up * Torque, Color.yellow);
                    
                }

                if (AddLinearForce)
                {
                    rb.AddForce(globallinear * mass * MassFactor, ForceMode.Force);
                    if (DebugMode)
                        Method.Global.DebugArrow(rb.position, globallinear, Color.green);
                }
            }

        }

        void Start()
        {
            AlignMUs = new List<MU>();
            ForceMUs = new List<MU>();
            var sensor = GetComponentInChildren<ProximitySensor>();
            sensor.OnEnter += OnEnter;
            sensor.OnExit += OnExit;
            var mesh = GetComponentInChildren<MeshRenderer>();
            mesh.enabled = DisplayStatus;
        }

        void FixedUpdate()
        {
            // Check if transportable is in station center
            int i = 0;
            while (i < AlignMUs.Count)
            {
                var mu = AlignMUs[i];
                var distance = Vector3.Distance(mu.transform.position, this.transform.position);
                if (distance > (float)distances[mu])
                {
                    Align(mu);
                }
                else
                {
                    distances[mu] = distance;
                    i++;
                }
            }
            if (AddLinearForce || AddForces)
            {
                AddForce();
            }

        }

        public override bool HideAllowed(GameObject gameObject)
        {

            return false;
        }

    }
}