using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using RDTS.Method;
using RDTS;

namespace RDTS
{
    /// <summary>
    /// GuideRolls预制体的基础脚本对LibraryObject覆写，用于改变GuideRolls的形状
    /// </summary>
    [ExecuteAlways]
    public class GuideRolls : LibraryObject
    {
        [Header("Guide Rolls Dimensions")]
        [OnValueChanged("Modify")] public bool InheritFromParent;
        [OnValueChanged("Modify")][HideIf("InheritFromParent")] public float ConveyorHeight;
        [OnValueChanged("Modify")][HideIf("InheritFromParent")] public float Width = 1;
        [OnValueChanged("Modify")] public float Height = 0.2f;
        [OnValueChanged("Modify")] public float Length = 0.5f;
        [OnValueChanged("Modify")] public float HeightPositionRolls = 0.0f;
        [OnValueChanged("Modify")] public float RollDiameter = 0.1f;
        [OnValueChanged("Modify")] public float RollWidth = 0.05f;
        [OnValueChanged("Modify")] public float RollDistanceInWidth = 0.2f;
        [OnValueChanged("Modify")] public float RollDistanceInLength = 0.2f;

        [Header("Display Options")]
        [OnValueChanged("Modify")] public bool ShowArea;
        [OnValueChanged("Modify")] public bool ShowRolls;


        [Header("Resources")][ShowIf("IsEditable")] public GameObject Roll;

        public override void OnParentModify()
        {
            if (InheritFromParent)
                Modify();
        }


        public override void OnParentChanged()
        {
            InheritFromParent = false;
            if (Parent != null)
                if (Parent.GetComponent<IConveyor>() != null)
                {
                    var par = Parent.GetComponent<IConveyor>();
                    InheritFromParent = true;
                    this.transform.localPosition = new Vector3(0, par.GetBottomHeight() + par.GetHeight(), 0);
                    this.transform.localRotation = Quaternion.identity;
                }

            Modify();
        }


        public override void OnModify()
        {
            var driverolls = GetComponentByName<Drive>("DriveRolls");
            var drivedirection = GetComponentByName<Drive>("DriveDirection");
            drivedirection.gameObject.SetActive(DisplayStatus);
            var speedoverlay = GetComponentByName<SpeedOverlayCollider>("SpeedOverlay");
            var drivearrow = GetComponentByName<DriveArrow>("DriveArrow");


            if (InheritFromParent && Parent != null)
            {

                if (Parent.GetType() == typeof(StraightConveyor))
                {
                    var par = (StraightConveyor)Parent;
                    BottomHeight = par.BottomHeight;
                    Width = par.Width;
                    ConveyorHeight = par.Height;
                }
                var parentcirc = GetComponentInParent<CurvedConveyor>();
                if (Parent.GetType() == typeof(CurvedConveyor))
                {
                    var par = (CurvedConveyor)Parent;
                    BottomHeight = par.BottomHeight;
                    Width = par.Width;
                    ConveyorHeight = par.Height;
                }
            }

            this.transform.position = new Vector3(this.transform.position.x, BottomHeight + ConveyorHeight, this.transform.position.z);
            speedoverlay.gameObject.transform.localScale = new Vector3(Length, Height, Width);
            speedoverlay.gameObject.transform.localPosition = new Vector3(Length / 2, Height / 2, 0);


            drivearrow.transform.localPosition = new Vector3(Length / 2, Height / 2, 0);

            var arearenderer = speedoverlay.GetComponent<MeshRenderer>();
            arearenderer.enabled = ShowArea;

            // Rolls
            Global.DestroyObjectsByComponent<Roll>(gameObject);
            if (ShowRolls && true)//if (ShowRolls && !IsPrefab())
            {
                var rollx = 0.0f;
                var rolly = 0.0f;
                var rollz = 0.0f;
                var rollsx = (Length / RollDistanceInLength) - 1;
                var rollsy = (Width / RollDistanceInWidth) - 1;
                if (rollsx > 0 && rollsy > 0 && rollsx < 100 && rollsy < 100)
                {
                    var distx = Length - ((rollsx - 1) * RollDistanceInLength);
                    var disty = Width - ((rollsy - 1) * RollDistanceInWidth);
                    for (int x = 0; x < rollsx; x++)
                    {
                        for (int y = 0; y < rollsy; y++)
                        {
                            rollx = distx / 2.0f + x * RollDistanceInLength;
                            rolly = disty / 2.0f + y * RollDistanceInWidth - Width / 2;
                            rollz = HeightPositionRolls;
                            var roll = CreatePart(Roll, "Rolls", new Vector3(rollx, rollz, rolly), new Vector3(90, 0, 0));
                            var rollscript = roll.GetComponent<Roll>();
                            rollscript.DirectionDrive = drivedirection;
                            rollscript.Diameter = RollDiameter;
                            rollscript.Width = RollWidth;
                            rollscript.Modify();
                        }
                    }

                }
            }


        }

        public override bool HideAllowed(GameObject gameObject)
        {
            if (gameObject.name == "DriveRolls")
                return false;
            if (gameObject.name == "DriveDirection")
                return false;
            return true;
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}

