
using UnityEngine;
using UnityEngine.UI;


namespace RDTS
{
    //! Displays a message box with a text field in the middle of the gameview
    //在游戏视图中间显示一个带有文本字段的消息框
    //快捷键切换相机视角时会显示（自动关闭的）消息框
    public class UIMessageBox : MonoBehaviour
    {
        public Text TextBox; //!< A pointer to the Unity UI text box


        private void DestroyMessage()
        {
            Destroy(gameObject);
        }

        //! Displays the message on the middle of the gameview
        public void DisplayMessage(string message, bool autoclose, float closeafterseconds)
        {

            TextBox.text = message;
            if (autoclose)
            {
                Invoke("DestroyMessage", closeafterseconds);
            }
        }
    }
}
