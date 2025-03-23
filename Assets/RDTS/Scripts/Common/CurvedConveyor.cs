using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using UnityEditor;
using RDTS.Method;
using RDTS;

[SelectionBase]//将其 GameObject 标记为 Scene View 选取的选择基础对象
[ExecuteAlways]//使脚本的实例在播放模式期间以及编辑时始终执行

//弯曲运输机核心脚本，控制弯曲运输机的各项参数

public class CurvedConveyor : LibraryObject, IConveyor, ISnapable
{
    [Header("Curve Conveyor Dimensions")]
    [OnValueChanged("Modify")]
    public float Height = 0.8f;

    [MinValue(0.5f), MaxValue(30)]
    [OnValueChanged("Modify")]
    public float Radius = 2f;

    [OnValueChanged("Modify")] public float Width = 1f;
    [OnValueChanged("Modify")] public bool Clockwise = false;

    [MinValue(-360), MaxValue(360)]
    [OnValueChanged("Modify")]
    public float StartAngle = 0;

    [MinValue(-360), MaxValue(360)]
    [OnValueChanged("Modify")]
    public float EndAngle = 90;

    [Header("Display Options")]
    [OnValueChanged("Modify")]
    public bool IsRollerConveyor = false;
    [ShowIf("IsRollerConveyor")][MinValue(0.1f), MaxValue(1f)] public float DistanceRolls = 0.2f;
    [MinValue(-360), MaxValue(360)] public float InnerSideGuideHeight = 0.30f;

    [OnValueChanged("Modify")] public float OuterSideGuideHeight = 0.30f;
    [OnValueChanged("Modify")] public bool Legs = true;
    [OnValueChanged("Modify")] public int LegsDistanceAngle = 45;

    [MinValue(1), MaxValue(30)]
    [OnValueChanged("Modify")]
    public int NumberSplinePoints = 10;

    [Header("Sensor")]
    [OnValueChanged("Modify")]
    public bool SensorAtEnd = true;

    [OnValueChanged("Modify")] public float SensorPosFromEnd = 5;
    [OnValueChanged("Modify")] public float SensorHeight = 0.1f;

    [Header("Physics")]
    [OnValueChanged("Modify")]
    public PhysicMaterial SurfaceMaterial;
    [OnValueChanged("Modify")] public bool AlignMUWithSurface = true;
    [OnValueChanged("Modify")] public bool OnExitAlignMUWithDirection = true;
    [Header("Resources")]
    [ShowIf("IsEditable")]
    public GameObject Leg;
    [ShowIf("IsEditable")] public GameObject Roll;
    [ShowIf("IsEditable")] public Material MaterialBelt;
    [ShowIf("IsEditable")] public Material MaterialNonVisible;


    private GameObject TransformIn;
    private GameObject TransformOut;
    private Quaternion directionOut;

    // Geometric Constants
    const float standardbeltdiameter = 0.10f;
    const float standardsideprofilehight = 0.15f;
    const float standardsideprofilewidth = 0.020f;
    const float bottomoverlapsideguide = 0.02f;
    const float standardlegwidth = 0.035f;

    // Interface Methods


    public Vector3 GetPosition(float position)
    {
        var lencurve = (2 * Mathf.PI * Radius) * (EndAngle - StartAngle) / 360;
        var perangle = position / lencurve * (EndAngle - StartAngle);
        var startpos = LibraryUtils.CalculateOneArcPos(Vector3.zero, Radius, 0, StartAngle, EndAngle,
            Height - Height, false, Clockwise);
        var positiononcircle = LibraryUtils.CalculateOneArcPos(Vector3.zero, Radius, perangle, StartAngle, EndAngle,
            Height - Height, false, Clockwise);
        var res = new Vector3(positiononcircle.x - startpos.x, positiononcircle.y, positiononcircle.z - startpos.z);
        return (res);
    }

    public Quaternion GetRotation(float position)
    {
        var lencurve = (2 * Mathf.PI * Radius) * (EndAngle - StartAngle) / 360;
        var perangle = position / lencurve * (EndAngle - StartAngle);

        var rotationoncircle = LibraryUtils.CalculateOneArcRot(Radius - Width / 2, perangle, StartAngle, EndAngle,
            Height - Height, Vector3.zero, Clockwise);
        if (!Clockwise)
            return Quaternion.Euler(rotationoncircle.x, rotationoncircle.y - 90, rotationoncircle.z);
        else
            return Quaternion.Euler(rotationoncircle.x, rotationoncircle.y, rotationoncircle.z);
    }

    public float GetHeight()
    {
        return Height;
    }

    public float GetLength()
    {
        return (2 * Mathf.PI * Radius) * (EndAngle - StartAngle) / 360;

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
        return InnerSideGuideHeight;
    }

    public float GetRightGuideHeight()
    {
        return OuterSideGuideHeight;
    }

    public float GetHeightIncrease()
    {
        return 0;
    }

    public bool GetIsRollerConveyor()
    {
        return IsRollerConveyor;
    }

    public float GetRollDistance()
    {
        return DistanceRolls;
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
    }

    [Button("Add Source")]
    public void AddSource()
    {
        var SnapIn = GetComponentByName<SnapPoint>("SnapIn");
        Attach("Assets/game4automation-Simulation/ConveyorSystems/Source.prefab", SnapIn);
    }

    public void OnEntered(Collision other, TransportSurface surface)
    {
        var mu = other.gameObject.GetComponent<MU>();

        //Global.DebugArrow(mu.transform.position,mu.transform.right,10);
        //mu.EnterAngle = Vector3.Angle(mu.transform.right, TransformIn.transform.right);


        if (AlignMUWithSurface)
        {
            mu.AlignWithSurface.Add(surface);
        }
    }

    public void OnExit(Collision other, TransportSurface surface)
    {
        var mu = other.gameObject.GetComponent<MU>();


        if (AlignMUWithSurface)
        {
            mu.AlignWithSurface.Remove(surface);
        }


        if (OnExitAlignMUWithDirection)
        {
            mu.Rigidbody.rotation = directionOut;
            Physics.SyncTransforms();
        }
    }


    public override void AttachTo(SnapPoint attachto)
    {
#if UNITY_EDITOR
        this.transform.position = attachto.transform.position;
        this.transform.rotation = attachto.transform.rotation;
        var libobj = attachto.GetComponentInParent<LibraryObject>();
        this.transform.parent = libobj.transform.parent;
        var straight = libobj.GetComponent<StraightConveyor>();
        var curve = libobj.GetComponent<CurvedConveyor>();

        var conv = libobj.GetComponent<IConveyor>();
        Height = conv.GetHeight() + conv.GetHeightIncrease();
        Width = conv.GetWidth();
        BottomHeight = conv.GetBottomHeight();
        Legs = conv.GetLegs();
        IsRollerConveyor = conv.GetIsRollerConveyor();
        DistanceRolls = conv.GetRollDistance();
        InnerSideGuideHeight = conv.GetLeftGuideHeight();
        OuterSideGuideHeight = conv.GetRightGuideHeight();
        Selection.activeGameObject = this.gameObject;
        Modify();
#endif
    }


    public override void OnModify()
    {
#if UNITY_EDITOR
        if (NumberSplinePoints < 1)
            NumberSplinePoints = 1;

        var Drive = GetComponentByName<Drive>("Drive");
        var InnerGuide = GetComponentByName<CurvedProfile>("InnerGuide");
        var OuterGuide = GetComponentByName<CurvedProfile>("OuterGuide");
        var SnapIn = GetComponentByName<SnapPoint>("SnapIn");
        var SnapOut = GetComponentByName<SnapPoint>("SnapOut");
        var Belt = GetComponentByName<CurvedProfile>("Belt");

        if (Radius < 0.5)
            Radius = 0.15f;
        if (Radius > 30f)
            Width = 30.0f;


        if (Width < 0.1)
            Width = 0.1f;
        if (Width > 5)
            Width = 5.0f;

        this.transform.position =
            new Vector3(this.transform.position.x, BottomHeight + Height, this.transform.position.z);

        var lenfullcurve = (2 * Mathf.PI * Radius);
        var degreepermm = 360 / (lenfullcurve * 1000);

        // Drive
        Drive.SpeedScaleTransportSurface = degreepermm;
        Drive.ReverseDirection = !Clockwise;
        EditorUtility.SetDirty(Drive);

        // Snap Points
        var positions = LibraryUtils.CalculateArcPos(Vector3.zero, Radius, StartAngle, EndAngle, Height - Height,
            NumberSplinePoints + 2, false, Clockwise);
        var rotations = LibraryUtils.CalculateArcRot(StartAngle, EndAngle, Radius, Height - Height,
            NumberSplinePoints + 2, Vector3.zero, false, Clockwise);
        var startpos = positions[0];

        SnapIn.transform.localRotation = Quaternion.Euler(rotations[0]);
        TransformIn = SnapIn.gameObject;
        TransformOut = SnapOut.gameObject;
        Rotate(SnapIn.gameObject, 0, -90, 0);
        var endppos = positions[positions.Count - 1];

        SnapOut.transform.localPosition = endppos - startpos;
        SnapOut.transform.localRotation = Quaternion.Euler(rotations[rotations.Count - 1]);
        Rotate(SnapOut.gameObject, 0, -90, 0);

        // Belt
        Belt.transform.localPosition =
            new Vector3(-startpos.x, -startpos.y - standardbeltdiameter / 2, -startpos.z);
        Belt.Radius = Radius;
        Belt.StartAngle = StartAngle;
        Belt.EndAngle = EndAngle;
        Belt.IntermediatePoints = NumberSplinePoints;
        Belt.XScale = Width;
        Belt.GetComponent<Collider>().material = SurfaceMaterial;
        var surface = GetComponentByName<TransportSurface>("Belt");

        surface.TextureScale = 10;
        if (Clockwise)
            surface.TextureScale = -surface.TextureScale;
        var renderer = Belt.GetComponent<Renderer>();
        renderer.sharedMaterial.mainTextureScale = new Vector2(Radius, 1);
        renderer.enabled = true;
        Belt.Clockwise = Clockwise;
        Belt.Modify();
        Global.DestroyObjectsByComponent<Roll>(Belt.gameObject);
        if (IsRollerConveyor)
        {
            if (DistanceRolls < 0.1)
                DistanceRolls = 0.1f;
            if (DistanceRolls > 1)
                DistanceRolls = 1.0f;

            renderer.material = MaterialNonVisible;
            var inradius = Radius - Width / 2;
            var len = (2 * Mathf.PI * Radius) * (EndAngle - StartAngle) / 360;
            var numrolls = (int)Math.Ceiling(len / DistanceRolls) - 1;
            float rolldistancestart = (len - (numrolls * DistanceRolls)) / 2;
            float rollanglestart = rolldistancestart / len * (EndAngle - StartAngle);
            float rolldistance = DistanceRolls / len * (EndAngle - StartAngle);
            for (int i = 0; i <= numrolls; i++)
            {
                var angle = rollanglestart + i * rolldistance;
                var pos = LibraryUtils.CalculateOneArcPos(new Vector3(0, 0, 0),
                    Radius, angle, StartAngle, EndAngle, 0, Clockwise, Clockwise);
                var rot = LibraryUtils.CalculateOneArcRot(Radius, angle, StartAngle,
                    EndAngle, 0, Vector3.zero, Clockwise, Clockwise);
                var rollgo = CreatePart(Roll, "Belt", new Vector3(pos.x - startpos.x, 0, pos.z), rot);
                Rotate(rollgo, 0, 0, 90);
                var roll = rollgo.GetComponent<Roll>();
                roll.Diameter = standardbeltdiameter;
                roll.Width = Width - 0.01f;
                roll.RotationDrive = GetComponentByName<Drive>("Drive");
                roll.Modify();
            }
        }
        else
        {
            renderer.material = MaterialBelt;
        }

        EditorUtility.SetDirty(Belt);

        // Hide Drive Gizmos
        HideDriveGizmos();

        // Inner Guide
        var height = InnerSideGuideHeight * (1 / standardsideprofilehight);
        var yposguide = startpos.y + InnerSideGuideHeight / 2 - standardbeltdiameter - bottomoverlapsideguide;
        InnerGuide.transform.localPosition = new Vector3(-startpos.x, yposguide, -startpos.z);
        var innerradius = Radius - Width / 2;
        if (innerradius < 0)
            innerradius = 0;
        InnerGuide.Radius = innerradius - standardsideprofilewidth;
        InnerGuide.StartAngle = StartAngle;
        InnerGuide.EndAngle = EndAngle;
        InnerGuide.IntermediatePoints = NumberSplinePoints;
        InnerGuide.Clockwise = Clockwise;
        InnerGuide.Flip = Clockwise;
        InnerGuide.YScale = height;
        InnerGuide.Modify();


        // Outer Guide
        height = OuterSideGuideHeight * (1 / standardsideprofilehight);
        yposguide = startpos.y + OuterSideGuideHeight / 2 - standardbeltdiameter - bottomoverlapsideguide;
        OuterGuide.transform.localPosition = new Vector3(-startpos.x, yposguide, -startpos.z);
        var outerradius = Radius + Width / 2;
        OuterGuide.Radius = outerradius + standardsideprofilewidth;
        OuterGuide.StartAngle = StartAngle;
        OuterGuide.EndAngle = EndAngle;
        OuterGuide.IntermediatePoints = NumberSplinePoints;
        OuterGuide.YScale = height;
        OuterGuide.Flip = !Clockwise;
        OuterGuide.Clockwise = Clockwise;
        OuterGuide.Modify();

        // Legs
        if(true)//if (!IsPrefab())
        {
            Global.DestroyObjectsByComponent<LegProfile>(gameObject);
            var LegsPerSide = (int)(EndAngle - StartAngle) / LegsDistanceAngle + 1;
            if (Legs)
            {
                var deltaangletotal = EndAngle - StartAngle;
                var deltaangle = 0.0f;
                var startangle = 0.0f;
                if (LegsPerSide == 1)
                {
                    deltaangle = deltaangletotal / 2;
                    startangle = deltaangle;
                }
                else
                {
                    startangle = 0;
                    deltaangle = deltaangletotal / (LegsPerSide - 1);
                }

                for (int i = 0; i < LegsPerSide; i++)
                {
                    var angle = startangle + (i * deltaangle);
                    var pos = LibraryUtils.CalculateOneArcPos(new Vector3(0, 0, 0),
                        innerradius - standardsideprofilewidth, angle, StartAngle, EndAngle, 0, Clockwise, Clockwise);
                    var rot = LibraryUtils.CalculateOneArcRot(innerradius - standardsideprofilewidth, angle, StartAngle,
                        EndAngle, 0, Vector3.zero, Clockwise, Clockwise);
                    var leg = CreatePart(Leg, "Legs",
                        new Vector3(pos.x - startpos.x, BottomHeight - Height, pos.z - startpos.z), rot);
                    var legprofile = leg.GetComponent<LegProfile>();
                    legprofile.Height = Height - standardbeltdiameter - bottomoverlapsideguide;
                    legprofile.Modify();
                    if (i == 0)
                        Move(leg, 0, 0, standardlegwidth);
                    if (i == LegsPerSide - 1)
                        Move(leg, 0, 0, -standardlegwidth);

                    angle = startangle + (i * deltaangle);
                    pos = LibraryUtils.CalculateOneArcPos(new Vector3(0, 0, 0), outerradius + standardsideprofilewidth,
                        angle, StartAngle, EndAngle, 0, !Clockwise, Clockwise);
                    rot = LibraryUtils.CalculateOneArcRot(outerradius + standardsideprofilewidth, angle, StartAngle,
                        EndAngle, 0, Vector3.zero, !Clockwise, Clockwise);
                    leg = CreatePart(Leg, "Legs",
                        new Vector3(pos.x - startpos.x, BottomHeight - Height, pos.z - startpos.z), rot);
                    legprofile = leg.GetComponent<LegProfile>();
                    legprofile.Height = Height - standardbeltdiameter - bottomoverlapsideguide;
                    legprofile.Modify();
                    if (i == 0)
                        Move(leg, 0, 0, standardlegwidth);
                    if (i == LegsPerSide - 1)
                        Move(leg, 0, 0, -standardlegwidth);
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

        // DriveStatus
        var drivestatus = GetComponentByName<DriveArrow>("DriveArrow");
        drivestatus.gameObject.SetActive(DisplayStatus);
        var posa = LibraryUtils.CalculateOneArcPos(new Vector3(0, 0, 0), Radius, (EndAngle - StartAngle) / 2,
            StartAngle, EndAngle, 0, false, Clockwise);
        var rota = LibraryUtils.CalculateOneArcRot(Radius, (EndAngle - StartAngle) / 2, StartAngle, EndAngle, 0,
            Vector3.zero, true, Clockwise);

        drivestatus.transform.localPosition = new Vector3(posa.x - startpos.x, 0, posa.z - startpos.z);
        drivestatus.transform.localRotation = Quaternion.Euler(rota);
        Rotate(drivestatus.gameObject, 0, 90, 0);
#endif
    }


    public void Connect(SnapPoint ownSnapPoint, SnapPoint snapPointMate, ISnapable mateObject,
        bool ismoved)
    {

        if (ismoved)
        {
            // Align to other object
            Align(ownSnapPoint, snapPointMate, Quaternion.identity);
        }
    }

    public void Disconnect(SnapPoint snapPoint, SnapPoint snapPointMate, ISnapable libraryobject, bool ismoved)
    {
        //Debug.Log("Disconnect " + this.name + " to " + libraryobject.name + " / " +ismoved);
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
        var surface = GetComponentByName<TransportSurface>("Belt");
        var SnapOut = GetComponentByName<SnapPoint>("SnapOut");
        directionOut = SnapOut.transform.rotation;
        surface.TextureScale = 10;
        if (Clockwise)
            surface.TextureScale = -surface.TextureScale;
        if (AlignMUWithSurface || OnExitAlignMUWithDirection)
        {
            surface.OnEnter += OnEntered;
            surface.OnExit += OnExit;
        }
        ChangeEditable();
    }


}