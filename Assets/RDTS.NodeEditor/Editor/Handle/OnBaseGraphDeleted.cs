using UnityEngine;
using UnityEditor;

namespace RDTS.NodeEditor
{
    [ExecuteAlways]
    public class DeleteCallback : UnityEditor.AssetModificationProcessor//允许保存在 Unity 内部编辑过的 序列化资源和场景
    {
        /// <summary>
        /// 当 Unity 即将从磁盘中删除资源时，则会调用此方法
        /// </summary>
        /// <param name="path"></param>
        /// <param name="options"></param>
        /// <returns></returns>
		static AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions options)
        {
            var objects = AssetDatabase.LoadAllAssetsAtPath(path);

            foreach (var obj in objects)
            {
                if (obj is BaseGraph b)
                {
                    foreach (var graphWindow in Resources.FindObjectsOfTypeAll<BaseGraphWindow>())
                        graphWindow.OnGraphDeleted();

                    b.OnAssetDeleted();
                }
            }

            return AssetDeleteResult.DidNotDelete;
        }
    }
}