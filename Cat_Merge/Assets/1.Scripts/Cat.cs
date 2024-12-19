using UnityEngine;

//// Cat Scriptable Object Script
//[CreateAssetMenu(fileName = "CatData", menuName = "ScriptableObjects/Cat", order = 1)]
//public class Cat : ScriptableObject
//{
//    // ����� ���� ��ȣ
//    [SerializeField] private int catId;
//    public int CatId { get => catId; set => catId = value; }

//    // ����� �̸�
//    [SerializeField] private string catName;
//    public string CatName { get => catName; set => catName = value; }

//    // ����� �̹���
//    [SerializeField] private Sprite catImage;
//    public Sprite CatImage { get => catImage; set => catImage = value; }

//    // ����� ����
//    [TextArea(3,10)]
//    [SerializeField] private string catExplain;
//    public string CatExplain { get => catExplain; set => catExplain = value; }

//    // ����� ȹ�� ��ȭ
//    [SerializeField] private int catGetCoin;
//    public int CatGetCoin { get => catGetCoin; set => catGetCoin = value; }

//}

[System.Serializable]
public class Cat
{
    public int CatId;
    public string CatName;
    public int CatGrade;
    public int CatDamage;
    public int CatGetCoin;
    public int CatHp;
    public Sprite CatImage;
    public string CatExplain;
    

    public Cat(int catId, string catName, int catGrade, int catDamage, int catGetCoin, int catHp, Sprite catImage, string catExplain)
    {
        CatId = catId;
        CatName = catName;
        CatGrade = catGrade;
        CatDamage = catDamage;
        CatGetCoin = catGetCoin;
        CatHp = catHp;
        CatImage = catImage;
        CatExplain = catExplain;
    }
}