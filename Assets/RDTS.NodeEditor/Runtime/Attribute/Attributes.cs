using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace RDTS.NodeEditor
{
    /// <summary>
    /// Tell that this field is will generate an input port  ����˿�
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class InputAttribute : Attribute
    {
        public string name;//�˿���ʾ����
        public bool allowMultiple = false;//�Ƿ������ж��edge

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
    /// Tell that this field is will generate an output port  ����˿�
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class OutputAttribute : Attribute
    {
        public string name;//�˿���ʾ����
        public bool allowMultiple = true;//�Ƿ������ж��edge

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
    /// Creates a vertical port instead of the default horizontal one  ��ֱ�˿�
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class VerticalAttribute : Attribute
    {
    }

    /// <summary>
    /// Register the node in the NodeProvider class. The node will also be available in the node creation window.
    /// �� NodeProvider ����ע��ڵ㡣 �ýڵ�Ҳ���ڽڵ㴴�������п��á�
    /// [�ڵ�˵�������](���ڽڵ���Node)
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class NodeMenuItemAttribute : Attribute
    {
        public string menuTitle;//�ڵ�Ĳ˵���·��
        public Type onlyCompatibleWithGraph;

        /// <summary>
        /// Register the node in the NodeProvider class. The node will also be available in the node creation window.
        /// </summary>
        /// <param name="menuTitle">Path in the menu, use / as folder separators �˵��е�·����ʹ�á�/����Ϊ�ļ��зָ���</param>
        public NodeMenuItemAttribute(string menuTitle = null, Type onlyCompatibleWithGraph = null)
        {
            this.menuTitle = menuTitle;
            this.onlyCompatibleWithGraph = onlyCompatibleWithGraph;
        }
    }

    /// <summary>
    /// Set a custom drawer for a field. It can then be created using the FieldFactory
    /// Ϊ�ֶ������Զ�����롣 Ȼ�����ʹ�� FieldFactory ������
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
    /// �����Զ���˿ڵ����빦��
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class CustomPortInputAttribute : Attribute
    {
        public string fieldName;//�ֶ�����
        public Type inputType;//��������
        public bool allowCast;//����edgeʱ�Ƿ�����ǿ��ת��

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
    /// �����Զ���˿ڵ��������
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
    /// ������ֶ��޸����ɵĶ˿���ͼ�� �����ڴ�һ���ֶ�����������edge���ӵĶ˿�
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class CustomPortBehaviorAttribute : Attribute
    {
        public string fieldName;//�ڵ��ֶ���

        /// <summary>
        /// Allow you to modify the generated port view from a field. Can be used to generate multiple ports from one field.
        /// You must add this attribute on a function of this signature
        /// ���������ֶ��޸����ɵĶ˿���ͼ�� �����ڴ�һ���ֶ����ɶ���˿�
        /// �������ڴ�ǩ���ĺ�������Ӵ�����
        /// <code>
        /// IEnumerable&lt;PortData&gt; MyCustomPortFunction(List&lt;SerializableEdge&gt; edges);
        /// </code>
        /// </summary>
        /// <param name="fieldName">local node field name ���ؽڵ��ֶ���</param>
        public CustomPortBehaviorAttribute(string fieldName)
        {
            this.fieldName = fieldName;
        }
    }

    /// <summary>
    /// Allow to bind a method to generate a specific set of ports based on a field type in a node
    /// ����󶨷����Ը��ݽڵ��е��ֶ���������һ���ض��Ķ˿�
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
    /// ����ӵ�� ��ջ �ڵ���Զ�����ͼ
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
    /// �����ɼ����ԣ��������(��һ)�ֶμ������ֶ�fieldName��ֵΪvalue������ʾ���и����Ե��ֶ�
    /// </summary>
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class VisibleIf : Attribute
    {
        public string fieldName;//�����ֶε��ֶ���
        public object value;//�����ֶε�ֵ

        public VisibleIf(string fieldName, object value)
        {
            this.fieldName = fieldName;
            this.value = value;
        }
    }

    /// <summary>
    /// �Ƿ��ֶ���ʾ�ڽڵ���ͼ��(ͼ��graph�в���ʾ)
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
    /// �ڵ�󶨵Ķ���
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


    /// <summary>��ʾ��������</summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class ShowAsDrawer : Attribute
    {
    }

    /// <summary>
    /// ���������ð�ť�������ص����������ֶ�
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
    /// ���ڽڵ��Զ���༭�����ԣ����ڽڵ���ͼ��(NodeView)
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