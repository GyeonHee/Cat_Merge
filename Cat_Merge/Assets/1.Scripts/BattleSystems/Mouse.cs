using UnityEngine;

// ���� ������ ��� Script
[System.Serializable]
public class Mouse : MonoBehaviour
{
    private int mouseId;                // �� �ε���
    public int MouseId { get => mouseId; set => mouseId = value; }

    private string mouseName;           // �� �̸�
    public string MouseName { get => mouseName; set => mouseName = value; }

    private int mouseGrade;             // �� ��� (��������)
    public int MouseGrade { get => mouseGrade; set => mouseGrade = value; }

    private int mouseDamage;            // �� ������
    public int MouseDamage { get => mouseDamage; set => mouseDamage = value; }

    private float mouseHp;              // �� ü��
    public float MouseHp { get => mouseHp; set => mouseHp = value; }

    private Sprite mouseImage;          // �� �̹���
    public Sprite MouseImage { get => mouseImage; set => mouseImage = value; }

    private int numOfAttack;            // ���� Ÿ�� ��
    public int NumOfAttack { get => numOfAttack; set => numOfAttack = value; }

    private int mouseAttackSpeed;       // �� ���ݼӵ�
    public int MouseAttackSpeed { get => mouseAttackSpeed; set => mouseAttackSpeed = value; }

    private int mouseArmor;             // �� ����
    public int MouseArmor { get => mouseArmor; set => mouseArmor = value; }

    public Mouse(int mouseId, string mouseName, int mouseGrade, int mouseDamage, float mouseHp, Sprite mouseImage, 
        int numOfAttack, int mouseAttackSpeed, int mouseArmor)
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
    }

}
