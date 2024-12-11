//*************************************************************************
//Thanks for the code reference game4automation provides.                 *
//                                                                        *
//*************************************************************************
using System;
using System.Linq;
using UnityEngine;


namespace RDTS
{
    //! Adds this Gameobject to a defined group. Is used for filtering the View or for defining a new kinematic structure with Kinematic script.
    //���� Gameobject ��ӵ���������С� ���ڹ�����ͼ��ʹ���˶�ѧ�ű������µ��˶�ѧ�ṹ��
    public class Group : RDTSBehavior
    {
        // Start is called before the first frame update
        public string GroupName; //!< The Group name
        public GameObject GroupNamePrefix; //!< A prefix for the Groupname (used for using Groups in reusable Prefabs) Groupname ��ǰ׺�������ڿ��ظ�ʹ�õ� Prefab ��ʹ�� Groups��

        // Gets the Groupname
        public string GetGroupName()
        {
            if (GroupNamePrefix != null)
                return (GroupNamePrefix.name + GroupName);
            else
                return GroupName;
        }

        // Gets the text for the hierarchy view
        public string GetVisuText()
        {
            string text = "";
            // Collect all groups
            var groups = GetComponents<Group>().ToArray();

            for (int i = 0; i < groups.Length; i++)
            {
                if (i != 0)
                    text = text + "/";
                text = text + groups[i].GroupName;
            }

            return text;
        }


        private new void Awake()
        {

        }


        public void ChangeConnectionMode()
        {

        }

    }
}