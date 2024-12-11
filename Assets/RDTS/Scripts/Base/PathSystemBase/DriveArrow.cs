using RDTS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RDTS
{
    //与Drive绑定来更改箭头方向
    public class DriveArrow : MonoBehaviour
    {

        public Drive Drive;
        public Drive DirectionDrive;

        private Material material;

        private Vector3 forwardscale = new Vector3(-1, 1, 1);
        private Vector3 backwardscale = new Vector3(1, 1, 1);

        private bool directiondrivenotnull;

        // Start is called before the first frame update
        void Start()
        {
            material = GetComponent<MeshRenderer>().material;
            directiondrivenotnull = DirectionDrive != null;
        }

        // Update is called once per frame
        void Update()
        {
            if (Drive.CurrentSpeed != 0)
                material.color = new Color(0, 1, 0, 0.05f);
            else
                material.color = new Color(1, 0, 0, 0.05f);

            if (Drive.CurrentSpeed > 0)
                gameObject.transform.localScale = forwardscale;
            if (Drive.CurrentSpeed < 0)
                gameObject.transform.localScale = backwardscale;
            if (directiondrivenotnull)
            {
                this.gameObject.transform.localRotation = Quaternion.Euler(new Vector3(0, DirectionDrive.CurrentPosition, 0));
            }
        }
    }
}

