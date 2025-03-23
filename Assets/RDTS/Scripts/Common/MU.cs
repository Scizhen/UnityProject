using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

namespace RDTS
{
    [System.Serializable]
    public class EventMU : UnityEvent<MU, bool>
    {
    }

    [System.Serializable]
    public class EventMUDelete : UnityEvent<MU>
    {
    }



    /// <summary>
    /// ���ƶ���Ԫ���ܹ���ץȡ��װ�ػ����ʹ��ű�Transport Surfaces�ƶ�������Source�ű���������Sink�ű����١��ṩ�����Fix��Load�ȷ�����Event�ӿڵ�
    /// </summary>
    [SelectionBase]
    //! Base class for free movable Unity (MU). MUs can be picked or loaded and they can be placed on Transport Surfaces. MUs are created by a Source and deleted by a Sink.
    public class MU : RDTSBehavior
    {
        #region Public Attributes
        [ReadOnly] public int ID; //!<  ID of this MU (increases on each creation on the MU source  ��Source����ʱ�����ԴID������SourceΨһ��
        [ReadOnly] public int GlobalID; //!< Global ID, increases for each MU independent on the source  ȫ��ID��������Source�ġ�����MU���ӵ�ID��ȫ��Ψһ��
        [ReorderableList] public List<GameObject> MUAppearences; //!< List of MU appearances for PartChanger  PartChangerʹ�õĲ�ͬMU��ۣ�PartChanger�ű���δ��ֲ��������Ҫ����ֲ��
        [ReadOnly] public GameObject FixedBy; //!< Current Gripper which is picking the part ��ǰ����ץȡ��MU��Grip
        [ReadOnly] public GameObject LoadedOn; //!< Current Part the MU is loaded on  װ�ش�MU����һ��MU
        [ReadOnly] public GameObject StandardParent; //!< The standard parent Gameobject of the MU  MU�ĸ�����
        [ReadOnly] public GameObject ParentBeforeFix; //!< The parent of the MU before the last Grip  ��MU��ץȡǰ�ĸ�����
        [ReadOnly] public List<Sensor> CollidedWithSensors; //!< The current Sensors the Part is colliding with  ���MU������ײ�Ĵ�����
        [ReadOnly] public List<MU> LoadedMus; //!< List of MUs whcih are loaded on this MU  ��װ�ص���MU�ϵ�MU�б�
        public float SurfaceAlignSmoothment = 50f;//���ڽ� MU �봫���������ƽ������ - ����  Simulation�� ʹ��

        [HideInInspector]
        public float FixerLastDistance;
        [HideInInspector] public Rigidbody Rigidbody;
        [HideInInspector] public List<TransportSurface> AlignWithSurface = new List<TransportSurface>();
        [HideInInspector] public List<TransportSurface> TransportSurfaces = new List<TransportSurface>();
        [HideInInspector] public float DissolveDuration = 0.5f;
        [HideInInspector] public float MaxDissolveValue = 0.2f;

        [ReadOnly] public float Velocity;
        #endregion

        private Vector3 lastDirection;
        private List<Material> materials = new List<Material>();


        // Deletes all MUs which are loaded on MU as Subcomponent 
        // (but not RigidBodies which are standing on this MU)
        #region Events
        [Foldout("Events")] public EventMUDelete EventMUDeleted;  //!< Event is called when MU is Deleted / Destroyed   ����MU��ɾ��������ʱ
        [Foldout("Events")] public EventMU EventMUIsLoaded; //!< Event is called when MU is loaded onto another   ����MU��Load����һ��������ʱ
        [Foldout("Events")] public EventMU EventMUGetsLoad; //!< Event is called when MU gets a MU loaded onto itself    �������MU��Load����MUʱ
        [Foldout("Events")] public EventMUSensor EventMUSensor;  //!< Event is called when MU collides with a Sensor    ����MU��Sensor����ײʱ��������˳����ᱻ����
        #endregion


        #region Public Methods

        //  Places the part with the Bottom on top of the defined position
        ///���ײ���������ڶ���λ�õĶ���
        public void PlaceMUOnTopOfPosition(Vector3 position)
        {
            Bounds bounds = new Bounds(transform.position, new Vector3(0, 0, 0));

            // Calculate Bounds

            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                bounds.Encapsulate(renderer.bounds);//��Χ����gameobject
            }

            // get bottom center ��ȡ��Χ�еײ�������λ��
            var center = new Vector3(bounds.min.x + bounds.extents.x, bounds.min.y, bounds.min.z + bounds.extents.z);

            // get distance from center to bounds
            var distance = transform.position - center;

            transform.position = position + distance;
        }

        //! Load the named MU on this mu
        public void LoadMu(MU mu)
        {
            mu.transform.SetParent(this.transform);//������MU��Ϊ��MU���Ӷ���
            mu.EventMuLoad();//���ø���MU��Load����һMUʱ���¼�����
            LoadedMus.Add(mu);//Load�б�����ӣ�����Inspector����й۲�
            EventMUGetsLoad.Invoke(this, true);//���ô�MU���¼�����
        }

        //! Event that this called when MU enters sensor
        //MU����sensor
        public void EventMUEnterSensor(Sensor sensor)
        {
            CollidedWithSensors.Add(sensor);//���б������
            EventMUSensor.Invoke(this, true);//�����¼�����
        }

        //! Event that this called when MU enters sensor
        //MU�˳�sensor
        public void EventMUExitSensor(Sensor sensor)
        {
            CollidedWithSensors.Remove(sensor);
            EventMUSensor.Invoke(this, false);
        }


        //! Event that this MU is loaded onto another
        public void EventMuLoad()
        {
            Rigidbody.isKinematic = true;
            Rigidbody.useGravity = false;
            LoadedOn = transform.parent.gameObject;
            EventMUIsLoaded.Invoke(this, true);
        }

        //! Event that this MU is unloaded from another
        public void EventMUUnLoad()
        {
            EventMUIsLoaded.Invoke(this, false);
            Rigidbody.isKinematic = false;
            Rigidbody.useGravity = true;
            transform.parent = StandardParent.transform;
            LoadedOn = null;
            Rigidbody.WakeUp();
        }

        //  Init the MU wi MUName and IDs
        //��ʼ��MU�����ƺ�ID����Source�б�����
        public void InitMu(string muname, int localid, int globalid)
        {
            ID = localid;
            GlobalID = globalid;
            name = muname;
            if (transform.parent != null)
            {
                StandardParent = transform.parent.gameObject;
            }
            else
            {
                StandardParent = transform.root.gameObject;
            }
        }

        //! Event that this MU is on Path
        /// <summary>
        /// MU����·�����¼�
        /// </summary>
        public void EventMuEnterPathSimulation()
        {
            Rigidbody.isKinematic = false;
            Rigidbody.useGravity = false;
        }

        //! Event that this MU is unloaded from Path
        /// <summary>
        /// MU�˳�·�����¼�
        /// </summary>
        public void EventMUExitPathSimulation()
        {
            Rigidbody.isKinematic = false;
            Rigidbody.useGravity = true;
            Rigidbody.WakeUp();
        }

        // Public method for fixing MU to a gameobject
        /// <summary>
        /// ����MU�̶���ָ��������
        /// </summary>
        /// <param name="fixto"></param>
        public void Fix(GameObject fixto)
        {
            if (FixedBy != null)
            {
                var fix = FixedBy.GetComponent<IFix>();//Grip�̳���IFix
                fix.Unfix(this);
            }
            else
            {
                if (this.transform.parent != null)
                    ParentBeforeFix = this.transform.parent.gameObject;
            }
            transform.SetParent(fixto.transform);
            Rigidbody.isKinematic = true;
            FixedBy = fixto;
        }

        // Public method for unfixing MU to a gameobject, parent changes are done based on parent before fix
        /// <summary>
        /// ����MU�Ӹ������Ͻ���̶�����ParentBeforeFix��Ϊ�������½�����Ϊ�����������ô�MU������Ϊ��
        /// </summary>
        public void Unfix()
        {
            if (ParentBeforeFix != null)
                transform.SetParent(ParentBeforeFix.transform);
            else
                transform.SetParent(null);
            ParentBeforeFix = null;
            Rigidbody.isKinematic = false;
            Rigidbody.WakeUp();
            FixedBy = null;
        }

        //! Unloads one of the MUs which are loaded on this MU
        //��ָ����MU��Load�б����Ƴ�������������¼�����
        public void UnloadOneMu(MU mu)
        {
            EventMUGetsLoad.Invoke(this, false);
            mu.EventMUUnLoad();
            LoadedMus.Remove(mu);
        }

        //! Unloads all  of the MUs which are loaded on this MU
        //�����б�Load��MU�Ƴ�
        public void UnloadAllMUs()
        {
            var tmploaded = LoadedMus.ToArray();
            foreach (var mu in tmploaded)
            {
                UnloadOneMu(mu);
            }
        }

        //! Slowly dissolves MU and destroys it  ���������Ⲣ���ٴ�MU
        public void Dissolve(float duration)
        {
            DissolveDuration = duration;
            if (DissolveDuration > 0)
                StartCoroutine(DissolveCoroutine());
            Invoke("Destroy", DissolveDuration);
        }

        public void Appear(float duration)
        {
            DissolveDuration = duration;
            if (duration > 0)
                StartCoroutine(AppearCoroutine());

        }

        #endregion

        IEnumerator AppearCoroutine()
        {
            float dissolveValue = MaxDissolveValue;
            var duration = DissolveDuration / Time.timeScale;
            while (dissolveValue > 0)
            {
                dissolveValue -= 0.01f;
                foreach (Material mat in materials)
                {
                    mat.SetFloat("_DissolveAmount", dissolveValue);
                    yield return null;
                }
                yield return new WaitForSeconds(duration / 100f);
            }
        }

        IEnumerator DissolveCoroutine()
        {
            float dissolveValue = 0f;
            var duration = DissolveDuration / Time.timeScale;
            while (dissolveValue < MaxDissolveValue)
            {
                dissolveValue += 0.01f;
                foreach (Material mat in materials)
                {
                    mat.SetFloat("_DissolveAmount", dissolveValue);
                    yield return null;
                }
                yield return new WaitForSeconds(DissolveDuration / 100f);
            }
        }


        //���ٴ˶���
        private void Destroy()
        {
            Destroy(this.gameObject);
        }

        private void OnDestroy()
        {
            if (CollidedWithSensors != null)
                foreach (var sensor in CollidedWithSensors.ToArray())
                {
                    sensor.OnMUDelete(this);
                }
            if (EventMUDeleted != null)
                EventMUDeleted.Invoke(this);
        }

        private void Start()
        {
            Renderer[] rends = GetComponentsInChildren<Renderer>();
            foreach (Renderer rend in rends)
            {
                materials.Add(rend.material);
            }
            Rigidbody = GetComponentInChildren<Rigidbody>();
            MaxDissolveValue = 0.3f;
        }

        public void FixedUpdate()
        {

            if (AlignWithSurface.Count > 0) // Only align if fully on one surface
            {
                var surface = AlignWithSurface[0];

                var destrot = Quaternion.FromToRotation(transform.up, surface.transform.up) * Rigidbody.rotation;
                Rigidbody.rotation = Quaternion.Lerp(Rigidbody.rotation, destrot, SurfaceAlignSmoothment * Time.fixedDeltaTime);
            }


        }
    }
}