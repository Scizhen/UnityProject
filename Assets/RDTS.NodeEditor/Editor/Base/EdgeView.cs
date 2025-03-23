using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;

namespace RDTS.NodeEditor
{
    /// <summary>
    /// GraphView��EdgeԪ�أ��Ƿ����ӣ���ʽ���ߴ磬�м̽ڵ����
    /// </summary>
	public class EdgeView : Edge
    {
        public bool isConnected = false;//�Ƿ�����

        public SerializableEdge serializedEdge { get { return userData as SerializableEdge; } }//userData�������Կ����ڽ��ض���Ӧ�ó�����û�������� VisualElement ����

        readonly string edgeStyle = "GraphStyles/EdgeView";

        protected BaseGraphView owner => ((input ?? output) as PortView).owner.owner;
        //??��null�ϲ����������inputΪnull��ֵΪoutput


        public EdgeView() : base()
        {
            styleSheets.Add(Resources.Load<StyleSheet>(edgeStyle));
            RegisterCallback<MouseDownEvent>(OnMouseDown);//��ʵ������¼��������OnMouseDown������м̽ڵ�
        }

        /// <summary>
        /// �ڱ�Ե�ϵĶ˿ڸ���ʱ���á�
        /// </summary>
        /// <param name="isInput">�������˿��Ѹ��ģ���Ϊ true���������˿��Ѹ��ģ���Ϊ false</param>
        public override void OnPortChanged(bool isInput)
        {
            base.OnPortChanged(isInput);
            UpdateEdgeSize();
        }

        public void UpdateEdgeSize()
        {
            if (input == null && output == null)
                return;

            PortData inputPortData = (input as PortView)?.portData;
            PortData outputPortData = (output as PortView)?.portData;

            for (int i = 1; i < 20; i++)
                RemoveFromClassList($"edge_{i}");//��Ԫ�ص����б���ɾ��һ���ࡣ  ������Ҫɾ�����б��е��������
            int maxPortSize = Mathf.Max(inputPortData?.sizeInPixel ?? 0, outputPortData?.sizeInPixel ?? 0);//���˿ڳߴ�
            if (maxPortSize > 0)
                AddToClassList($"edge_{Mathf.Max(1, maxPortSize - 6)}");//��һ������ӵ�Ԫ�ص����б��У��Ա�� USS ������ʽ��
        }

        /// <summary>
        /// �ڽ����Զ�����ʽ����ʱ����
        /// </summary>
        /// <param name="styles"></param>
		protected override void OnCustomStyleResolved(ICustomStyle styles)
        {
            base.OnCustomStyleResolved(styles);

            UpdateEdgeControl();//���±�Ե�� EdgeControl
        }

        /// <summary>
        /// ��갴���£��ڹ�괦�����м̽ڵ�
        /// </summary>
        /// <param name="e"></param>
		void OnMouseDown(MouseDownEvent e)
        {
            if (e.clickCount == 2)//���¼��Ĵ���
            {
                // Empirical offset: ����ƫ��
                var position = e.mousePosition;
                position += new Vector2(-10f, -28);
                //ChangeCoordinatesTo����һ�����һ��Ԫ�صľֲ��ռ�ת������һ��Ԫ�صľֲ��ռ�
                Vector2 mousePos = owner.ChangeCoordinatesTo(owner.contentViewContainer, position);

                owner.AddRelayNode(input as PortView, output as PortView, mousePos);//����м̽ڵ�
                Debug.Log("��������£������м̽ڵ�");
            }
        }
    }
}