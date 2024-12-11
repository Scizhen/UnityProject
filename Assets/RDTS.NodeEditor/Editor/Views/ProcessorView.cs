using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RDTS.NodeEditor
{
    /// <summary>
    /// Process panel����Ӧ�Ľű�����ӡ�Play������ť��ʵ��ͼ��graphһ�������ļ��㴦������
    /// </summary>
	public class ProcessorView : PinnedElementView
    {
        BaseGraphProcessor processor;

        public ProcessorView()
        {
            title = "Process panel";//������
        }

        /// <summary>
        /// [����]
        /// </summary>
        /// <param name="graphView"></param>
		protected override void Initialize(BaseGraphView graphView)
        {
            processor = new ProcessGraphProcessor(graphView.graph);

            graphView.computeOrderUpdated += processor.UpdateComputeOrder;//���¼���˳��

            //��Ӱ�ť
            Button b = new Button(OnPlay) { name = "ActionButton", text = "Play !" };
            content.Add(b);
        }

        void OnPlay()
        {
            processor.Run();
        }
    }
}
