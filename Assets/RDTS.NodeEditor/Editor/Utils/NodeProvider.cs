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
    /// 静态类：储存graph中有关端口、节点、Mono脚本相关(描述)信息，提供搜索窗口菜单项信息
    /// </summary>
	public static class NodeProvider
    {
        /// <summary>端口描述信息</summary>
		public struct PortDescription
        {
            public Type nodeType;//节点类型
            public Type portType;//端口类型
            public bool isInput;//输入or输出端口
            public string portFieldName;//端口字段名
            public string portIdentifier;//端口标识符GUID
            public string portDisplayName;//端口显示名称
        }

        ///MonoScript：脚本资源的表示，该类表示存储在项目中的 C#、JavaScript 和 Boo 文件

        /// <summary>字典（节点视图类名，类名对应的脚本）</summary>
        static Dictionary<Type, MonoScript> nodeViewScripts = new Dictionary<Type, MonoScript>();
        /// <summary>字典（节点类名，类名对应的脚本）</summary>
		static Dictionary<Type, MonoScript> nodeScripts = new Dictionary<Type, MonoScript>();
        /// <summary>Node与NodeView对应的字典（NodeType，NodeViewType）</summary>
		static Dictionary<Type, Type> nodeViewPerType = new Dictionary<Type, Type>();

        /// <summary>节点描述信息</summary>
		public class NodeDescriptions
        {
            /// <summary>字典：（节点的菜单路径，节点类名）</summary>
			public Dictionary<string, Type> nodePerMenuTitle = new Dictionary<string, Type>();
            /// <summary>记录节点输入/输出端口(类名)的列表</summary>
			public List<Type> slotTypes = new List<Type>();
            /// <summary>记录端口描述信息的列表</summary>
			public List<PortDescription> nodeCreatePortDescription = new List<PortDescription>();
        }

        /// <summary>结构体：特定于图表的节点的信息</summary>
		public struct NodeSpecificToGraph
        {
            public Type nodeType;//节点类型(类名)
            public List<MethodInfo> isCompatibleWithGraph;//节点中具有IsCompatibleWithGraph特性、返回类型为bool、方法参数个数为1个、方法参数类型为BaseGraph的方法
            public Type compatibleWithGraphType;//兼容的graph类型（类名）？
        }

        /// <summary>具体节点的描述：一张图表对应其一系列具体的节点描述信息</summary>
        static Dictionary<BaseGraph, NodeDescriptions> specificNodeDescriptions = new Dictionary<BaseGraph, NodeDescriptions>();
        /// <summary>特定于图表的节点的列表(一般情况为0)</summary>
		static List<NodeSpecificToGraph> specificNodes = new List<NodeSpecificToGraph>();

        /// <summary>通用节点的描述信息：（节点的菜单路径，节点类名）字典，记录节点输入/输出端口(类名)的列表，记录端口描述信息的列表</summary>
		static NodeDescriptions genericNodes = new NodeDescriptions();

        /// <summary>
        /// 构造函数：构建节点和节点视图脚本的缓存，节点信息的缓存
        /// </summary>
		static NodeProvider()
        {
            BuildScriptCache();//构建BaseNode和BaseNodeView两类及其派生类的脚本的缓存
            BuildGenericNodeCache();//构建通用节点缓存：经过遍历从BaseNode派生的类型的集合，将所有节点信息存储到genericNodes对象中
        }

        public static void LoadGraph(BaseGraph graph)
        {
            // Clear old graph data in case there was some 清除旧的图表数据以防万一
            specificNodeDescriptions.Remove(graph);
            var descriptions = new NodeDescriptions();
            specificNodeDescriptions.Add(graph, descriptions);

            var graphType = graph.GetType();//获取该图表graph类型
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
                    BuildCacheForNode(nodeInfo.nodeType, descriptions, graph);//将兼容的节点描述信息缓存到descriptions中
            }
        }

        public static void UnloadGraph(BaseGraph graph)
        {
            specificNodeDescriptions.Remove(graph);
        }

        /// <summary>
        /// 构建通用节点缓存：经过遍历从BaseNode派生的类型的集合，将所有节点信息存储到genericNodes对象中
        /// </summary>
        static void BuildGenericNodeCache()
        {
            foreach (var nodeType in TypeCache.GetTypesDerivedFrom<BaseNode>())//GetTypesDerivedFrom：获取从T(BaseNode)类型派生的类型的集合
            {
                if (!IsNodeAccessibleFromMenu(nodeType))//若不具有节点菜单项特性，则跳过
                    continue;

                if (IsNodeSpecificToGraph(nodeType))//节点是否有任何取决于graph类型或设置的内容
                    continue;

                BuildCacheForNode(nodeType, genericNodes);//为节点构建缓存：将一个节点所需记录的信息储存到genericNodes
            }
        }

        /// <summary>
        /// 为节点构建缓存：
        ///     查找节点菜单项特性，记录到 [指定节点描述信息对象] 的字典中；
        ///     查找节点的输入/输出端口，记录到 [指定节点描述信息对象] 对应列表中；
        ///     记录到 [指定节点描述信息对象] 节点端口的描述信息
        /// </summary>
        /// <param name="nodeType">节点类型</param>
        /// <param name="targetDescription">要填充的节点描述信息类对象</param>
        /// <param name="graph"></param>
		static void BuildCacheForNode(Type nodeType, NodeDescriptions targetDescription, BaseGraph graph = null)
        {
            ///查找节点菜单项特性
			var attrs = nodeType.GetCustomAttributes(typeof(NodeMenuItemAttribute), false) as NodeMenuItemAttribute[];//获取节点类下的NodeMenuItemAttribute特性
            if (attrs != null && attrs.Length > 0)//若节点具有该特性
            {
                foreach (var attr in attrs)
                    targetDescription.nodePerMenuTitle[attr.menuTitle] = nodeType;//*加入字典中
            }

            ///查找输入/输出端口
			foreach (var field in nodeType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))//遍历节点类中指定类型的字段
            {
                //若字段不具有HideInInspector特性，且字段具有InputAttribute或OutputAttribute特性（标为输入/输出端口）
                if (field.GetCustomAttribute<HideInInspector>() == null && field.GetCustomAttributes().Any(c => c is InputAttribute || c is OutputAttribute))
                    targetDescription.slotTypes.Add(field.FieldType);//*将该字段类型加入slotTypes列表
            }

            ///提供nodeType的端口描述信息（在targetDescription中的nodeCreatePortDescription添加端口信息）
			ProvideNodePortCreationDescription(nodeType, targetDescription, graph);
        }

        /// <summary>
        /// 是否可以从菜单访问节点（是否具有节点菜单项特性）
        /// </summary>
        /// <param name="nodeType">节点类型</param>
		static bool IsNodeAccessibleFromMenu(Type nodeType)
        {
            //此type是抽象的就返回false
            if (nodeType.IsAbstract)//如果Type是抽象的(不能实例化,但只能用作派生类的基类)，则为 true；否则为 false
                return false;
            //不是抽象的，就检索NodeMenuItemAttribute自定义特性的个数，若存在就返回true
            return nodeType.GetCustomAttributes<NodeMenuItemAttribute>().Count() > 0;
        }

        // Check if node has anything that depends on the graph type or settings
        // 检查节点是否有任何取决于graph类型或设置的内容
        static bool IsNodeSpecificToGraph(Type nodeType)
        {
            //查找节点中具有IsCompatibleWithGraph特性的方法
            var isCompatibleWithGraphMethods = nodeType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).Where(m => m.GetCustomAttribute<IsCompatibleWithGraph>() != null);
            //检索节点中NodeMenuItemAttribute特性
            var nodeMenuAttributes = nodeType.GetCustomAttributes<NodeMenuItemAttribute>();

            //获取NodeMenuItemAttribute特性中的不为null的onlyCompatibleWithGraph参数 （即兼容的graph类型？）
            List<Type> compatibleGraphTypes = nodeMenuAttributes.Where(n => n.onlyCompatibleWithGraph != null).Select(a => a.onlyCompatibleWithGraph).ToList();
            //存储节点中具有IsCompatibleWithGraph特性、返回类型为bool、方法参数个数为1个、方法参数类型为BaseGraph的方法
            List<MethodInfo> compatibleMethods = new List<MethodInfo>();

            //遍历具有IsCompatibleWithGraph特性的方法
            foreach (var method in isCompatibleWithGraphMethods)
            {
                // Check if the method is static and have the correct prototype 
                // 检查方法是否是静态的并且具有正确的原型
                ///GetParameters()：获取指定的方法或构造函数的参数
                ///ReturnType：获取此方法的返回类型
                ///ParameterType：获取该参数的Type
                var p = method.GetParameters();
                if (method.ReturnType != typeof(bool) || p.Count() != 1 || p[0].ParameterType != typeof(BaseGraph))//若方法返回类型不是bool 或方法的参数个数不为1 或方法的参数类型不是BaseGraph
                    Debug.LogError($"The function '{method.Name}' marked with the IsCompatibleWithGraph attribute either doesn't return a boolean or doesn't take one parameter of BaseGraph type.");
                else//方法返回类型为bool 且方法参数个数为1个 且方法参数类型为BaseGraph
                    compatibleMethods.Add(method);
            }

            //节点中具有具有IsCompatibleWithGraph特性、返回类型为bool、方法参数个数为1个、方法参数类型为BaseGraph的方法 或 特性中onlyCompatibleWithGraph不为空
            if (compatibleMethods.Count > 0 || compatibleGraphTypes.Count > 0)
            {
                // We still need to add the element in specificNode even without specific graph
                // 即使没有特定的graph，仍然需要在 specificNode 中添加元素

                if (compatibleGraphTypes.Count == 0)
                    compatibleGraphTypes.Add(null);

                foreach (var graphType in compatibleGraphTypes)//有几个不为null的onlyCompatibleWithGraph参数，就要向specificNodes添加对应数量的元素
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
        /// 构建BaseNode和BaseNodeView两类及其派生类的脚本的缓存
        /// </summary>
        static void BuildScriptCache()
        {
            //遍历从BaseNode派生的类型的集合
            foreach (var nodeType in TypeCache.GetTypesDerivedFrom<BaseNode>())//TypeCache：提供从加载到 Unity 域的程序集中进行快速类型提取的方法。使用 TypeCache 可访问属性和派生的类型信息。此缓存允许从原生缓存数据中利用任意编辑器代码
            {
                if (!IsNodeAccessibleFromMenu(nodeType))//若不具有节点菜单项特性，就跳过
                    continue;

                //具有菜单项特性，就查找指定类型(节点的类名)在项目中的Mono脚本，若找到则记录到字典nodeScripts中
                AddNodeScriptAsset(nodeType);
            }
            //遍历从BaseNodeView派生的类型的集合
            foreach (var nodeViewType in TypeCache.GetTypesDerivedFrom<BaseNodeView>())
            {
                //不是抽象类，就查找指定类型(节点视图的类名)在项目中的Mono脚本，若找到则记录到字典nodeScripts中
                if (!nodeViewType.IsAbstract)
                    AddNodeViewScriptAsset(nodeViewType);
            }
        }

        static FieldInfo SetGraph = typeof(BaseNode).GetField("graph", BindingFlags.NonPublic | BindingFlags.Instance);//GetField：使用指定绑定约束搜索指定字段。 若找到符合指定要求的字段的对象就返回，否则返回null。
        /// <summary>
        /// 提供指定节点的端口描述信息
        /// </summary>
        /// <param name="nodeType">节点类型</param>
        /// <param name="targetDescription">节点描述信息</param>
        /// <param name="graph">要关联的图表</param>
        static void ProvideNodePortCreationDescription(Type nodeType, NodeDescriptions targetDescription, BaseGraph graph = null)
        {
            var node = Activator.CreateInstance(nodeType) as BaseNode;
            try
            {
                SetGraph.SetValue(node, graph);//将值(graph)设置到给定对象(node)的字段
                node.InitializePorts();//初始化端口
                node.UpdateAllPorts();//更新端口
            }
            catch (Exception) { }

            foreach (var p in node.inputPorts)//添加输入端口描述信息
                AddPort(p, true);
            foreach (var p in node.outputPorts)//添加输出端口描述信息
                AddPort(p, false);

            //在节点描述信息NodeDescriptions中添加一个端口描述信息
            void AddPort(NodePort p, bool input)
            {
                //添加端口描述信息
                targetDescription.nodeCreatePortDescription.Add(new PortDescription
                {
                    nodeType = nodeType,//节点类型(类名)
                    portType = p.portData.displayType ?? p.fieldInfo.FieldType,
                    isInput = input,//是否为输入端口
                    portFieldName = p.fieldName,//端口字段名称
                    portDisplayName = p.portData.displayName ?? p.fieldName,//端口显示名称
                    portIdentifier = p.portData.identifier,//端口识别符GUID
                });
            }
        }

        /// <summary>
        /// 查找指定类型(节点的类名)在项目中的Mono脚本，若找到则记录到字典nodeScripts中
        /// </summary>
        /// <param name="type"></param>
		static void AddNodeScriptAsset(Type type)
        {
            //查找此类名的脚本
            var nodeScriptAsset = FindScriptFromClassName(type.Name);

            // Try find the class name with Node name at the end  
            // 尝试寻找 名为“类名+Node”的脚本
            if (nodeScriptAsset == null)
                nodeScriptAsset = FindScriptFromClassName(type.Name + "Node");
            // 若在上述2次查找中找到了对应的脚本，就添加入字典中
            if (nodeScriptAsset != null)
                nodeScripts[type] = nodeScriptAsset;
        }
        /// <summary>
        ///  查找指定类型(节点视图的类名)在项目中的Mono脚本，若找到则记录到字典nodeScripts中
        /// </summary>
        /// <param name="type"></param>
		static void AddNodeViewScriptAsset(Type type)
        {
            var attrs = type.GetCustomAttributes(typeof(NodeCustomEditor), false) as NodeCustomEditor[];//查找type类中的NodeCustomEditor特性

            if (attrs != null && attrs.Length > 0)//若此类具有NodeCustomEditor特性
            {
                Type nodeType = attrs.First().nodeType;//获取到节点类型(不带View的，如ColorNode、IfNode等)
                nodeViewPerType[nodeType] = type;

                //依次查找类名、类名+View、类名+NodeView在项目文件夹中对应的脚本
                var nodeViewScriptAsset = FindScriptFromClassName(type.Name);
                if (nodeViewScriptAsset == null)
                    nodeViewScriptAsset = FindScriptFromClassName(type.Name + "View");
                if (nodeViewScriptAsset == null)
                    nodeViewScriptAsset = FindScriptFromClassName(type.Name + "NodeView");

                // 若在上述3次查找中找到了对应的脚本，就添加入字典中
                if (nodeViewScriptAsset != null)
                    nodeViewScripts[type] = nodeViewScriptAsset;
            }
        }

        /// <summary>
        /// 按照类名在项目文件夹中查找对应的脚本
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
		static MonoScript FindScriptFromClassName(string className)
        {
            ///关键字 't:' 用于指定正在查找的类名的类型
			var scriptGUIDs = AssetDatabase.FindAssets($"t:script {className}");//若查找到匹配的asset就返回其GUID的数组，否则返回空数组

            if (scriptGUIDs.Length == 0)
                return null;

            foreach (var scriptGUID in scriptGUIDs)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(scriptGUID);//返回资产相对于Project文件夹的路径“Assets/,,,”
                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);//返回给定路径assetPath下MonoScript类型的第一个资源对象

                ///Equals：按照指定的排序规则，确定两个String对象是否具有相同的值
                ///Path.GetFileNameWithoutExtension：返回[不具有扩展名]的指定路径字符串的[文件名]
                ///StringComparison.OrdinalIgnoreCase：通过使用序号（二进制）区分区域性的排序规则并忽略所比较的字符串的大小写，来比较字符串
                if (script != null && String.Equals(className, Path.GetFileNameWithoutExtension(assetPath), StringComparison.OrdinalIgnoreCase))//若类名与路径字符串的文件名相同，返回此路径下的MonoScript
                    return script;
            }

            return null;
        }

        /// <summary>
        /// 找到指定nodeType对应的nodeViewType，若没有则找其父类的nodeViewType
        /// </summary>
        /// <param name="nodeType"></param>
        /// <returns></returns>
		public static Type GetNodeViewTypeFromType(Type nodeType)
        {
            Type view;//NodeViewType（在字典中对于的viewtype）

            ///从字典中获取以nodeType为键对应的值NodeViewType，[如果有就直接返回]
            if (nodeViewPerType.TryGetValue(nodeType, out view))
                return view;

            ///若上面未找到对应的NodeViewType，则从字典中找是否有nodeType的父类，若有则将父类的Type获取
            Type baseType = null;//NodeViewType（）

            // Allow for inheritance in node views: multiple C# node using the same view
            // 允许在node views中继承：多个节点使用相同的view
            foreach (var type in nodeViewPerType)
            {
                // Find a view (not first fitted view) of nodeType 查找 nodeType 的视图（不是第一个拟合视图）
                // 如果nodeType是字典中某个键的子类，且（baseType为空 或 该键的值为baseType的子类），则将该键的NodeViewType赋给baseType
                if (nodeType.IsSubclassOf(type.Key) && (baseType == null || type.Value.IsSubclassOf(baseType)))
                    baseType = type.Value;
            }

            if (baseType != null)
                return baseType;

            return view;
        }

        /// <summary>获取节点菜单条目</summary>
        public static IEnumerable<(string path, Type type)> GetNodeMenuEntries(BaseGraph graph = null)
        {
            foreach (var node in genericNodes.nodePerMenuTitle)
                yield return (node.Key, node.Value);//一个一个返回（节点的菜单路径，节点类名）

            if (graph != null && specificNodeDescriptions.TryGetValue(graph, out var specificNodes))
            {
                foreach (var node in specificNodes.nodePerMenuTitle)
                    yield return (node.Key, node.Value);
            }
        }

        /// <summary>获取节点视图类名对应的Mono脚本</summary>
		public static MonoScript GetNodeViewScript(Type type)
        {
            nodeViewScripts.TryGetValue(type, out var script);

            return script;
        }
        /// <summary>获取节点类名对应的Mono脚本</summary>
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
        /// 获取边创建节点菜单项：从不同类型的端口拉出edge而创建出搜索窗口时，会根据端口类型显示处兼容的菜单项
        /// </summary>
        /// <param name="portView">拉出edge的端口</param>
        /// <param name="graph"></param>
        /// <returns></returns>
		public static IEnumerable<PortDescription> GetEdgeCreationNodeMenuEntry(PortView portView, BaseGraph graph = null)
        {
            foreach (var description in genericNodes.nodeCreatePortDescription)
            {
                //判断是否为兼容的端口
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

            ///是否为与portView兼容的端口
			bool IsPortCompatible(PortDescription description)
            {
                //拉出edge的端口，需与要连接的端口的Direction不同才能连接
                if ((portView.direction == Direction.Input && description.isInput) || (portView.direction == Direction.Output && !description.isInput))
                    return false;

                //两个端口类型需匹配才能链接
                if (!BaseGraph.TypesAreConnectable(description.portType, portView.portType))
                    return false;

                return true;
            }
        }
    }
}
