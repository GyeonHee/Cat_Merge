using UnityEngine;
using TMPro;

// GameManager Script
public class GameManager : MonoBehaviour
{
    // Data
    private Cat[] allCatData;                               // ��� ����� ������ ����
    public Cat[] AllCatData => allCatData;

    private int maxCats = 8;                                // ȭ�� �� �ִ� ����� ��
    private int currentCatCount = 0;                        // ȭ�� �� ����� ��

    // UI
    [SerializeField] private TextMeshProUGUI catCountText;  // ����� �� �ؽ�Ʈ

    private void Awake()
    {
        LoadAllCats();
        UpdateCatCountText();
    }

    // ����� ���� Load �Լ�
    private void LoadAllCats()
    {
        allCatData = Resources.LoadAll<Cat>("Cats");
    }

    // ����� �� �Ǻ� �Լ�
    public bool CanSpawnCat()
    {
        return currentCatCount < maxCats;
    }

    // ���� ����� �� ������Ű�� �Լ�
    public void AddCatCount()
    {
        if (currentCatCount < maxCats)
        {
            currentCatCount++;
            UpdateCatCountText();
        }
    }

    // ���� ����� �� ���ҽ�Ű�� �Լ�
    public void DeleteCatCount()
    {
        if (currentCatCount > 0)
        {
            currentCatCount--;
            UpdateCatCountText();
        }
    }

    // ����� �� �ؽ�Ʈ UI ������Ʈ�ϴ� �Լ�
    private void UpdateCatCountText()
    {
        if (catCountText != null)
        {
            catCountText.text = $"{currentCatCount} / {maxCats}";
        }
    }

}
