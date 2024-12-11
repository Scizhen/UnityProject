
using UnityEngine;
using NaughtyAttributes;
using UnityEditor;

//直式运输机组核心脚本
namespace RDTS
{

    public class StraightConveyorGroup : LibraryObject
    {
        [Header("Straight Conveyor Dimensions")]
        [OnValueChanged("Modify")] public float Height = 0.8f;
        [OnValueChanged("Modify")] public float Length = 5;
        [OnValueChanged("Modify")] public float Width = 1.0f;
        [OnValueChanged("Modify")] public float SegmentLength = 1.0f;
        [OnValueChanged("Modify")] public float StartSegmentLenth = 0f;
        [OnValueChanged("Modify")][ReadOnly] public float EndSegmentLength = 0.5f;
        [OnValueChanged("Modify")] [ReadOnly] public int NumberOfSegments = 0;
      

        [Header("Sensors")] [OnValueChanged("Modify")]
        public bool SensorAtEnd = true;
        [OnValueChanged("Modify")]
        public float SensorPosFromEnd = 0.1f;
        [OnValueChanged("Modify")]
        public float SensorHeight = 0.1f;
        
        [Header("Display Options")]
        [OnValueChanged("Modify")] public bool IsRollerConveyor = false;
        [OnValueChanged("Modify")] [ShowIf("IsRollerConveyor")] public float RollDistance=0.2f;
        [OnValueChanged("Modify")] public float LeftSideGuideHeight = 0.3f;
        [OnValueChanged("Modify")] public float RightSideGuideHeight = 0.3f;
        [OnValueChanged("Modify")] public bool Legs = true;
        [OnValueChanged("Modify")] public float MaxLegDistance = 1;
        
        [OnValueChanged("Modify")] public GameObject StraightConveyorTemplate;
        public override void OnModify()
        {
            #if UNITY_EDITOR
            NumberOfSegments = (int)Mathf.Floor((Length-StartSegmentLenth) / SegmentLength)-1;
            EndSegmentLength = (Length - StartSegmentLenth) - (NumberOfSegments * SegmentLength);
            // Save all objects
            var allobjects = GetObjects<StraightConveyor>();
            if (StraightConveyorTemplate != null)
            {
                var currentpos = 0.0f;
                for (int i = 0; i < NumberOfSegments+1; i++)
                {
                 
                    var name = "Segment" + (i + 1).ToString();
                    StraightConveyor convcom = GetComponentByName<StraightConveyor>(name);
                    GameObject conveyor = null;
                    if (convcom == null)
                    {
                        conveyor = PrefabUtility.InstantiatePrefab(StraightConveyorTemplate) as GameObject;
                        convcom = conveyor.GetComponent<StraightConveyor>();
                        convcom.EnableSnap(false);
                        convcom.InheritFromParent = true;
                        conveyor.name = name;
                        conveyor.transform.parent = this.transform;
                    }
                    else
                    {
                        conveyor = convcom.gameObject;
                    }

                    convcom = conveyor.GetComponent<StraightConveyor>();
                    convcom.Height = Height;
                    convcom.Width = Width;
                    if (convcom.InheritFromParent)
                    {
                        convcom.LeftSideGuideHeight = LeftSideGuideHeight;
                        convcom.RightSideGuideHeight = RightSideGuideHeight;
                        convcom.Legs = Legs;
                        convcom.MaxLegDistance = MaxLegDistance;
                        convcom.SensorHeight = SensorHeight;
                        convcom.SensorAtEnd = SensorAtEnd;
                        convcom.SensorPosFromEnd = SensorPosFromEnd;
                        convcom.IsRollerConveyor = IsRollerConveyor;
                        convcom.DistanceRolls = RollDistance;
                    }

                    if (i == 0)
                    {
                        convcom.Length = StartSegmentLenth;
                        if (StartSegmentLenth == 0)
                            convcom.Length = SegmentLength;
                    }
                    else
                    {
                        convcom.Length = SegmentLength;
                        if (i==NumberOfSegments)
                            convcom.Length = EndSegmentLength;
                    }
                    convcom.transform.localPosition = new Vector3(currentpos,0,0);
                    convcom.Modify();
                    allobjects.Remove(convcom);
                    currentpos = currentpos + convcom.Length;
                }

                foreach (var obj in allobjects)
                {
                    DestroyImmediate(obj.gameObject);
                }

                var convcoms = GetComponentsInChildren<StraightConveyor>();
              
            }
            else
            {
                Debug.LogError("No Straigh Conveyor Template defined");
            }
            #endif
        }
    
        public override bool HideAllowed(GameObject gameObject)
        {
            if (gameObject.name == "DriveRolls")
                return false;
            if (gameObject.name == "DriveDirection")
                return false;
            return true;
        }

   
    }
}

