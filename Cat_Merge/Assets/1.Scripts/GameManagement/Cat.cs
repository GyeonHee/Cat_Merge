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

    private int catAttackSpeed;  // ����� ���ݼӵ�
    public int CatAttackSpeed { get => catAttackSpeed; set => catAttackSpeed = value; }

    private int catArmor;        // ����� ����
    public int CatArmor { get => catArmor; set => catArmor = value; }

    private int catMoveSpeed;    // ����� �̵��ӵ�
    public int CatMoveSpeed { get => catMoveSpeed; set => catMoveSpeed = value; }

    public Cat(int catId, string catName, int catGrade, int catDamage, int catGetCoin, int catHp, Sprite catImage, 
        string catExplain, int catAttackSpeed, int catArmor, int catMoveSpeed)
    {
        CatId = catId;
        CatName = catName;
        CatGrade = catGrade;
        CatDamage = catDamage;
        CatGetCoin = catGetCoin;
        CatHp = catHp;
        CatImage = catImage;
        CatExplain = catExplain;
        CatAttackSpeed = catAttackSpeed;
        CatArmor = catArmor;
        CatMoveSpeed = catMoveSpeed;
    }
}