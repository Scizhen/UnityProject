using UnityEngine;

//This class animates a gameobject along the spline at a specific speed. 
//Also it demonstrates how to use the 'custom value feature'. In this example it alters
//the color of an attached mesh renderer. The custom values for the nodes can be set via script
//or in the Inspector - simply click on a gameObject that has the SplineNode-component 
//attached to it.
////这个类以特定的速度沿着样条动画一个游戏对象。它还演示了如何使用“自定义值特性”。
//在这个例子中，它改变了附加网格渲染器的颜色。节点的自定义值可以通过脚本或在检查器中设置-只需点击一个附加了splinenode组件的gameObject。
[AddComponentMenu("")]
[RequireComponent( typeof( MeshRenderer ) )]
public class SplineAnimatorCustomValue : MonoBehaviour
{
	public SuperSpline spline;
	
	public WrapMode wrapMode = WrapMode.Clamp;
	
	public float speed = 1f;
	public float offSet = 0f;
	
	public float passedTime = 0f;
	
	void Update( ) 
	{
		passedTime += Time.deltaTime * speed;
		
		float clampedParam = WrapValue( passedTime + offSet, 0f, 1f, wrapMode );
		
		//Interpolate a custom value according to the interpolation type and spline settings
		//The custom values can be set in the SplineNode scripts or in the inspector
		float customValue = spline.GetCustomValueOnSpline( clampedParam );
		
		transform.position = spline.GetPositionOnSpline( clampedParam );
		transform.rotation = spline.GetOrientationOnSpline( clampedParam );
		
		GetComponent<Renderer>().material.color = Color.red * (1f-customValue) + Color.blue * (customValue);
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
