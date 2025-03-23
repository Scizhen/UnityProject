using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace RDTS.NodeEditor
{
    /// <summary>
    /// Tell that this field is will generate an input port  输入端口
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class InputAttribute : Attribute
    {
        public string name;//端口显示名称
        public bool allowMultiple = false;//是否允许有多个edge

        /// <summary>
        /// Mark the field as an input port
        /// </summary>
        /// <param name="name">display name</param>
        /// <param name="allowMultiple">is connecting multiple edges allowed</param>
        public InputAttribute(string name = null, bool allowMultiple = false)
        {
            this.name = name;
            this.allowMultiple = allowMultiple;
        }
    }

    /// <summary>
    /// Tell that this field is will generate an output port  输出端口
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class OutputAttribute : Attribute
    {
        public string name;//端口显示名称
        public bool allowMultiple = true;//是否允许有多个edge

        /// <summary>
        /// Mark the field as an output port
        /// </summary>
        /// <param name="name">display name</param>
        /// <param name="allowMultiple">is connecting multiple edges allowed</param>
        public OutputAttribute(string name = null, bool allowMultiple = true)
        {
            this.name = name;
            this.allowMultiple = allowMultiple;
        }
    }

    /// <summary>
    /// Creates a vertical port instead of the default horizontal one  垂直端口
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class VerticalAttribute : Attribute
    {
    }

    /// <summary>
    /// Register the node in the NodeProvider class. The node will also be available in the node creation window.
    /// 在 NodeProvider 类中注册节点。 该节点也将在节点创建窗口中可用。
    /// [节点菜单项特性](用于节点类Node)
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class NodeMenuItemAttribute : Attribute
    {
        public string menuTitle;//节点的菜单栏路径
        public Type onlyCompatibleWithGraph;

        /// <summary>
        /// Register the node in the NodeProvider class. The node will also be available in the node creation window.
        /// </summary>
        /// <param name="menuTitle">Path in the menu, use / as folder separators 菜单中的路径，使用“/”作为文件夹分隔符</param>
        public NodeMenuItemAttribute(string menuTitle = null, Type onlyCompatibleWithGraph = null)
        {
            this.menuTitle = menuTitle;
            this.onlyCompatibleWithGraph = onlyCompatibleWithGraph;
        }
    }

    /// <summary>
    /// Set a custom drawer for a field. It can then be created using the FieldFactory
    /// 为字段设置自定义抽屉。 然后可以使用 FieldFactory 创建它
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    [Obsolete("You can use the standard Unity CustomPropertyDrawer instead.")]
    public class FieldDrawerAttribute : Attribute
    {
        public Type fieldType;

        /// <summary>
        /// Register a custom view for a type in the FieldFactory class
        /// </summary>
        /// <param name="fieldType"></param>
        public FieldDrawerAttribute(Type fieldType)
        {
            this.fieldType = fieldType;
        }
    }

    /// <summary>
    /// Allow you to customize the input function of a port
    /// 允许自定义端口的输入功能
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class CustomPortInputAttribute : Attribute
    {
        public string fieldName;//字段名称
        public Type inputType;//输入类型
        public bool allowCast;//连接edge时是否允许强制转换

        /// <summary>
        /// Allow you to customize the input function of a port.
        /// See CustomPortsNode example in Samples.
        /// </summary>
        /// <param name="fieldName">local field of the node</param>
        /// <param name="inputType">type of input of the port</param>
        /// <param name="allowCast">if cast is allowed when connecting an edge</param>
        public CustomPortInputAttribute(string fieldName, Type inputType, bool allowCast = true)
        {
            this.fieldName = fieldName;
            this.inputType = inputType;
            this.allowCast = allowCast;
        }
    }

    /// <summary>
    /// Allow you to customize the output function of a port
    /// 允许自定义端口的输出功能
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class CustomPortOutputAttribute : Attribute
    {
        public string fieldName;
        public Type outputType;
        public bool allowCast;

        /// <summary>
        /// Allow you to customize the output function of a port.
        /// See CustomPortsNode example in Samples.
        /// </summary>
        /// <param name="fieldName">local field of the node</param>
        /// <param name="inputType">type of input of the port</param>
        /// <param name="allowCast">if cast is allowed when connecting an edge</param>
        public CustomPortOutputAttribute(string fieldName, Type outputType, bool allowCast = true)
        {
            this.fieldName = fieldName;
            this.outputType = outputType;
            this.allowCast = allowCast;
        }
    }

    /// <summary>
    /// Allow you to modify the generated port view from a field. Can be used to generate multiple ports from one field.
    /// 允许从字段修改生成的端口视图。 可用于从一个字段生成允许多个edge连接的端口
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class CustomPortBehaviorAttribute : Attribute
    {
        public string fieldName;//节点字段名

        /// <summary>
        /// Allow you to modify the generated port view from a field. Can be used to generate multiple ports from one field.
        /// You must add this attribute on a function of this signature
        /// 允许您从字段修改生成的端口视图。 可用于从一个字段生成多个端口
        /// 您必须在此签名的函数上添加此属性
        /// <code>
        /// IEnumerable&lt;PortData&gt; MyCustomPortFunction(List&lt;SerializableEdge&gt; edges);
        /// </code>
        /// </summary>
        /// <param name="fieldName">local node field name 本地节点字段名</param>
        public CustomPortBehaviorAttribute(string fieldName)
        {
            this.fieldName = fieldName;
        }
    }

    /// <summary>
    /// Allow to bind a method to generate a specific set of ports based on a field type in a node
    /// 允许绑定方法以根据节点中的字段类型生成一组特定的端口
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class CustomPortTypeBehavior : Attribute
    {
        /// <summary>
        /// Target type
        /// </summary>
        public Type type;

        public CustomPortTypeBehavior(Type type)
        {
            this.type = type;
        }
    }

    /// <summary>
    /// Allow you to have a custom view for your stack nodes
    /// 允许拥有 堆栈 节点的自定义视图
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CustomStackNodeView : Attribute
    {
        public Type stackNodeType;

        /// <summary>
        /// Allow you to have a custom view for your stack nodes
        /// </summary>
        /// <param name="stackNodeType">The type of the stack node you target</param>
        public CustomStackNodeView(Type stackNodeType)
        {
            this.stackNodeType = stackNodeType;
        }
    }

    /// <summary>
    /// 条件可见特性：如果满足(另一)字段即条件字段fieldName的值为value，就显示具有该特性的字段
    /// </summary>
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class VisibleIf : Attribute
    {
        public string fieldName;//条件字段的字段名
        public object value;//条件字段的值

        public VisibleIf(string fieldName, object value)
        {
            this.fieldName = fieldName;
            this.value = value;
        }
    }

    /// <summary>
    /// 是否将字段显示在节点视图中(图表graph中不显示)
    /// </summary>
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class ShowInInspector : Attribute
    {
        public bool showInNode;

        public ShowInInspector(bool showInNode = false)
        {
            this.showInNode = showInNode;
        }
    }


    /// <summary>
    /// 节点绑定的对象
    /// </summary>
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class BindObjectAttribute : Attribute
    {
        public string showName;
        public Type fieldType;

        public BindObjectAttribute(string showName = null, Type fieldType = null)
        {
            this.showName = showName;
            this.fieldType = fieldType;
        }

    }


    /// <summary>显示抽屉特性</summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class ShowAsDrawer : Attribute
    {
    }

    /// <summary>
    /// 用于在设置按钮中添加相关的设置属性字段
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class SettingAttribute : Attribute
    {
        public string name;

        public SettingAttribute(string name = null)
        {
            this.name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class IsCompatibleWithGraph : Attribute { }




    /// <summary>
    /// 用于节点自定义编辑的特性，用于节点视图类(NodeView)
    /// </summary>
	[AttributeUsage(AttributeTargets.Class)]
    public class NodeCustomEditor : Attribute
    {
        public Type nodeType;

        public NodeCustomEditor(Type nodeType)
        {
            this.nodeType = nodeType;
        }
    }


}