using UnityEngine;
using System.Collections.Generic;

//弯曲运输机中对编辑器中添加弯曲点的脚本？
[ExecuteInEditMode]
public class SplineClone : MonoBehaviour
{
	public SuperSpline Spline;
	public GameObject ClonedObj;
	public float Distance;
	public float StartOffset;
	public Vector3 PosOffset;
	public Vector3 RotOffset;
	public Material Material;
	public bool Align;

	private Vector3 oldDirection;
	private int count;
	private float pos;
	private float t;
	private float oldLength;
	private MeshRenderer Mrenderer;
	private float distance;
	private int oldCount;
	private List<GameObject> clonedObjs;

	private void Update()
	{
		if (Spline != null & ClonedObj != null)
		{
			if (Spline.Length != oldLength)
			{
				Modify();				
				oldLength = Spline.Length;
			}
		}
	}

	
	public void Modify ()
	{
		//gameObject.RemoveModifiedChildren();
		Spline.UpdateSpline();
		count = Mathf.CeilToInt(Spline.Length / Distance);
		count = Mathf.Clamp(count, 2, int.MaxValue);
		if (Align)
		{
			distance = (Spline.Length - 2 * StartOffset) / (count - 1);
		}
		else
		{
			distance = Distance;
		}

		clonedObjs = new List<GameObject>();
		pos = StartOffset;
		for (int i = 1; i <= count; i++)
		{
			if (pos <= Spline.Length)
			{
				t = pos / Spline.Length;
				GameObject newObj = GameObject.Instantiate(ClonedObj) as GameObject;
				newObj.tag = "Modified";
				newObj.name = ClonedObj.name + "_" + i.ToString();
				Mrenderer = newObj.GetComponent<MeshRenderer>();
				if (Mrenderer != null)
				{
					Mrenderer.sharedMaterial = Material;
				}
				newObj.transform.position = Spline.GetPositionOnSpline(t);
				newObj.transform.Translate(PosOffset);
				newObj.transform.rotation = Spline.GetOrientationOnSpline(t);
				newObj.transform.Rotate(RotOffset);
				newObj.transform.parent = transform;
				clonedObjs.Add(newObj);
			}
			pos += distance;
		}
	}

	public List<GameObject> GetClonedObjs()
	{
		return clonedObjs;
	}

	public int GetCount()
	{
		return clonedObjs.Count;
	}
}
