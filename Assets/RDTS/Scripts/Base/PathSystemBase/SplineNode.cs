using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// This class represents a control node of the Spline.
/// 弯曲运输机中表示样条的控制节点。
/// </summary>
/// <remarks>
/// This class stores data about the position and orientation of the control node as well as information about 这个类存储关于控制节点的位置和方向的数据，以及与控制节点相关联的齿条参数的信息，以及到下一个相邻控制节点的归一化距离。
/// the spline parameter that is associated to the control node and the normalized distance to the next adjacent control node.
/// For advanced use there is also a custom value that will be interpolated according to the interpolation mode that is used to calculate the spline.对于高级使用，还有一个自定义值，该值将根据用于计算样条的插值模式进行插值。
/// </remarks>
[AddComponentMenu("SuperSplines/Spline Node")]
public class SplineNode : MonoBehaviour 
{
	public Vector3 Position { get{ return transform.position; } set{ transform.position = value; } } 	///< Quick access to the control node's position. 快速访问控制节点的位置。
	public Quaternion Rotation { get{ return transform.rotation; } set{ transform.rotation = value; } } ///< Quick access to the control node's orientation.快速访问控制节点的方向。

    public float CustomValue{ get{ return customValue; } set{ customValue = value; } }                  ///< Quick access to the control node's custom value.快速访问控制节点的自定义值。

    public Vector3 TransformedNormal{ get{ return transform.TransformDirection( normal ); } }           ///< Quick access to the control node's transformed normal.快速访问控制节点的转换正常。

    public float customValue = 0f;									///< A custom value that can be interpolated by the Spline-class, for advanced applications 可由样条类内插的自定义值，适用于高级应用程序
	public float tension = 1f;										///< Specifies the curve's tension at the node's position (used for hermite-interpolation) 指定曲线在节点位置的张力(用于隐士插值)
	public Vector3 normal = Vector3.up;                             ///< Represents the spline's up-vector / normal at the node's position 表示样条在节点位置的上向量/法向量

                                                                    /// <summary>
                                                                    /// A dictionary of the spline's parameters.
                                                                    /// </summary>
                                                                    /// <remarks>
                                                                    /// Elements of the dictionary can be accessed by the spline that the parameter's are needed for.
                                                                    /// </remarks>
                                                                    /// <example>
                                                                    /// Get the location of the node on a specific spline:
                                                                    /// <code>
                                                                    /// 
                                                                    /// Spline mySpline;
                                                                    /// SplineNode node;
                                                                    /// 
                                                                    /// float splineParameter = node.Parameters[mySpline].PosOnSpline;
                                                                    /// float distanceToNextNode = node.Parameters[mySpline].Length;
                                                                    /// 
                                                                    /// </code>
                                                                    /// </example>
                                                                    /// <value>
                                                                    /// The parameters.
                                                                    /// </value>
    public NodeParameterRegister Parameters{ get{ return parameters; } }
	private NodeParameterRegister parameters = new NodeParameterRegister( );

    /// <summary>
    /// A register for spline related node parameters. 与样条相关的节点参数寄存器。
    /// </summary>
    /// <remarks>
    /// The node's parameters can be accessed using the indexer with a specific spline as parameter.节点的参数可以使用以特定样条为参数的索引器来访问。
    /// </remarks>
    public sealed class NodeParameterRegister : Dictionary<SuperSpline,NodeParameters>
	{
        /// <summary>
        /// Gets the <see cref="NodeParameters"/> for the specified spline.获取指定样条的<see cref="NodeParameters"/>。
        /// </summary>
        /// <remarks>
        /// If there is no entry for the passed Spline reference, a new instance of NodeParameters will be generated.如果传递的样条引用没有条目，则会生成一个新的NodeParameters实例。
        /// </remarks>
        /// <param name='spline'>
        /// A spline that the node is used in.
        /// </param>
        public new NodeParameters this[SuperSpline spline]
		{
			get
			{
				if( !ContainsKey( spline ) )
					Add( spline, new NodeParameters( spline, 0, 0 ) );
			
				return base[spline];
			}
		}
	}
	
//	private void OnDrawGizmosSelected( )
//	{
//		Gizmos.color = new Color( 1f, 0.5f, 0f, 0.75f );
//		Gizmos.DrawSphere( Position, GetSizeMultiplier( this ) * 2.5f );
//	}
	
//	Copied from SplineGizmos.cs
//	private float GetSizeMultiplier( SplineNode node )
//	{
//		if( !Camera.current.orthographic )
//		{
//			Plane screen = new Plane( );
//			
//			float sizeMultiplier = 0f;
//			
//			screen.SetNormalAndPosition( Camera.current.transform.forward, Camera.current.transform.position );
//			screen.Raycast( new Ray( node.Position, Camera.current.transform.forward ), out sizeMultiplier );
//			
//			return sizeMultiplier * .0075f;
//		}
//	
//		return Camera.current.orthographicSize * 0.01875f;
//	}
}

/// <summary>
/// The parameters of SplineNode that specify its position on the spline in relation to its length. 
/// </summary>
public class NodeParameters
{
	public double position;
	public double length;
	
	public SuperSpline spline;
	
	public float PosInSpline{ get{ return (float) position; } } 	///< Normalized position on the spline (parameter from 0 to 1).
	public float Length{ get{ return (float) length; } } 			///< Normalized distance to the next adjacent node.
	
	/// <summary>
	/// Initializes a new instance of the <see cref="NodeParameters"/> class.
	/// </summary>
	/// <param name='spline'>
	/// The Spline that the node belongs to.
	/// </param>
	/// <param name='position'>
	/// The spline parameter representing this node.
	/// </param>
	/// <param name='length'>
	/// The normalized distance to the next node.
	/// </param>
	public NodeParameters( SuperSpline spline, float position, float length )
	{
		this.position = position;
		this.length = length;
		this.spline = spline;
	}
	
	/// <summary>
	/// Reset position and length of the node to zero.
	/// </summary>
	public void Reset( )
	{
		position = 0;
		length = 0;
	}
}
