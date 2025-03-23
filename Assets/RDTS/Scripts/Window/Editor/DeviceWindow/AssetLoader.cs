using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RDTS.Data;

public class AssetLoader : MonoBehaviour
{
    DeviceLibraryData InputdevicelibraryData;//模型库的数据
    MaterialLibraryData InputmaterialLibraryData;//材质库的数据
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        string deviceassetPath = "Assets/RDTS/Data/DeviceLibraryData.asset";
        string materialassetPath = "Assets/RDTS/Data/MaterialLibraryData.asset";
        InputdevicelibraryData = AssetDatabase.LoadAssetAtPath<DeviceLibraryData>(deviceassetPath);
        InputmaterialLibraryData = AssetDatabase.LoadAssetAtPath<MaterialLibraryData>(materialassetPath);
        Texture titleIcon = AssetDatabase.LoadAssetAtPath<Texture>("Assets/RDTS/Private/Resources/Icons/WindowIcon/Parallel.png");
        Debug.Log(InputdevicelibraryData);
        Debug.Log(InputmaterialLibraryData);
        Debug.Log(titleIcon);
    }
}
