using UnityEngine;

public class CatManager : MonoBehaviour
{
    [SerializeField] private GameObject catPrefab;      // ����� UI ������
    [SerializeField] private Transform catUIParent;     // ����̸� ��ġ�� �θ� Transform (UI Panel ��)
    private int catCount = 10;                          // 

    private void Start()
    {
        // CatMerge ��ũ��Ʈ�� ã�� allCatData�� ������
        CatMerge catMerge = FindObjectOfType<CatMerge>();
        if (catMerge != null)
        {
            LoadAndDisplayCats(catMerge.AllCatData);
        }
        else
        {
            Debug.LogError("CatMerge�� ã�� �� �����ϴ�.");
        }
    }

    private void LoadAndDisplayCats(Cat[] allCatData)
    {
        // Panel�� RectTransform�� ������ (��ġ�� ����)
        RectTransform panelRectTransform = catUIParent.GetComponent<RectTransform>();
        if (panelRectTransform == null)
        {
            Debug.LogError("catUIParent�� RectTransform�� ������ ���� �ʽ��ϴ�.");
            return;
        }

        for (int i = 0; i < catCount; i++)
        {
            GameObject catUIObject = Instantiate(catPrefab, catUIParent);

            CatData catData = catUIObject.GetComponent<CatData>();
            catData.SetCatData(allCatData[0]);

            Vector2 randomPos = GetRandomPosition(panelRectTransform);
            catUIObject.GetComponent<RectTransform>().anchoredPosition = randomPos;
        }
    }

    // ���� ��ġ ��� �Լ� (Panel ������)
    private Vector2 GetRandomPosition(RectTransform panelRectTransform)
    {
        // Panel�� �ʺ�� ���̸� ����
        float panelWidth = panelRectTransform.rect.width;
        float panelHeight = panelRectTransform.rect.height;

        // ������ X, Y ��ǥ�� ��� (Panel�� ���� ������)
        float randomX = Random.Range(-panelWidth / 2, panelWidth / 2);
        float randomY = Random.Range(-panelHeight / 2, panelHeight / 2);

        return new Vector2(randomX, randomY);
    }

}
