using UnityEngine;

// ������� ������ ��� ��ũ��Ʈ
[System.Serializable]
public class Cat
{


    #region Basic Properties

    private int catId;          // ����� �ε���
    public int CatId { get => catId; set => catId = value; }

    private string catName;     // ����� �̸�
    public string CatName { get => catName; set => catName = value; }

    private int catGrade;       // ����� ���
    public int CatGrade { get => catGrade; set => catGrade = value; }

    #endregion


    #region Combat Stats

    private int baseDamage;     // �⺻ ���ݷ� (�⺻�� 100%)
    public int BaseDamage { get => baseDamage; set => baseDamage = value; }

    private int baseHp;         // �⺻ ü��
    public int BaseHp { get => baseHp; set => baseHp = value; }

    private int growthDamage;   // ������ ���ݷ�
    public int GrowthDamage { get => growthDamage; set => growthDamage = value; }

    private int growthHp;       // ������ ü��
    public int GrowthHp { get => growthHp; set => growthHp = value; }

    private int catAttackSpeed;     // ����� ���ݼӵ�
    public int CatAttackSpeed { get => catAttackSpeed; set => catAttackSpeed = value; }

    private int catArmor;           // ����� ����
    public int CatArmor { get => catArmor; set => catArmor = value; }

    private int catMoveSpeed;       // ����� �̵��ӵ�
    public int CatMoveSpeed { get => catMoveSpeed; set => catMoveSpeed = value; }

    #endregion


    #region Passive Stats

    private float passiveAttackDamage = 1.0f;       // �нú� ���ݷ� ���� ���� (�⺻�� 1 = 100%)
    public float PassiveAttackDamage { get => passiveAttackDamage; set => passiveAttackDamage = value; }

    private float passiveCoinCollectSpeed = 0f;     // �нú� ��ȭ ���� �ӵ� ������ (�⺻�� 0��)
    public float PassiveCoinCollectSpeed { get => passiveCoinCollectSpeed; set => passiveCoinCollectSpeed = value; }

    private float passiveAttackSpeed = 0f;          // �нú� ���� �ӵ� ������ (�⺻�� 0��)
    public float PassiveAttackSpeed { get => passiveAttackSpeed; set => passiveAttackSpeed = value; }

    #endregion


    #region Calculated Stats

    // ���� ���� ���
    public int CatDamage => (int)((GrowthDamage * (BaseDamage * 0.01) + GrowthDamage) * PassiveAttackDamage);
    public int CatHp => (int)(GrowthHp * (BaseHp * 0.01)) + GrowthHp;

    #endregion


    #region Additional Properties

    private int catGetCoin;         // ����� �ڵ� ��ȭ ȹ�淮
    public int CatGetCoin { get => catGetCoin; set => catGetCoin = value; }

    private Sprite catImage;        // ����� �̹���
    public Sprite CatImage { get => catImage; set => catImage = value; }

    private string catExplain;      // ����� ����
    public string CatExplain { get => catExplain; set => catExplain = value; }

    private int canOpener;          // ����� ���� �رݰ���
    public int CanOpener { get => canOpener; set => canOpener = value; }

    private int catFirstOpenCash;   // ����� ù ȹ��� ��� ���̾�
    public int CatFirstOpenCash { get => catFirstOpenCash; set => catFirstOpenCash = value; }

    #endregion


    #region Constructor

    // ����� ��ü ������
    public Cat(int catId, string catName, int catGrade, int catDamage, int catGetCoin, int catHp, Sprite catImage,
        string catExplain, int catAttackSpeed, int catArmor, int catMoveSpeed, int canOpener, int catFirstOpenCash)
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
        CanOpener = canOpener;
        CatFirstOpenCash = catFirstOpenCash;
    }

    #endregion


    #region Growth Methods

    // ���� ���� ���� �Լ�
    public void GrowStat(int addDamage, int addHp)
    {
        GrowthDamage += addDamage;
        GrowthHp += addHp;
    }

    // ���� ���� �ʱ�ȭ �Լ�
    public void ResetGrowth()
    {
        GrowthDamage = 0;
        GrowthHp = 0;
    }

    #endregion


    #region Passive Buff Methods

    // �нú� ���ݷ� ���� �Լ�
    public void AddPassiveAttackDamageBuff(float percentage)
    {
        PassiveAttackDamage += percentage;
    }

    // �нú� ���ݷ� ���� �ʱ�ȭ �Լ�
    public void ResetPassiveAttackDamageBuff()
    {
        PassiveAttackDamage = 1f;
    }


    // �нú� ��ȭ ���� �ӵ� ���� �Լ�
    public void AddPassiveCoinCollectSpeedBuff(float seconds)
    {
        PassiveCoinCollectSpeed += seconds;
    }

    // �нú� ���ݷ� ���� �ʱ�ȭ �Լ�
    public void ResetPassiveCoinCollectSpeedBuff()
    {
        PassiveCoinCollectSpeed = 0f;
    }

    // �нú� ���� �ӵ� ���� �Լ�
    public void AddPassiveAttackSpeedBuff(float seconds)
    {
        PassiveAttackSpeed += seconds;
    }

    // �нú� ���� ���� �ʱ�ȭ �Լ�
    public void ResetPassiveAttackSpeedBuff()
    {
        PassiveAttackSpeed = 0f;
    }

    #endregion


}
