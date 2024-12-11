using UnityEngine;
using UnityEditor;

namespace RDTS.NodeEditor
{
    [ExecuteAlways]
    public class DeleteCallback : UnityEditor.AssetModificationProcessor//�������� Unity �ڲ��༭���� ���л���Դ�ͳ���
    {
        /// <summary>
        /// �� Unity �����Ӵ�����ɾ����Դʱ�������ô˷���
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