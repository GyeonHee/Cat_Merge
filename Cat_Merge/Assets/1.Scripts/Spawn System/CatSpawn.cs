using System.Collections;
using UnityEngine;

public class CatSpawn : MonoBehaviour
{
    [SerializeField] private GameObject catPrefab;      // ����� UI ������
    [SerializeField] private Transform catUIParent;     // ����̸� ��ġ�� �θ� Transform (UI Panel ��)
    RectTransform panelRectTransform;                   // Panel�� ũ�� ���� (��ġ�� ����)

    // ���� ��ư ���� �� ����
    public void Spawn()
    {
        // ����� �����͸� ��� �־ ������
        CatMerge catMerge = FindObjectOfType<CatMerge>();
        panelRectTransform = catUIParent.GetComponent<RectTransform>();
        if (panelRectTransform == null)
        {
            Debug.LogError("catUIParent�� RectTransform�� ������ ���� �ʽ��ϴ�.");
            return;
        }

        LoadAndDisplayCats(catMerge.AllCatData);
    }

    // Panel�� ���� ��ġ ���
    Vector3 GetRandomPosition(RectTransform panelRectTransform)
    {
        float panelWidth = panelRectTransform.rect.width;
        float panelHeight = panelRectTransform.rect.height;

        float randomX = Random.Range(-panelWidth / 2, panelWidth / 2);
        float randomY = Random.Range(-panelHeight / 2, panelHeight / 2);

        Vector3 respawnPos = new Vector3(randomX, randomY, 0f);
        return respawnPos;
    }

    private void LoadAndDisplayCats(Cat[] allCatData)
    {
        // Panel�� ũ�� ���� (��ġ�� ����)
        RectTransform panelRectTransform = catUIParent.GetComponent<RectTransform>();
        if (panelRectTransform == null)
        {
            Debug.LogError("catUIParent�� RectTransform�� ������ ���� �ʽ��ϴ�.");
            return;
        }

        for (int i = 0; i < 1; i++)
        {
            GameObject catUIObject = Instantiate(catPrefab, catUIParent);

            CatData catData = catUIObject.GetComponent<CatData>();
            catData.SetCatData(allCatData[0]);

            Vector2 randomPos = GetRandomPosition(panelRectTransform);
            catUIObject.GetComponent<RectTransform>().anchoredPosition = randomPos;
        }
    }
}
