using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEngine;
using System;

namespace RDTS.NodeEditor
{
    /// <summary>
    /// ���ڰ塱Ԫ��PinnedElement��ͼ�Ļ��࣬ȷ��Ԫ����ʽ����������(���϶���������ȷ���ߴ��С��)�����⡢��ʼ��
    /// </summary>
	public abstract class PinnedElementView : GraphElement
    {
        protected PinnedElement pinnedElement;//[�����л�]��ڰ�һ������ͼ�ε�Ԫ��
        protected VisualElement root;
        protected VisualElement content;
        protected VisualElement header;

        protected event Action onResized;

        VisualElement main;
        Label titleLabel;
        bool _scrollable;
        ScrollView scrollView;//ScrollView���ڿɹ����������ʾ������

        static readonly string pinnedElementStyle = "GraphStyles/PinnedElementView";//uss�ļ���ȷ����ʽ
        static readonly string pinnedElementTree = "GraphUXML/PinnedElement";//uxml�ļ�������Ԫ�صĽ���

        /// <summary>
        /// [����]���⣺���ڻ�ȡ������titleLabel
        /// </summary>
        public override string title
        {
            get { return titleLabel.text; }
            set { titleLabel.text = value; }
        }

        protected bool scrollable
        {
            get
            {
                return _scrollable;
            }
            set
            {
                if (_scrollable == value)//���ѵ���Ҫ���õ�ֵ����ֱ�ӷ���
                    return;

                _scrollable = value;

                style.position = Position.Absolute;//Absolute��Ԫ�صķ������丸��������أ����Ҳ���Ӱ�쵽����
                if (_scrollable)//����Ҫ������ʾ�����scrollViewԪ�ز���contentԪ����Ϊ���Ӽ�
                {
                    content.RemoveFromHierarchy();//�Ӹ����ӽ���contentԪ���Ƴ�
                    root.Add(scrollView);//��ӹ������
                    scrollView.Add(content);//�ڹ����������Ӵ�content
                    AddToClassList("scrollable");
                }
                else//�����������ʾ����scrollViewԪ���Ƴ�������contentԪ����Ϊroot���Ӽ�
                {
                    scrollView.RemoveFromHierarchy();//�Ӹ����ӽ���scrollViewԪ���Ƴ�
                    content.RemoveFromHierarchy();
                    root.Add(content);
                    RemoveFromClassList("scrollable");
                }
            }
        }

        /// <summary>
        /// ���캯������ʽ��������ã����϶��Ҳ��ᱻ�ϳ���Ե��������ȷ���ߴ��С
        /// </summary>
		public PinnedElementView()
        {
            var tpl = Resources.Load<VisualTreeAsset>(pinnedElementTree);//���õ�Asset����Ƶ�Ԫ�ؽ��漴PinnedElement.uxml
            styleSheets.Add(Resources.Load<StyleSheet>(pinnedElementStyle));

            main = tpl.CloneTree();
            main.AddToClassList("mainContainer");
            scrollView = new ScrollView(ScrollViewMode.VerticalAndHorizontal);

            root = main.Q("content");//PinnedElement.uxml�ĸ�����

            header = main.Q("header");//PinnedElement.uxml�ı�ͷ

            titleLabel = main.Q<Label>(name: "titleLabel");//PinnedElement.uxml�ı����ǩ
            content = main.Q<VisualElement>(name: "contentContainer");//PinnedElement.uxml����������

            ///hierarchy�����ʴ�Ԫ������㼶��ͼ
            hierarchy.Add(main);

            capabilities |= Capabilities.Movable | Capabilities.Resizable;//���ƶ���������ȷ����С
            ///overflow���������������������ʱ����Ϊ
            ///Overflow�����嵱�������Ԫ�ر߽�ʱ����δ���
            style.overflow = Overflow.Hidden;//Hidden����������ݻᱻ�������������ݽ����ɼ�

            ClearClassList();
            AddToClassList("pinnedElement");

            ///Dragger������ʹ������϶�Ԫ�صĻ����ٿ���
            //ʹ�ô�Ԫ���ܹ����϶����Ҳ��ᱻ�ϳ�graph�ı�Ե
            this.AddManipulator(new Dragger { clampToParentEdges = true });//clampToParentEdges���Ϊ true�������������϶���Ԫ���˳���Ԫ�صı�Ե

            scrollable = false;

            ///Resizer����С��������ٿ���Ԫ��
            //ʹ�ô�Ԫ���ܹ����Զ���ߴ��С
            hierarchy.Add(new Resizer(() => onResized?.Invoke()));

            ///DragUpdatedEvent���϶���Ԫ�ؽ�����ܵķ���Ŀ��ʱ���͵��¼�
            RegisterCallback<DragUpdatedEvent>(e =>
            {
                e.StopPropagation();
            });

            title = "PinnedElementView";
        }

        /// <summary>
        /// ��ʼ��PinnedElementԪ����ͼ
        /// </summary>
        /// <param name="pinnedElement"></param>
        /// <param name="graphView"></param>
		public void InitializeGraphView(PinnedElement pinnedElement, BaseGraphView graphView)
        {
            this.pinnedElement = pinnedElement;
            SetPosition(pinnedElement.position);//����λ�úʹ�С

            //Ϊί�з�����ӵ��÷���
            onResized += () => {
                pinnedElement.position.size = layout.size;
                ///Debug.Log("Resizer pinnedElement");//���ı�Ԫ�ش�С������ɿ�ʱ��ӡ
            };
            //ע��ص�
            RegisterCallback<MouseUpEvent>(e => {
                pinnedElement.position.position = layout.position;
                ///Debug.Log("Set position pinnedElement");//�����Ԫ��(�����϶�Ԫ��)����갴��̧��ʱ��ӡ
            });

            Initialize(graphView);//��ʼ���������ͼ��graph���������
        }

        /// <summary>����Ԫ��λ�úͳߴ��С</summary>
		public void ResetPosition()
        {
            pinnedElement.position = new Rect(Vector2.zero, PinnedElement.defaultSize);
            SetPosition(pinnedElement.position);
        }

        /// <summary>
        /// ��ʼ���������ͼ��graph��������ã��ڼ̳е�������ʵ�ִ˷�����ʵ�־���Ĺ���
        /// </summary>
        /// <param name="graphView">���ڲ�����ͼ��graph</param>
		protected abstract void Initialize(BaseGraphView graphView);

        ~PinnedElementView()
        {
            Destroy();
        }

        protected virtual void Destroy() { }
    }
}
