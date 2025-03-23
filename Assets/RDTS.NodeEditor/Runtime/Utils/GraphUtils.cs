using System;
using System.Linq;
using System.Collections.Generic;

namespace RDTS.NodeEditor
{
    /// <summary>
    /// ����ͼ��graph�Ĺ��ܷ�������
    /// </summary>
    public static class GraphUtils
    {
        /// <summary>��ʾTarversalNode�Ĳ�ͬ״̬</summary>
        enum State
        {
            White,//Ĭ��
            Grey,//���ڲ���
            Black,//�������
        }

        class TarversalNode
        {
            public BaseNode node;//TarversalNode�����ڵ��Ӧ��BaseNode�ڵ�
            public List<TarversalNode> inputs = new List<TarversalNode>();//��Ӧ������˿����ӵĽڵ�
            public List<TarversalNode> outputs = new List<TarversalNode>();//��Ӧ������˿����ӵĽڵ�
            public State state = State.White;

            public TarversalNode(BaseNode node) { this.node = node; }
        }

        // A structure made for easy graph traversal һ�ֱ���ͼ�����Ľṹ
        class TraversalGraph
        {
            public List<TarversalNode> nodes = new List<TarversalNode>();//ͼ�������еĽڵ�
            public List<TarversalNode> outputs = new List<TarversalNode>();//���һ�����ڵ������е����һ���ڵ�
        }

        /// <summary>
        /// ��BaseGraph���͵�ͼ��ת��(��¡)��TraversalGraph���͵�ͼ����Ҫ����ͼ���е����нڵ㡢end nodes
        /// </summary>
        /// <param name="graph"></param>
        /// <returns></returns>
        static TraversalGraph ConvertGraphToTraversalGraph(BaseGraph graph)
        {
            TraversalGraph g = new TraversalGraph();//����ͼ��
            Dictionary<BaseNode, TarversalNode> nodeMap = new Dictionary<BaseNode, TarversalNode>();

            foreach (var node in graph.nodes)//����ָ��ͼ�������еĽڵ�
            {
                var tn = new TarversalNode(node);//�Խڵ㴴����Ӧ�ı����ڵ�
                g.nodes.Add(tn);//�ڱ���ͼ������ӽڵ�
                nodeMap[node] = tn;//�ڽڵ�ӳ����ֵ������

                if (graph.graphOutputs.Contains(node))//�����end nodes����Ҳ���˽ڵ���뵽TraversalGraph�е�outputs�б���
                    g.outputs.Add(tn);
            }

            foreach (var tn in g.nodes)
            {
                tn.inputs = tn.node.GetInputNodes().Where(n => nodeMap.ContainsKey(n)).Select(n => nodeMap[n]).ToList();//�����ӵ�BaseNode����˿ڵĽڵ㡰���ơ�����Ӧ��TarversalNode��inputs�б���
                tn.outputs = tn.node.GetOutputNodes().Where(n => nodeMap.ContainsKey(n)).Select(n => nodeMap[n]).ToList();////�����ӵ�BaseNode����˿ڵĽڵ㡰���ơ�����Ӧ��TarversalNode��outputs�б���
            }

            return g;
        }

        /// <summary>
        /// ���������������Ľڵ��б�
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        public static List<BaseNode> DepthFirstSort(BaseGraph g)
        {
            var graph = ConvertGraphToTraversalGraph(g);//��ָ����BaseGraphת����TraversalGraph
            List<BaseNode> depthFirstNodes = new List<BaseNode>();

            foreach (var n in graph.nodes)//����ͼ���е����нڵ�
                DFS(n);

            void DFS(TarversalNode n)
            {
                if (n.state == State.Black)
                    return;

                n.state = State.Grey;

                //����Ӧ�Ľڵ���ParameterNode���ͣ��� ΪParameterAccessor.Get
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
                // ֻ�д˽ڵ�������ȫ����ʱ����ӽڵ�
                depthFirstNodes.Add(n.node);//��ǰ������ӽڵ�
            }

            return depthFirstNodes;
        }

        /// <summary>
        /// ��ͼ�в���ѭ��������нڵ������β�����ġ�ѭ����������Щ�ڵ���ҳ�����������ָ����ί�з���
        /// </summary>
        /// <param name="g"></param>
        /// <param name="cyclicNode"></param>
        public static void FindCyclesInGraph(BaseGraph g, Action<BaseNode> cyclicNode)
        {
            var graph = ConvertGraphToTraversalGraph(g);//��ָ����BaseGraphת����TraversalGraph
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