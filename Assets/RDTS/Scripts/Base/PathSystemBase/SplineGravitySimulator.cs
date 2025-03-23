using UnityEngine;

//This class applies gravity towards a spline to rigidbodies that this script is attached to
//弯曲运输机中，这个类将重力作用于这个脚本附加到的刚体上的样条
[AddComponentMenu("SuperSplines/Animation/Gravity Animator")]
public class SplineGravitySimulator : MonoBehaviour
{
	public SuperSpline spline;
	
	public float gravityConstant = 9.81f;
	
	public int iterations = 5;
	
	void Start( )
	{
		//Disable default gravity calculations
		GetComponent<Rigidbody>().useGravity = false;
	}
	
	void FixedUpdate( ) 
	{
		if( GetComponent<Rigidbody>() == null || spline == null )
			return;
		
		Vector3 closestPointOnSpline = spline.GetPositionOnSpline( spline.GetClosestPointParam( GetComponent<Rigidbody>().position, iterations ) ); 
		Vector3 shortestConnection = closestPointOnSpline - GetComponent<Rigidbody>().position;
		
		//Calculate gravity force according to Newton's law of universal gravity
		Vector3 force = shortestConnection * Mathf.Pow( shortestConnection.magnitude, -3 ) * gravityConstant * GetComponent<Rigidbody>().mass;
		
		GetComponent<Rigidbody>().AddForce( force );
	}
}
