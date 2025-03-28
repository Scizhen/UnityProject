
using UnityEngine;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine.UI;


namespace RDTS
{
    //路径系统中使用的箭头定义的基类
    [RequireComponent(typeof(LineRenderer))]
    [ExecuteAlways]
    //! Base class for arrow definition used in the path system
    public class Arrow : MonoBehaviour
    {
        public bool CondensedView = true;//!< Boolean to set the condensed view active
        public float Size;//!< Arrow size
        public Material Material;//!< Arrow material

        [HideInInspector] public LineRenderer Linerenderer;

        private SimulationPath path;

        void Init()
        {
            Linerenderer = GetComponent<LineRenderer>();
            path = GetComponentInParent<SimulationPath>();
            // Set Material if not there
            Linerenderer.alignment = LineAlignment.TransformZ;

            Linerenderer.alignment = LineAlignment.TransformZ;
            if (CondensedView)
            {
                if (Linerenderer.sharedMaterial != null)
                    Linerenderer.sharedMaterial.hideFlags = HideFlags.HideInInspector;
                Linerenderer.hideFlags = HideFlags.HideInInspector;
            }
            else
            {
                if (Linerenderer.sharedMaterial != null)
                    Linerenderer.sharedMaterial.hideFlags = HideFlags.None;
                Linerenderer.hideFlags = HideFlags.None;
            }
            transform.localScale = new Vector3(1, 1, 1);
            Linerenderer.enabled = path.ShowPathOnSimulation;

        }

        void Reset()
        {
            Init();
        }

        void Awake()
        {
            Init();
        }

        public void Draw()
        {
            Vector3 dir;
            if (name == "SnapIn")
            {
                dir = path.GetDirection(0);
            }
            else
            {
                dir = path.GetDirection(1);
            }
            Linerenderer.useWorldSpace = true;
            Linerenderer.SetPosition(0, transform.position - (dir * Size) / 2);
            Linerenderer.SetPosition(1, transform.position + (dir * Size) / 2);
            Linerenderer.startWidth = Size;
            Linerenderer.endWidth = 0;
            Linerenderer.material = Material;

        }

        void Update()
        {
            if (transform.hasChanged)
                Draw();
        }
    }
}
