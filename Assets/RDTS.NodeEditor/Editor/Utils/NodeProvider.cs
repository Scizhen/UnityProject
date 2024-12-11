using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.IO;
using System.Reflection;
using UnityEditor.Experimental.GraphView;

namespace RDTS.NodeEditor
{
    /// <summary>
    /// ��̬�ࣺ����graph���йض˿ڡ��ڵ㡢Mono�ű����(����)��Ϣ���ṩ�������ڲ˵�����Ϣ
    /// </summary>
	public static class NodeProvider
    {
        /// <summary>�˿�������Ϣ</summary>
		public struct PortDescription
        {
            public Type nodeType;//�ڵ�����
            public Type portType;//�˿�����
            public bool isInput;//����or����˿�
            public string portFieldName;//�˿��ֶ���
            public string portIdentifier;//�˿ڱ�ʶ��GUID
            public string portDisplayName;//�˿���ʾ����
        }

        ///MonoScript���ű���Դ�ı�ʾ�������ʾ�洢����Ŀ�е� C#��JavaScript �� Boo �ļ�

        /// <summary>�ֵ䣨�ڵ���ͼ������������Ӧ�Ľű���</summary>
        static Dictionary<Type, MonoScript> nodeViewScripts = new Dictionary<Type, MonoScript>();
        /// <summary>�ֵ䣨�ڵ�������������Ӧ�Ľű���</summary>
		static Dictionary<Type, MonoScript> nodeScripts = new Dictionary<Type, MonoScript>();
        /// <summary>Node��NodeView��Ӧ���ֵ䣨NodeType��NodeViewType��</summary>
		static Dictionary<Type, Type> nodeViewPerType = new Dictionary<Type, Type>();

        /// <summary>�ڵ�������Ϣ</summary>
		public class NodeDescriptions
        {
            /// <summary>�ֵ䣺���ڵ�Ĳ˵�·�����ڵ�������</summary>
			public Dictionary<string, Type> nodePerMenuTitle = new Dictionary<string, Type>();
            /// <summary>��¼�ڵ�����/����˿�(����)���б�</summary>
			public List<Type> slotTypes = new List<Type>();
            /// <summary>��¼�˿�������Ϣ���б�</summary>
			public List<PortDescription> nodeCreatePortDescription = new List<PortDescription>();
        }

        /// <summary>�ṹ�壺�ض���ͼ��Ľڵ����Ϣ</summary>
		public struct NodeSpecificToGraph
        {
            public Type nodeType;//�ڵ�����(����)
            public List<MethodInfo> isCompatibleWithGraph;//�ڵ��о���IsCompatibleWithGraph���ԡ���������Ϊbool��������������Ϊ1����������������ΪBaseGraph�ķ���
            public Type compatibleWithGraphType;//���ݵ�graph���ͣ���������
        }

        /// <summary>����ڵ��������һ��ͼ���Ӧ��һϵ�о���Ľڵ�������Ϣ</summary>
        static Dictionary<BaseGraph, NodeDescriptions> specificNodeDescriptions = new Dictionary<BaseGraph, NodeDescriptions>();
        /// <summary>�ض���ͼ��Ľڵ���б�(һ�����Ϊ0)</summary>
		static List<NodeSpecificToGraph> specificNodes = new List<NodeSpecificToGraph>();

        /// <summary>ͨ�ýڵ��������Ϣ�����ڵ�Ĳ˵�·�����ڵ��������ֵ䣬��¼�ڵ�����/����˿�(����)���б���¼�˿�������Ϣ���б�</summary>
		static NodeDescriptions genericNodes = new NodeDescriptions();

        /// <summary>
        /// ���캯���������ڵ�ͽڵ���ͼ�ű��Ļ��棬�ڵ���Ϣ�Ļ���
        /// </summary>
		static NodeProvider()
        {
            BuildScriptCache();//����BaseNode��BaseNodeView���༰��������Ľű��Ļ���
            BuildGenericNodeCache();//����ͨ�ýڵ㻺�棺����������BaseNode���������͵ļ��ϣ������нڵ���Ϣ�洢��genericNodes������
        }

        public static void LoadGraph(BaseGraph graph)
        {
            // Clear old graph data in case there was some ����ɵ�ͼ�������Է���һ
            specificNodeDescriptions.Remove(graph);
            var descriptions = new NodeDescriptions();
            specificNodeDescriptions.Add(graph, descriptions);

            var graphType = graph.GetType();//��ȡ��ͼ��graph����
            ///Debug.Log("specificNodes Count:" + specificNodes.Count);
			foreach (var nodeInfo in specificNodes)
            {
                bool compatible = nodeInfo.compatibleWithGraphType == null || nodeInfo.compatibleWithGraphType == graphType;

                if (nodeInfo.isCompatibleWithGraph != null)
                {
                    foreach (var method in nodeInfo.isCompatibleWithGraph)
                        compatible &= (bool)method?.Invoke(null, new object[] { graph });
                }

                if (compatible)
                    BuildCacheForNode(nodeInfo.nodeType, descriptions, graph);//�����ݵĽڵ�������Ϣ���浽descriptions��
            }
        }

        public static void UnloadGraph(BaseGraph graph)
        {
            specificNodeDescriptions.Remove(graph);
        }

        /// <summary>
        /// ����ͨ�ýڵ㻺�棺����������BaseNode���������͵ļ��ϣ������нڵ���Ϣ�洢��genericNodes������
        /// </summary>
        static void BuildGenericNodeCache()
        {
            foreach (var nodeType in TypeCache.GetTypesDerivedFrom<BaseNode>())//GetTypesDerivedFrom����ȡ��T(BaseNode)�������������͵ļ���
            {
                if (!IsNodeAccessibleFromMenu(nodeType))//�������нڵ�˵������ԣ�������
                    continue;

                if (IsNodeSpecificToGraph(nodeType))//�ڵ��Ƿ����κ�ȡ����graph���ͻ����õ�����
                    continue;

                BuildCacheForNode(nodeType, genericNodes);//Ϊ�ڵ㹹�����棺��һ���ڵ������¼����Ϣ���浽genericNodes
            }
        }

        /// <summary>
        /// Ϊ�ڵ㹹�����棺
        ///     ���ҽڵ�˵������ԣ���¼�� [ָ���ڵ�������Ϣ����] ���ֵ��У�
        ///     ���ҽڵ������/����˿ڣ���¼�� [ָ���ڵ�������Ϣ����] ��Ӧ�б��У�
        ///     ��¼�� [ָ���ڵ�������Ϣ����] �ڵ�˿ڵ�������Ϣ
        /// </summary>
        /// <param name="nodeType">�ڵ�����</param>
        /// <param name="targetDescription">Ҫ���Ľڵ�������Ϣ�����</param>
        /// <param name="graph"></param>
		static void BuildCacheForNode(Type nodeType, NodeDescriptions targetDescription, BaseGraph graph = null)
        {
            ///���ҽڵ�˵�������
			var attrs = nodeType.GetCustomAttributes(typeof(NodeMenuItemAttribute), false) as NodeMenuItemAttribute[];//��ȡ�ڵ����µ�NodeMenuItemAttribute����
            if (attrs != null && attrs.Length > 0)//���ڵ���и�����
            {
                foreach (var attr in attrs)
                    targetDescription.nodePerMenuTitle[attr.menuTitle] = nodeType;//*�����ֵ���
            }

            ///��������/����˿�
			foreach (var field in nodeType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))//�����ڵ�����ָ�����͵��ֶ�
            {
                //���ֶβ�����HideInInspector���ԣ����ֶξ���InputAttribute��OutputAttribute���ԣ���Ϊ����/����˿ڣ�
                if (field.GetCustomAttribute<HideInInspector>() == null && field.GetCustomAttributes().Any(c => c is InputAttribute || c is OutputAttribute))
                    targetDescription.slotTypes.Add(field.FieldType);//*�����ֶ����ͼ���slotTypes�б�
            }

            ///�ṩnodeType�Ķ˿�������Ϣ����targetDescription�е�nodeCreatePortDescription��Ӷ˿���Ϣ��
			ProvideNodePortCreationDescription(nodeType, targetDescription, graph);
        }

        /// <summary>
        /// �Ƿ���ԴӲ˵����ʽڵ㣨�Ƿ���нڵ�˵������ԣ�
        /// </summary>
        /// <param name="nodeType">�ڵ�����</param>
		static bool IsNodeAccessibleFromMenu(Type nodeType)
        {
            //��type�ǳ���ľͷ���false
            if (nodeType.IsAbstract)//���Type�ǳ����(����ʵ����,��ֻ������������Ļ���)����Ϊ true������Ϊ false
                return false;
            //���ǳ���ģ��ͼ���NodeMenuItemAttribute�Զ������Եĸ����������ھͷ���true
            return nodeType.GetCustomAttributes<NodeMenuItemAttribute>().Count() > 0;
        }

        // Check if node has anything that depends on the graph type or settings
        // ���ڵ��Ƿ����κ�ȡ����graph���ͻ����õ�����
        static bool IsNodeSpecificToGraph(Type nodeType)
        {
            //���ҽڵ��о���IsCompatibleWithGraph���Եķ���
            var isCompatibleWithGraphMethods = nodeType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).Where(m => m.GetCustomAttribute<IsCompatibleWithGraph>() != null);
            //�����ڵ���NodeMenuItemAttribute����
            var nodeMenuAttributes = nodeType.GetCustomAttributes<NodeMenuItemAttribute>();

            //��ȡNodeMenuItemAttribute�����еĲ�Ϊnull��onlyCompatibleWithGraph���� �������ݵ�graph���ͣ���
            List<Type> compatibleGraphTypes = nodeMenuAttributes.Where(n => n.onlyCompatibleWithGraph != null).Select(a => a.onlyCompatibleWithGraph).ToList();
            //�洢�ڵ��о���IsCompatibleWithGraph���ԡ���������Ϊbool��������������Ϊ1����������������ΪBaseGraph�ķ���
            List<MethodInfo> compatibleMethods = new List<MethodInfo>();

            //��������IsCompatibleWithGraph���Եķ���
            foreach (var method in isCompatibleWithGraphMethods)
            {
                // Check if the method is static and have the correct prototype 
                // ��鷽���Ƿ��Ǿ�̬�Ĳ��Ҿ�����ȷ��ԭ��
                ///GetParameters()����ȡָ���ķ������캯���Ĳ���
                ///ReturnType����ȡ�˷����ķ�������
                ///ParameterType����ȡ�ò�����Type
                var p = method.GetParameters();
                if (method.ReturnType != typeof(bool) || p.Count() != 1 || p[0].ParameterType != typeof(BaseGraph))//�������������Ͳ���bool �򷽷��Ĳ���������Ϊ1 �򷽷��Ĳ������Ͳ���BaseGraph
                    Debug.LogError($"The function '{method.Name}' marked with the IsCompatibleWithGraph attribute either doesn't return a boolean or doesn't take one parameter of BaseGraph type.");
                else//������������Ϊbool �ҷ�����������Ϊ1�� �ҷ�����������ΪBaseGraph
                    compatibleMethods.Add(method);
            }

            //�ڵ��о��о���IsCompatibleWithGraph���ԡ���������Ϊbool��������������Ϊ1����������������ΪBaseGraph�ķ��� �� ������onlyCompatibleWithGraph��Ϊ��
            if (compatibleMethods.Count > 0 || compatibleGraphTypes.Count > 0)
            {
                // We still need to add the element in specificNode even without specific graph
                // ��ʹû���ض���graph����Ȼ��Ҫ�� specificNode �����Ԫ��

                if (compatibleGraphTypes.Count == 0)
                    compatibleGraphTypes.Add(null);

                foreach (var graphType in compatibleGraphTypes)//�м�����Ϊnull��onlyCompatibleWithGraph��������Ҫ��specificNodes��Ӷ�Ӧ������Ԫ��
                {
                    specificNodes.Add(new NodeSpecificToGraph
                    {
                        nodeType = nodeType,
                        isCompatibleWithGraph = compatibleMethods,
                        compatibleWithGraphType = graphType
                    });
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// ����BaseNode��BaseNodeView���༰��������Ľű��Ļ���
        /// </summary>
        static void BuildScriptCache()
        {
            //������BaseNode���������͵ļ���
            foreach (var nodeType in TypeCache.GetTypesDerivedFrom<BaseNode>())//TypeCache���ṩ�Ӽ��ص� Unity ��ĳ����н��п���������ȡ�ķ�����ʹ�� TypeCache �ɷ������Ժ�������������Ϣ���˻��������ԭ��������������������༭������
            {
                if (!IsNodeAccessibleFromMenu(nodeType))//�������нڵ�˵������ԣ�������
                    continue;

                //���в˵������ԣ��Ͳ���ָ������(�ڵ������)����Ŀ�е�Mono�ű������ҵ����¼���ֵ�nodeScripts��
                AddNodeScriptAsset(nodeType);
            }
            //������BaseNodeView���������͵ļ���
            foreach (var nodeViewType in TypeCache.GetTypesDerivedFrom<BaseNodeView>())
            {
                //���ǳ����࣬�Ͳ���ָ������(�ڵ���ͼ������)����Ŀ�е�Mono�ű������ҵ����¼���ֵ�nodeScripts��
                if (!nodeViewType.IsAbstract)
                    AddNodeViewScriptAsset(nodeViewType);
            }
        }

        static FieldInfo SetGraph = typeof(BaseNode).GetField("graph", BindingFlags.NonPublic | BindingFlags.Instance);//GetField��ʹ��ָ����Լ������ָ���ֶΡ� ���ҵ�����ָ��Ҫ����ֶεĶ���ͷ��أ����򷵻�null��
        /// <summary>
        /// �ṩָ���ڵ�Ķ˿�������Ϣ
        /// </summary>
        /// <param name="nodeType">�ڵ�����</param>
        /// <param name="targetDescription">�ڵ�������Ϣ</param>
        /// <param name="graph">Ҫ������ͼ��</param>
        static void ProvideNodePortCreationDescription(Type nodeType, NodeDescriptions targetDescription, BaseGraph graph = null)
        {
            var node = Activator.CreateInstance(nodeType) as BaseNode;
            try
            {
                SetGraph.SetValue(node, graph);//��ֵ(graph)���õ���������(node)���ֶ�
                node.InitializePorts();//��ʼ���˿�
                node.UpdateAllPorts();//���¶˿�
            }
            catch (Exception) { }

            foreach (var p in node.inputPorts)//�������˿�������Ϣ
                AddPort(p, true);
            foreach (var p in node.outputPorts)//�������˿�������Ϣ
                AddPort(p, false);

            //�ڽڵ�������ϢNodeDescriptions�����һ���˿�������Ϣ
            void AddPort(NodePort p, bool input)
            {
                //��Ӷ˿�������Ϣ
                targetDescription.nodeCreatePortDescription.Add(new PortDescription
                {
                    nodeType = nodeType,//�ڵ�����(����)
                    portType = p.portData.displayType ?? p.fieldInfo.FieldType,
                    isInput = input,//�Ƿ�Ϊ����˿�
                    portFieldName = p.fieldName,//�˿��ֶ�����
                    portDisplayName = p.portData.displayName ?? p.fieldName,//�˿���ʾ����
                    portIdentifier = p.portData.identifier,//�˿�ʶ���GUID
                });
            }
        }

        /// <summary>
        /// ����ָ������(�ڵ������)����Ŀ�е�Mono�ű������ҵ����¼���ֵ�nodeScripts��
        /// </summary>
        /// <param name="type"></param>
		static void AddNodeScriptAsset(Type type)
        {
            //���Ҵ������Ľű�
            var nodeScriptAsset = FindScriptFromClassName(type.Name);

            // Try find the class name with Node name at the end  
            // ����Ѱ�� ��Ϊ������+Node���Ľű�
            if (nodeScriptAsset == null)
                nodeScriptAsset = FindScriptFromClassName(type.Name + "Node");
            // ��������2�β������ҵ��˶�Ӧ�Ľű�����������ֵ���
            if (nodeScriptAsset != null)
                nodeScripts[type] = nodeScriptAsset;
        }
        /// <summary>
        ///  ����ָ������(�ڵ���ͼ������)����Ŀ�е�Mono�ű������ҵ����¼���ֵ�nodeScripts��
        /// </summary>
        /// <param name="type"></param>
		static void AddNodeViewScriptAsset(Type type)
        {
            var attrs = type.GetCustomAttributes(typeof(NodeCustomEditor), false) as NodeCustomEditor[];//����type���е�NodeCustomEditor����

            if (attrs != null && attrs.Length > 0)//���������NodeCustomEditor����
            {
                Type nodeType = attrs.First().nodeType;//��ȡ���ڵ�����(����View�ģ���ColorNode��IfNode��)
                nodeViewPerType[nodeType] = type;

                //���β�������������+View������+NodeView����Ŀ�ļ����ж�Ӧ�Ľű�
                var nodeViewScriptAsset = FindScriptFromClassName(type.Name);
                if (nodeViewScriptAsset == null)
                    nodeViewScriptAsset = FindScriptFromClassName(type.Name + "View");
                if (nodeViewScriptAsset == null)
                    nodeViewScriptAsset = FindScriptFromClassName(type.Name + "NodeView");

                // ��������3�β������ҵ��˶�Ӧ�Ľű�����������ֵ���
                if (nodeViewScriptAsset != null)
                    nodeViewScripts[type] = nodeViewScriptAsset;
            }
        }

        /// <summary>
        /// ������������Ŀ�ļ����в��Ҷ�Ӧ�Ľű�
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
		static MonoScript FindScriptFromClassName(string className)
        {
            ///�ؼ��� 't:' ����ָ�����ڲ��ҵ�����������
			var scriptGUIDs = AssetDatabase.FindAssets($"t:script {className}");//�����ҵ�ƥ���asset�ͷ�����GUID�����飬���򷵻ؿ�����

            if (scriptGUIDs.Length == 0)
                return null;

            foreach (var scriptGUID in scriptGUIDs)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(scriptGUID);//�����ʲ������Project�ļ��е�·����Assets/,,,��
                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);//���ظ���·��assetPath��MonoScript���͵ĵ�һ����Դ����

                ///Equals������ָ�����������ȷ������String�����Ƿ������ͬ��ֵ
                ///Path.GetFileNameWithoutExtension������[��������չ��]��ָ��·���ַ�����[�ļ���]
                ///StringComparison.OrdinalIgnoreCase��ͨ��ʹ����ţ������ƣ����������Ե�������򲢺������Ƚϵ��ַ����Ĵ�Сд�����Ƚ��ַ���
                if (script != null && String.Equals(className, Path.GetFileNameWithoutExtension(assetPath), StringComparison.OrdinalIgnoreCase))//��������·���ַ������ļ�����ͬ�����ش�·���µ�MonoScript
                    return script;
            }

            return null;
        }

        /// <summary>
        /// �ҵ�ָ��nodeType��Ӧ��nodeViewType����û�������丸���nodeViewType
        /// </summary>
        /// <param name="nodeType"></param>
        /// <returns></returns>
		public static Type GetNodeViewTypeFromType(Type nodeType)
        {
            Type view;//NodeViewType�����ֵ��ж��ڵ�viewtype��

            ///���ֵ��л�ȡ��nodeTypeΪ����Ӧ��ֵNodeViewType��[����о�ֱ�ӷ���]
            if (nodeViewPerType.TryGetValue(nodeType, out view))
                return view;

            ///������δ�ҵ���Ӧ��NodeViewType������ֵ������Ƿ���nodeType�ĸ��࣬�����򽫸����Type��ȡ
            Type baseType = null;//NodeViewType����

            // Allow for inheritance in node views: multiple C# node using the same view
            // ������node views�м̳У�����ڵ�ʹ����ͬ��view
            foreach (var type in nodeViewPerType)
            {
                // Find a view (not first fitted view) of nodeType ���� nodeType ����ͼ�����ǵ�һ�������ͼ��
                // ���nodeType���ֵ���ĳ���������࣬�ң�baseTypeΪ�� �� �ü���ֵΪbaseType�����ࣩ���򽫸ü���NodeViewType����baseType
                if (nodeType.IsSubclassOf(type.Key) && (baseType == null || type.Value.IsSubclassOf(baseType)))
                    baseType = type.Value;
            }

            if (baseType != null)
                return baseType;

            return view;
        }

        /// <summary>��ȡ�ڵ�˵���Ŀ</summary>
        public static IEnumerable<(string path, Type type)> GetNodeMenuEntries(BaseGraph graph = null)
        {
            foreach (var node in genericNodes.nodePerMenuTitle)
                yield return (node.Key, node.Value);//һ��һ�����أ��ڵ�Ĳ˵�·�����ڵ�������

            if (graph != null && specificNodeDescriptions.TryGetValue(graph, out var specificNodes))
            {
                foreach (var node in specificNodes.nodePerMenuTitle)
                    yield return (node.Key, node.Value);
            }
        }

        /// <summary>��ȡ�ڵ���ͼ������Ӧ��Mono�ű�</summary>
		public static MonoScript GetNodeViewScript(Type type)
        {
            nodeViewScripts.TryGetValue(type, out var script);

            return script;
        }
        /// <summary>��ȡ�ڵ�������Ӧ��Mono�ű�</summary>
		public static MonoScript GetNodeScript(Type type)
        {
            nodeScripts.TryGetValue(type, out var script);

            return script;
        }

        public static IEnumerable<Type> GetSlotTypes(BaseGraph graph = null)
        {
            foreach (var type in genericNodes.slotTypes)
                yield return type;

            if (graph != null && specificNodeDescriptions.TryGetValue(graph, out var specificNodes))
            {
                foreach (var type in specificNodes.slotTypes)
                    yield return type;
            }
        }

        /// <summary>
        /// ��ȡ�ߴ����ڵ�˵���Ӳ�ͬ���͵Ķ˿�����edge����������������ʱ������ݶ˿�������ʾ�����ݵĲ˵���
        /// </summary>
        /// <param name="portView">����edge�Ķ˿�</param>
        /// <param name="graph"></param>
        /// <returns></returns>
		public static IEnumerable<PortDescription> GetEdgeCreationNodeMenuEntry(PortView portView, BaseGraph graph = null)
        {
            foreach (var description in genericNodes.nodeCreatePortDescription)
            {
                //�ж��Ƿ�Ϊ���ݵĶ˿�
                if (!IsPortCompatible(description))
                    continue;

                yield return description;
            }

            if (graph != null && specificNodeDescriptions.TryGetValue(graph, out var specificNodes))
            {
                foreach (var description in specificNodes.nodeCreatePortDescription)
                {
                    if (!IsPortCompatible(description))
                        continue;
                    yield return description;
                }
            }

            ///�Ƿ�Ϊ��portView���ݵĶ˿�
			bool IsPortCompatible(PortDescription description)
            {
                //����edge�Ķ˿ڣ�����Ҫ���ӵĶ˿ڵ�Direction��ͬ��������
                if ((portView.direction == Direction.Input && description.isInput) || (portView.direction == Direction.Output && !description.isInput))
                    return false;

                //�����˿�������ƥ���������
                if (!BaseGraph.TypesAreConnectable(description.portType, portView.portType))
                    return false;

                return true;
            }
        }
    }
}
