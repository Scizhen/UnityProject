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
    /// ��������ʾһ�����ڵİ�����ʾ�����ܲ���������
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
            scroll = GUILayout.BeginScrollView(scroll, GUILayout.ExpandWidth(true));///������
            for (int i=0; i< List_showContents.Count; i++)
            {
                GUILayout.Label($"{i+1}. {List_showContents[i]}", GUILayout.ExpandWidth(true), GUILayout.Height(25));
            }
            GUILayout.EndScrollView();///������
        }


        /// <summary>
        /// ����Ҫ��ʾ�����ݣ����ⲿ����
        /// </summary>
        /// <param name="contentList"></param>
        public void SetShowContents(List<string> contentList)
        {
            if (contentList == null)
                return; 

            List_showContents = contentList;

        }


        /// <summary>
        /// ��ȡָ�������ŵ�����
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
