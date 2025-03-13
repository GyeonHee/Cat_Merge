using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    private void Start()
    {
        //// 60������ ����
        //Application.targetFrameRate = 60;
    }

    // ���� ����
    public void GameStart()
    {
        if (GoogleManager.Instance != null)
        {
            // �ε� ȭ�� ǥ�� ��û
            GoogleManager.Instance.ShowLoadingScreen(true);
        }

        // ���� �� �ε�
        SceneManager.LoadScene("GameScene-Han");
    }

}
