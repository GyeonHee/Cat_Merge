using System.Collections.Generic;
using System.IO;
using UnityEngine;

[DefaultExecutionOrder(-3)]  // CatDataLoader�� ���� ������ ����
public class TrainingDataLoader : MonoBehaviour
{
    // ����̺� �Ʒ� �����͸� ������ Dictionary (Key: CatId)
    public Dictionary<int, TrainingData> trainingDictionary = new Dictionary<int, TrainingData>();

    private void Awake()
    {
        LoadTrainingDataFromCSV();
    }

    // CSV ������ �о� TrainingData ��ü�� ��ȯ �� Dictionary�� ����
    private void LoadTrainingDataFromCSV()
    {
        TextAsset csvFile = Resources.Load<TextAsset>("TrainingDB");
        if (csvFile == null)
        {
            Debug.LogError("Training CSV ������ ������� �ʾҽ��ϴ�");
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
            if (lineNumber >= 5) continue;

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
                int growthDamage = int.Parse(validValues[2]);
                int growthHp = int.Parse(validValues[3]);

                // TrainingData ��ü ���� �� Dictionary�� �߰�
                TrainingData trainingData = new TrainingData(growthDamage, growthHp);
                if (!trainingDictionary.ContainsKey(catId))
                {
                    trainingDictionary.Add(catId, trainingData);
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
}

// �Ʒ� �����͸� ��� Ŭ����
public class TrainingData
{
    private int growthDamage;
    public int GrowthDamage { get => growthDamage; set => growthDamage = value; }

    private int growthHp;
    public int GrowthHp { get => growthHp; set => growthHp = value; }

    public TrainingData(int growthDamage, int growthHp)
    {
        GrowthDamage = growthDamage;
        GrowthHp = growthHp;
    }
}