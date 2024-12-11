using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.Linq;

namespace RDTS.Utility.Editor
{
    /// <summary>
    /// �ڼ������Inspector��Ԥ������������������playģʽ�����³�����ͼ��
    /// </summary>
    [CustomEditor(typeof(SceneAsset))]
    [CanEditMultipleObjects]
    public class ScenePreview : UnityEditor.Editor
    {
        const string PreviewFolders = "RDTS/Private/Resources/ScenePreview"; //�����޸�Ϊ�Լ���·����������ų�������ͼ

        float ScreenWidth; //<! ���ڿ��
        float ScreenHeight;//<! ���ڸ߶�

        static bool _shouldRefreshDatabase;
        [RuntimeInitializeOnLoadMethod]//�ڳ���������Game����ʱ�����õķ�������Awake����֮�󱻵��ã�
        public static void CaptureScreenshot()
        {
            var previewPath = GetPreviewPath(SceneManager.GetActiveScene().name);//��Ϊ��{0}/{1}/Resources/{2}.png
            var dir = Path.GetDirectoryName(previewPath);//��Ϊ��{0}/{1}/Resources
            if (!Directory.Exists(dir))//�ж��Ƿ���ڴ��ļ���·����û�оʹ���
            {
                Directory.CreateDirectory(dir);
            }
            Debug.LogFormat("Saving scene preview at {0}", previewPath);
            ///ScreenCapture����������
            ///CaptureScreenshot����·�� previewPath ���������������Ϊ PNG �ļ�
            ScreenCapture.CaptureScreenshot(previewPath);
            Debug.LogFormat("Scene preview saved at {0}", previewPath);
            _shouldRefreshDatabase = true;
        }

        public override void OnInspectorGUI()
        {
            if (_shouldRefreshDatabase)
            {
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);//���ģ�ˢ�£����û��������Դ����
                _shouldRefreshDatabase = false;
            }
            var sceneNames = targets.Select(t => ((SceneAsset)t).name).OrderBy(n => n).ToArray();
            var previewWidth = 400;
            var previewHeight = 400;
            for (int i = 0; i < sceneNames.Length; i++)
            {
                DrawPreview(i, sceneNames[i], previewWidth, previewHeight);
            }
        }

        void DrawPreview(int index, string sceneName, float width, float height)
        {
            var previewPath = GetPreviewPath(sceneName);
            
            var preview = Resources.Load<Texture>($"ScenePreview/{sceneName}") as Texture;//���ճ���������Ԥ��ͼ
            if (preview == null)
            {
                EditorGUILayout.HelpBox(
                    string.Format("��û�г���{0}��Ԥ��ͼ{1}. ���л����������Ȼ�������ţ����Զ����ɸó���������ͼ", 
                    sceneName, previewPath), MessageType.Info);
            }
            else
            {
                GUILayout.Space(10);
                GUILayout.Label("����Ԥ��ͼ���£�");
                GUILayout.Box(preview, GUILayout.ExpandWidth(true));
            }
        }

        /// <summary>
        /// ��ȡԤ��ͼ��·��
        /// </summary>
        /// <param name="sceneName">��������</param>
        /// <returns>Ԥ��ͼ�ڹ����е�·��</returns>
        static string GetPreviewPath(string sceneName)
        {
            return string.Format("{0}/{1}/{2}.png", Application.dataPath, PreviewFolders, sceneName);
        }
    }


}
