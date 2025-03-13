using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    // ���� ����
    public void GameStart()
    {
        // ���� ���� �� ������ ���� (Ȥ�� �� ��츦 ���)
        if (GoogleManager.Instance != null)
        {
            GoogleManager.Instance.SaveGameState();

            // �ε� ȭ�� ǥ�� ��û
            GoogleManager.Instance.ShowLoadingScreen(true);
        }

        // ���� �� �ε�
        SceneManager.LoadScene("GameScene-Han");
    }

}
