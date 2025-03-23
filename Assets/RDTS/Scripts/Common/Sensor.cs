//*************************************************************************
//Thanks for the code reference game4automation provides.                 *
//                                                                        *
//*************************************************************************
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;
namespace RDTS
{
    [System.Serializable]
    public class EventMUSensor : UnityEvent<MU, bool>
    {
    }

    [System.Serializable]
    public class EventGameobjectSensor : UnityEvent<GameObject, bool>
    {
    }

    /// <summary>
    /// ���MU���趨Layer�Ķ��󡢻��Ƹ������ߡ���ʾ���״̬��Ϣ
    /// </summary>
    [SelectionBase]
    //! The sensor is used for detecting MUs.  ���ڼ��MU
    //! Sensors are using Box Colliders for detecting MUs. The Sensor should be on Layer *Sensor*  ʹ����ײ��BoxCollider�����MU���Ҵ˲㼶LayerӦ��ΪSensor
    //! Layer settings are used. A behavior component (e.g. *Sensor_Standard*) must be added to the Sensor for providing connection to Values Input and 
    //! outputs.
    [ExecuteInEditMode]
    public class Sensor : BaseSensor, IValueInterface
    {
        // Public - UI Variables 
        [BoxGroup("Settings")] public bool DisplayStatus = true;//�Ƿ���ʾGizmo״̬
        [BoxGroup("Settings")]
        public string
            LimitSensorToTag; //!< Limits the function of the sensor to a certain MU tag - also MU names are working
                              //���������Ĺ�������Ϊĳ�� MU ��ǩ - MU ����Ҳ��Ч
        [BoxGroup("Settings")] public bool UseRaycast = false;//�Ƿ�ʹ�����߼��

        [BoxGroup("Settings")] [ShowIf("UseRaycast")] public Vector3 RayCastDirection = new Vector3(1, 0, 0);//���߷���
        [BoxGroup("Settings")] [ShowIf("UseRaycast")] public float RayCastLength = 1000f;//���߳���
        [BoxGroup("Settings")] [ShowIf("UseRaycast")] public float RayCastDisplayWidth = 0.01f;//����Ⱦ�������߿��
        [BoxGroup("Settings")] [ShowIf("UseRaycast")] [ReorderableList] public List<string> AdditionalRayCastLayers;
        [BoxGroup("Settings")] [ShowIf("UseRaycast")] public bool ShowSensorLinerenderer = false;//����Ⱦ���������� 3D �ռ��л������ɸ�������


        //!<  Display the status of the sensor by changing the material (color).
        [BoxGroup("Settings")] public Material MaterialOccupied; //!<  Material for displaying the occupied status. ��⵽����ʱ��ʾ�Ĳ���
        [BoxGroup("Settings")] public Material MaterialNotOccupied; //!<  Material for displaying the not occupied status. δ��⵽����ʱ��ʾ�Ĳ���
        [BoxGroup("Settings")] public bool PauseOnSensor = false; //!<  Pause simulation if sensor is getting high - can be used for debuging  ��ͣ������Inspector�ϵ�ĳЩֵ
        [BoxGroup("Interface Connection")] public ValueInputBool SensorOccupied; //! Boolean Value input for the Sensor signal. ����ź�


        private bool _isOccupiedNotNull;

        [Foldout("Events")]
        public EventMUSensor
            EventMUSensor; //!<  Unity event which is called for MU enter and exit. On enter it passes MU and true. On exit it passes MU and false.
        [Foldout("Events")]
        public EventGameobjectSensor
        EventNonMUGameObjectSensor; //!<  Unity event which is called for non MU objects enter and exit. On enter it passes gameobject (on which the collider was detected) and true. On exit it passes gameobject and false.

        [Foldout("Status")] public bool Occupied = false; //!<  True if sensor is occupied. ����⵽����Ϊtrue
        [Foldout("Status")] public GameObject LastTriggeredBy; //!< Last MU which has triggered the sensor. ��һ������⵽�Ķ���
        [Foldout("Status")] [ShowIf("UseRaycast")] public float RayCastDistance; //!< Last RayCast Distance if Raycast is used   ����ԭ�㵽���Ŀ��ľ���
        [Foldout("Status")] public int LastTriggeredID; //!< Last MUID which has triggered the sensor. �������⵽��MU��ID
        [Foldout("Status")] public int LastTriggeredGlobalID; //!<  Last GloabalID which has triggerd the sensor. �������⵽��MU��GlobalID
        [Foldout("Status")] public int Counter;//�ӷ��濪ʼ��⵽��������
        [Foldout("Status")] public List<MU> CollidingMus; // Currently colliding MUs with the sensor.  ��Sensor��⵽��MU
        [Foldout("Status")]
        public List<GameObject>//��Sensor��⵽�Ķ���MU�¿��ܰ�����������
            CollidingObjects; // Currently colliding GameObjects with the sensor (which can be more than MU because a MU can contain several GameObjects.


        public delegate void
            OnEnterDelegate(GameObject obj); //!< Delegate function for GameObjects entering the Sensor.

        public event OnEnterDelegate EventEnter;

        public delegate void OnExitDelegate(GameObject obj); //!< Delegate function for GameObjects leaving the Sensor.

        public event OnExitDelegate EventExit;


        // Private Variables
        private bool _occupied = false;
        private MeshRenderer _meshrenderer;
        private BoxCollider _boxcollider;
        private int layermask;//������
        [SerializeField] [HideInInspector] private float scale = 1000;
        private RaycastHit hit;//��ǰ�������еĶ���RaycastHit��������Ͷ���ȡ��Ϣ
        private RaycastHit lasthit;//��һ���������еĶ���
        private bool raycasthasthit;//Ϊtrue�����߽Ӵ������У���ĳ����Ϊfalse������δ�Ӵ�������
        private bool lastraycasthasthit;//��һ�������Ƿ�Ӵ�������
        private bool raycasthitchanged;//�������еĶ����Ƿ�ı�
        private Vector3 startposraycast;//����Ⱦ�����Ƶ��ߵ���ʼ�㣨���ߵ���ʼ�㣩
        private Vector3 endposraycast;//����Ⱦ�����Ƶ��ߵ�ĩ�˵㣨���ߵ�ĩ�˵㣩

        private LineRenderer linerenderer;//����Ⱦ��

        //! Delete all MUs in Sensor Area.
        /// <summary>
        /// ɾ�����м�⵽��MU
        /// </summary>
        public void DeleteMUs()
        {
            var tmpcolliding = CollidingObjects;
            foreach (var obj in tmpcolliding.ToArray())
            {
                var mu = GetTopOfMu(obj);
                if (mu != null)
                {
                    Destroy(mu.gameObject);
                }

                CollidingObjects.Remove(obj);
            }
        }


       
        // Use this when Script is inserted or Reset is pressed
        private void Reset()
        {
            ///���ô���Layer
            AdditionalRayCastLayers = new List<string>();
            AdditionalRayCastLayers.Add(RDTSLayer.MU.ToString());
            AdditionalRayCastLayers.Add(RDTSLayer.Obstacle.ToString());
            AdditionalRayCastLayers.Add(RDTSLayer.MovableTransport.ToString());

            ///����sensor����
            if (MaterialOccupied == null)
            {
                MaterialOccupied = UnityEngine.Resources.Load(RDTSPath.path_SensorOccupied, typeof(Material)) as Material;//����ָ��·���Ĳ���
            }

            if (MaterialNotOccupied == null)
            {
                MaterialNotOccupied = UnityEngine.Resources.Load(RDTSPath.path_SensorNotOccupied, typeof(Material)) as Material;//����ָ��·���Ĳ���
            }
            ///����ײ�оͻ�ȡ��û������������Ͷ��
            _boxcollider = GetComponent<BoxCollider>();
            if (_boxcollider != null)
                _boxcollider.isTrigger = true;
            else
                UseRaycast = true;
        }

        // Use this for initialization
        private void Start()
        {
            _isOccupiedNotNull = SensorOccupied != null;//����ź��Ƿ�Ϊ��
            CollidingObjects = new List<GameObject>();
            CollidingMus = new List<MU>();
            if (LimitSensorToTag == null)
                LimitSensorToTag = "";
            _boxcollider = GetComponent<BoxCollider>();
            if (_boxcollider != null)
            {
                _meshrenderer = _boxcollider.gameObject.GetComponent<MeshRenderer>();
            }

            if (_boxcollider == null && !UseRaycast && Application.isPlaying)
            {
                ErrorMessage("Sensors which are not using a Raycast need to have a BoxCollider on the same Gameobject as this Sensor script is attached to");
            }

            if (Application.isPlaying)///������ģʽ��
            {
                scale = RDTSController.Scale;
                AdditionalRayCastLayers.Add(LayerMask.LayerToName(gameObject.layer));//���Ӵ˶���Ĳ㼶
                // create line renderer for raycast if not existing
                //������Ͷ��û������Ⱦ������Ϊ������
                if (UseRaycast && ShowSensorLinerenderer)
                {
                    linerenderer = GetComponent<LineRenderer>();
                    if (linerenderer == null)
                        linerenderer = gameObject.AddComponent<LineRenderer>();
                }

            }

            if (AdditionalRayCastLayers == null)
                AdditionalRayCastLayers = new List<string>();
            layermask = LayerMask.GetMask(AdditionalRayCastLayers.ToArray());
            ShowStatus();
        }

        /// <summary>
        /// ������Ⱦ�����ƹ���
        /// </summary>
        private void DrawLine()
        {
            if (ShowSensorLinerenderer)
            {
                List<Vector3> pos = new List<Vector3>();
                pos.Add(startposraycast);
                pos.Add(endposraycast);
                linerenderer.startWidth = RayCastDisplayWidth;
                linerenderer.endWidth = RayCastDisplayWidth;
                linerenderer.SetPositions(pos.ToArray());
                linerenderer.useWorldSpace = true;
                if (raycasthasthit)
                {
                    linerenderer.material = MaterialOccupied;
                }
                else
                {
                    linerenderer.material = MaterialNotOccupied;
                }
            }
        }

        /// <summary>
        /// ��Debug����ƹ��ߣ�����������ײ���ཻ����ֺ�ɫ�ҳ���Ϊԭ�㵽��ײ�㣬δ�ཻ����ֻ�ɫ�ҳ���Ϊ�����ĳ���
        /// </summary>
        private void Raycast()
        {
            if (!Application.isPlaying)//����playģʽ��
            {
                var list = new List<string>(AdditionalRayCastLayers);
                list.Add(LayerMask.LayerToName(gameObject.layer));
                layermask = LayerMask.GetMask(list.ToArray());
            }

            float scale = 1000;
            raycasthitchanged = false;
            var globaldir = transform.TransformDirection(RayCastDirection);
            var display = Vector3.Normalize(globaldir) * RayCastLength / scale;//�������ߵĳ���
            startposraycast = transform.position;
            if (Physics.Raycast(transform.position, globaldir, out hit, RayCastLength / scale, layermask))//�򳡾��е�������ײ��Ͷ��һ�����ߡ� ����������κ���ײ���ཻ������ true�����򷵻� false
            {
                //hit.distance��������ԭ�㵽ײ����ľ���
                var dir = Vector3.Normalize(globaldir) * hit.distance;//����ԭ�㵽ײ����ľ���
                if (Application.isPlaying)
                    scale = RDTSController.Scale;

                RayCastDistance = hit.distance * scale;
                if (DisplayStatus) Debug.DrawRay(transform.position, dir, Color.red, 0, true);//��DisplayStatusΪtrue�����ƺ�ɫ����
                raycasthasthit = true;//���ñ�־λ
                if (hit.collider != lasthit.collider)//���������е���ײ������һ�����еĲ�ͬ
                    raycasthitchanged = true;
                endposraycast = startposraycast + dir;//����ĩ�˵�����
            }
            else
            {
                if (DisplayStatus) Debug.DrawRay(transform.position, display, Color.yellow, 0, true);//��DisplayStatusΪtrue�����ƻ�ɫ����
                raycasthasthit = false;//���ñ�־λ
                endposraycast = startposraycast + display;
                RayCastDistance = 0;
            }

        }

        // Shows Status of Sensor
        /// <summary>
        /// ��ʾ��ص�״̬��Ϣ
        /// </summary>
        private void ShowStatus()
        {

            if (CollidingObjects.Count == 0)//û�м�⵽��ײ����
            {
                LastTriggeredBy = null;
                LastTriggeredID = 0;
                LastTriggeredGlobalID = 0;
            }
            else
            {
                GameObject obj = CollidingObjects[CollidingObjects.Count - 1];
                if (!ReferenceEquals(obj, null))//��ײ����Ϊ��
                {
                    var LastTriggeredByMU = GetTopOfMu(obj);//��ȡ��ײ��������MU���
                    if (!ReferenceEquals(LastTriggeredByMU, null))//��MU
                        LastTriggeredBy = LastTriggeredByMU.gameObject;
                    else//����MU����⵽�����㼶
                        LastTriggeredBy = obj;

                    if (LastTriggeredByMU != null)
                    {
                        LastTriggeredID = LastTriggeredByMU.ID;
                        LastTriggeredGlobalID = LastTriggeredByMU.GlobalID;
                    }
                }
            }

            //������ײ�ͼ�⵽�������������ò��ʣ��Ըı�Sensor�����״̬
            if (CollidingObjects.Count > 0)
            {
                _occupied = true;
                if (DisplayStatus && _meshrenderer != null)
                {
                    _meshrenderer.material = MaterialOccupied;
                }
            }
            else
            {
                _occupied = false;
                if (DisplayStatus && _meshrenderer != null)
                {
                    _meshrenderer.material = MaterialNotOccupied;
                }
            }

            Occupied = _occupied;
        }

        // ON Collission Enter ������ײ��������ʱ������
        private void OnTriggerEnter(Collider other)
        {
            GameObject obj = other.gameObject;
            var tmpcolliding = CollidingObjects;
            var muobj = GetTopOfMu(obj);//��ȡ��MU����

            if ((LimitSensorToTag == "" || ((muobj.tag == LimitSensorToTag) || muobj.Name == LimitSensorToTag)))//δ���Ʊ�ǩ ���⵽��MU�ı�ǩ�����Ƶı�ǩ ���⵽��MU�����������Ƶı�ǩ
            {
                if (PauseOnSensor)
                    Debug.Break();//��ͣ�� Editor�� ��Ҫ���� Inspector �ϵ�ĳЩֵ�����޷��ֶ���ͣ��ʱ����ǳ�����
                if (!CollidingObjects.Contains(obj))
                    CollidingObjects.Add(obj);


                ShowStatus();//��ʾ״̬��Ϣ

                if (muobj != null)
                {
                    if (!CollidingMus.Contains(muobj))
                    {
                        if (EventEnter != null)
                            EventEnter(muobj.gameObject);
                        Counter++;
                        muobj.EventMUEnterSensor(this);
                        CollidingMus.Add(muobj);
                        if (EventMUSensor != null)
                            EventMUSensor.Invoke(muobj, true);//��⵽��MU��Event����
                    }
                }
                else
                {
                    if (EventEnter != null)
                        EventEnter(obj);
                    if (EventNonMUGameObjectSensor != null)
                        EventNonMUGameObjectSensor.Invoke(obj, true);//��⵽�Ĳ���MU��Event����
                }
            }
        }

        public void OnMUPartsDestroyed(GameObject obj)
        {
            CollidingObjects.Remove(obj);
        }

        //�Ƴ���MU
        public void OnMUDelete(MU muobj)
        {

            CollidingObjects.Remove(muobj.gameObject);

            // Check if remaining colliding objects belong to same mu
            var coolliding = CollidingObjects.ToArray();
            var i = 0;
            do
            {
                if (i < coolliding.Length)
                {
                    var thismuobj = GetTopOfMu(coolliding[i]);
                    if (thismuobj == muobj)
                    {
                        CollidingObjects.Remove(coolliding[i]);
                    }
                }

                i++;
            } while (i < coolliding.Length);
            CollidingMus.Remove(muobj);
            if (EventExit != null)
                EventExit(muobj.gameObject);
            if (EventMUSensor != null)
                EventMUSensor.Invoke(muobj, false);
            muobj.EventMUExitSensor(this);
            LastTriggeredBy = null;
            LastTriggeredID = 0;
            LastTriggeredGlobalID = 0;
            ShowStatus();
        }



        // ON Collission Exit  �˳���ײ��������ʱ������
        private void OnTriggerExit(Collider other)
        {
            GameObject obj = other.gameObject;
            if (!ReferenceEquals(obj, null))
            {

                var muobj = GetTopOfMu(obj);
                var tmpcolliding = CollidingObjects;
                var dontdelete = false;
                CollidingObjects.Remove(obj);

                // Check if remaining colliding objects belong to same mu
                foreach (var thisobj in CollidingObjects)
                {
                    var thismuobj = GetTopOfMu(thisobj);
                    if (thismuobj == muobj)
                    {
                        dontdelete = true;
                    }
                }

                if (!dontdelete)
                {

                    if (muobj != null && CollidingMus.Contains(muobj))
                    {
                        CollidingMus.Remove(muobj);
                        if (EventExit != null)
                            EventExit(muobj.gameObject);
                        if (EventMUSensor != null)
                            EventMUSensor.Invoke(muobj, false);
                        muobj.EventMUExitSensor(this);
                    }
                    else
                    {
                        if (EventNonMUGameObjectSensor != null)
                            EventNonMUGameObjectSensor.Invoke(obj, false);
                        if (EventExit != null)
                            EventExit(obj);
                    }
                }
                ShowStatus();
            }
        }

        private void FixedUpdate()
        {
            if (Application.isPlaying && UseRaycast)
            {
                Raycast();

                // last raycast has left 
                if ((lastraycasthasthit && !raycasthasthit) || raycasthitchanged)
                {
                    if (lasthit.collider != null)
                        OnTriggerExit(lasthit.collider);//����Ҫ�뿪���߼�ⷶΧʱ
                }

                if ((raycasthasthit && !lastraycasthasthit) || raycasthitchanged)
                {
                    // new raycast hit
                    OnTriggerEnter(hit.collider);//����Ҫ�������߼�ⷶΧʱ
                }

                lastraycasthasthit = raycasthasthit;
                lasthit = hit;

            }
            if (Application.isPlaying)
                // Set external Value Outputs
                if (_isOccupiedNotNull)
                    SensorOccupied.Value = Occupied;
        }

        private void Update()
        {
            if (!Application.isPlaying && UseRaycast)
            {
                layermask = LayerMask.GetMask(AdditionalRayCastLayers.ToArray());
                Raycast();//Debug����ƹ��ߣ��޿��
            }

            if (Application.isPlaying && UseRaycast && DisplayStatus)
            {
                DrawLine();//����Ⱦ�����ƹ��ߣ��п��
            }
        }
    }
}