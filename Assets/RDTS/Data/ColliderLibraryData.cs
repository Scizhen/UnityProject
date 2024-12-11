using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;


namespace RDTS.Data
{

    [CreateAssetMenu(menuName = "Parallel-RDTS/Create ColliderLibraryData", fileName = "ColliderLibraryData")]
    public class ColliderLibraryData : ScriptableObject//所有碰撞体
    {

        public List<ColliderLibrary> ColliderLibraryDataList = new List<ColliderLibrary>() { new ColliderLibrary() };

    }

    [Serializable]
    public class ColliderLibrary//一类碰撞体
    {
        public string type;//碰撞体类型名称
        public List<Colliders> colliders;

    }

    [Serializable]
    public class Colliders//一个碰撞体
    {
        public string name;//碰撞体类名
        public Texture icon;//图标
        public string notice;//注意
        public List<ColliderDetails> colliderDetails;//碰撞体组成

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
    public class ColliderDetails//碰撞体细节
    {
        [BoxGroup("Settings")] public ColliderSelect colliderSelect = ColliderSelect.BoxCollider;
        [BoxGroup("Settings")] public PhysicMaterial physicMaterial;
        [BoxGroup("Settings")] public Vector3 center;
        [ShowIf("colliderSelect", ColliderSelect.BoxCollider)][BoxGroup("Settings")] public Vector3 size;//方形碰撞体的大小
        [ShowIf("colliderSelect", ColliderSelect.CapsuleCollider)][BoxGroup("Settings")] public float radius;//胶囊半径
        [ShowIf("colliderSelect", ColliderSelect.CapsuleCollider)][BoxGroup("Settings")] public float height;//胶囊高
        [ShowIf("colliderSelect", ColliderSelect.CapsuleCollider)][BoxGroup("Settings")] public CapsuleColliderDirection direction;//胶囊方向
        [ShowIf("colliderSelect", ColliderSelect.SphereCollider)][BoxGroup("Settings")] public float sphereRadius;//球体半径

    }




}

