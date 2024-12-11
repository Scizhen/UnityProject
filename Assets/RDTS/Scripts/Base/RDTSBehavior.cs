//*************************************************************************
//Thanks for the code reference game4automation provides.                 *
//                                                                        *
//*************************************************************************
using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NaughtyAttributes;
using Object = UnityEngine.Object;

namespace RDTS
{


    //! This is the base class for all Parallel-RDTS objects. This base clase is providing some additional scripts and properties for all components.
    //���������� Parallel-RDTS ����Ļ��ࡣ �������Ϊ��������ṩ��һЩ����Ľű������ԡ�
    public class RDTSBehavior : MonoBehaviour
    {
        [ReadOnly]public string Guid;

        [HideIf("hidename")]
        public string Name; //!< The name of the component if it should be different from the GameObject name

        [HideIf("hideactiveonly")]
        public ActiveOnly Active;
        [HideInInspector] public GameObject FromTemplate;
        [HideInInspector]
        public RDTSController RDTSController;
        [HideInInspector] [SerializeField] public bool HideNonG44Components;//��ɾ

        private static ILogger _logger = Debug.unityLogger;

        protected bool hidename() { return true; }
        protected bool hideactiveonly() { return false; }

        //! Transfers the direction enumeration to a vector
        /// <summary>
        /// ������DIRECTION��ö��ֵת���ɶ�Ӧ��λ����
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public Vector3 DirectionToVector(DIRECTION direction)
        {
            Vector3 result = Vector3.up;
            switch (direction)
            {
                case DIRECTION.LinearX:
                    result = Vector3.right;
                    break;
                case DIRECTION.LinearY:
                    result = Vector3.up;
                    break;
                case DIRECTION.LinearZ:
                    result = Vector3.forward;
                    break;
                case DIRECTION.RotationX:
                    result = Vector3.right;
                    break;
                case DIRECTION.RotationY:
                    result = Vector3.up;
                    break;
                case DIRECTION.RotationZ:
                    result = Vector3.forward;
                    break;
                case DIRECTION.Virtual:
                    result = Vector3.zero;
                    break;
            }

            return result;
        }

        //! Transfers a vector to the direction enumeration ���������䵽����ö��
        public DIRECTION VectorToDirection(bool torotatoin, Vector3 vector)
        {
            if (!torotatoin)
            {
                if (Vector3.Dot(vector, DirectionToVector(DIRECTION.LinearX)) == 1)//Dot:���������ĵ����
                {
                    return DIRECTION.LinearX;
                }

                if (Vector3.Dot(vector, DirectionToVector(DIRECTION.LinearY)) == 1)
                {
                    return DIRECTION.LinearY;
                }

                if (Vector3.Dot(vector, DirectionToVector(DIRECTION.LinearZ)) == 1)
                {
                    return DIRECTION.LinearZ;
                }
            }
            else
            {
                if (Vector3.Dot(vector, DirectionToVector(DIRECTION.RotationX)) == 1)
                {
                    return DIRECTION.RotationX;
                }
                if (Vector3.Dot(vector, DirectionToVector(DIRECTION.RotationY)) == 1)
                {
                    return DIRECTION.RotationY;
                }
                if (Vector3.Dot(vector, DirectionToVector(DIRECTION.RotationZ)) == 1)
                {
                    return DIRECTION.RotationZ;
                }
            }
            // if nothing return virtual
            return DIRECTION.Virtual;
        }

        /// <summary>
        /// ��ȡָ��Transform��ĳ�������ϵ�����ֵ
        /// </summary>
        /// <param name="thetransform"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public float GetLocalScale(Transform thetransform, DIRECTION direction)
        {
            float result = 1;
            switch (direction)
            {
                case DIRECTION.LinearX:
                    result = thetransform.lossyScale.x;
                    break;
                case DIRECTION.LinearY:
                    result = thetransform.lossyScale.y;
                    break;
                case DIRECTION.LinearZ:
                    result = thetransform.lossyScale.z;
                    break;
            }

            return result;
        }

        //! Gets back if the direction is linear or a rotation
        public static bool DirectionIsLinear(DIRECTION direction)
        {
            bool result = false;
            switch (direction)
            {
                case DIRECTION.LinearX:
                    result = true;
                    break;
                case DIRECTION.LinearY:
                    result = true;
                    break;
                case DIRECTION.LinearZ:
                    result = true;
                    break;
                case DIRECTION.RotationX:
                    result = false;
                    break;
                case DIRECTION.RotationY:
                    result = false;
                    break;
                case DIRECTION.RotationZ:
                    result = false;
                    break;
                case DIRECTION.Virtual:
                    result = true;
                    break;
            }

            return result;
        }

        /// <summary>
        /// ���¡���ȡ���и���Value�����ֶε���Ϣ��BehaviorInterfaceConnection�����������̳д���������е����е�Value���͵��ֶε����ƺ����ͽ��л�ȡ��
        /// </summary>
        /// <returns></returns>
        public List<BehaviorInterfaceConnection> UpdateConnectionInfo()
        {
            var ConnectionInfo = new List<BehaviorInterfaceConnection>();
            Type mytype = this.GetType();//���̳д�������ࣩ��ȡ���������
            FieldInfo[] fields = mytype.GetFields();//FieldInfo:�����ֶε����Բ��ṩ���ֶ�Ԫ���ݵķ���Ȩ��

            foreach (FieldInfo field in fields)
            {
                if (field != null)
                {
                    var type = field.FieldType;//FieldType:��ȡ���ֶζ�������͡�
                    if (type.IsSubclassOf(typeof(RDTS.Value)))//��Value�������(��Drive_Simple�ű��а�����ValueOutputFloat���͵��ֶ�Speed)
                    {
                        var info = new BehaviorInterfaceConnection();
                        info.Name = field.Name;//�ֶ�����
                        info.Value = (Value)field.GetValue(this);//��ȡ����̳�Value�������
                        ConnectionInfo.Add(info);
                    }
                }
            }
            return ConnectionInfo;
        }

        /// <summary>
        /// ��ȡ����Value���͵��ֶ�
        /// </summary>
        /// <returns></returns>
        public List<Value> GetConnectedValues()
        {
            var values = new List<Value>();
            Type mytype = this.GetType();//���̳д�������ࣩ��ȡ���������
            FieldInfo[] fields = mytype.GetFields();

            foreach (FieldInfo field in fields)
            {
                if (field != null)
                {
                    var type = field.FieldType;//�ֶ�����
                    if (type.IsSubclassOf(typeof(RDTS.Value)))//��Value�������(��Drive_Simple�ű��а�����ValueOutputFloat���͵��ֶ�Speed)
                    {
                        var sig = (Value)field.GetValue(this);
                        if (!ReferenceEquals(sig, null))
                            values.Add((Value)field.GetValue(this));//��ȡ����̳�Value�������
                    }
                }
            }
            return values;
        }

        //! Sets the visibility of this object including all subobjects  
        /// <summary>
        /// ���ô˶���Ŀɼ��ԣ����������Ӷ���
        /// </summary>
        /// <param name="visibility"></param>
        public void SetVisibility(bool visibility)
        {
            Renderer[] components = gameObject.gameObject.GetComponentsInChildren<Renderer>();
            if (components != null)
            {
                foreach (Renderer component in components)
                    component.enabled = visibility;
            }

            MeshRenderer[] meshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>();
            if (meshRenderers != null)
            {
                foreach (MeshRenderer meshrenderer in meshRenderers)
                    meshrenderer.enabled = visibility;
            }
        }
        /// <summary>
        /// ����ָ������Ŀɼ���
        /// </summary>
        /// <param name="go"></param>
        /// <param name="visibility"></param>
        public void SetVisibility(GameObject go, bool visibility)
        {
            Renderer[] components = go.gameObject.GetComponentsInChildren<Renderer>();
            if (components != null)
            {
                foreach (Renderer component in components)
                    component.enabled = visibility;
            }

            MeshRenderer[] meshRenderers = go.GetComponentsInChildren<MeshRenderer>();
            if (meshRenderers != null)
            {
                foreach (MeshRenderer meshrenderer in meshRenderers)
                    meshrenderer.enabled = visibility;
            }
        }



        /// <summary>
        /// ��ȡ���и���Value�����ֶε���Ϣ��BehaviorInterfaceConnection����
        /// �̳д�������������̳��� IValueInterface �ӿڣ���˷�������ʵ����IValueInterface�ӿ��еķ�����
        /// </summary>
        /// <returns></returns>
        public List<BehaviorInterfaceConnection> GetConnections()
        {
            return UpdateConnectionInfo(); 
        }

        /// <summary>
        /// ��ȡ����Value���͵��ֶ�
        /// </summary>
        /// <returns></returns>
        public List<Value> GetValues()
        {
            return GetConnectedValues();
        }

        //! Gets a child by name  ͨ������Ѱ��1���Ӷ���
        public GameObject GetChildByName(string name)
        {
            Transform[] children = transform.GetComponentsInChildren<Transform>();
            foreach (var child in children)
            {
                if (child.name == name)
                {
                    return child.gameObject;
                }
            }

            return null;
        }


        //! Gets all child by name ͨ������Ѱ�����е��Ӷ���
        public List<GameObject> GetChildsByName(string name)
        {
            List<GameObject> childs = new List<GameObject>();
            Transform[] children = transform.GetComponentsInChildren<Transform>();
            foreach (var child in children)
            {
                if (child.name == name)
                {
                    childs.Add(child.gameObject);
                }
            }

            return childs;
        }


        public GameObject GetChildByNameAlsoHidden(string name)
        {
            Transform[] children = transform.GetComponentsInChildren<Transform>(true);
            foreach (var child in children)
            {
                if (child.name == name)
                {
                    return child.gameObject;
                }
            }

            return null;
        }

        public List<GameObject> GetAllMeshesWithGroup(string group)
        {
            List<GameObject> list = new List<GameObject>();
            var groupcomps = Object.FindObjectsOfType<Group>();
            foreach (var groupcomp in groupcomps)
            {
                if (groupcomp.GetGroupName() == group)
                {
                    // Check if one parent has the same group
                    var mesh = groupcomp.gameObject.GetComponent<MeshFilter>();

                    if (!ReferenceEquals(mesh, null))
                    {
                        list.Add(groupcomp.gameObject);
                    }
                }
            }
            return list;
        }

        public List<GameObject> GetAllWithGroup(string group)
        {
            List<GameObject> list = new List<GameObject>();
            var groupcomps = Object.FindObjectsOfType<Group>();
            foreach (var groupcomp in groupcomps)
            {
                if (groupcomp.GetGroupName() == group)
                {
                    // Check if one parent has the same group
                    var parent = groupcomp.transform.parent;
                    bool add = true;
                    if (!ReferenceEquals(parent, null))
                    {
                        // search upwards
                        var uppergroups = parent.gameObject.GetComponentsInParent<Group>();
                        // is the group in one of the upper parents?
                        foreach (var uppergroup in uppergroups)
                        {
                            if (uppergroup.GetGroupName() == group)
                            {
                                add = false;
                            }
                        }
                    }

                    if (add)
                        list.Add(groupcomp.gameObject);
                }
            }
            return list;
        }

        public List<GameObject> GetAllWithGroups(List<string> groups)
        {
            List<GameObject> first;
            first = GetAllWithGroup(groups[0]);

            for (int i = 1; i < groups.Count; i++)
            {
                var newobjs = GetAllWithGroup(groups[i]);
                IEnumerable<GameObject> res = first.AsQueryable().Intersect(newobjs);
                first = res.ToList();
            }

            return first;
        }

        public List<GameObject> GetAllMeshesWithGroups(List<string> groups)
        {
            List<GameObject> first;
            first = GetAllMeshesWithGroup(groups[0]);

            for (int i = 1; i < groups.Count; i++)
            {
                var newobjs = GetAllWithGroup(groups[i]);
                IEnumerable<GameObject> res = first.AsQueryable().Intersect(newobjs);
                first = res.ToList();
            }
            return first;
        }

        public List<string> GetMyGroups()
        {
            List<string> list = new List<string>();
            var groups = GetComponents<Group>();
            foreach (var group in groups)
            {
                list.Add(group.GroupName);
            }

            return list;
        }

        public List<GameObject> GetMeshesWithSameGroups()
        {
            var list = GetMyGroups();
            var list2 = GetAllMeshesWithGroups(list);
            list2.Remove(this.gameObject);
            return list2;
        }


        public List<GameObject> GetAllWithSameGroups()
        {
            var list = GetMyGroups();
            var list2 = GetAllWithGroups(list);
            list2.Remove(this.gameObject);
            return list2;
        }

        //! Gets the top of an MU component (the first MU script going up in the hierarchy)
        //��ȡ MU ����Ķ�������νṹ�������ĵ�һ�� MU �ű���
        protected MU GetTopOfMu(GameObject obj)
        {
            if (obj != null)
            {
                var res = obj.transform.GetComponentsInParent<MU>(true);
                if (res.Length > 0)
                    return res[0];
                else
                    return null;
            }

            return null;
        }

        //! Gets the mesh renderers in the childrens
        public MeshRenderer GetMeshRenderer()
        {
            MeshRenderer renderers = gameObject.GetComponentInChildren<MeshRenderer>();
            return renderers;
        }

        //! sets the collider in all child objects
        /// <summary>
        /// ����/���ô˶��������е�Collider
        /// </summary>
        /// <param name="enabled"></param>
        public void SetCollider(bool enabled)
        {
            Collider[] components = gameObject.GetComponentsInChildren<Collider>();
            if (components != null)
            {
                foreach (Collider component in components)
                    component.enabled = enabled;
            }
        }
        /// <summary>
        /// ����/����ָ�����������е�Collider
        /// </summary>
        /// <param name="go"></param>
        /// <param name="enabled"></param>
        public void SetCollider(GameObject go, bool enabled)
        {
            Collider[] components = go.GetComponentsInChildren<Collider>();
            if (components != null)
            {
                foreach (Collider component in components)
                    component.enabled = enabled;
            }
        }

        //! Displays an error message
        public void ErrorMessage(string message)
        {
#if (UNITY_EDITOR)
            EditorUtility.DisplayDialog("RDTS Error for [" + this.gameObject.name + "]", message, "OK", "");
#endif
            Error(message, this);
        }

        public void ChangeConnectionMode(bool isconnected)
        {
            if (Active == ActiveOnly.DontChange)
                return;

            if (Active == ActiveOnly.Always)
            {
                if (this.enabled == false)
                    this.enabled = true;
            }

            if (Active == ActiveOnly.Connected)
            {
                if (isconnected)
                    this.enabled = true;
                else
                    this.enabled = false;
            }

            if (Active == ActiveOnly.Disconnected)
            {
                if (!isconnected)
                    this.enabled = true;
                else
                    this.enabled = false;
            }

            if (Active == ActiveOnly.Never)
            {
                this.enabled = false;
            }
        }

        //! Logs a message
        public void Log(string message)
        {
            _logger.Log("RDTS: " + message);
        }

        //! Logs a message with a relation to an object
        public void Log(string message, object obj)
        {
            _logger.Log("RDTS: Object [" + this.gameObject.name + "] " + message, obj);
        }

        //! Logs a warinng with a relation to an object
        public void Warning(string message, object obj)
        {
            _logger.LogWarning("RDTS: Object [" + this.gameObject.name + "] " + message, obj);
        }

        //! Logs an error with a relation to an object
        public void Error(string message, object obj)
        {
            _logger.LogError("RDTS: Object [" + this.gameObject.name + "] " + message, obj);
        }

        //! Logs an error
        public void Error(string message)
        {
            _logger.LogError("RDTS: " + message, this);
        }

        ////! Displays a gizmo for debugging positions ��ʾ����λ�õ�С����
        //public GameObject DebugPosition(string debugname, Vector3 position, Quaternion quaternation, float scale)
        //{
        //    GameObject debuggizmo = null;

        //    if (Game4AutomationController.EnablePositionDebug)//Ĭ��Ϊtrue
        //    {
        //        debuggizmo = GameObject.Find("debugname");
        //        if (debuggizmo == null)
        //        {
        //            var gizmo = UnityEngine.Resources.Load("Gizmo/Gizmo", typeof(GameObject));//����ϵС����
        //            debuggizmo = Instantiate((GameObject)gizmo, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0));
        //            debuggizmo.layer = Game4AutomationController.DebugLayer;
        //        }

        //        debuggizmo.transform.position = position;
        //        debuggizmo.transform.rotation = quaternation;
        //        debuggizmo.transform.localScale = Vector3.one * scale;
        //        debuggizmo.name = debugname;
        //    }

        //    return debuggizmo;
        //}

        //! Freezes all child components to the current poosition
        //Source�ű����õ�
        /// <summary>
        /// trueʱ����˶������ת���˶���falseʱ�����κ�����
        /// </summary>
        /// <param name="enabled"></param>
        public void SetFreezePosition(bool enabled)
        {
            Rigidbody[] components = gameObject.GetComponentsInChildren<Rigidbody>();
            if (components != null)
            {
                foreach (Rigidbody rigid in components)
                    if (enabled)
                    {
                        rigid.constraints = RigidbodyConstraints.FreezeAll;
                    }
                    else
                    {
                        rigid.constraints = RigidbodyConstraints.None;
                    }
            }
        }
        /// <summary>
        /// trueʱ����ָ���������ת���˶���falseʱ�����κ�����
        /// </summary>
        /// <param name="go"></param>
        /// <param name="enabled"></param>
        public void SetFreezePosition(GameObject go, bool enabled)
        {
            Rigidbody[] components = go.GetComponentsInChildren<Rigidbody>();
            if (components != null)
            {
                foreach (Rigidbody rigid in components)
                    if (enabled)
                    {
                        rigid.constraints = RigidbodyConstraints.FreezeAll;
                    }
                    else
                    {
                        rigid.constraints = RigidbodyConstraints.None;
                    }
            }
        }

        //! Initialiates the components and gets the reference to the RDTSController in the scene
        /// <summary>
        /// ��ʼ���������ȡ�Գ����� RDTSController ������
        /// </summary>
        protected void InitRDTS()
        {
            RDTSController = UnityEngine.Object.FindObjectOfType<RDTSController>();
            if (RDTSController == null)
            {
                Error(
                    "No RDTSController found - RDTSController Script needs to be once inside every RDTS Scene");
                Debug.Break();
            }

            if (Name == "")
            {
                Name = gameObject.name;
            }

            ChangeConnectionMode(RDTSController.Connected);


        }

        protected virtual void AfterAwake()
        {

        }



        public virtual void AwakeAlsoDeactivated()
        {

        }


        protected void Awake()
        {

            if (Application.isPlaying)
                InitRDTS();
            AfterAwake();
        }
    }
}