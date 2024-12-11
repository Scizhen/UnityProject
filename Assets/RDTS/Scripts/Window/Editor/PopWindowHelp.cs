using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using RDTS;
using RDTS.Utility;
using RDTS.Interface;
using RDTS.Data;

namespace RDTS.Window
{
#if UNITY_EDITOR

    /// <summary>
    /// 弹窗：显示一个窗口的帮助提示，介绍操作方法。
    /// </summary>
    public class PopWindowHelp : EditorWindow
    {

        private HelpWindowContentData contentsData;
        private List<string> List_showContents = new List<string>();



        void OnGUI()
        {
            GUILayout.Space(5);

            DrawWindowContent();

        }


        void OnEnable()
        {
            contentsData = AssetDatabase.LoadAssetAtPath<HelpWindowContentData>(RDTSPath.path_Data + "HelpWindowContentData.asset");



        }


        Vector2 scroll;
        void DrawWindowContent()
        {
            scroll = GUILayout.BeginScrollView(scroll, GUILayout.ExpandWidth(true));///滚动条
            for (int i=0; i< List_showContents.Count; i++)
            {
                GUILayout.Label($"{i+1}. {List_showContents[i]}", GUILayout.ExpandWidth(true), GUILayout.Height(25));
            }
            GUILayout.EndScrollView();///滚动条
        }


        /// <summary>
        /// 设置要显示的内容，供外部调用
        /// </summary>
        /// <param name="contentList"></param>
        public void SetShowContents(List<string> contentList)
        {
            if (contentList == null)
                return; 

            List_showContents = contentList;

        }


        /// <summary>
        /// 读取指定索引号的数据
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public List<string> ReadHelpData(int index = 0)
        {

            if (contentsData == null)
                return null;

            return contentsData.helpDatas[index].contents;
        }


    }


#endif
}
