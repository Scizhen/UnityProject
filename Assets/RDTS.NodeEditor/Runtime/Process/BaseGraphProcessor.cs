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
    /// Graph processor 图表处理的基类
    /// </summary>
    public abstract class BaseGraphProcessor
    {
        protected BaseGraph graph;

        /// <summary>
        /// Manage graph scheduling and processing 管理图表的调度和处理
        /// </summary>
        /// <param name="graph">Graph to be processed</param>
        public BaseGraphProcessor(BaseGraph graph)
        {
            this.graph = graph;

            UpdateComputeOrder();
        }

        public abstract void UpdateComputeOrder();

        /// <summary>
        /// Schedule the graph into the job system  将图表安排到作业系统中
        /// </summary>
        public abstract void Run();
    }
}
