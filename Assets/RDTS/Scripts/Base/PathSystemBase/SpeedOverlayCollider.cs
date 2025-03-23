using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using RDTS.Method;
using RDTS;
using UnityEngine;



namespace RDTS
{
    //与运输机上的滚轮碰撞检测有关
    public class SpeedOverlayCollider : LibraryPart
    {

        public Drive DirectionDrive;
        public Drive SpeedDrive;
        public bool OverlaySpeed;
        public bool DebugMode = false;
        private List<Rigidbody> rigidbodies = new List<Rigidbody>();
        private Rigidbody rbold;
        private bool hitbefore;

        private Hashtable ht = new Hashtable();


        public void OnTriggerEnter(Collider other)
        {
            rigidbodies.Add(other.attachedRigidbody);

            ht.Add(other.attachedRigidbody, other.attachedRigidbody.velocity);
        }

        public void OnTriggerExit(Collider other)
        {
            rigidbodies.Remove(other.attachedRigidbody);

            ht.Remove(other.attachedRigidbody);
        }

        private void FixedUpdate()
        {
            Quaternion rotation = Quaternion.AngleAxis(DirectionDrive.CurrentPosition, Vector3.up);
            var directionspeed = rotation * transform.right;
            foreach (var rb in rigidbodies)
            {
                if (OverlaySpeed)
                {
                    var vel = (Vector3)ht[rb] + directionspeed * SpeedDrive.CurrentSpeed / 1000;
                    var vectorside = new Vector3(0, 0, vel.z);
                    var speeddrive = (Vector3)ht[rb];
                    var vectorforward = new Vector3(vel.x, 0, 0);
                    if (DebugMode)
                    {
                        Global.DebugArrow(this.transform.position, vel, 0);
                        Global.DebugArrow(this.transform.position, vectorside, Color.green, 0);
                        Global.DebugArrow(this.transform.position, vectorforward, Color.blue, 0);
                    }

                    rb.velocity = vel;
                }
                else
                {
                    var vel = directionspeed * SpeedDrive.CurrentSpeed / 1000;
                    if (DebugMode)
                        Global.DebugArrow(this.transform.position, vel, 0);
                    rb.velocity = vel;

                }
            }

        }
    }
}