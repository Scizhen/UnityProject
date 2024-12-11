using RDTS.Method;
using RDTS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace RDTS
{
    /// <summary>
    /// Source和SourceTote预制体的基础脚本对LibraryObject覆写，用于改变Source和SourceTote的形状
    /// </summary>
    [System.Serializable]
    public struct BoxDimensions
    {
        public string Name;
        public float Propability;
        public float Width;
        public float Height;
        public float Length;
        public float Weight;
        public Material Material;
    }

    [SelectionBase]
    [ExecuteAlways]
    public class BoxSource : LibraryObject
    {

        public Sensor BlockSourceOnSensorOccupied;
        public int RandomSeed = 1;
        public float Distance;
        public float CreateFadeTime = 0.5f;
        [RDTS.Utility.ReadOnly] public float SumCreated;
        [RDTS.Utility.ReadOnly] public float CreatedPerHour;
        public List<BoxDimensions> BoxTypes;


        private Source source;
        private MU lastcreated;
        private int nexttype;
        private float sum;
        private Distribution distribution;
        private float waitfordistance;
        private float lastlength;
        private float nextlength;
        private bool lastcreatednotnull;
        private bool sensornotnull;
        private GameObject MUs;
        private MU musource;
        public override void AttachTo(SnapPoint attachto)
        {
            transform.position = attachto.transform.position;
            transform.rotation = attachto.transform.rotation;
            var libobj = attachto.GetComponentInParent<LibraryObject>();
            transform.parent = libobj.gameObject.transform;
        }


        public override void OnParentChanged()
        {
            if (Parent != null)
            {
                var straight = Parent.GetComponent<StraightConveyor>();
                if (straight != null)
                {
                    this.transform.localPosition = new Vector3(0, 0);
                    this.transform.localRotation = Quaternion.identity;
                }
            }

            Modify();
        }


        void Selectnexttype()
        {
            if (BoxTypes.Count == 0)
            {
                nexttype = -1;
                var box = GetComponentInChildren<Carton>();
                nextlength = box.Length;
                return;
            }

            var random = distribution.GetNextRandom();
            var sums = 0.0f;
            for (int i = 0; i < BoxTypes.Count; i++)
            {
                sums = sums + BoxTypes[i].Propability;

                if (random < sums)
                {
                    nexttype = i;
                    break;
                }
            }

            nextlength = BoxTypes[nexttype].Length;
        }

        public void Create()
        {
            SumCreated++;
            lastcreated = source.Generate();
            lastcreated.transform.parent = MUs.transform;
            lastcreatednotnull = true;
            var box = lastcreated.GetComponentInChildren<Carton>();

            var rb = lastcreated.GetComponent<Rigidbody>();
            if (nexttype != -1)
            {
                rb.mass = BoxTypes[nexttype].Weight;
                if (BoxTypes[nexttype].Height != 0)
                    box.Height = BoxTypes[nexttype].Height;
                if (BoxTypes[nexttype].Width != 0)
                    box.Width = BoxTypes[nexttype].Width;
                if (BoxTypes[nexttype].Length != 0)
                    box.Length = BoxTypes[nexttype].Length;
                lastcreated.gameObject.name = BoxTypes[nexttype].Name + "-" + lastcreated.GlobalID.ToString();
                if (BoxTypes[nexttype].Material != null)
                {
                    var renderer = lastcreated.GetComponentInChildren<Renderer>();
                    renderer.material = BoxTypes[nexttype].Material;
                }

                box.Modify();
            }

            rb.interpolation = RigidbodyInterpolation.None;
            lastcreated.MaxDissolveValue = 0.3f;
            lastcreated.DissolveDuration = 0.6f;
            lastcreated.Appear(CreateFadeTime);
            lastlength = box.Length;
            Selectnexttype();
        }

        // Start is called before the first frame update


        void Start()
        {
            if (!Application.isPlaying)
                return;

            MUs = Global.AddGameObjectIfNotExisting("MUs");
            source = GetComponent<Source>();
            if (source == null)
                return;

            source.AutomaticGeneration = false;

            musource = GetComponent<MU>();

            sum = 0;
            foreach (var boxes in BoxTypes)
            {
                sum = sum + boxes.Propability;
            }

            distribution = new Distribution();
            distribution.Type = Distribution.DistributionType.Uniform;
            distribution.Seed = RandomSeed;
            distribution.Min = 0;
            distribution.Max = sum;
            distribution.Init();
            sensornotnull = BlockSourceOnSensorOccupied != null;
            Selectnexttype();
            if (Distance >= 0)
                Create();
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            CreatedPerHour = SumCreated / (Time.time / 3600);
            if (!lastcreatednotnull)
                return;
            if (Distance < 0)
                return;
            var distancetosource = Vector3.Magnitude(lastcreated.transform.position - this.transform.position);

            if (distancetosource - nextlength / 2 - lastlength / 2 >= Distance)
            {
                if (!sensornotnull)
                    Create();
                else
                {
                    if (!BlockSourceOnSensorOccupied.Occupied)
                        Create();
                }
            }
        }
    }
}