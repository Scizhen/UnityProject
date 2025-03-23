using System;
using UnityEngine;
using NaughtyAttributes;
using UnityEditor;
using RDTS.Method;
using RDTS;

//直式运输机组核心脚本

[SelectionBase]
[ExecuteAlways]
public class StraightConveyor : LibraryObject, IConveyor, ISnapable

{
    public bool InheritFromParent = false;

    [Header("Straight Conveyor Dimensions")]
    [OnValueChanged("Modify")]
    [HideIf("InheritFromParent")]
    public float Height = 0.8f;

    [OnValueChanged("Modify")]
    [MinValue(-20f), MaxValue(20f)]
    [HideIf("InheritFromParent")]
    public float HeightIncrease = 0.0f;

    [OnValueChanged("Modify")]
    [MinValue(0.2f), MaxValue(40f)]
    [HideIf("InheritFromParent")]
    public float Length = 2;

    [OnValueChanged("Modify")]
    [MinValue(0.2f), MaxValue(4f)]
    [HideIf("InheritFromParent")]
    public float Width = 1f;

    [OnValueChanged("Modify")]
    [MinValue(-60f), MaxValue(60f)]
    [HideIf("InheritFromParent")]
    public float StartAngle = 0;

    [OnValueChanged("Modify")]
    [MinValue(-60f), MaxValue(60f)]
    [HideIf("InheritFromParent")]
    public float EndAngle = 0;

    [Header("Display Options")]
    [OnValueChanged("Modify")]
    [HideIf("InheritFromParent")]
    public bool IsRollerConveyor = false;

    [ShowIf("IsRollerConveyor")]
    [MinValue(0.1f), MaxValue(1f)]
    [OnValueChanged("Modify")]
    public float DistanceRolls = 0.2f;

    [OnValueChanged("Modify")] public float LeftSideGuideHeight = 0.3f;

    [OnValueChanged("Modify")]
    [MinValue(0.0f), MaxValue(0.5f)]
    [HideIf("InheritFromParent")]
    public float RightSideGuideHeight = 0.3f;

    [OnValueChanged("Modify")]
    [HideIf("InheritFromParent")]
    public bool Legs = true;

    [OnValueChanged("Modify")]
    [MinValue(0.1f), MaxValue(5f)]
    [HideIf("InheritFromParent")]
    public float MaxLegDistance = 1;


    [Header("Sensor")]
    [OnValueChanged("Modify")]
    [HideIf("InheritFromParent")]
    public bool SensorAtEnd = true;

    [OnValueChanged("Modify")]
    [MinValue(-10f), MaxValue(10f)]
    [HideIf("InheritFromParent")]
    public float SensorPosFromEnd = 0.1f;

    [OnValueChanged("Modify")]
    [HideIf("InheritFromParent")]
    public float SensorHeight = 0.1f;

    [Header("Physics")]
    [OnValueChanged("Modify")]
    public PhysicMaterial SurfaceMaterial;

    public bool AlignMUWithSurface = true;
    public bool OnEnterAlignMUWithDirection = true;

    [Header("Resources")]
    [ShowIf("IsEditable")]
    public GameObject Leg;

    [ShowIf("IsEditable")] public GameObject Roll;
    [ShowIf("IsEditable")] public Material MaterialBelt;
    [ShowIf("IsEditable")] public Material MaterialNonVisible;

    [HideInInspector] public bool DisapleSnappoints = true;

    // Geometric Constants
    const float standardbeltdiameter = 0.10f;
    const float standardsideprofilehight = 0.15f;
    const float standardsideprofilewidth = 0.020f;
    const float bottomoverlapsideguide = 0.02f;
    const float standardlegwidth = 0.035f;


    // Interface Methods

    public Vector3 GetPosition(float position)
    {
        var SnapOut = GetComponentByName<SnapPoint>("SnapOut");
        var SnapIn = GetComponentByName<SnapPoint>("SnapIn");
        var vectorconv = Vector3.Normalize(SnapOut.transform.localPosition - Vector3.zero);
        var vec = Vector3.zero + vectorconv * position;
        return vec;
    }

    public Quaternion GetRotation(float position)
    {
        var belt = GetComponentByName<StraightBelt>("Belt");
        return belt.transform.localRotation;
    }

    public float GetHeight()
    {
        return Height;
    }

    public float GetLength()
    {
        return Length;
    }

    public bool GetIsRollerConveyor()
    {
        return IsRollerConveyor;
    }

    public float GetRollDistance()
    {
        return DistanceRolls;
    }


    public float GetWidth()
    {
        return Width;
    }

    public float GetBottomHeight()
    {
        return BottomHeight;
    }

    public bool GetLegs()
    {
        return Legs;
    }

    public float GetLeftGuideHeight()
    {
        return LeftSideGuideHeight;
    }

    public float GetRightGuideHeight()
    {
        return RightSideGuideHeight;
    }

    public float GetHeightIncrease()
    {
        return HeightIncrease;
    }


    [Button("Attach Straight Conveyor")]
    public void Attach_Straight_Conveyor()
    {
        var SnapOut = GetComponentByName<SnapPoint>("SnapOut");
        Attach("Assets/game4automation-Simulation/ConveyorSystems/StraightConveyor.prefab", SnapOut);
    }

    [Button("Attach Curve Conveyor")]
    public void Attach_Curve_Conveyor()
    {
        var SnapOut = GetComponentByName<SnapPoint>("SnapOut");
        Attach("Assets/game4automation-Simulation/ConveyorSystems/CurvedConveyor.prefab", SnapOut);
    }

    [Button("Add Sensor")]
    public void AddSensor()
    {
        Insert("Assets/game4automation-Simulation/ConveyorSystems/Sensor.prefab");
    }

    [Button("Insert/Remove Source")]
    public void AddSource()
    {
        if (!Global.DestroyObjectsByComponent<BoxSource>(this.gameObject))
            Insert("Assets/game4automation-Simulation/ConveyorSystems/Source.prefab");
    }

    [Button("Insert/Remove Sink")]
    public void Sink()
    {
        if (!Global.DestroyObjectsByComponent<BoxSink>(this.gameObject))
            Insert("Assets/game4automation-Simulation/ConveyorSystems/Sink.prefab");
    }

    public void OnEntered(Collision other, TransportSurface surface)
    {
        var mu = other.gameObject.GetComponent<MU>();

        if (AlignMUWithSurface)
        {
            mu.AlignWithSurface.Add(surface);
        }

        if (OnEnterAlignMUWithDirection)
        {
            mu.Rigidbody.rotation = this.transform.rotation;
            Physics.SyncTransforms();
        }
    }

    public void OnExit(Collision other, TransportSurface surface)
    {
        var mu = other.gameObject.GetComponent<MU>();

        if (AlignMUWithSurface)
        {
            mu.AlignWithSurface.Remove(surface);
        }
    }


    public override void AttachTo(SnapPoint attachto)
    {
#if UNITY_EDITOR
        this.transform.position = attachto.transform.position;
        this.transform.rotation = attachto.transform.rotation;
        var libobj = attachto.GetComponentInParent<LibraryObject>();
        this.transform.parent = libobj.transform.parent;
        var conv = libobj.GetComponent<IConveyor>();
        Height = conv.GetHeight() + conv.GetHeightIncrease();
        var sconv = attachto.GetComponentInParent<StraightConveyor>();
        if (sconv != null)
            Length = conv.GetLength();
        Width = conv.GetWidth();
        IsRollerConveyor = conv.GetIsRollerConveyor();
        DistanceRolls = conv.GetRollDistance();
        BottomHeight = conv.GetBottomHeight();
        Legs = conv.GetLegs();
        LeftSideGuideHeight = conv.GetLeftGuideHeight();
        RightSideGuideHeight = conv.GetRightGuideHeight();
        Selection.activeGameObject = this.gameObject;
        Modify();
#endif
    }


    public override void OnModify()
    {
#if UNITY_EDITOR
        if (Length < 0.1f)
            Length = 0.1f;

        if (Width < 0.1)
            Width = 0.1f;
        if (Width > 5)
            Width = 5.0f;

        this.transform.position =
            new Vector3(this.transform.position.x, BottomHeight + Height, this.transform.position.z);
        var distancestart = Width * Mathf.Tan((Mathf.PI / 180) * Mathf.Abs(StartAngle));
        var distanceend = Width * Mathf.Tan((Mathf.PI / 180) * Mathf.Abs(EndAngle));

        var lengthonground = Mathf.Sqrt(Length * Length - HeightIncrease * HeightIncrease);

        // Snapin
        var SnapIn = GetComponentByName<SnapPoint>("SnapIn");
        if (SnapIn != null)
        {
            SnapIn.SnapType = SnapPoint.SNAPTYPE.IN;
            SnapIn.transform.localPosition = new Vector3(0 + distancestart / 2, 0, 0);
            SnapIn.transform.localRotation = Quaternion.Euler(0, StartAngle, 0);
        }

        var SnapOut = GetComponentByName<SnapPoint>("SnapOut");
        if (SnapOut != null)
        {
            SnapOut.SnapType = SnapPoint.SNAPTYPE.OUT;

            SnapOut.transform.localPosition = new Vector3(lengthonground - distanceend / 2, HeightIncrease, 0);
            SnapOut.transform.localRotation = Quaternion.Euler(0, -EndAngle, 0);
        }

        float distancestartleft = 0;
        float distancestartright = 0;
        float distanceendleft = 0;
        float distanceendright = 0;

        if (StartAngle > 0)
        {
            distancestartleft = distancestart;
        }

        if (StartAngle < 0)
        {
            distancestartright = distancestart;
        }

        if (EndAngle > 0)
        {
            distanceendleft = distanceend;
        }

        if (EndAngle < 0)
        {
            distanceendright = distanceend;
        }

        var SnapLeft = GetComponentByName<SnapPoint>("SnapLeft");
        if (SnapLeft != null)
        {
            SnapLeft.SnapType = SnapPoint.SNAPTYPE.ALL;
            SnapLeft.transform.localPosition =
                new Vector3(lengthonground / 2 + distancestartleft / 2 - distanceendleft / 2, HeightIncrease / 2,
                    Width / 2);
            SnapLeft.transform.localRotation = Quaternion.Euler(0, -90, 0);
        }

        var SnapRight = GetComponentByName<SnapPoint>("SnapRight");
        if (SnapRight != null)
        {
            SnapRight.SnapType = SnapPoint.SNAPTYPE.ALL;
            SnapRight.transform.localPosition = new Vector3(
                lengthonground / 2 + distancestartright / 2 - distanceendright / 2,
                HeightIncrease / 2, -Width / 2);
            SnapRight.transform.localRotation = Quaternion.Euler(0, 90, 0);
        }

        // Guides
        var LeftGuide = GetComponentByName<StraightProfile>("LeftGuide");
        var RightGuide = GetComponentByName<StraightProfile>("RightGuide");
        var leftheightguide = 0 + LeftSideGuideHeight / 2 - standardbeltdiameter - bottomoverlapsideguide +
                              HeightIncrease / 2;
        var rightheightguide = 0 + RightSideGuideHeight / 2 - standardbeltdiameter - bottomoverlapsideguide +
                               HeightIncrease / 2;
        var guideleftz = Width / 2 + standardsideprofilewidth;
        var guiderightz = -Width / 2 - standardsideprofilewidth;
        var guidex = lengthonground / 2;
        var angleup = Mathf.Asin(HeightIncrease / Length) * Mathf.Rad2Deg;

        if (LeftGuide != null)
        {
            LeftGuide.transform.localPosition = new Vector3(guidex + distancestartleft / 2 - distanceendleft / 2,
                leftheightguide, guideleftz);
            LeftGuide.transform.localRotation = Quaternion.Euler(-angleup, 90, 0);
            LeftGuide.Height = LeftSideGuideHeight;
            LeftGuide.Length = Length - (distancestartleft + distanceendleft);
            LeftGuide.Modify();
            LeftGuide.gameObject.SetActive(LeftSideGuideHeight != 0);
        }

        if (RightGuide != null)
        {
            RightGuide.transform.localPosition = new Vector3(guidex + distancestartright / 2 - distanceendright / 2,
                rightheightguide, guiderightz);
            RightGuide.transform.localRotation = Quaternion.Euler(angleup, -90, 0);
            RightGuide.Height = RightSideGuideHeight;
            RightGuide.Length = Length - (distancestartright + distanceendright);
            RightGuide.Modify();
            RightGuide.gameObject.SetActive(RightSideGuideHeight != 0);
        }

        // DriveStatus
        var midconveyor = new Vector3(Length / 2, HeightIncrease / 2, 0);
        var drivestatus = GetComponentByName<DriveArrow>("DriveArrow");
        if (HeightIncrease > 0)
            midconveyor = new Vector3(Length / 2, HeightIncrease / 2 + 0.05f, 0);
        drivestatus.gameObject.SetActive(DisplayStatus);
        drivestatus.transform.localPosition = midconveyor;
        drivestatus.transform.localRotation = Quaternion.Euler(0, 0, angleup);

        // Belt
        var Belt = GetComponentByName<StraightBelt>("Belt");
        Belt.Height = standardbeltdiameter;
        Belt.Width = Width;
        Belt.Length = Length;
        Belt.transform.localPosition = Vector3.zero;
        Belt.StartAngle = StartAngle;
        Belt.EndAngle = EndAngle;
        Belt.gameObject.transform.localRotation = Quaternion.Euler(0, 0, angleup);
        Belt.Modify();
        var collider = Belt.GetComponent<Collider>();
        if (collider != null)
            collider.material = SurfaceMaterial;
        var surface = GetComponentByName<TransportSurface>("Belt");

        surface.TextureScale = -1;
        var renderer = Belt.GetComponent<Renderer>();
        renderer.enabled = true;
        renderer.sharedMaterial.mainTextureScale = new Vector2(Length, 1);

        // Roller Conveyor
        Global.DestroyObjectsByComponent<Roll>(Belt.gameObject);
        if (IsRollerConveyor)
        {
            if (DistanceRolls < 0.1)
                DistanceRolls = 0.1f;
            if (DistanceRolls > 1)
                DistanceRolls = 1.0f;

            renderer.sharedMaterial = MaterialNonVisible;
            var numrolls = (int)Math.Ceiling((Length - standardbeltdiameter) / DistanceRolls) - 1;
            float rolldistancestart = (Length - (numrolls * DistanceRolls)) / 2;
            for (int i = 0; i <= numrolls; i++)
            {
                var rollwidth = Width - 0.005f;
                float deltaz = 0;
                var rollx = rolldistancestart + i * DistanceRolls;
                var relpos = rollx - (Length - distanceendleft);
                if (relpos > 0)
                {
                    rollwidth = rollwidth * ((distanceendleft - relpos) / distanceendleft);
                    deltaz = -(Width - 0.005f - rollwidth) / 2;
                }

                relpos = rollx - (Length - distanceendright);
                if (relpos > 0)
                {
                    rollwidth = rollwidth * ((distanceendright - relpos) / distanceendright);
                    deltaz = (Width - 0.005f - rollwidth) / 2;
                }

                relpos = distancestartleft - rollx;
                if (relpos > 0)
                {
                    rollwidth = rollwidth * ((distancestartleft - relpos) / distancestartleft);
                    deltaz = -(Width - 0.005f - rollwidth) / 2;
                }

                relpos = distancestartright - rollx;
                if (relpos > 0)
                {
                    rollwidth = rollwidth * ((distancestartright - relpos) / distancestartright);
                    deltaz = (Width - 0.005f - rollwidth) / 2;
                }

                Vector3 pos = new Vector3(rolldistancestart + i * DistanceRolls, -standardbeltdiameter / 2, deltaz);
                var rollgo = CreatePart(Roll, "Belt", pos, new Vector3(90, 0, 0));
                var roll = rollgo.GetComponent<Roll>();
                roll.Diameter = standardbeltdiameter;
                roll.Width = rollwidth - 0.005f;
                roll.RotationDrive = GetComponentByName<Drive>("Drive");
                roll.Modify();
            }
        }
        else
        {
            renderer.sharedMaterial = MaterialBelt;
        }

        EditorUtility.SetDirty(Belt);

        // Legs
        Global.DestroyObjectsByComponent<LegProfile>(gameObject);
        if (true) //if (!IsPrefab())
        {
            // Legs left
            var lengthleft = lengthonground - distancestartleft - distanceendleft;
            var LegsPerSide = (int)Math.Ceiling(lengthleft / MaxLegDistance);
            var deltalegs = 0.0f;
            if (Legs)
            {
                if (LegsPerSide <= 1)
                {
                    deltalegs = lengthonground;
                    LegsPerSide = 2;
                }
                else
                {
                    deltalegs = lengthleft / (LegsPerSide - 1);
                }

                for (int i = 0; i < LegsPerSide; i++)
                {
                    var posx = deltalegs * i + distancestartleft;
                    if (i == LegsPerSide - 1)
                        posx = lengthleft + distancestartleft;
                    Vector3 posleft = new Vector3(posx, -Height, Width / 2);
                    var leg = CreatePart(Leg, "Legs", posleft, new Vector3(0, 90, 0));
                    var legprofile = leg.GetComponent<LegProfile>();
                    var legheight = posx * Mathf.Tan(Mathf.Deg2Rad * angleup);
                    legprofile.Height = legheight + Height - standardbeltdiameter - bottomoverlapsideguide;
                    legprofile.Modify();
                    if (i == 0)
                        Move(leg, 0, 0, standardlegwidth);
                    if (i == LegsPerSide - 1)
                        Move(leg, 0, 0, -standardlegwidth);
                }
            }

            // Legs right
            var lenthright = lengthonground - distancestartright - distanceendright;
            LegsPerSide = (int)Math.Ceiling(lenthright / MaxLegDistance);
            deltalegs = 0.0f;
            if (Legs)
            {
                if (LegsPerSide <= 1)
                {
                    deltalegs = lenthright;
                    LegsPerSide = 2;
                }
                else
                {
                    deltalegs = lenthright / (LegsPerSide - 1);
                }

                for (int i = 0; i < LegsPerSide; i++)
                {
                    var posx = deltalegs * i + distancestartright;
                    if (i == LegsPerSide - 1)
                        posx = lenthright + distancestartright;
                    var posright = new Vector3(posx, -Height, -Width / 2);
                    var leg = CreatePart(Leg, "Legs", posright, new Vector3(0, -90, 0));
                    var legprofile = leg.GetComponent<LegProfile>();
                    var legheight = posx * Mathf.Tan(Mathf.Deg2Rad * angleup);
                    legprofile.Height = legheight + Height - standardbeltdiameter - bottomoverlapsideguide;
                    legprofile.Modify();
                    if (i == 0)
                        Move(leg, 0, 0, -standardlegwidth);
                    if (i == LegsPerSide - 1)
                        Move(leg, 0, 0, standardlegwidth);
                }
            }
        }

        // Sensor
        var sensor = GetComponentByName<SensorConveyor>("Sensor");
        if (SensorAtEnd)
        {
            sensor.gameObject.SetActive(true);
            sensor.SensorHeight = SensorHeight;
            sensor.SensorPosOnConveyor = -SensorPosFromEnd;
        }
        else
        {
            sensor.gameObject.SetActive(false);
        }

        EditorUtility.SetDirty(sensor);

        if (!IsPrefab())
            DisapleSnappoints = false;
#endif
    }


    public void Connect(SnapPoint ownSnapPoint, SnapPoint snapPointMate, ISnapable mateObject,
        bool ismoved)
    {
        if (ismoved)
        {
            // Align to other object

            if ((snapPointMate.gameObject.name == "SnapLeft" || snapPointMate.gameObject.name == "SnapRight") &&
                ownSnapPoint.name == "SnapOut")
            {
                Align(ownSnapPoint, snapPointMate, Quaternion.Euler(0, 180, 0));
            }
            else
            {
                if ((ownSnapPoint.name != "SnapLeft") && (ownSnapPoint.name != "SnapRight"))
                {
                    Align(ownSnapPoint, snapPointMate, Quaternion.Euler(0, 0, 0));
                }
                else
                {
                    if (snapPointMate.gameObject.name == "SnapIn")
                    {
                        Align(ownSnapPoint, snapPointMate, Quaternion.Euler(0, 0, 0));
                    }
                    else
                    {
                        Align(ownSnapPoint, snapPointMate, Quaternion.Euler(0, 180, 0));
                    }
                }
            }
        }

        if (ownSnapPoint.gameObject.name == "SnapLeft")
        {
            LeftSideGuideHeight = standardbeltdiameter;
            OnModify();
        }

        if (ownSnapPoint.gameObject.name == "SnapRight")
        {
            RightSideGuideHeight = standardbeltdiameter;
            OnModify();
        }
    }

    public void Disconnect(SnapPoint snapPoint, SnapPoint snapPointMate, ISnapable libraryobject, bool ismoved)
    {
    }


    public override bool HideAllowed(GameObject gameObject)
    {
        if (gameObject.name == "Drive")
            return false;
        if (gameObject.name == "Sensor")
            return false;
        if (gameObject.name == "Belt")
            return false;
        return true;
    }


    // Start is called before the first frame update
    void Start()
    {
        var Belt = GetComponentByName<StraightBelt>("Belt");
        var renderer = Belt.GetComponent<Renderer>();
        if (Application.isPlaying)
            renderer.material.mainTextureScale = new Vector2(Length, 1);
        var surface = GetComponentByName<TransportSurface>("Belt");

        if (AlignMUWithSurface || OnEnterAlignMUWithDirection)
        {
            surface.OnEnter += OnEntered;
            surface.OnExit += OnExit;
        }

        ChangeEditable();
    }
}