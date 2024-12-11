
using UnityEngine;
using UnityEngine.UI;


namespace RDTS
{
    //! Displays a message box with a text field in the middle of the gameview
    //����Ϸ��ͼ�м���ʾһ�������ı��ֶε���Ϣ��
    //��ݼ��л�����ӽ�ʱ����ʾ���Զ��رյģ���Ϣ��
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
