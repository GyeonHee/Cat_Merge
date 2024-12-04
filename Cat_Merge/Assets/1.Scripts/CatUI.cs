using UnityEngine;
using UnityEngine.UI;

public class CatUI : MonoBehaviour
{
    [SerializeField] private Cat catData;           // ����� ������
    private Image catImage;                         // ����� �̹���

    private void Awake()
    {
        catImage = GetComponent<Image>();
    }

    private void Start()
    {
        UpdateCatUI();
    }

    public void UpdateCatUI()
    {
        catImage.sprite = catData.CatImage;
    }

    // Cat ������ ���� (GameObject�� Ȱ��ȭ�� �� �ܺο��� ȣ��� �� ����)
    public void SetCatData(Cat cat)
    {
        catData = cat;

        UpdateCatUI();
    }
}
