using System;
using System.Linq;
using System.Collections.Generic;

namespace RDTS.NodeEditor
{
    /// <summary>
    /// 对于图表graph的功能方法定义
    /// </summary>
    public static class GraphUtils
    {
        /// <summary>显示TarversalNode的不同状态</summary>
        enum State
        {
            White,//默认
            Grey,//正在操作
            Black,//操作完毕
        }

        class TarversalNode
        {
            public BaseNode node;//TarversalNode遍历节点对应的BaseNode节点
            public List<TarversalNode> inputs = new List<TarversalNode>();//对应的输入端口连接的节点
            public List<TarversalNode> outputs = new List<TarversalNode>();//对应的输出端口连接的节点
            public State state = State.White;

            public TarversalNode(BaseNode node) { this.node = node; }
        }

        // A structure made for easy graph traversal 一种便于图遍历的结构
        class TraversalGraph
        {
            public List<TarversalNode> nodes = new List<TarversalNode>();//图表中所有的节点
            public List<TarversalNode> outputs = new List<TarversalNode>();//存放一条“节点链”中的最后一个节点
        }

        /// <summary>
        /// 将BaseGraph类型的图表转换(克隆)到TraversalGraph类型的图表，主要包含图表中的所有节点、end nodes
        /// </summary>
        /// <param name="graph"></param>
        /// <returns></returns>
        static TraversalGraph ConvertGraphToTraversalGraph(BaseGraph graph)
        {
            TraversalGraph g = new TraversalGraph();//遍历图表
            Dictionary<BaseNode, TarversalNode> nodeMap = new Dictionary<BaseNode, TarversalNode>();

            foreach (var node in graph.nodes)//遍历指定图表中所有的节点
            {
                var tn = new TarversalNode(node);//对节点创建对应的遍历节点
                g.nodes.Add(tn);//在遍历图表中添加节点
                nodeMap[node] = tn;//在节点映射的字典中添加

                if (graph.graphOutputs.Contains(node))//如果是end nodes，则也将此节点加入到TraversalGraph中的outputs列表中
                    g.outputs.Add(tn);
            }

            foreach (var tn in g.nodes)
            {
                tn.inputs = tn.node.GetInputNodes().Where(n => nodeMap.ContainsKey(n)).Select(n => nodeMap[n]).ToList();//将连接到BaseNode输入端口的节点“复制”到对应的TarversalNode的inputs列表中
                tn.outputs = tn.node.GetOutputNodes().Where(n => nodeMap.ContainsKey(n)).Select(n => nodeMap[n]).ToList();////将连接到BaseNode输出端口的节点“复制”到对应的TarversalNode的outputs列表中
            }

            return g;
        }

        /// <summary>
        /// 按照深度优先排序的节点列表
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        public static List<BaseNode> DepthFirstSort(BaseGraph g)
        {
            var graph = ConvertGraphToTraversalGraph(g);//将指定的BaseGraph转换到TraversalGraph
            List<BaseNode> depthFirstNodes = new List<BaseNode>();

            foreach (var n in graph.nodes)//遍历图表中的所有节点
                DFS(n);

            void DFS(TarversalNode n)
            {
                if (n.state == State.Black)
                    return;

                n.state = State.Grey;

                //若对应的节点是ParameterNode类型，且 为ParameterAccessor.Get
                if (n.node is ParameterNode parameterNode && parameterNode.accessor == ParameterAccessor.Get)
                {
                    foreach (var setter in graph.nodes.Where(x =>
                        x.node is ParameterNode p &&
                        p.parameterGUID == parameterNode.parameterGUID &&
                        p.accessor == ParameterAccessor.Set))
                    {
                        if (setter.state == State.White)
                            DFS(setter);
                    }
                }
                else
                {
                    foreach (var input in n.inputs)
                    {
                        if (input.state == State.White)
                            DFS(input);
                    }
                }

                n.state = State.Black;

                // Only add the node when his children are completely visited
                // 只有此节点的子项被完全访问时才添加节点
                depthFirstNodes.Add(n.node);//从前往后添加节点
            }

            return depthFirstNodes;
        }

        /// <summary>
        /// 在图中查找循环：如果有节点出现首尾相连的“循环”，则将这些节点查找出来，并调用指定的委托方法
        /// </summary>
        /// <param name="g"></param>
        /// <param name="cyclicNode"></param>
        public static void FindCyclesInGraph(BaseGraph g, Action<BaseNode> cyclicNode)
        {
            var graph = ConvertGraphToTraversalGraph(g);//将指定的BaseGraph转换到TraversalGraph
            List<TarversalNode> cyclicNodes = new List<TarversalNode>();

            foreach (var n in graph.nodes)
                DFS(n);

            void DFS(TarversalNode n)
            {
                if (n.state == State.Black)
                    return;

                n.state = State.Grey;

                foreach (var input in n.inputs)
                {
                    if (input.state == State.White)
                        DFS(input);
                    else if (input.state == State.Grey)
                        cyclicNodes.Add(n);
                }
                n.state = State.Black;
            }

            cyclicNodes.ForEach((tn) => cyclicNode?.Invoke(tn.node));
        }
    }
}