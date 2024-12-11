using System.Reflection;
using UnityEditor;
using UnityEngine;


namespace RDTS.Utility.Editor
{
#if UNITY_EDITOR
    /// <summary>
    /// 在Inspector面板中，鼠标右键点击来反转值对象的Value脚本类型;
    /// 在主菜单栏Parallel-RDTS中添加反转值对象类型的菜单项Item
    /// </summary>
    [InitializeOnLoad]
    public static class ValueHierarchyContextMenu
    {



        private static void ChangeDirection(GameObject gameobjec)
        {
            var value = gameobjec.GetComponent<Value>();
            if (value == null)
            {
                EditorUtility.DisplayDialog("缺少目标对象",
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
                EditorUtility.DisplayDialog("缺少目标对象",
                   "Please select first an Object where the script should be added to!",
                   "OK");
            }

           

        }

        //MenuCommand：用于提取 MenuItem 的上下文，菜单会添加到对象中，并且可通过在检视面板中右键单击来访问，脚本代码需要 CONTEXT 选项。
        [MenuItem("CONTEXT/Component/Parallel-RDTS/Change ValueObj Direction")]
        public static void ComtextChangeValueObjDirection(MenuCommand command)
        {
            var gameobject = command.context;//context：是作为菜单命令目标的对象。
            var obj = (Component)gameobject;
            ChangeDirection(obj.gameObject);

        }

    }
#endif
}