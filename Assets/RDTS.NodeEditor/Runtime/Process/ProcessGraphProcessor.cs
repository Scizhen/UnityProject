using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Jobs;
using Unity.Collections;
// using Unity.Entities;

namespace RDTS.NodeEditor
{

    /// <summary>
    /// Graph processor 基本的图表处理器的实现：更新节点计算顺序(升序排列)，对节点进行处理
    /// </summary>
    public class ProcessGraphProcessor : BaseGraphProcessor
    {
        List<BaseNode> processList;

        /// <summary>
        /// Manage graph scheduling and processing  管理图表调度和处理
        /// </summary>
        /// <param name="graph">Graph to be processed</param>
        public ProcessGraphProcessor(BaseGraph graph) : base(graph) { }

        /// <summary>
        /// 更新节点的计算顺序，按升序排列，便于对节点计算的处理
        /// </summary>
		public override void UpdateComputeOrder()
        {
            //根据键按升序对序列的元素进行排序
            processList = graph.nodes.OrderBy(n => n.computeOrder).ToList();
        }

        /// <summary>
        /// Process all the nodes following the compute order. 
        /// 按照计算顺序处理所有节点
        /// </summary>
        public override void Run()
        {
            int count = processList.Count;

            for (int i = 0; i < count; i++)
                processList[i].OnProcess();
        }
    }
}

