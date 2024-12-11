///********************************************************************************************
///this is part of Robot Digital Twin System.                                                 *
///@Copyright by Shaw.S , 2023                                                                *
///Do not distribute without authorization,thanks!                                            *
///********************************************************************************************
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RDTS.Utility
{

 
    [SelectionBase]
    [ExecuteInEditMode]
    /// <summary>
    /// �������ר������������
    /// ���ּ�ⷽʽ��1.��ײ������⣨����Χ������Ҫ���BoxCollider�����������isTriggerΪtrue��
    ///               2.���߼�⣨�߶η�Χ������useRaycast��Ϊtrue���������ߵķ���ͳ���
    /// </summary>
    public class Detector : BaseDetectEffector
    {
        [BoxGroup("Signal Output")]
        public ValueMiddleBool SignalDetect; //����ź�
        [BoxGroup("Signal Output")]
        public ValueMiddleInt SignalCount; //����Ѽ������ź�

        //[BoxGroup("Settings")] public bool pauseEditor = false; //!< ��ͣ������Inspector�ϵ�ĳЩֵ����δ���Ӵ˹��ܣ�
        [BoxGroup("Settings")] public bool useRaycast = true;//<! �Ƿ�ʹ�����߼��
        [BoxGroup("Settings")] [ShowIf("useRaycast")] public Vector3 RayCastDirection = new Vector3(1, 0, 0);//<! ���߷���
        [BoxGroup("Settings")] [ShowIf("useRaycast")] public float RayCastLength = 1000f;//<! ���߳���
        //[BoxGroup("Settings")] [ShowIf("useRaycast")] public Material MaterialDetected; //!< ��⵽����ʱ��ʾ�Ĳ���
        //[BoxGroup("Settings")] [ShowIf("useRaycast")] public Material MaterialNotDetected; //!< δ��⵽����ʱ��ʾ�Ĳ���
        [BoxGroup("Settings")] [ShowIf("useRaycast")] public float RayCastDistance;//<! ����ԭ�㵽����ľ���
        [BoxGroup("Settings")] public bool isGetCompetent = true;//<! �Ƿ��ȡ�������ϵ�������



        /* �����¼� */
        [Foldout("Events")] public EventEffect EvenDetectStart;
        [Foldout("Events")] public EventEffect EvenDetectEnd;


        private bool _lastSignalClear = false;

        protected BoxCollider _boxCollider;

        protected RaycastHit hit;//��ǰ�������еĶ���RaycastHit��������Ͷ���ȡ��Ϣ
        protected RaycastHit lasthit;//��һ���������еĶ���
        protected bool raycasthasthit;//Ϊtrue�����߽Ӵ������У���ĳ����Ϊfalse������δ�Ӵ�������
        protected bool lastraycasthasthit;//��һ�������Ƿ�Ӵ�������
        protected bool raycasthitchanged;//�������еĶ����Ƿ�ı�
        protected Vector3 startposraycast;//���ߵ���ʼ��
        protected Vector3 endposraycast;//���ߵ�ĩ�˵�

        protected int layermask;//���ֲ�



        // Start is called before the first frame update
        private void Start()
        {
            if (SignalClear != null)
                _lastSignalClear = SignalClear.Value;

        }

        // Update is called once per frame
        private void Update()
        {

            if(SignalClear != null && SignalClear.Value && !_lastSignalClear)
            {
                ClearEffectObjects();
            }

            ///�ڱ༭ģʽ�£��ҿ��������߼��ģʽ
            if (!Application.isPlaying && useRaycast)
            {
                DrawRaycast();//Debug����ƹ��ߣ��޿��

                if (StopEffectorDetectBySignal())
                    return;

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



            //�м�⵽�Ķ������true����֮���false
            if (SignalDetect != null)
                SignalDetect.Value = (EffectObjects.Count != 0) ? true : false;
            //�����⵽�Ķ�������
            if (SignalCount != null)
                SignalCount.Value = EffectObjects.Count;
            //��¼����źŵ�ֵ
            if (SignalClear != null)
                _lastSignalClear = SignalClear.Value;
        }

        private void FixedUpdate()
        {
          

            ///������ģʽ�£��ҿ��������߼��ģʽ
            if (Application.isPlaying && useRaycast)
            {
                DrawRaycast();

                if (StopEffectorDetectBySignal())
                    return;

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
        }

        protected void OnEnable()
        {
            //������ײ��Ϊ������ʽ
            _boxCollider = GetComponent<BoxCollider>();
            if (_boxCollider != null)
                _boxCollider.isTrigger = true;
            else//����Χ�в�������ʹ������ģʽ
                useRaycast = true;

            AddDefaultTagAndLayer();

            ////���ü��������
            //if (MaterialDetected == null)
            //{
            //    MaterialDetected = UnityEngine.Resources.Load(RDTSPath.path_SensorOccupied, typeof(Material)) as Material;//����ָ��·���Ĳ���
            //}

            //if (MaterialNotDetected == null)
            //{
            //    MaterialNotDetected = UnityEngine.Resources.Load(RDTSPath.path_SensorNotOccupied, typeof(Material)) as Material;//����ָ��·���Ĳ���
            //}
        }

        private void Reset()
        {
            //������ײ��Ϊ������ʽ
            _boxCollider = GetComponent<BoxCollider>();
            if (_boxCollider != null)
                _boxCollider.isTrigger = true;
            else//����Χ�в�������ʹ������ģʽ
                useRaycast = true;

            AddDefaultTagAndLayer();
        }


        protected void OnTriggerEnter(Collider other)
        {
            if (StopEffectorDetectBySignal())
                return;

            GameObject go = other.gameObject;
            //�ж���ײ�����Ķ����Ƿ����Tag��Layer������
            if (!SusGameObjectLimitTagAndlayer(go))
                return;

            if (!EffectObjects.Contains(go))
            {
                EffectObjects.Add(go);//���뵽�б�
                if(isGetCompetent)
                    effectObjPerGo[go] = new EffectObject() { rigidbody = other.GetComponent<Rigidbody>() ?? null };//�����ֵ�
            }


            EvenDetectStart.Invoke();

        }




        protected void OnTriggerExit(Collider other)
        {
            if (StopEffectorDetectBySignal())
                return;


            GameObject go = other.gameObject;

            if (EffectObjects.Contains(go))
            {
                EffectObjects.Remove(go);//���б��Ƴ�
                if (effectObjPerGo.ContainsKey(go))//���ֵ��Ƴ�
                    effectObjPerGo.Remove(go);          
            }



            EvenDetectEnd.Invoke();

        }


        /// <summary>�Ӽ������б����Ƴ�ָ���Ķ��� </summary>
        public void RemoveGivenObject(GameObject go)
        {
            if (EffectObjects.Contains(go))
                EffectObjects.Remove(go);
        }


        /// <summary> ��ռ������б� </summary>
        public void ClearDetectObjects()
        {
            EffectObjects.Clear();
        }



        #region  ���߼��ģʽ


        protected void DrawRaycast()
        {

            float scale = 1000;
            raycasthitchanged = false;
            var globaldir = transform.TransformDirection(RayCastDirection);
            var display = Vector3.Normalize(globaldir) * RayCastLength / scale;//�������ߵĳ���
            startposraycast = transform.position;
            layermask = QM.GetLayerMask(LimitLayer);
            if (Physics.Raycast(startposraycast, globaldir, out hit, RayCastLength / scale, layermask))//�򳡾��е�������ײ��Ͷ��һ�����ߡ� ����������κ���ײ���ཻ������ true�����򷵻� false
            {
                //hit.distance��������ԭ�㵽ײ����ľ���
                var dir = Vector3.Normalize(globaldir) * hit.distance;//����ԭ�㵽ײ����ľ���

                RayCastDistance = hit.distance * scale;
                Debug.DrawRay(startposraycast, dir, Color.red, 0, true);//��DisplayStatusΪtrue�����ƺ�ɫ����
                raycasthasthit = true;//���ñ�־λ
                if (hit.collider != lasthit.collider)//���������е���ײ������һ�����еĲ�ͬ
                    raycasthitchanged = true;
                endposraycast = startposraycast + dir;//����ĩ�˵�����
            }
            else
            {
                Debug.DrawRay(startposraycast, display, Color.yellow, 0, true);//��DisplayStatusΪtrue�����ƻ�ɫ����
                raycasthasthit = false;//���ñ�־λ
                endposraycast = startposraycast + display;
                RayCastDistance = 0;
            }


        }





        #endregion




    }



}