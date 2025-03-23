using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RDTS;
using NaughtyAttributes;
using UnityEditor;
using System.Linq;

namespace RDTS.NodeEditor
{

    public enum Status
    {
        Active,
        Disabled
    }


    [ExecuteAlways]
    public class NodeEditorInterface : BaseNodeEditorInterface
    {
        //public Status status = Status.Disabled;
        public BaseGraph graph;
        public ProcessGraphProcessor processor;

        public List<RDTSBehavior> BindObject = new List<RDTSBehavior>();
        List<string> bindGuids => BindObject.Select(bj => bj.Guid) as List<string>;
        public Dictionary<string, RDTSBehavior> bindObjPerGuid = new Dictionary<string, RDTSBehavior>();

        private bool isBind = false;//与graph绑定的处理

        [Button("Open NodeGraph")]
        void OpenNodeGraph()
        {
#if UNITY_EDITOR

            BindObject.ForEach(bj => bindObjPerGuid[bj.Guid] = bj);
            EditorWindow.GetWindow<StandardGraphWindow>().InitializeGraph(graph, bindObjPerGuid);
#endif
        }

        
        private void Awake()
        {
            ///Debug.LogWarning("awake !");
        }

        private void OnEnable()
        {
             isBind = true;
#if UNITY_EDITOR
            if (EditorWindow.HasOpenInstances<StandardGraphWindow>())
            {
                OpenNodeGraph();
            }

#endif

            /// Debug.LogWarning("OnEnable !");
        }


        private void Start()
        {

            if (Application.IsPlaying(gameObject))
            {
                if (graph != null)
                    processor = new ProcessGraphProcessor(graph);
            }


            /// Debug.LogWarning("Start !");

        }


        void Update()
        {


            if (Application.IsPlaying(gameObject))//play模式下
            {


                if (graph != null && processor != null)
                    processor.Run();
            }
#if UNITY_EDITOR
            else//编辑模式下
            {
                if (isBind)
                {
                    isBind = false;
                    OpenNodeGraph();
                }

            }
#endif

            /// Debug.LogWarning("Update !");

        }



    }





}
