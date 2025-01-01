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

    private int mouseHp;                // �� ü��
    public int MouseHp { get => mouseHp; set => mouseHp = value; }

    private Sprite mouseImage;          // �� �̹���
    public Sprite MouseImage { get => mouseImage; set => mouseImage = value; }

    private int numOfAttack;            // ���� Ÿ�� ��
    public int NumOfAttack { get => numOfAttack; set => numOfAttack = value; }

    public Mouse(int mouseId, string mouseName, int mouseGrade, int mouseDamage, int mouseHp, Sprite mouseImage, int numOfAttack)
    {
        MouseId = mouseId;
        MouseName = mouseName;
        MouseGrade = mouseGrade;
        MouseDamage = mouseDamage;
        MouseHp = mouseHp;
        MouseImage = mouseImage;
        NumOfAttack = numOfAttack;
    }

}
