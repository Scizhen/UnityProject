using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NaughtyAttributes;
using RDTS.Method;
using RDTS;

[SelectionBase]
[ExecuteAlways]
public class LibraryObject : MonoBehaviour
{
    //路径系统中各组件核心脚本的基类脚本，主要负责总体控制和管理子对象
    [OnValueChanged("ChangeEditable")] public bool IsEditable;
    [OnValueChanged("Modify")] public float BottomHeight = 0;
    [OnValueChanged("Modify")] public bool DisplayStatus = true;


    protected LibraryObject Parent;

    private Color oldColor;
    private ISnapable _snapableImplementation;

    public void ChangeEditable()
    {
        ShowChildren(IsEditable);
        ShowInspector(gameObject, IsEditable);
    }

    public bool IsPrefab()
    {
#if UNITY_EDITOR

        var a = PrefabUtility.GetPrefabAssetType(this.gameObject);
        if (a == PrefabAssetType.NotAPrefab)
            return true;
        else
            return false;
#else
        return false;
#endif
    }


    [Button("Update")]
    public void Modify()
    {
        if (!gameObject.activeSelf)
            return;
        SetParent();
        OnModify();
        ChangeEditable();
        var children = GetComponentsInChildren<LibraryObject>();
        foreach (var child in children)
        {
            if (child != this)
                child.ParentChanged();
        }
        CheckSnapPoints();
    }


    public void EnableSnap(bool enable)
    {
        var snappoints = Global.GetComponentsAlsoInactive<SnapPoint>(gameObject);
        foreach (var snappoint in snappoints)
        {
            snappoint.Enable(enable);
        }
    }

    public void CheckSnapPoints()
    {
        var snappoints = GetComponentsInChildren<SnapPoint>();
        foreach (var snappoint in snappoints)
        {
            snappoint.CheckSnap();
        }
    }

    public void ParentChanged()
    {
        SetParent();
        OnParentModify();
    }

    protected void SetParent()
    {
        Parent = null;
        var parents = GetComponentsInParent<LibraryObject>();
        foreach (var parent in parents)
        {
            if (parent != this)
            {
                Parent = parent;
                return;
            }
        }
    }

    public virtual void OnModify()
    {
    }

    public virtual void OnParentModify()
    {
    }


    public virtual void AttachTo(SnapPoint attachto)
    {
    }

    public void Attach(string path, SnapPoint attachto)
    {
#if UNITY_EDITOR
        var original = (GameObject)AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));
        var newobj = PrefabUtility.InstantiatePrefab(original) as GameObject;
        Undo.RegisterCreatedObjectUndo(newobj, "Attach " + newobj);
        var libobj = newobj.GetComponent<LibraryObject>();
        libobj.gameObject.name = original.name;
        libobj.AttachTo(attachto);
#endif
    }

    public void Insert(string path)
    {
#if UNITY_EDITOR
        var original = (GameObject)AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));
        var newobj = PrefabUtility.InstantiatePrefab(original) as GameObject;
        var libobj = newobj.GetComponent<LibraryObject>();
        libobj.gameObject.name = original.name;
        libobj.transform.parent = this.transform;
#endif
    }


    public void ShowChildren(bool visible)
    {
        HideFlags flags = 0;
        if (!visible)
        {
            flags = HideFlags.HideInHierarchy;
        }

        Transform[] allChildren = GetComponentsInChildren<Transform>();
        foreach (Transform child in allChildren)
        {
            if (child.GetComponentInParent<LibraryObject>() == this)
            {
                if (HideAllowed(child.gameObject) && child.gameObject != this.gameObject)
                {
                    child.gameObject.hideFlags = flags;
                }
                else
                {
                    // child.gameObject.hideFlags = HideFlags.None;
                    ShowInspector(child.gameObject, visible);
#if UNITY_EDITOR
                    EditorUtility.SetDirty(child.gameObject);
#endif
                }
            }
        }
#if UNITY_EDITOR
        EditorUtility.SetDirty(this.gameObject);
#endif
    }

    public List<T> GetObjects<T>() where T : Component
    {
        var firstList = GetComponentsInChildren<T>(true);
        List<T> list = new List<T>();
        for (var i = 0; i < firstList.Length; i++)
        {
            var parents = firstList[i].transform.GetComponentsInParent<LibraryObject>();
            foreach (var parent in parents)
            {
                if (parent != this)
                {
                    list.Add(firstList[i]);
                }
            }
        }

        return list;
    }


    public void Align(SnapPoint ownsnappoint, SnapPoint matesnappoint, Quaternion additonalrotation)
    {
        transform.Translate(matesnappoint.transform.position - ownsnappoint.transform.position, Space.World);
        if (matesnappoint.name != ownsnappoint.name)
        {
            var qdelta = matesnappoint.transform.rotation * Quaternion.Inverse(ownsnappoint.transform.rotation);
            this.transform.rotation = qdelta * this.transform.rotation;
        }
        else
        {
            // do same rotation
            var qdelta2 = matesnappoint.transform.rotation * Quaternion.Inverse(ownsnappoint.transform.rotation);
            this.transform.rotation = qdelta2 * this.transform.rotation;
            // Same snappoint so turn 180 degrees
            qdelta2 = Quaternion.Euler(0, 180, 0);
            this.transform.rotation = qdelta2 * this.transform.rotation;
            // and now align again
            transform.Translate(matesnappoint.transform.position - ownsnappoint.transform.position, Space.World);
        }

        this.transform.rotation = additonalrotation * this.transform.rotation;
        transform.Translate(matesnappoint.transform.position - ownsnappoint.transform.position, Space.World);
    }

    private void On()
    {
        throw new NotImplementedException();
    }


    public void HideDriveGizmos()
    {
        var drives = GetComponentsInChildren<Drive>();
        {
            foreach (var drive in drives)
            {
                drive.HideGizmos = true;
#if UNITY_EDITOR
                EditorUtility.SetDirty(drive);
#endif
            }
        }
    }


    public void Move(GameObject obj, float x, float y, float z)
    {
        obj.transform.position = obj.transform.position + obj.transform.TransformDirection(new Vector3(x, y, z));
    }

    public void Rotate(GameObject obj, float rx, float ry, float rz)
    {
        var local = obj.transform.localRotation.eulerAngles;
        obj.transform.localRotation = Quaternion.Euler(local.x + rx, local.y + ry, local.z + rz);
    }

    public void Move(GameObject obj, Vector3 relative)
    {
        obj.transform.position = obj.transform.position + obj.transform.TransformDirection(relative);
    }



    public GameObject CreatePart(GameObject prefab, string insuboject, Vector3 position, Vector3 rotation)
    {
        Transform trans = null;
        if (insuboject != "")
        {
            var childs = GetComponentsInChildren<Transform>();

            foreach (var child in childs)
            {
                if (child.gameObject.name == insuboject)
                    trans = child.transform;
            }

            if (trans == null)
            {
                var sub = new GameObject(insuboject);
                sub.transform.parent = this.transform;
                sub.transform.localPosition = Vector3.zero;
                sub.transform.localRotation = Quaternion.identity;
                trans = sub.transform;
            }
        }

        GameObject created;


        created = Instantiate(prefab);
        created.transform.parent = trans;
        created.transform.localPosition = position;
        created.transform.localRotation = Quaternion.Euler(rotation);

        return created;
    }

    public void OnTransformParentChanged()
    {
        SetParent();
        OnParentChanged();
    }

    public T GetComponentByName<T>(string name) where T : Component//T为泛型类，where为泛型的约束条件
    {
        var firstList = GetComponentsInChildren<T>(true);

        for (var i = 0; i < firstList.Length; i++)
        {
            var parents = firstList[i].transform.GetComponentsInParent<LibraryObject>();
            foreach (var parent in parents)
            {
                if (firstList[i].gameObject.name == name)
                //   if (firstList[i].gameObject.name == name && parent != this)
                {
                    return firstList[i];
                }
            }
        }

        return null;
    }

    public void ShowInspector(GameObject gameobject, bool visible)
    {

        HideFlags flags = 0;
        if (!visible)
        {
            flags = HideFlags.HideInInspector;
        }

        MeshFilter meshFilter = gameobject.GetComponent<MeshFilter>();
        if (meshFilter != null)
        {
            meshFilter.hideFlags = flags;
        }

        MeshRenderer renderer = gameobject.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.hideFlags = flags;
        }

        Collider collider = gameobject.GetComponent<Collider>();
        if (collider != null)
        {
            collider.hideFlags = flags;
        }

        Rigidbody rigidbody = gameobject.GetComponent<Rigidbody>();
        if (rigidbody != null)
        {
            rigidbody.hideFlags = flags;
        }

#if UNITY_EDITOR
        EditorUtility.SetDirty(gameobject);
#endif
    }

    public virtual void OnParentChanged()
    {
    }

    public virtual bool HideAllowed(GameObject gameObject)
    {
        return false;
    }

    public void OnKeyPressed(KeyCode keycode)
    {

    }

    public void Reset()
    {
        Modify();
    }

    // Start is called before the first frame update
    void Start()
    {
        ChangeEditable();
    }

    private void OnEnable()
    {
        if (!Application.isPlaying)
        {
            // IsEditable = false;
            ChangeEditable();
        }
    }

    private void OnEnteredEditMode()
    {
        ChangeEditable();
    }



}