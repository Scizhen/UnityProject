using UnityEngine;
using UnityEditor;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace RDTS.Utility.Editor
{
#if UNITY_EDITOR
    [InitializeOnLoad]
    //! The class is automatically saving the scene when run is started in the Unity editor. It can be turned off by the toggle in the Parallel-RDTS menu
    //当在 Unity 编辑器中启动运行时，该类会自动保存场景。 它可以通过 Parallel-RDTS 菜单中的切换关闭
    public class AutoSaveOnRunMenuItem
    {


        public const string MenuName = "Parallel-RDTS/Auto Save";//菜单路径
        private static bool isToggled;

        static AutoSaveOnRunMenuItem()
        {
            EditorApplication.delayCall += () =>
            {
                isToggled = EditorPrefs.GetBool(MenuName, false);
                UnityEditor.Menu.SetChecked(MenuName, isToggled);
                SetMode();
            };
        }

        [MenuItem(MenuName, false, 500)]
        private static void ToggleMode()
        {
            isToggled = !isToggled;
            UnityEditor.Menu.SetChecked(MenuName, isToggled);
            EditorPrefs.SetBool(MenuName, isToggled);
            SetMode();
        }

        private static void SetMode()
        {
            if (isToggled)
            {
                //playModeStateChanged：每当编辑器的播放模式状态发生更改时引发的事件
                EditorApplication.playModeStateChanged += AutoSaveOnRun;
            }
            else
            {
                EditorApplication.playModeStateChanged -= AutoSaveOnRun;
            }
        }

        private static void AutoSaveOnRun(PlayModeStateChange state)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)//进去播放模式前被调用
            {
                Debug.Log("Auto-Saving before entering Play mode");

                EditorSceneManager.SaveOpenScenes();//保存所有打开的场景
                AssetDatabase.SaveAssets();
            }
        }
    }
#endif
}