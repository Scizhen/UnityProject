using UnityEngine;
using System.Collections.Generic;

namespace RDTS
{
    //可能是控制弯曲运输机移动的脚本？

    public class CurvedConveying : BaseConveying
    {
        public Drive Drive;
        public float Velocity;
        public bool Reverse;
        public PhysicMaterial NoFriction;
        public PhysicMaterial Kinematic;
        public float factor;
        public float InnerRadius, OuterRadius;
        private float curveFactor;

        private void Start()
        {
            collider = GetComponent<Collider>();
            dynamics = new List<MoveMU>();
            removeDynamics = new List<MoveMU>();
        }

        public void Update()
        {
     

            Velocity = Drive.CurrentSpeed / 1000;
            if (Velocity != 0)
            {
                collider.sharedMaterial = NoFriction;
            }
            else
            {
                collider.sharedMaterial = Kinematic;
            }

            removeDynamics.Clear();

            foreach (MoveMU dynObject in dynamics)
            {
                factor = GetOverlapping(collider, dynObject);
                if (factor == -1)
                {
                    removeDynamics.Add(dynObject);
                }
                else
                {
                    if (factor >= 0.5f)
                    {
                        curveFactor = (dynObject.transform.position - transform.position).magnitude /
                                      (InnerRadius + 0.5f * (OuterRadius - InnerRadius));
                        if (Reverse)
                        {
                            dynObject.Direction += 1 * Vector3.Cross(Vector3.up,
                                dynObject.transform.position - transform.position);
                        }
                        else
                        {
                            dynObject.Direction += -1*Vector3.Cross(Vector3.up,
                                dynObject.transform.position - transform.position);
                        }
                        // Global.DebugArrow(dynObject.transform.position, dynObject.Direction);
                        dynObject.Velocity += Velocity;
                        dynObject.Align = true;
                    }
                }
            }

            foreach (MoveMU dynObject in removeDynamics)
            {
                dynamics.Remove(dynObject);
            }
        }
    }
}