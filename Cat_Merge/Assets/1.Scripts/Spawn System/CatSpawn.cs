using UnityEngine;

// ����� ���� ��ư ���� ��ũ��Ʈ
public class CatSpawn : MonoBehaviour
{
    [SerializeField] private GameObject catPrefab;      // ����� UI ������
    [SerializeField] private Transform catUIParent;     // ����̸� ��ġ�� �θ� Transform (UI Panel ��)
    private RectTransform panelRectTransform;           // Panel�� ũ�� ���� (��ġ�� ����)
    private GameManager gameManager;                    // GameManager

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        panelRectTransform = catUIParent.GetComponent<RectTransform>();
        if (panelRectTransform == null)
        {
            Debug.LogError("catUIParent�� RectTransform�� ������ ���� �ʽ��ϴ�.");
            return;
        }
    }

    // ����� ���� ��ư
    public void OnClickedSpawn()
    {
        if (gameManager.CanSpawnCat())
        {
            LoadAndDisplayCats(gameManager.AllCatData);
            gameManager.AddCatCount();
        }
        else
        {
            Debug.Log("����� �ִ� ���� ������ �����߽��ϴ�!");
        }
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

    // Panel�� ���� ��ġ ��ġ : ���� ���� ��� ����� ���� (���׷��̵� �ý��� ���� �� �ڵ� �Ϻ� ���� ����)
    private void LoadAndDisplayCats(Cat[] allCatData)
    {
        GameObject catUIObject = Instantiate(catPrefab, catUIParent);

        CatData catData = catUIObject.GetComponent<CatData>();
        catData.SetCatData(allCatData[0]);

        Vector2 randomPos = GetRandomPosition(panelRectTransform);
        catUIObject.GetComponent<RectTransform>().anchoredPosition = randomPos;
    }


}
