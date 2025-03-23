using UnityEngine;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEditor;
using log4net.Util;
using RDTS;

//直式运输机表面传送带脚本

namespace RDTS
{
    public class StraightBelt : LibraryPart
    {

        [OnValueChanged("Modify")] public float Length = 1;
        [OnValueChanged("Modify")] public float Height = 1;
        [OnValueChanged("Modify")] public float Width = 1;
        [OnValueChanged("Modify")] public float StartAngle = 0;
        [OnValueChanged("Modify")] public float EndAngle = 0;

        private ChangeMesh changemesh;

        public void SetCorner(Vector3 positon, Vector3 corner)
        {
            var delta = positon - corner;
            changemesh.MoveMeshVertices(corner, 0.2f, delta);
        }

        [Button("Test")]
        public void Test()
        {
            var par = GetComponentInParent<LibraryObject>();
            par.ShowInspector(gameObject, false);
        }

        [Button("Update")]
        public override void Modify()
        {
            transform.localScale = new Vector3(1, 1, 1);
            changemesh = GetComponent<ChangeMesh>();
            changemesh.ResetMesh();

            var leftup = new Vector3(0, 0, Width / 2);
            var leftdown = new Vector3(0, -Height, Width / 2);
            var rightup = new Vector3(0, 0, -Width / 2);
            var rightdown = new Vector3(0, -Height, -Width / 2);

            var endleftup = new Vector3(Length, 0, Width / 2);
            var endleftdown = new Vector3(Length, -Height, Width / 2);
            var endrightup = new Vector3(Length, 0, -Width / 2);
            var endrightdown = new Vector3(Length, -Height, -Width / 2);

            if (StartAngle != 0)
            {
                var distance = Width * Mathf.Tan((Mathf.PI / 180) * Mathf.Abs(StartAngle));
                if (distance >= Length)
                {
                    distance = Length;
                }
                if (StartAngle > 0)
                {
                    leftup = new Vector3(distance, 0, Width / 2);
                    leftdown = new Vector3(distance, -Height, Width / 2);
                }
                else
                {
                    rightup = new Vector3(distance, 0, -Width / 2);
                    rightdown = new Vector3(distance, -Height, -Width / 2);
                }
            }

            if (EndAngle != 0)
            {
                var distance = Width * Mathf.Tan((Mathf.PI / 180) * Mathf.Abs(EndAngle));
                if (distance >= Length)
                {
                    distance = Length;
                }
                if (EndAngle > 0)
                {
                    endleftup = new Vector3(Length - distance, 0, Width / 2);
                    endleftdown = new Vector3(Length - distance, -Height, Width / 2);
                }
                else
                {
                    endrightup = new Vector3(Length - distance, 0, -Width / 2);
                    endrightdown = new Vector3(Length - distance, -Height, -Width / 2);

                }
            }

            // Set Corners
            SetCorner(leftup, new Vector3(0, 0, 0.5f));
            SetCorner(leftdown, new Vector3(0, -1, 0.5f));
            SetCorner(rightup, new Vector3(0, 0, -0.5f));
            SetCorner(rightdown, new Vector3(0, -1, -0.5f));

            SetCorner(endleftup, new Vector3(1, 0, 0.5f));
            SetCorner(endleftdown, new Vector3(1, -1, 0.5f));
            SetCorner(endrightup, new Vector3(1, 0, -0.5f));
            SetCorner(endrightdown, new Vector3(1, -1, -0.5f));

            var meshfilter = GetComponent<MeshFilter>();
            meshfilter.sharedMesh.RecalculateBounds(); ;

            var boxcollider = GetComponent<BoxCollider>();
            if (boxcollider != null)
            {
                DestroyImmediate(boxcollider);
            }

            var meshcollider = GetComponent<MeshCollider>();
            if (meshcollider != null)
            {
                DestroyImmediate(meshcollider);
            }

            var surface = GetComponent<TransportSurface>();

            if (StartAngle == 0 && EndAngle == 0)
            {
                gameObject.AddComponent<BoxCollider>();
                if (surface != null)
                    surface.UseMeshCollider = false;
            }
            else
            {
                var mesh = gameObject.AddComponent<MeshCollider>();
                surface.UseMeshCollider = true;
                mesh.convex = true;
#if UNITY_EDITOR
                if (surface != null)
                    EditorUtility.SetDirty(surface);
#endif
            }
            meshfilter.sharedMesh.RecalculateBounds(); ;
        }
    }

}
