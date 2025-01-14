using UnityEngine;
using NaughtyAttributes;
using UnityEngine.Serialization;


namespace RDTS
{
    [RequireComponent(typeof(LineRenderer))]
    [ExecuteAlways]
    public class Line : SimulationPath
    {
        //! Line class within Simulation Path. Used to build the path system for AGV or MU. Posses catch points to support the construction of the path system.
        /// <summary>
        /// AGV路径中的直线基本脚本
        /// </summary>
        #region PublicProperties

        [FormerlySerializedAs("DirectionArrowActive")][OnValueChanged("DrawPath")] public bool DirectionArrow = true;//!< Boolean which activate/ deactivate the direction arrow in the middle of the line
        [Header("Line Properties")]
        [OnValueChanged("DirectionChanged")]
        [HideInInspector] public Vector3 Direction;//! < direction of the line

        #endregion

        #region PublicOverrides

        // Return the length of the line
        public override float GetLength()
        {
            return Length;
        }

        // Return the position as vector3 at a certain point (parameter normalizedposition) of the line (Linerenderer functionality)
        public override Vector3 GetPosition(float normalizedposition, ref BasePath currentpath)
        {
            Vector3 dir = EndPos - StartPos;
            return StartPos + dir * normalizedposition;
        }

        // Return the direction as vector3 at a certain point (parameter normalizedposition) of the line (Linerenderer functionality)
        public override Vector3 GetDirection(float normalizedposition)
        {
            Vector3 dir = EndPos - StartPos;
            return Vector3.Normalize(dir);
        }

        // Drawing the path
        public override void DrawPath()
        {

            if (blockdraw || name == "blockdraw")
                return;
            Linerenderer.useWorldSpace = false;
            Linerenderer.SetPosition(0, LocalStartPos);
            Linerenderer.SetPosition(1, LocalEndPos);
            Linerenderer.startWidth = Thickness;
            Linerenderer.endWidth = Thickness;
            Length = Vector3.Distance(LocalStartPos, LocalEndPos);
            Direction = Vector3.Normalize(LocalEndPos - LocalStartPos);
            var arrow = this.GetComponentInChildren<MidArrow>();

            if (DirectionArrow == true)
            {

                arrow.Linerenderer.startWidth = SizeDirectionArrow;
                arrow.Linerenderer.endWidth = 0;
                arrow.transform.position = StartPos + ((EndPos - StartPos) / 2);
                arrow.Draw();
            }
            else
            {
                arrow.Hide();
            }
            // Debug.Log("DrawLine");
        }

        // The method is used when another segment is attached to the line by using the buttons "Attach Line" or "Attach Curve".
        public override void AttachTo(SimulationPath path)
        {
            // Create new Gameobject
            Drive = path.Drive;
            transform.rotation = path.EndPoint.transform.rotation;
            transform.parent = path.transform.parent;
            transform.position = path.EndPos;
            transform.localScale = new Vector3(1, 1, 1);
            ChangeDirection(Direction);
            DrawPath();
            Physics.SyncTransforms();
            CheckSnapping();
        }


        #endregion


        #region PublicMethods
        // init line
        [Button("Init")]
        public void ButtonInit()
        {
            Reset();
        }
        //set start point
        [Button("Set Start to 0")]
        public void setStart()
        {
            SetStartTo0();
        }
        // attach line to the current object
        [Button("Attach Line")]
        public void AttachLine()
        {

            AttachPathLine("Assets/game4automation-Simulation/PathSystems/Line.prefab", this.gameObject);

#if UNITY_EDITOR

#endif
        }
        // attach curve to the current object
        [Button("Attach Curve")]
        public void AttachCurve()
        {
            AttachPathCurve("Assets/game4automation-Simulation/PathSystems/Curve.prefab", this.gameObject);
        }


        // Changes the direction at the end of the line.
        public void ChangeDirection(Vector3 direction)
        {
            Direction = direction;
            LocalEndPos = LocalStartPos + Direction * Length;

        }

        #endregion

        #region PrivateMethods

        // Reset the line to the parent parameter.
        public void Reset()
        {

            Debug.Log("Reset Line");
            EnableSnap = true;
            if (transform.childCount < 1)
            {
                var obj = new GameObject();
                obj.transform.parent = transform;
                obj.transform.localPosition = new Vector3(0, 0, 0);
                obj.transform.localRotation = Quaternion.identity;
                obj.name = "Start";
            }

            if (transform.childCount < 2)
            {
                var obj = new GameObject();
                obj.transform.parent = transform;
                obj.transform.localPosition = new Vector3(1, 0, 0);
                obj.transform.localRotation = Quaternion.identity;
                obj.name = "End";
            }

            StartPoint = transform.GetChild(0).gameObject;
            EndPoint = transform.GetChild(1).gameObject;
            LocalStartPos = new Vector3(0, 0, 0);
            BaseReset();
            Length = 1;
            Direction = new Vector3(1, 0, 0);
            var end = LocalStartPos + Length * Direction;
            LocalEndPos = end;
            DirectionArrow = true;


            DrawPath();

        }


        protected override void LengthChanged()
        {
            if (Length < 0.01f)
                Length = 1;
            if (Direction == Vector3.zero)
            {
                Direction = new Vector3(1, 0, 0);
            }

            var end = LocalStartPos + Direction * Length;
            LocalEndPos = end;
            DrawPath();
        }

        #endregion


    }
}
