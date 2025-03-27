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



    private int baseDamage;      // �⺻ ���ݷ�
    public int BaseDamage { get => baseDamage; set => baseDamage = value; }

    private int baseHp;          // �⺻ ü��
    public int BaseHp { get => baseHp; set => baseHp = value; }

    private int growthDamage;    // ������ ���ݷ�
    public int GrowthDamage { get => growthDamage; set => growthDamage = value; }

    private int growthHp;        // ������ ü��
    public int GrowthHp { get => growthHp; set => growthHp = value; }

    // ���� ���� ���� ������Ƽ
    public int CatDamage => (int)(GrowthDamage * (BaseDamage * 0.01)) + GrowthDamage; 
    public int CatHp => (int)(GrowthHp * (BaseHp * 0.01)) + GrowthHp;



    private int catGetCoin;      // ����� �ڵ� ��ȭ ȹ�淮
    public int CatGetCoin { get => catGetCoin; set => catGetCoin = value; }

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
        BaseDamage = catDamage;
        BaseHp = catHp;
        GrowthDamage = 0;
        GrowthHp = 0;
        CatGetCoin = catGetCoin;
        CatImage = catImage;
        CatExplain = catExplain;
        CatAttackSpeed = catAttackSpeed;
        CatArmor = catArmor;
        CatMoveSpeed = catMoveSpeed;
    }

    // ���� ���� �޼���
    public void GrowStat(int addDamage, int addHp)
    {
        GrowthDamage += addDamage;
        GrowthHp += addHp;
    }

    // ���� �ʱ�ȭ �޼���
    public void ResetGrowth()
    {
        GrowthDamage = 0;
        GrowthHp = 0;
    }

}
