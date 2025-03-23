using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.Linq;

namespace RDTS.Utility.Editor
{
    /// <summary>
    /// 在检视面板Inspector中预览场景（当场景进入play模式后会更新场景截图）
    /// </summary>
    [CustomEditor(typeof(SceneAsset))]
    [CanEditMultipleObjects]
    public class ScenePreview : UnityEditor.Editor
    {
        const string PreviewFolders = "RDTS/Private/Resources/ScenePreview"; //可以修改为自己的路径，用来存放场景缩略图

        float ScreenWidth; //<! 窗口宽度
        float ScreenHeight;//<! 窗口高度

        static bool _shouldRefreshDatabase;
        [RuntimeInitializeOnLoadMethod]//在场景加载且Game运行时被调用的方法（在Awake方法之后被调用）
        public static void CaptureScreenshot()
        {
            var previewPath = GetPreviewPath(SceneManager.GetActiveScene().name);//若为：{0}/{1}/Resources/{2}.png
            var dir = Path.GetDirectoryName(previewPath);//则为：{0}/{1}/Resources
            if (!Directory.Exists(dir))//判断是否存在此文件的路径，没有就创建
            {
                Directory.CreateDirectory(dir);
            }
            Debug.LogFormat("Saving scene preview at {0}", previewPath);
            ///ScreenCapture：截屏功能
            ///CaptureScreenshot：在路径 previewPath 捕获截屏并将其作为 PNG 文件
            ScreenCapture.CaptureScreenshot(previewPath);
            Debug.LogFormat("Scene preview saved at {0}", previewPath);
            _shouldRefreshDatabase = true;
        }

        public override void OnInspectorGUI()
        {
            if (_shouldRefreshDatabase)
            {
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);//更改（刷新）由用户发起的资源导入
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
            
            var preview = Resources.Load<Texture>($"ScenePreview/{sceneName}") as Texture;//按照场景名加载预览图
            if (preview == null)
            {
                EditorGUILayout.HelpBox(
                    string.Format("还没有场景{0}的预览图{1}. 请切换到这个场景然后点击播放，会自动生成该场景的缩略图", 
                    sceneName, previewPath), MessageType.Info);
            }
            else
            {
                GUILayout.Space(10);
                GUILayout.Label("场景预览图如下：");
                GUILayout.Box(preview, GUILayout.ExpandWidth(true));
            }
        }

        /// <summary>
        /// 获取预览图的路径
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        /// <returns>预览图在工程中的路径</returns>
        static string GetPreviewPath(string sceneName)
        {
            return string.Format("{0}/{1}/{2}.png", Application.dataPath, PreviewFolders, sceneName);
        }
    }


}
