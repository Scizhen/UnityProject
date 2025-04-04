using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(SuperSpline))]
public partial class SplineEditor : InstantInspector
{
	//Hi! Thank you very much for buying the pro-version of SuperSplines!
	
	[MenuItem( "GameObject/Create Other/Spline/Hermite" )]
	static void CreateHermiteSpline( )
	{
		SuperSpline spline = CreateSplineGameObject( );
		
		spline.interpolationMode = SuperSpline.InterpolationMode.Hermite;
		
		SetupChildren( spline );
	}
	
	[MenuItem( "GameObject/Create Other/Spline/Bezier" )]
	static void CreateBezierSpline( )
	{
		SuperSpline spline = CreateSplineGameObject( );
		
		spline.interpolationMode = SuperSpline.InterpolationMode.Bezier;
		
		SetupChildren( spline );
	}
	
	[MenuItem( "GameObject/Create Other/Spline/B-Spline" )]
	static void CreateBSpline( )
	{
		SuperSpline spline = CreateSplineGameObject( );
		
		spline.interpolationMode = SuperSpline.InterpolationMode.BSpline;
		
		SetupChildren( spline );
	}
	
	private static SuperSpline CreateSplineGameObject( )
	{
		// Undo.RegisterSceneUndo( "Create new spline" );
		
		GameObject gObject = new GameObject( );
		
		gObject.name = "New Spline";
		
		gObject.transform.localPosition = Vector3.zero;
		gObject.transform.localRotation = Quaternion.identity;
		gObject.transform.localScale = Vector3.one;
		
		Selection.activeGameObject = gObject;
		
		return gObject.AddComponent<SuperSpline>( );
	}
	
	private static void SetupChildren( SuperSpline spline )
	{
		for( int i = 0; i < 4; i++ )
		{
			GameObject newNode = spline.AddSplineNode( );
			
			newNode.name = GetNodeName( i );
			newNode.transform.parent = spline.transform;
			newNode.transform.localPosition = -Vector3.forward * 1.5f + Vector3.forward * i + ( Vector3.right * ((i%3==0) ? 0 : ((i%3) - 1.5f )) );
			newNode.transform.localRotation = Quaternion.identity;
			newNode.transform.localScale = Vector3.one;
		}
	}
	
	private static string GetNodeName( int num )
	{
		string res = "";
		
		for( int i = 1; i<4; i++ )
			if( num < Mathf.Pow( 10, i ) )
				res += "0";
		
		return( res + num.ToString( ) );
	}
}
