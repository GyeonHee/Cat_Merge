using System.Collections.Generic;
using System.IO;
using UnityEngine;

// CatDataLoader Script
[DefaultExecutionOrder(-6)]
public class MouseDataLoader : MonoBehaviour
{
    // �� �����͸� ������ Dictionary
    public Dictionary<int, Mouse> mouseDictionary = new Dictionary<int, Mouse>();

    // ======================================================================================================================

    private void Awake()
    {
        LoadMouseDataFromCSV();
    }

    // ======================================================================================================================

    // CSV ������ �о� Mouse ��ü�� ��ȯ �� Dictionary�� ����
    public void LoadMouseDataFromCSV()
    {
        TextAsset csvFile = Resources.Load<TextAsset>("MouseDB");
        if (csvFile == null)
        {
            Debug.LogError("CSV ������ ������� �ʾҽ��ϴ�");
            return;
        }

        StringReader stringReader = new StringReader(csvFile.text);
        int lineNumber = 0;

        while (true)
        {
            string line = stringReader.ReadLine();
            if (line == null) break;

            lineNumber++;
            if (lineNumber <= 1) continue;

            string[] values = line.Split(',');

            // �� ĭ�� �߰��ϸ� �ű������ ó��
            List<string> validValues = new List<string>();
            foreach (string value in values)
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    //Debug.Log($"���� {lineNumber}: �� ĭ �߰� - �ش� ĭ ���� �����͸� �����մϴ�.");
                    break;
                }
                validValues.Add(value);
            }

            try
            {
                // ������ �Ľ�
                int mouseId = int.Parse(validValues[0]);
                string mouseName = validValues[1];
                int mouseGrade = int.Parse(validValues[2]);
                double mouseDamage = double.Parse(validValues[3]);
                double mouseHp = double.Parse(validValues[4]);
                Sprite mouseImage = LoadSprite(validValues[5]);
                int numOfAttack = int.Parse(validValues[6]);
                int mouseAttackSpeed = int.Parse(validValues[7]);
                int mouseArmor = int.Parse(validValues[8]);

                // Mouse ��ü ���� �� Dictionary�� �߰�
                Mouse newMouse = new Mouse(mouseId, mouseName, mouseGrade, mouseDamage, mouseHp, mouseImage, numOfAttack, mouseAttackSpeed, mouseArmor);
                if (!mouseDictionary.ContainsKey(mouseId))
                {
                    mouseDictionary.Add(mouseId, newMouse);
                }
                else
                {
                    Debug.LogWarning($"�ߺ��� MouseId�� �߰ߵǾ����ϴ�: {mouseId}");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"���� {lineNumber}: ������ ó�� �� ���� �߻� - {ex.Message}");
            }
        }

        //Debug.Log("�� ������ �ε� �Ϸ�: " + mouseDictionary.Count + "��");
    }

    // Resources �������� ��������Ʈ �ε�
    private Sprite LoadSprite(string path)
    {
        Sprite sprite = Resources.Load<Sprite>("Sprites/Mouses/" + path);
        if (sprite == null)
        {
            Debug.LogError($"�̹����� ã�� �� �����ϴ�: {path}");
        }
        return sprite;
    }
}
