using UnityEngine;
using TMPro;

// GameManager
public class GameManager : MonoBehaviour
{
    // Data
    private Cat[] allCatData;                   // ��� ����� ������ ����
    public Cat[] AllCatData => allCatData;

    private int maxCats = 8;                    // ȭ�� �� �ִ� ����� ����
    private int currentCatCount = 0;            // ȭ�� �� ����� ����

    // UI
    [SerializeField] private TextMeshProUGUI catCountText;

    private void Awake()
    {
        LoadAllCats();
        UpdateCatCountText();
    }

    // ����� ���� Load
    private void LoadAllCats()
    {
        allCatData = Resources.LoadAll<Cat>("Cats");
    }

    // ����� �� �Ǻ�
    public bool CanSpawnCat()
    {
        return currentCatCount < maxCats;
    }

    // ���� ����� �� ����
    public void AddCatCount()
    {
        if (currentCatCount < maxCats)
        {
            currentCatCount++;
            UpdateCatCountText();
        }
    }

    // ���� ����� �� ����
    public void DeleteCatCount()
    {
        if (currentCatCount > 0)
        {
            currentCatCount--;
            UpdateCatCountText();
        }
    }

    // UI �ؽ�Ʈ ������Ʈ
    private void UpdateCatCountText()
    {
        if (catCountText != null)
        {
            catCountText.text = $"{currentCatCount} / {maxCats}";
        }
    }

}
