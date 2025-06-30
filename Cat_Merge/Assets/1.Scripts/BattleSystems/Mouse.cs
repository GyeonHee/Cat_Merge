using UnityEngine;

// ��(����)�� ������ ��� ��ũ��Ʈ
[System.Serializable]
public class Mouse
{


    #region Mouse Stats

    private int mouseId;                // �� �ε���
    public int MouseId { get => mouseId; set => mouseId = value; }

    private string mouseName;           // �� �̸�
    public string MouseName { get => mouseName; set => mouseName = value; }

    private int mouseGrade;             // �� ��� (��������)
    public int MouseGrade { get => mouseGrade; set => mouseGrade = value; }

    private double mouseDamage;         // �� ������
    public double MouseDamage { get => mouseDamage; set => mouseDamage = value; }

    private double mouseHp;             // �� ü��
    public double MouseHp { get => mouseHp; set => mouseHp = value; }

    private Sprite mouseImage;          // �� �̹���
    public Sprite MouseImage { get => mouseImage; set => mouseImage = value; }

    private int numOfAttack;            // ���� Ÿ�� ��
    public int NumOfAttack { get => numOfAttack; set => numOfAttack = value; }

    private int mouseAttackSpeed;       // �� ���ݼӵ�
    public int MouseAttackSpeed { get => mouseAttackSpeed; set => mouseAttackSpeed = value; }

    private int mouseArmor;             // �� ����
    public int MouseArmor { get => mouseArmor; set => mouseArmor = value; }

    #endregion


    #region Rewards

    private int clearCashReward;            // ù Ŭ���� ĳ�� ����
    public int ClearCashReward { get => clearCashReward; set => clearCashReward = value; }

    private decimal clearCoinReward;        // ù Ŭ���� ���� ����
    public decimal ClearCoinReward { get => clearCoinReward; set => clearCoinReward = value; }

    private decimal repeatclearCoinReward;  // �ݺ� Ŭ���� ���� ����
    public decimal RepeatclearCoinReward { get => repeatclearCoinReward; set => repeatclearCoinReward = value; }

    #endregion


    #region Constructor

    // �� ������ �ʱ�ȭ ������
    public Mouse(int mouseId, string mouseName, int mouseGrade, double mouseDamage, double mouseHp,
                Sprite mouseImage, int numOfAttack, int mouseAttackSpeed, int mouseArmor,
                int clearCashReward, decimal clearCoinReward, decimal repeatclearCoinReward)
    {
        MouseId = mouseId;
        MouseName = mouseName;
        MouseGrade = mouseGrade;
        MouseDamage = mouseDamage;
        MouseHp = mouseHp;
        MouseImage = mouseImage;
        NumOfAttack = numOfAttack;
        MouseAttackSpeed = mouseAttackSpeed;
        MouseArmor = mouseArmor;
        ClearCashReward = clearCashReward;
        ClearCoinReward = clearCoinReward;
        RepeatclearCoinReward = repeatclearCoinReward;
    }

    #endregion


}
