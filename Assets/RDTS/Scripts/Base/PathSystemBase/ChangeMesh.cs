
using UnityEngine;
using NaughtyAttributes;

namespace RDTS
{
    //更改运输机渲染
    public class ChangeMesh : MonoBehaviour
    {
        public Mesh OriginalMesh;
        public Mesh ClonedMesh;

        public void MoveMeshVertices(Vector3 fromposition, float maxdistance, Vector3 deltaposition)
        {
            var meshfilter = GetComponent<MeshFilter>();
            var originalMesh = meshfilter.sharedMesh;
            ClonedMesh = new Mesh();

            ClonedMesh.name = "clone";
            ClonedMesh.vertices = originalMesh.vertices;
            ClonedMesh.triangles = originalMesh.triangles;
            ClonedMesh.normals = originalMesh.normals;
            ClonedMesh.uv = originalMesh.uv;
            var vertices = ClonedMesh.vertices;
            for (int i = 0; i < ClonedMesh.vertexCount; i++)
            {
                var currpos = ClonedMesh.vertices[i];
                var delta = currpos - fromposition;
                if (Vector3.Magnitude(delta) <= maxdistance)
                {
                    var newpos = ClonedMesh.vertices[i] + deltaposition;
                    vertices[i] = newpos;
                }
            }

            ClonedMesh.vertices = vertices;
            ClonedMesh.RecalculateNormals();
            meshfilter.mesh = ClonedMesh;
        }


        // Start is called before the first frame update
        [Button("ResetMesh")]
        public void ResetMesh()
        {
            var meshfilter = GetComponent<MeshFilter>();
            ClonedMesh = new Mesh();
            ClonedMesh.name = "cloned";
            ClonedMesh.vertices = OriginalMesh.vertices;
            ClonedMesh.triangles = OriginalMesh.triangles;
            ClonedMesh.normals = OriginalMesh.normals;
            ClonedMesh.uv = OriginalMesh.uv;
            meshfilter.mesh = OriginalMesh;
        }


    }
}

