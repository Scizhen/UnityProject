using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;



namespace VisualSpline
{

    [CustomEditor(typeof(Xml2AGVMap))]
    public class Xml2AGVMapEditor : Editor
    {
        Xml2AGVMap _target;

        void OnEnable()
        {
            _target = target as Xml2AGVMap;
        }


        //Inspector界面绘制:
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();//绘制默认界面
            DrawAddButton();//额外绘制按钮
        }


        void DrawAddButton()
        {
            if (GUILayout.Button("Draw Map"))
            {
                _target.LoadXmlAndDrawMap();
            }
            
            if (GUILayout.Button("Remove All Point"))
            {
                _target.RemoveAllPoint();
            }
        }



    }

}
