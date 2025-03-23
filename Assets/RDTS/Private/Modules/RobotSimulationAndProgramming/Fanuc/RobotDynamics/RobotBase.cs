using RobotDynamics.Controller;
using RobotDynamics.MathUtilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class RobotBase : MonoBehaviour
{
    [Range(0, 1)]
    [Tooltip("A vlue used for pseudo inverses of matrices to stabalize in the case of singularities. Is added to the diagonal entries of the matrix before inverting.")]
    public float Lambda = 0.001f;
    [Range(0, 1)]
    [Tooltip("Step size factor in the numeric algorithm. [q = q + dq * alpha]")]
    public float Alpha = 0.05f;
    [Tooltip("If true, then linear joints will make larger step sizes")]
    public bool PrioritizeLinearJoints = true;
    [Range(0, 5)]
    [Tooltip("Proportional controller parameter used for smoothing the angles")]
    public float Kp = 2;
    [Range(0, 500)]
    [Tooltip("The maximum number of iterations per update inversekinematics call")]
    public int MaxIter = 100;
    [Tooltip("The tolerance of the endeffector's position and orientation")]
    public float Tolerance = 0.001f;

    ///public GameObject offset;//*

    public void Start()
    {
        Robot.AttachJointController(Kp, 0.01f);
        Robot.JointController.jointsChangedEvent += ControlledJointsChanged;

        alpha = Matrix.Eye(Robot.Links.Count) * Alpha;

        if (PrioritizeLinearJoints)
        {
            for (int i = 0; i < Robot.Links.Count; i++)
            {
                if (Robot.Links[i].Type == RobotDynamics.Robots.Link.JointType.Linear)
                {
                    alpha.matrix[i, i] *= 20;
                }
            }
        }

    }

    protected RobotDynamics.Robots.Robot Robot;
    public List<JointHandler> Joints;

    public bool EnableInverseKinematics = false;
    public bool EnableForwardKinematics = false;
    public bool EnablePController = false;
    public bool UseLastQAsInit = true;

    private double[] last_q;
    public double[] ForwardKinematicsQ;
    private Matrix alpha;

    protected void SetQ(double[] q)
    {
        var transformations = Robot.ComputeForwardKinematics(q);

        for (int i = 0; i < transformations.Count - 1; i++)
        {
            Joints[i].SetJointValue(transformations[i], transformations[i + 1], Robot.Links[i], q[i]);
        }
    }


    private Vector3 lastPos;
    private Quaternion lastRot;
    public void FollowTargetOneStep(GameObject Target)
    {

        if (EnableInverseKinematics)
        {
            if (Target.transform.localPosition != lastPos || Target.transform.localRotation != lastRot)
            {
                lastPos = Target.transform.localPosition;
                lastRot = Target.transform.localRotation;

                ////目标点偏移量测试
                ////Vector3 offset = Target.transform.localPosition + new Vector3(0f, 0f, 0.5f);
                ////Transform newEndPoint = Instantiate(Target.transform);  
                ////newEndPoint.position +=  new Vector3(0f, -50f, 0f);

                ////Vector3 pos = Target.transform.localPosition;
                ////Transform newEndPoint2;
                ////newEndPoint2.position = Target.transform.localPosition;

                ////offsetTransform.position = Target.transform.position;
                ////offsetTransform.rotation = Target.transform.rotation;
                ////offsetTransform.position += new Vector3(0f, -50f, 0f);

                ////Vector r_des = offsetTransform.transform.localPosition.ToVector();
                ////RotationMatrix C_des = offsetTransform.transform.EulerAnglesToRotationMatrix();

                ////Inverse Kinematics
                //offset.transform.localPosition = Target.transform.localPosition;// + new Vector3(0f, -50f, 0f);
                //offset.transform.localRotation = Target.transform.localRotation;

                //Vector r_des = offset.transform.localPosition.ToVector();
                //RotationMatrix C_des = Target.transform.EulerAnglesToRotationMatrix();


                //Inverse Kinematics
                Vector r_des = Target.transform.localPosition.ToVector();
                RotationMatrix C_des = Target.transform.EulerAnglesToRotationMatrix();


                var result = Robot.ComputeInverseKinematics(r_des, C_des, alpha, Lambda, MaxIter, Tolerance, UseLastQAsInit ? last_q : null);

                if (result.DidConverge)
                {
                    last_q = result.q;

                    if (!EnablePController)
                    {
                        SetQ(result.q);
                    }
                }
            }
        }
    }



    private void ControlledJointsChanged(object sender, JointsChangedEventArgs e)
    {
        if (!EnablePController) return;

        if (e.DidConverge)
            SetQ(e.joint_values);
    }
}
