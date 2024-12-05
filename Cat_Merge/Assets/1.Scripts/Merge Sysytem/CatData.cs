using UnityEngine;
using UnityEngine.UI;

public class CatData : MonoBehaviour
{
    private Cat catData;                    // ����� ������
    private Image catImage;                 // ����� �̹���
    private CatDragAndDrop catDragAndDrop;  // CatDragAndDrop ����

    private void Awake()
    {
        catImage = GetComponent<Image>();
        catDragAndDrop = GetComponentInParent<CatDragAndDrop>();
    }

    private void Start()
    {
        UpdateCatUI();
    }

    public void UpdateCatUI()
    {
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
