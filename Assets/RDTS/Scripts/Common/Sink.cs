
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

namespace RDTS
{

    [System.Serializable]
    public class SinkEventOnDestroy : UnityEngine.Events.UnityEvent<MU> { }


    /// <summary>
    /// 消除MU。标志位DeleteMus为true时，在碰撞检测到对象时就会消除，也可通过接收信号消除。
    /// </summary>
    [SelectionBase]
    [RequireComponent(typeof(BoxCollider))]
    //! Sink to destroy objects in the scene
    public class Sink : BaseSink
    {
        // Public - UI Variables 
        [Header("Settings")] public bool DeleteMus = true; //!< Delete MUs 是否删除MU
        [ShowIf("DeleteMus")] public bool Dissolve = true; //!< Dissolve MUs 是否消解MU
        [ShowIf("DeleteMus")] public string DeleteOnlyTag; //!< Delete only MUs with defined Tag 只删除指定Tag的MU
        [ShowIf("DeleteMus")] public float DestroyFadeTime=0.5f; //!< Time to fade out MU 消解的延迟时间
        [Header("Sink IO's")] public ValueOutputBool Delete; //!< PLC output for deleting MUs  删除信号，若不为空则赋值给DeleteMus
        private bool _lastdeletemus = false;
    
        [Header("Status")] 
        [ReadOnly] public float SumDestroyed; //!< Sum of destroyed objects  已销毁删除的MU数量
        [ReadOnly] public float DestroyedPerHour; //!< Sum of destroyed objects per Hour  每一小时销毁的MU数量
        [ReadOnly] public List<GameObject> CollidingObjects; //!< Currently colliding objects  当前碰撞检测到的对象列表

        public SinkEventOnDestroy OnMUDelete;//删除MU时调用的事件
        
        private bool _isDeleteNotNull;

        // Use this when Script is inserted or Reset is pressed
        private void Reset()
        {
            GetComponent<BoxCollider>().isTrigger = true;
        }    
    
        // Use this for initialization
        private void Start()
        {
            _isDeleteNotNull = Delete != null;
        }

        /// <summary>
        /// 删除CollidingObjects列表中所有的MU对象
        /// </summary>
        public void DeleteMUs()
        {
            
            var tmpcolliding = CollidingObjects;
            foreach (var obj in tmpcolliding.ToArray())
            {
                var mu = GetTopOfMu(obj);
                if (mu != null)
                {
                    if (DeleteOnlyTag == "" || (mu.gameObject.tag == DeleteOnlyTag))
                    {

                        OnMUDelete.Invoke(mu);
                        if (!Dissolve)
                             Destroy(mu.gameObject);
                        else
                            mu.Dissolve(DestroyFadeTime);
                        SumDestroyed++;
                    }
                }

                CollidingObjects.Remove(obj);
            }
        }
    
        // ON Collission Enter
        private void OnTriggerEnter(Collider other)
        {
            GameObject obj = other.gameObject;
            CollidingObjects.Add(obj);
            if (DeleteMus==true)
            {
                // Act as Sink
                DeleteMUs();
            }
        }
    
        // ON Collission Exit
        private void OnTriggerExit(Collider other)
        {
            GameObject obj = other.gameObject;
            CollidingObjects.Remove(obj);
        }

        private void Update()
        {
            DestroyedPerHour = SumDestroyed / (Time.time / 3600);
            if (_isDeleteNotNull)
            {
                DeleteMus = Delete.Value;
            }
        
            if (DeleteMus && !_lastdeletemus)
            {
                DeleteMUs();
            }
            _lastdeletemus = DeleteMus;

        }
    }
}