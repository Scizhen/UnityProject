using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LibraryPart : MonoBehaviour
{
    //路径系统中的基类脚本，包含打开预制体、获取对象名等方法
    private Color oldColor;


    public void UnpackPrefab()
    {
#if UNITY_EDITOR
        if (PrefabUtility.GetPrefabInstanceHandle(this))
        {
            var prefab = GetComponentInParent<LibraryObject>().gameObject;
            PrefabUtility.UnpackPrefabInstance(prefab, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        }
#endif
    }

    public List<T> GetObjectsByName<T>(string name) where T : Component
    {
        var firstList = GetComponentsInChildren<T>();
        var secondlist = new List<T>();
        for (var i = 0; i < firstList.Length; i++)
        {
            if (firstList[i].gameObject.name == name)
            {
                secondlist.Add(firstList[i]);
            }
        }

        return secondlist;
    }

    public T GetObjectByName<T>(string name) where T : Component
    {
        var firstList = GetComponentsInChildren<T>();

        for (var i = 0; i < firstList.Length; i++)
        {
            if (firstList[i].gameObject.name == name)
            {
                return firstList[i];
            }
        }

        return null;
    }

    public void Move(Vector3 relative)
    {
        this.transform.localPosition = this.transform.localPosition + relative;
    }


    public virtual void Modify()
    {

    }

}
