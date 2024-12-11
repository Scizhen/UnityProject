using UnityEngine;

//This class computes a point on a spline that is as close as possible to a given gameobject
//弯曲运输机中，这个类计算一个尽可能接近给定游戏对象的样条上的点
[AddComponentMenu("SuperSplines/Animation/Closest Point Animator")]
public class SplineAnimatorClosestPoint : MonoBehaviour
{
	public SuperSpline spline;
	
	public WrapMode wMode = WrapMode.Clamp;
	
	public Transform target;
	
	public int iterations = 5;
	public float diff = 0.5f;
	public float offset = 0;
	
//	private float lastParam;
	
	void Update( ) 
	{
		if( target == null || spline == null )
			return;
		
		float param = WrapValue( spline.GetClosestPointParam( target.position, iterations ) + offset, 0f, 1f, wMode );
//		float param = WrapValue( spline.GetClosestPointParam( target.position, iterations, lastParam, diff ) + offset, 0f, 1f, wMode );
		
		transform.position = spline.GetPositionOnSpline( param );
		transform.rotation = spline.GetOrientationOnSpline( param );
		
//		lastParam = param;
	}
	
	private float WrapValue( float v, float start, float end, WrapMode wMode )
	{
		switch( wMode )
		{
		case WrapMode.Clamp:
		case WrapMode.ClampForever:
			return Mathf.Clamp( v, start, end );
		case WrapMode.Default:
		case WrapMode.Loop:
			return Mathf.Repeat( v, end - start ) + start;
		case WrapMode.PingPong:
			return Mathf.PingPong( v, end - start ) + start;
		default:
			return v;
		}
	}
}
