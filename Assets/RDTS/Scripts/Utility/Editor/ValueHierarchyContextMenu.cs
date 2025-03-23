using System.Reflection;
using UnityEditor;
using UnityEngine;


namespace RDTS.Utility.Editor
{
#if UNITY_EDITOR
    /// <summary>
    /// ��Inspector����У�����Ҽ��������תֵ�����Value�ű�����;
    /// �����˵���Parallel-RDTS����ӷ�תֵ�������͵Ĳ˵���Item
    /// </summary>
    [InitializeOnLoad]
    public static class ValueHierarchyContextMenu
    {



        private static void ChangeDirection(GameObject gameobjec)
        {
            var value = gameobjec.GetComponent<Value>();
            if (value == null)
            {
                EditorUtility.DisplayDialog("ȱ��Ŀ�����",
                   "Please select first an Object where the script should be added to!",
                   "OK");
                return;
            }

            Value newvalue = value;

            var type = value.GetType();
            if (value.IsInput())
            {
                if (type == typeof(ValueInputBool))
                {
                    newvalue = gameobjec.AddComponent<ValueOutputBool>();
                }
                if (type == typeof(ValueInputInt))
                {
                    newvalue = gameobjec.AddComponent<ValueOutputInt>();
                }
                if (type == typeof(ValueInputFloat))
                {
                    newvalue = gameobjec.AddComponent<ValueOutputFloat>();
                }
            }
            else
            {
                if (type == typeof(ValueOutputBool))
                {
                    newvalue = gameobjec.AddComponent<ValueInputBool>();
                }
                if (type == typeof(ValueOutputInt))
                {
                    newvalue = gameobjec.AddComponent<ValueInputInt>();
                }
                if (type == typeof(ValueOutputFloat))
                {
                    newvalue = gameobjec.AddComponent<ValueInputFloat>();
                }

            }

            newvalue.Name = value.Name;
            newvalue.Comment = value.Comment;
            newvalue.OriginDataType = value.OriginDataType;
            Object.DestroyImmediate(value);
        }



        [MenuItem("Parallel-RDTS/Utility/Change ValueObj Direction", false, 200)]
        public static void HierarchyChangeValueObjDirection()
        {
            if(Selection.objects.Length != 0)
            {
                foreach (var obj in Selection.objects)
                {
                    var gameobject = (GameObject)obj;
                    ChangeDirection(gameobject);
                }

            }
            else
            {
                EditorUtility.DisplayDialog("ȱ��Ŀ�����",
                   "Please select first an Object where the script should be added to!",
                   "OK");
            }

           

        }

        //MenuCommand��������ȡ MenuItem �������ģ��˵�����ӵ������У����ҿ�ͨ���ڼ���������Ҽ����������ʣ��ű�������Ҫ CONTEXT ѡ�
        [MenuItem("CONTEXT/Component/Parallel-RDTS/Change ValueObj Direction")]
        public static void ComtextChangeValueObjDirection(MenuCommand command)
        {
            var gameobject = command.context;//context������Ϊ�˵�����Ŀ��Ķ���
            var obj = (Component)gameobject;
            ChangeDirection(obj.gameObject);

        }

    }
#endif
}