///********************************************************************************************
///this is part of Robot Digital Twin System.                                                 *
///@Copyright by Shaw.S , 2023                                                                *
///Do not distribute without authorization,thanks!                                            *
///********************************************************************************************
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;



namespace RDTS.Utility
{


    /// <summary>
    /// �����������������ЧӦ��ʹ�ã����ڵ��������Transform��Rigidbody��MeshRender�������
    /// </summary>
    public class Adjuster : BaseWorkEffector
    {
        public BaseEffector effector;//��ϵ�ЧӦ��

        [BoxGroup("Settings")] public float adjustDelay = 0;//�������ӳ�ʱ��
        [BoxGroup("Settings")] public bool isAdjustTransform = false;//�Ƿ���б任�ĵ���
        [BoxGroup("Settings")] public bool isAdjustRigidbody = false;//�Ƿ���и���ĵ���
        [BoxGroup("Settings")] public bool isAdjustMaterial = false;//�Ƿ���в��ʵĵ���


        [BoxGroup("Adjust Transform")][ShowIf("isAdjustTransform")] public bool isLocal = true;//��Ϊtrue���������ڸ��任�����ꡣΪfalse�������������
        [BoxGroup("Adjust Transform")][ShowIf("isAdjustTransform")] public Vector3 newPosition;//λ��
        [BoxGroup("Adjust Transform")][ShowIf("isAdjustTransform")] public Vector3 newRotation;//��ת
        [BoxGroup("Adjust Transform")][ShowIf("isAdjustTransform")] public Vector3 newScale;//����

        [BoxGroup("Adjust Rigidbody")][ShowIf("isAdjustRigidbody")] public bool useGravity = true;//�Ƿ�ʹ������
        [BoxGroup("Adjust Rigidbody")][ShowIf("isAdjustRigidbody")] public bool isKinematic = false;//�Ƿ����˶�ѧ

        [BoxGroup("Adjust Material")][ShowIf("isAdjustMaterial")] public Material newMaterial;//Ҫ���е����Ĳ���


        [BoxGroup("Signal Input")] public ValueMiddleBool SignalStartAdjust; //��ʼ�����ź�
        [BoxGroup("Signal Input")][ReadOnly] public  bool _lastSignalStartAdjust = false;
       
     


        /* �����¼� */
        [Foldout("Events")] public EventEffect EvenAdjustStart;
        [Foldout("Events")] public EventEffect EvenAdjustEnd;

        private Coroutine adjustCoroutine;///�ñ����洢Э����Ϣ�����Թر�Э��


        // Start is called before the first frame update
        void Start()
        {
            //���ù����ź�
            SetSignalWorkStart(false);
            SetSignalWorkEnd(false);
        }


        // Update is called once per frame
        void FixedUpdate()
        {
            if(StopEffectorWorkBySignal())
                return;

            //�����ЧӦ��Ϊ�գ�Ҳ��������κε���
            if (effector == null)
                return;

            //��ȡ����ЧӦ����Ч������
            EffectObjects = effector.EffectObjects;

            if (SignalStartAdjust != null &&  SignalStartAdjust.Value && !_lastSignalStartAdjust)//�ź�ֵ��falseתΪtrueʱ
            {
                ///QM.Log("SignalStartAdjust");
                //�������������ֱ�ӷ���
                if (!isAdjustTransform && !isAdjustRigidbody && !isAdjustMaterial && EffectObjects.Count==0)
                    return;

                //���ù�����ʼ�ź�
                SetSignalWorkStart(true);
                SetSignalWorkEnd(false);


                //���е�������
                AdjustObjects(adjustDelay);




            }


            //��¼��һ���ź�ֵ
            if(SignalStartAdjust != null)  _lastSignalStartAdjust = SignalStartAdjust.Value;


        }



        /// <summary>
        /// �ӳٵ���
        /// </summary>
        /// <param name="delay"></param>
        protected void AdjustObjects(float delay = 0f)
        {
            adjustCoroutine = StartCoroutine(DelayAjustObjects(delay));///����Э��
            EvenAdjustStart.Invoke();//��ʼ����ʱ���¼�����
        }


        /// <summary>
        /// Э�̷������ӳٵ���
        /// </summary>
        /// <param name="delay"></param>
        /// <returns></returns>
        protected IEnumerator DelayAjustObjects(float delay = 0f)
        {
            yield return new WaitForSeconds(delay);//�ӳ�ʱ��

            //���е�������
            EffectObjects.ForEach(eobj => {

                ///���������λ�˱任��λ�ã���ת������
                if (isAdjustTransform)
                {
                    if (isLocal)
                    {
                        eobj.transform.localPosition += newPosition;
                        //eobj.transform.localRotation = Quaternion.Euler(newRotation);//��ת���̶�ֵnewRotation
                        eobj.transform.Rotate(newRotation, Space.Self);//��ԭ���Ƕ��ϼ�����תnewRotation
                        eobj.transform.localScale += newScale;
                    }
                    else
                    {
                        eobj.transform.position += newPosition;
                        //eobj.transform.rotation = Quaternion.Euler(newRotation);
                        eobj.transform.Rotate(newRotation, Space.World);
                        eobj.transform.localScale += newScale;
                    }
                }
                ///��������ĸ�������
                if (isAdjustRigidbody)
                {
                    //��νϺõ����ø������
                    effector.effectObjPerGo[eobj].rigidbody.useGravity = useGravity;
                    effector.effectObjPerGo[eobj].rigidbody.isKinematic = isKinematic;
                }
                ///��������Ĳ���
                if (isAdjustMaterial)
                {
                    QM.AddMaterialToGameobject(eobj, newMaterial);
                }

            });

            //���ù�������ź�
            SetSignalWorkStart(false);
            SetSignalWorkEnd(true);

            EvenAdjustEnd.Invoke();//��������ʱ���¼�����

        }



    }



}
