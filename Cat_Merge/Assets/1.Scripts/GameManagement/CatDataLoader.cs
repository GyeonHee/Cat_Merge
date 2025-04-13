using System.Collections.Generic;
using System.IO;
using UnityEngine;

// CatDataLoader Script
[DefaultExecutionOrder(-6)]
public class CatDataLoader : MonoBehaviour
{
    // ����� �����͸� ������ Dictionary
    public Dictionary<int, Cat> catDictionary = new Dictionary<int, Cat>();

    // ======================================================================================================================

    private void Awake()
    {
        LoadCatDataFromCSV();
    }

    // ======================================================================================================================

    // CSV ������ �о� Cat ��ü�� ��ȯ �� Dictionary�� ����
    public void LoadCatDataFromCSV()
    {
        TextAsset csvFile = Resources.Load<TextAsset>("CatDB");
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
            if (lineNumber >= 32) continue;

            // CSV �Ľ� - ����ǥ ������ ��ǥ�� �����ϰ� ���� �����ڸ� ó��
            List<string> values = new List<string>();
            bool insideQuotes = false;
            int startIndex = 0;

            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == '"')
                {
                    insideQuotes = !insideQuotes;
                }
                else if (line[i] == ',' && !insideQuotes)
                {
                    string value = line.Substring(startIndex, i - startIndex).Trim();
                    value = value.Trim('"');
                    values.Add(value);
                    startIndex = i + 1;
                }
            }
            // ������ �� �߰�
            string lastValue = line.Substring(startIndex).Trim();
            lastValue = lastValue.Trim('"');
            values.Add(lastValue);

            // �� ĭ�� �߰��ϸ� �ű������ ó��
            List<string> validValues = new List<string>();
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
                // ������ �Ľ�
                int catId = int.Parse(validValues[0]);
                string catName = validValues[1];
                int catGrade = int.Parse(validValues[2]);
                int catDamage = int.Parse(validValues[3]);
                int catGetCoin = int.Parse(validValues[4]);
                int catHp = int.Parse(validValues[5]);
                Sprite catImage = LoadSprite(validValues[6]);
                string catExplain = validValues[7];
                int catAttackSpeed = int.Parse(validValues[8]);
                int catArmor = int.Parse(validValues[9]);
                int catMoveSpeed = int.Parse(validValues[10]);

                // Cat ��ü ���� �� Dictionary�� �߰�
                Cat newCat = new Cat(catId, catName, catGrade, catDamage, catGetCoin, catHp, catImage, catExplain, catAttackSpeed, catArmor, catMoveSpeed);
                if (!catDictionary.ContainsKey(catId))
                {
                    catDictionary.Add(catId, newCat);
                }
                else
                {
                    Debug.LogWarning($"�ߺ��� CatId�� �߰ߵǾ����ϴ�: {catId}");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"���� {lineNumber}: ������ ó�� �� ���� �߻� - {ex.Message}");
            }
        }
    }

    // Resources �������� ��������Ʈ �ε�
    private Sprite LoadSprite(string path)
    {
        Sprite sprite = Resources.Load<Sprite>("Sprites/Cats/" + path);
        if (sprite == null)
        {
            Debug.LogError($"�̹����� ã�� �� �����ϴ�: {path}");
        }
        return sprite;
    }
}
