using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;


namespace RDTS.Data
{

    [CreateAssetMenu(menuName = "Parallel-RDTS/Create ColliderLibraryData", fileName = "ColliderLibraryData")]
    public class ColliderLibraryData : ScriptableObject//������ײ��
    {

        public List<ColliderLibrary> ColliderLibraryDataList = new List<ColliderLibrary>() { new ColliderLibrary() };

    }

    [Serializable]
    public class ColliderLibrary//һ����ײ��
    {
        public string type;//��ײ����������
        public List<Colliders> colliders;

    }

    [Serializable]
    public class Colliders//һ����ײ��
    {
        public string name;//��ײ������
        public Texture icon;//ͼ��
        public string notice;//ע��
        public List<ColliderDetails> colliderDetails;//��ײ�����

    }

    public enum ColliderSelect
    { 
        BoxCollider,
        CapsuleCollider,
        SphereCollider,
        TerrainCollider,
        WheelCollider

    }
    public enum CapsuleColliderDirection
    { 
        X,
        Y,
        Z

    }

    [Serializable]
    public class ColliderDetails//��ײ��ϸ��
    {
        [BoxGroup("Settings")] public ColliderSelect colliderSelect = ColliderSelect.BoxCollider;
        [BoxGroup("Settings")] public PhysicMaterial physicMaterial;
        [BoxGroup("Settings")] public Vector3 center;
        [ShowIf("colliderSelect", ColliderSelect.BoxCollider)][BoxGroup("Settings")] public Vector3 size;//������ײ��Ĵ�С
        [ShowIf("colliderSelect", ColliderSelect.CapsuleCollider)][BoxGroup("Settings")] public float radius;//���Ұ뾶
        [ShowIf("colliderSelect", ColliderSelect.CapsuleCollider)][BoxGroup("Settings")] public float height;//���Ҹ�
        [ShowIf("colliderSelect", ColliderSelect.CapsuleCollider)][BoxGroup("Settings")] public CapsuleColliderDirection direction;//���ҷ���
        [ShowIf("colliderSelect", ColliderSelect.SphereCollider)][BoxGroup("Settings")] public float sphereRadius;//����뾶

    }




}

