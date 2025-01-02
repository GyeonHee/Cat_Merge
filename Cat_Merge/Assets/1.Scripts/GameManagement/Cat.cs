using UnityEngine;

// ������� ������ ��� Script
[System.Serializable]
public class Cat
{
    private int catId;           // ����� �ε���
    public int CatId { get => catId; set => catId = value; }

    private string catName;      // ����� �̸�
    public string CatName { get => catName; set => catName = value; }

    private int catGrade;        // ����� ���
    public int CatGrade { get => catGrade; set => catGrade = value; }

    private int catDamage;       // ����� ������
    public int CatDamage { get => catDamage; set => catDamage = value; }

    private int catGetCoin;      // ����� �ڵ� ��ȭ ȹ�淮
    public int CatGetCoin { get => catGetCoin; set => catGetCoin = value; }

    private int catHp;           // ����� ü��
    public int CatHp { get => catHp; set => catHp = value; }

    private Sprite catImage;     // ����� �̹���
    public Sprite CatImage { get => catImage; set => catImage = value; }

    private string catExplain;   // ����� ����
    public string CatExplain { get => catExplain; set => catExplain = value; }

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