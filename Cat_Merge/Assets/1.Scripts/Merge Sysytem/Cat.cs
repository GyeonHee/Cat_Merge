using UnityEngine;

// Cat Scriptable Object
[CreateAssetMenu(fileName = "CatData", menuName = "ScriptableObjects/Cat", order = 1)]
public class Cat : ScriptableObject
{
    // ����� ���� ��ȣ
    [SerializeField] private int catId;
    public int CatId { get => catId; set => catId = value; }

    // ����� �̸�
    [SerializeField] private string catName;
    public string CatName { get => catName; set => catName = value; }

    // ����� ������ (�ӽ÷� catID * 50)
    [SerializeField] private int catDamage { get => catId * 50; }
    public int CatDamage { get => catDamage; }

    // ����� �̹���
    [SerializeField] private Sprite catImage;
    public Sprite CatImage { get => catImage; set => catImage = value; }
    
}
