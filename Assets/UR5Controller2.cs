
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Collections.Generic;


namespace RDTS.RobotSimulationProgramming
{

    [ExecuteInEditMode]
    public class UR5Controller2 : MonoBehaviour
    {

        public GameObject RobotBase;
        public GameObject controlCube;

        public GameObject[] jointList = new GameObject[6];//记录每个运动的关节
        public float[] jointValues = new float[6];
        public float x = 0, y = 6, z = 3, phi = 0, theta = 0, psi = 0, oldX, oldY, oldZ, oldPhi, oldTheta, oldPsi;
        public string stringX, stringY, stringZ, stringPhi, stringTheta, stringPsi;
        public bool userHasHitOk = false;

        private int counter = 1;
        private int frameCount = 200;
        private float floatX, floatY, floatZ, floatPhi, floatTheta, floatPsi;

        private UR5_Solver Robot1 = new UR5_Solver();
        private UR5_Solver Robot11
        {
            get
            {
                return Robot1;
            }

            set
            {
                Robot1 = value;
            }
        }

        // Use this for initialization
        void Start()
        {
            //initializeJoints();
            initializeCube();
        }

        // Update is called once per frame
        void Update()
        {
            // controlCube.transform.position = new Vector3(floatX, floatY, floatZ);
            // controlCube.transform.eulerAngles = new Vector3(floatPhi, floatTheta, floatPsi);

            oldX = x;
            oldY = y;
            oldZ = z;
            oldPhi = phi;
            oldTheta = theta;
            oldPsi = psi;

            if (userHasHitOk)
            {
                if (counter >= frameCount)
                {
                    counter = 1;
                    // userHasHitOk = false;
                }

                x = oldX + (counter * (controlCube.transform.position.x - oldX) / frameCount);
                y = oldY + (counter * (controlCube.transform.position.y - oldY) / frameCount);
                z = oldZ + (counter * (controlCube.transform.position.z - oldZ) / frameCount);
                phi = controlCube.transform.eulerAngles.x * Mathf.Deg2Rad;
                theta = controlCube.transform.eulerAngles.y * Mathf.Deg2Rad;
                psi = controlCube.transform.eulerAngles.z * Mathf.Deg2Rad;
                //Debug.Log("psi = " + psi);

                Robot11.Solve(x, y, z, phi, theta, psi);

                for (int j = 0; j < 6; j++)
                {
                    Vector3 currentRotation = jointList[j].transform.localEulerAngles;
                    if (!float.IsNaN(Robot11.solutionArray[j]))
                    {
                        currentRotation.z = jointValues[j] = Robot11.solutionArray[j];
                        jointList[j].transform.localEulerAngles = currentRotation;


                    }

                }

                counter++;
            }
        }

        void initializeCube()
        {
            if (controlCube == null)
                controlCube = GameObject.Find("Target");

            controlCube.transform.position = new Vector3(0f, 6f, 3f); //note, this is in format x, y, z - but y is up
            controlCube.transform.localScale = new Vector3(.3f, 1f, .3f);
            controlCube.transform.eulerAngles = new Vector3(0f, 0f, 0f); //in degrees




            floatX = 3f;
            floatY = 4f;
            floatZ = 3f;
            floatPhi = 0f;
            floatTheta = 0f;
            floatPsi = 0f;
        }

        void OnGUI()
        {
            // GUI.color = Color.black;

            // if (userHasHitOk)
            // {
            //     floatX = float.Parse(stringX);
            //     floatY = float.Parse(stringY);
            //     floatZ = float.Parse(stringZ);
            //     floatPhi = float.Parse(stringPhi);
            //     floatTheta = float.Parse(stringTheta);
            //     floatPsi = float.Parse(stringPsi);

            //     //Debug.Log("floatZ = " + floatZ + " and floatY = " + floatY);
            // }
            // else
            // {
            //     GUI.Label(new Rect(10, 10, 200, 30), "Enter X:");
            //     stringX = GUI.TextField(new Rect(150, 10, 100, 25), stringX, 40);
            //     GUI.Label(new Rect(10, 70, 150, 30), "Enter Psi (Degrees):");
            //     stringPsi = GUI.TextField(new Rect(150, 70, 100, 25), stringPsi, 40);
            //     GUI.Label(new Rect(10, 130, 100, 30), "Enter Y:");
            //     stringY = GUI.TextField(new Rect(150, 130, 100, 25), stringY, 40);
            //     GUI.Label(new Rect(10, 190, 150, 30), "Enter Theta (Degrees):");
            //     stringTheta = GUI.TextField(new Rect(150, 190, 100, 25), stringTheta, 40);
            //     GUI.Label(new Rect(10, 250, 100, 30), "Enter Z:");
            //     stringZ = GUI.TextField(new Rect(150, 250, 100, 25), stringZ, 40);
            //     GUI.Label(new Rect(10, 310, 150, 30), "Enter Phi (Degrees):");
            //     stringPhi = GUI.TextField(new Rect(150, 310, 100, 25), stringPhi, 40);



            // }

            // if (GUI.Button(new Rect(10, 370, 50, 30), "Enter"))
            //     userHasHitOk = true;
        }

        // Create the list of GameObjects that represent each joint of the robot
        void initializeJoints()
        {
            var RobotChildren = RobotBase.GetComponentsInChildren<Transform>();
            for (int i = 0; i < RobotChildren.Length; i++)
            {
                if (RobotChildren[i].name == "Axis1")
                {
                    jointList[0] = RobotChildren[i].gameObject;
                }
                else if (RobotChildren[i].name == "Axis2")
                {
                    jointList[1] = RobotChildren[i].gameObject;
                }
                else if (RobotChildren[i].name == "Axis3")
                {
                    jointList[2] = RobotChildren[i].gameObject;
                }
                else if (RobotChildren[i].name == "Axis4")
                {
                    jointList[3] = RobotChildren[i].gameObject;
                }
                else if (RobotChildren[i].name == "Axis5")
                {
                    jointList[4] = RobotChildren[i].gameObject;
                }
                else if (RobotChildren[i].name == "Axis6")
                {
                    jointList[5] = RobotChildren[i].gameObject;
                }
            }
        }
    }


}