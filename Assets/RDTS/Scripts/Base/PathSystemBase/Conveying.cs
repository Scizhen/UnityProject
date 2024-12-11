using UnityEngine;
using System.Collections.Generic;

namespace RDTS
{
    //�����Ǹ������������������˶�ʱ�Ĳ��ʺͷ���Ľű�
    public class Conveying : BaseConveying
    {
        public Drive Drive;
        public Vector3 Direction;
        public float Velocity;
        public bool Reverse;
        public PhysicMaterial NoFriction;
        public PhysicMaterial Kinematic;
        
    
       
        private float factor;

        private void Start()
        {
            collider = GetComponent<Collider>();
            dynamics = new List<MoveMU>();
            removeDynamics = new List<MoveMU>();
        }
       
        public void Update()
        {
            var direction = transform.TransformDirection(Direction);
            Velocity = Drive.CurrentSpeed/1000;
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
                        if (Reverse)
                        {
                            dynObject.Direction += -1 * direction;
                        }
                        else
                        {
                            dynObject.Direction += direction;
                        }

                        dynObject.Velocity += Velocity;
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