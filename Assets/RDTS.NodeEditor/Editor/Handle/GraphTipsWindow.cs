using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RDTS.NodeEditor
{
    public class GraphTipsWindow : PopupWindowContent
    {
        Vector2 _scrollview;
        public override void OnGUI(Rect rect)
        {
            GUILayout.Label("[Help]��������", EditorStyles.boldLabel);
            GUILayout.Space(5);

            _scrollview = GUILayout.BeginScrollView(_scrollview);//<! ������
            GUILayout.Label("1.�����ˢ�¡���ť���������¼����豸�ⴰ�ڵ�����");
            GUILayout.Label("2.������豸ͼ�ꡱ�ɿ���һ����ק�������հ״���������������ק��Scene��ͼ�У��������̧��ʱ�����λ����Ӵ��豸");
            GUILayout.Label("3.������������豸����ť�������ڡ����λ�á������괦�����Ӧ�豸");
            GUILayout.EndScrollView();//<! ������
        }



    }
}
