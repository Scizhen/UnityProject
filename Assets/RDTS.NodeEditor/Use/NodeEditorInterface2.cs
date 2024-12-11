using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RDTS;
using NaughtyAttributes;
using UnityEditor;
using System.Linq;

namespace RDTS.NodeEditor
{

    public class NodeEditorInterface2 : BaseNodeEditorInterface
    {
        //public Status status = Status.Disabled;
        public BaseGraph graph;
        public ProcessGraphProcessor processor;

        public List<RDTSBehavior> BindObject;
        List<string> bindGuids => BindObject.Select(bj => bj.Guid) as List<string>;
        public Dictionary<string, RDTSBehavior> bindObjPerGuid = new Dictionary<string, RDTSBehavior>();

        //private bool isBind = false;//��graph�󶨵Ĵ���

#if UNITY_EDITOR
        [Button("Open NodeGraph")]
        void OpenNodeGraph()
        {

            BindObject.ForEach(bj => bindObjPerGuid[bj.Guid] = bj);
            EditorWindow.GetWindow<StandardGraphWindow>().InitializeGraph(graph, bindObjPerGuid);

        }
#endif

        private void Awake()
        {
            ///Debug.LogWarning("awake !");
        }

        private void OnEnable()
        {
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


            if (Application.IsPlaying(gameObject))//playģʽ��
            {
                

                if (graph != null && processor != null)
                {
                    processor.Run();
                   
                }
                   
            }



            /// Debug.LogWarning("Update !");

        }



    }





}
