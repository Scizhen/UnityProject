using UnityEngine;
using System.Collections.Generic;

namespace RDTS
{
    //Conveying脚本的基类
    public class BaseConveying : MonoBehaviour
    {
        protected MoveMU dynObject;
        protected List<MoveMU> dynamics;
        protected List<MoveMU> removeDynamics;
        protected new Collider collider;
        
        protected class Area
        {
            public float Value;
            public bool Empty;
            public float x;
            public float z;
            public float xLen;
            public float zLen;
            public float xMin;
            public float xMax;
            public float zMin;
            public float zMax;

            public Area(float x, float z, float xLen, float zLen)
            {
                this.x = x;
                this.z = z;
                this.xLen = xLen;
                this.zLen = zLen;
                this.xMin = x - xLen / 2.0f;
                this.xMax = x + xLen / 2.0f;
                this.zMin = z - zLen / 2.0f;
                this.zMax = z + zLen / 2.0f;
                this.Value = xLen * zLen;
                Empty = this.Value == 0.0f;
            }

            public Area(Vector3 pos, Vector3 size)
            {
                this.x = pos.x;
                this.z = pos.z;
                this.xLen = size.x;
                this.zLen = size.z;
                this.xMin = x - xLen / 2.0f;
                this.xMax = x + xLen / 2.0f;
                this.zMin = z - zLen / 2.0f;
                this.zMax = z + zLen / 2.0f;
                this.Value = xLen * zLen;
                Empty = this.Value == 0.0f;
            }

            public Area()
            {
                this.x = 0;
                this.z = 0;
                this.xMin = 0;
                this.xMax = 0;
                this.zMin = 0;
                this.zMax = 0;
                this.Value = 0;
                Empty = true;
            }

            public static Area Intersect(Area area1, Area area2)
            {
                float xDistance = 0.0f;
                float zDistance = 0.0f;
                Area intersection = new Area();

                if (area2.xMax <= area1.xMin - 0.01f)
                {
                    return null;
                }

                if (area2.xMin >= area1.xMax + 0.01f)
                {
                    return null;
                }

                if (area2.zMax <= area1.zMin - 0.01f)
                {
                    return null;
                }

                if (area2.zMin >= area1.zMax + 0.01f)
                {
                    return null;
                }

                if (area2.xMax <= area1.xMin)
                {
                    return intersection;
                }

                if (area2.xMin >= area1.xMax)
                {
                    return intersection;
                }

                if (area2.zMax <= area1.zMin)
                {
                    return intersection;
                }

                if (area2.zMin >= area1.zMax)
                {
                    return intersection;
                }

                if (area2.xMax > area1.xMax)
                {
                    xDistance = area2.xMax - area1.xMax;
                }
                else if (area2.xMin < area1.xMin)
                {
                    xDistance = area1.xMin - area2.xMin;
                }

                if (area2.zMax > area1.zMax)
                {
                    zDistance = area2.zMax - area1.zMax;
                }
                else if (area2.zMin < area1.zMin)
                {
                    zDistance = area1.zMin - area2.zMin;
                }

                if (xDistance == 0 && zDistance == 0)
                {
                    return area2;
                }

                intersection.Set(area2.x, area2.z, area2.xLen - xDistance, area2.zLen - zDistance);
                return intersection;
            }

            public void Move(float x, float z)
            {
                this.x += x;
                this.z += z;
                this.xMax += x;
                this.xMin += x;
                this.zMax += z;
                this.zMin += z;
            }

            public void Clear()
            {
                this.x = 0;
                this.z = 0;
                this.xMin = 0;
                this.xMax = 0;
                this.zMin = 0;
                this.zMax = 0;
                this.Value = 0;
                Empty = true;
            }

            public void Set(float x, float z, float xLen, float zLen)
            {
                this.x = x;
                this.z = z;
                this.xLen = xLen;
                this.zLen = zLen;
                this.xMin = x - xLen / 2.0f;
                this.xMax = x + xLen / 2.0f;
                this.zMin = z - zLen / 2.0f;
                this.zMax = z + zLen / 2.0f;
                this.Value = xLen * zLen;
                Empty = this.Value == 0.0f;
            }
        }

        public static float GetOverlapping(Collider collider, MoveMU obj)
        {
            Area area1 = new Area(collider.bounds.center, collider.bounds.size);
            Area area2 = new Area(obj.transform.position, obj.BoxCollider.bounds.size);
            //Global.DebugGlobalAxis(collider.bounds.center,1);
            //Global.DebugGlobalAxis(obj.transform.position,1);
            Area intersection = Area.Intersect(area1, area2);
            if (intersection == null)
            {
                return -1;
            }

            if (!intersection.Empty)
            {
                return intersection.Value / area2.Value;
            }

            return 0;
        }
        
        public List<MoveMU> getAllDynamics()
        {
            return dynamics;
        }

        public void LockAllDynamics(GameObject parent)
        {
            foreach (MoveMU currDyn in dynamics)
            {
                if (currDyn.transform.parent != parent.transform)
                {
                    currDyn.GetComponent<Rigidbody>().velocity = Vector3.zero;
                    currDyn.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
                    currDyn.GetComponent<Rigidbody>().isKinematic = true;
                    currDyn.transform.parent = parent.transform;
                }
            }
        }

        public void UnlockAllDynamics(GameObject parent)
        {
            foreach (MoveMU currDyn in dynamics)
            {
                if (currDyn.transform.parent != parent.transform)
                {
                    currDyn.GetComponent<Rigidbody>().isKinematic = false;
                    currDyn.GetComponent<Rigidbody>().velocity = Vector3.zero;
                    currDyn.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
                    currDyn.transform.parent = parent.transform;
                }
            }
        }
     
        private void OnCollisionEnter(Collision other)
        {
            if (!isActiveAndEnabled)
                return;
            dynObject = other.gameObject.GetComponent<MoveMU>();
            if (dynObject != null)
            {
                if (!dynamics.Contains(dynObject))
                {
                    dynamics.Add(dynObject);
                }
            }
        }

        private void OnCollisionExit(Collision other)
        {
            if (!isActiveAndEnabled)
                return;
            dynObject = other.gameObject.GetComponent<MoveMU>();

            if (!dynObject.GetComponent<Rigidbody>().isKinematic)
            {
                if (dynObject != null)
                {
                    if (dynamics.Contains(dynObject))
                    {
                        dynamics.Remove(dynObject);
                    }
                }
            }
        }

    }
}