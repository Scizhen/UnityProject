using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RDTS.NodeEditor
{
    /// <summary>
    /// Process panel面板对应的脚本，添加“Play！”按钮，实现图表graph一次完整的计算处理运行
    /// </summary>
	public class ProcessorView : PinnedElementView
    {
        BaseGraphProcessor processor;

        public ProcessorView()
        {
            title = "Process panel";//标题名
        }

        /// <summary>
        /// [重载]
        /// </summary>
        /// <param name="graphView"></param>
		protected override void Initialize(BaseGraphView graphView)
        {
            processor = new ProcessGraphProcessor(graphView.graph);

            graphView.computeOrderUpdated += processor.UpdateComputeOrder;//更新计算顺序

            //添加按钮
            Button b = new Button(OnPlay) { name = "ActionButton", text = "Play !" };
            content.Add(b);
        }

        void OnPlay()
        {
            processor.Run();
        }
    }
}
