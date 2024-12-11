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
    //���� Unity �༭������������ʱ��������Զ����泡���� ������ͨ�� Parallel-RDTS �˵��е��л��ر�
    public class AutoSaveOnRunMenuItem
    {


        public const string MenuName = "Parallel-RDTS/Auto Save";//�˵�·��
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
                //playModeStateChanged��ÿ���༭���Ĳ���ģʽ״̬��������ʱ�������¼�
                EditorApplication.playModeStateChanged += AutoSaveOnRun;
            }
            else
            {
                EditorApplication.playModeStateChanged -= AutoSaveOnRun;
            }
        }

        private static void AutoSaveOnRun(PlayModeStateChange state)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)//��ȥ����ģʽǰ������
            {
                Debug.Log("Auto-Saving before entering Play mode");

                EditorSceneManager.SaveOpenScenes();//�������д򿪵ĳ���
                AssetDatabase.SaveAssets();
            }
        }
    }
#endif
}