using UnityEngine;
using UnityEngine.UI;

// ����� ���ε��� ����
public class CatData : MonoBehaviour
{
    private Cat catData;                        // ����� ������
    private Image catImage;                     // ����� �̹���

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
        CatDragAndDrop catDragAndDrop = GetComponentInParent<CatDragAndDrop>();

        if (catDragAndDrop != null)
        {
            catDragAndDrop.catData = catData;
        }
        catImage.sprite = catData.CatImage;
    }

    // Cat ������ ����
    public void SetCatData(Cat cat)
    {
        catData = cat;

        UpdateCatUI();
    }


}
