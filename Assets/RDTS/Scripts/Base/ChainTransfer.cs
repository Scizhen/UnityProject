using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using UnityEditor;

namespace RDTS
{
    /// <summary>
    /// ChainTransfer预制体的基础脚本对LibraryObject覆写，用于改变ChainTransfer的形状
    /// </summary>
    [ExecuteAlways]
    public class ChainTransfer : LibraryObject, ISnapable
    {
        [Header("Chain Transfer Dimensions")]
        [OnValueChanged("Modify")] public bool InheritFromParent;
        [OnValueChanged("Modify")] [HideIf("InheritFromParent")] public float ConveyorHeight = 0.8f;
        [OnValueChanged("Modify")] [HideIf("InheritFromParent")] public float Width = 1;
        [OnValueChanged("Modify")] public float DistanceRolls = 0.2f;
        [OnValueChanged("Modify")] public int NumberRollsBetweenChains = 4;
        [OnValueChanged("Modify")] public float AdditonalSizeChainCollider = 0.1f;
        [OnValueChanged("Modify")] public float LiftDistance = 0.1f;
        [OnValueChanged("Modify")] public float PositionUp = 0.05f;
        [OnValueChanged("Modify")] public float TimeUpDown = 0.5f;

        const float standardbeltdiameter = 0.10f;

        public override void OnParentModify()
        {
            if (InheritFromParent)
                Modify();
        }

        public new void OnTransformParentChanged()
        {
            SetParent();
            if (Parent != null)
                InheritFromParent = true;
            else
                InheritFromParent = false;
            Modify();
        }

        public override void OnModify()
        {
#if UNITY_EDITOR
            var chainleft = GetComponentByName<TransferChain>("ChainLeft");
            var chainright = GetComponentByName<TransferChain>("ChainRight");
            var belt = GetComponentByName<StraightBelt>("VirtualBelt");
            var snap = GetComponentByName<SnapPoint>("SnapChainTransfer");
            var lift = GetComponentByName<Drive>("Lift");
            var drive = GetComponentByName<Drive>("Drive");
            var surface = GetComponentByName<TransportSurface>("VirtualBelt");

            surface.UseThisDrive = drive;

            if (InheritFromParent)
            {
                var par = Parent.GetComponent<StraightConveyor>();
                if (par != null)
                {
                    Width = par.Width;
                    BottomHeight = par.BottomHeight;
                    ConveyorHeight = par.Height;
                }
            }

            this.transform.position =
                new Vector3(this.transform.position.x, BottomHeight + ConveyorHeight, this.transform.position.z);
            snap.transform.localPosition = new Vector3(0, 0, 0);


            float chainx = Width / 2;
            float chainy = -standardbeltdiameter / 2;
            float chainz = DistanceRolls * NumberRollsBetweenChains / 2;
            chainleft.Diameter = standardbeltdiameter;
            chainleft.Length = Width;
            chainleft.transform.localPosition = new Vector3(chainx, chainy, chainz);
            chainleft.Modify();
            chainright.Diameter = standardbeltdiameter;
            chainright.Length = Width;
            chainright.transform.localPosition = new Vector3(chainx, chainy, -chainz);
            chainright.Modify();

            belt.Height = standardbeltdiameter;
            belt.Width = DistanceRolls * NumberRollsBetweenChains + AdditonalSizeChainCollider;
            belt.Length = Width;
            belt.transform.localPosition = new Vector3(0, 0, 0);
            belt.Modify();

            var liftcylinder = GetComponentByName<Drive_Cylinder>("Lift");
            liftcylinder.MaxPos = PositionUp * 1000;
            liftcylinder.MinPos = PositionUp * 1000 - LiftDistance * 1000;
            liftcylinder.TimeIn = TimeUpDown;
            liftcylinder.TimeOut = TimeUpDown;

            EditorUtility.SetDirty(liftcylinder);
#endif
        }



        public void Connect(SnapPoint ownSnapPoint, SnapPoint snapPointMate, ISnapable mateObject,
            bool ismoved)
        {
            if (ismoved)
            {

                {
                    Align(ownSnapPoint, snapPointMate, Quaternion.Euler(0, 180, 0));
                }
            }
        }

        public void Disconnect(SnapPoint snapPoint, SnapPoint snapPointMate, ISnapable libraryobject, bool ismoved)
        {
        }

        public override bool HideAllowed(GameObject gameObject)
        {
            if (gameObject.name == "Drive")
                return false;

            if (gameObject.name == "Lift")
                return false;

            return true;
        }


    }
}

