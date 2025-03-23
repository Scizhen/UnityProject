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
            GUILayout.Label("[Help]操作帮助", EditorStyles.boldLabel);
            GUILayout.Space(5);

            _scrollview = GUILayout.BeginScrollView(_scrollview);//<! 滑动条
            GUILayout.Label("1.点击“刷新”按钮，即可重新加载设备库窗口的内容");
            GUILayout.Label("2.点击“设备图标”可开启一次拖拽操作，空白处按下鼠标左键并拖拽至Scene视图中，将在左键抬起时的鼠标位置添加此设备");
            GUILayout.Label("3.点击“点击添加设备”按钮，即可在“添加位置”的坐标处添加相应设备");
            GUILayout.EndScrollView();//<! 滑动条
        }



    }
}
