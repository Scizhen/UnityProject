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
    /// 可移动单元，能够被抓取和装载或被输送带脚本Transport Surfaces移动，可由Source脚本创建，被Sink脚本销毁。提供了相关Fix、Load等方法，Event接口等
    /// </summary>
    [SelectionBase]
    //! Base class for free movable Unity (MU). MUs can be picked or loaded and they can be placed on Transport Surfaces. MUs are created by a Source and deleted by a Sink.
    public class MU : RDTSBehavior
    {
        #region Public Attributes
        [ReadOnly] public int ID; //!<  ID of this MU (increases on each creation on the MU source  由Source创建时赋予的源ID（仅对Source唯一）
        [ReadOnly] public int GlobalID; //!< Global ID, increases for each MU independent on the source  全局ID，独立于Source的、随着MU增加的ID（全局唯一）
        [ReorderableList] public List<GameObject> MUAppearences; //!< List of MU appearances for PartChanger  PartChanger使用的不同MU外观（PartChanger脚本并未移植，若有需要可移植）
        [ReadOnly] public GameObject FixedBy; //!< Current Gripper which is picking the part 当前正在抓取此MU的Grip
        [ReadOnly] public GameObject LoadedOn; //!< Current Part the MU is loaded on  装载此MU的另一个MU
        [ReadOnly] public GameObject StandardParent; //!< The standard parent Gameobject of the MU  MU的父对象
        [ReadOnly] public GameObject ParentBeforeFix; //!< The parent of the MU before the last Grip  在MU被抓取前的父对象
        [ReadOnly] public List<Sensor> CollidedWithSensors; //!< The current Sensors the Part is colliding with  与此MU发生碰撞的传感器
        [ReadOnly] public List<MU> LoadedMus; //!< List of MUs whcih are loaded on this MU  被装载到此MU上的MU列表
        public float SurfaceAlignSmoothment = 50f;//用于将 MU 与传输表面对齐的平滑参数 - 仅由  Simulation包 使用

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
        [Foldout("Events")] public EventMUDelete EventMUDeleted;  //!< Event is called when MU is Deleted / Destroyed   当此MU被删除或销毁时
        [Foldout("Events")] public EventMU EventMUIsLoaded; //!< Event is called when MU is loaded onto another   当此MU被Load到另一个对象上时
        [Foldout("Events")] public EventMU EventMUGetsLoad; //!< Event is called when MU gets a MU loaded onto itself    当另外的MU被Load到此MU时
        [Foldout("Events")] public EventMUSensor EventMUSensor;  //!< Event is called when MU collides with a Sensor    当此MU与Sensor相碰撞时。进入或退出均会被调用
        #endregion


        #region Public Methods

        //  Places the part with the Bottom on top of the defined position
        ///将底部的零件放在定义位置的顶部
        public void PlaceMUOnTopOfPosition(Vector3 position)
        {
            Bounds bounds = new Bounds(transform.position, new Vector3(0, 0, 0));

            // Calculate Bounds

            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                bounds.Encapsulate(renderer.bounds);//包围整个gameobject
            }

            // get bottom center 获取包围盒底部的中心位置
            var center = new Vector3(bounds.min.x + bounds.extents.x, bounds.min.y, bounds.min.z + bounds.extents.z);

            // get distance from center to bounds
            var distance = transform.position - center;

            transform.position = position + distance;
        }

        //! Load the named MU on this mu
        public void LoadMu(MU mu)
        {
            mu.transform.SetParent(this.transform);//将给定MU设为此MU的子对象
            mu.EventMuLoad();//调用给定MU被Load到另一MU时的事件函数
            LoadedMus.Add(mu);//Load列表中添加，可在Inspector面板中观察
            EventMUGetsLoad.Invoke(this, true);//调用此MU的事件函数
        }

        //! Event that this called when MU enters sensor
        //MU进入sensor
        public void EventMUEnterSensor(Sensor sensor)
        {
            CollidedWithSensors.Add(sensor);//在列表中添加
            EventMUSensor.Invoke(this, true);//调用事件函数
        }

        //! Event that this called when MU enters sensor
        //MU退出sensor
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
        //初始化MU的名称和ID，在Source中被调用
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
        /// MU进入路径的事件
        /// </summary>
        public void EventMuEnterPathSimulation()
        {
            Rigidbody.isKinematic = false;
            Rigidbody.useGravity = false;
        }

        //! Event that this MU is unloaded from Path
        /// <summary>
        /// MU退出路径的事件
        /// </summary>
        public void EventMUExitPathSimulation()
        {
            Rigidbody.isKinematic = false;
            Rigidbody.useGravity = true;
            Rigidbody.WakeUp();
        }

        // Public method for fixing MU to a gameobject
        /// <summary>
        /// 将此MU固定到指定对象上
        /// </summary>
        /// <param name="fixto"></param>
        public void Fix(GameObject fixto)
        {
            if (FixedBy != null)
            {
                var fix = FixedBy.GetComponent<IFix>();//Grip继承了IFix
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
        /// 将此MU从父对象上解除固定。若ParentBeforeFix不为空则重新将其设为父，否则设置此MU父对象为空
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
        //将指定的MU从Load列表中移除，并调用相关事件函数
        public void UnloadOneMu(MU mu)
        {
            EventMUGetsLoad.Invoke(this, false);
            mu.EventMUUnLoad();
            LoadedMus.Remove(mu);
        }

        //! Unloads all  of the MUs which are loaded on this MU
        //将所有被Load的MU移除
        public void UnloadAllMUs()
        {
            var tmploaded = LoadedMus.ToArray();
            foreach (var mu in tmploaded)
            {
                UnloadOneMu(mu);
            }
        }

        //! Slowly dissolves MU and destroys it  缓慢地消解并销毁此MU
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


        //销毁此对象
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