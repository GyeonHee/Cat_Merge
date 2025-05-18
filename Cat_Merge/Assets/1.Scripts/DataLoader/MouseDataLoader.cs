using System.Collections.Generic;
using System.IO;
using UnityEngine;

// �� �����͸� �ε��ϰ� �����ϴ� ��ũ��Ʈ
[DefaultExecutionOrder(-10)]
public class MouseDataLoader : MonoBehaviour
{


    #region Variables

    // �� �����͸� ������ Dictionary
    public Dictionary<int, Mouse> mouseDictionary = new Dictionary<int, Mouse>();

    private readonly List<string> validValues = new List<string>(15);

    #endregion


    #region Unity Methods

    private void Awake()
    {
        LoadMouseDataFromCSV();
    }

    #endregion


    #region Data Loading

    // CSV ������ �о� Mouse ��ü�� ��ȯ �� Dictionary�� �����ϴ� �Լ�
    public void LoadMouseDataFromCSV()
    {
        TextAsset csvFile = Resources.Load<TextAsset>("MouseDB");
        if (csvFile == null)
        {
            //Debug.LogError("CSV ������ ������� �ʾҽ��ϴ�");
            return;
        }

        using (StringReader stringReader = new StringReader(csvFile.text))
        {
            int lineNumber = 0;

            while (true)
            {
                string line = stringReader.ReadLine();
                if (line == null) break;

                lineNumber++;
                if (lineNumber <= 1) continue;

                string[] values = line.Split(',');

                // �� ĭ�� �߰��ϸ� �ű������ ó��
                validValues.Clear();
                foreach (string value in values)
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        break;
                    }
                    validValues.Add(value);
                }

                try
                {
                    ParseAndAddMouse(validValues, lineNumber);
                }
                catch (System.Exception ex)
                {
                    //Debug.LogError($"���� {lineNumber}: ������ ó�� �� ���� �߻� - {ex.Message}");
                }
            }
        }
    }

    // ���콺 �����͸� �Ľ��ϰ� Dictionary�� �߰��ϴ� �Լ�
    private void ParseAndAddMouse(List<string> values, int lineNumber)
    {
        int mouseId = int.Parse(values[0]);
        string mouseName = values[1];
        int mouseGrade = int.Parse(values[2]);
        double mouseDamage = double.Parse(values[3]);
        double mouseHp = double.Parse(values[4]);
        Sprite mouseImage = LoadSprite(values[5]);
        int numOfAttack = int.Parse(values[6]);
        int mouseAttackSpeed = int.Parse(values[7]);
        int mouseArmor = int.Parse(values[8]);
        int clearCashReward = int.Parse(values[9]);
        decimal clearCoinReward = decimal.Parse(values[10]);
        decimal repeatclearCoinReward = decimal.Parse(values[11]);

        Mouse newMouse = new Mouse(mouseId, mouseName, mouseGrade, mouseDamage, mouseHp, mouseImage,
                                 numOfAttack, mouseAttackSpeed, mouseArmor,
                                 clearCashReward, clearCoinReward, repeatclearCoinReward);

        if (!mouseDictionary.ContainsKey(mouseId))
        {
            mouseDictionary.Add(mouseId, newMouse);
        }
        else
        {
            //Debug.LogWarning($"�ߺ��� MouseId�� �߰ߵǾ����ϴ�: {mouseId}");
        }
    }

    #endregion


    #region Resource Loading

    // Resources �������� ��������Ʈ �ε��ϴ� �Լ�
    private Sprite LoadSprite(string path)
    {
        Sprite sprite = Resources.Load<Sprite>("Sprites/Mouses/" + path);
        if (sprite == null)
        {
            //Debug.LogError($"�̹����� ã�� �� �����ϴ�: {path}");
        }
        return sprite;
    }

    #endregion


}
