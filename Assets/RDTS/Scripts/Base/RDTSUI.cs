using UnityEngine;

namespace RDTS
{

    public class RDTSUI : BehaviorInterface
    {

        public void SetColor(GameObject obj, Color color)
        {
            var renderers = obj.GetComponentsInChildren<MeshRenderer>();
            foreach (Renderer render in renderers)
            {
                MaterialPropertyBlock props = new MaterialPropertyBlock();
                props.SetColor("_Color", color);
                props.SetColor("_Emission", color);
                render.SetPropertyBlock(props);
            }
        }

        public void ResetColor(GameObject obj)
        {
            var renderers = obj.GetComponentsInChildren<MeshRenderer>();
            foreach (Renderer render in renderers)
            {
                render.SetPropertyBlock(null);
            }
        }

    }
}