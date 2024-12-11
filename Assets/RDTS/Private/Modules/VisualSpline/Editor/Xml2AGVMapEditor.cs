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


        //Inspector�������:
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();//����Ĭ�Ͻ���
            DrawAddButton();//������ư�ť
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
