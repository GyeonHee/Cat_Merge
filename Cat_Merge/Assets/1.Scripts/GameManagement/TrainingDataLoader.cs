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
                int catId = int.Parse(validValues[1]);
                int growthDamage = int.Parse(validValues[2]);
                int growthHp = int.Parse(validValues[3]);
                string extraAbilityName = validValues[4];                   // �߰� ȹ�� �ɷ�ġ �̸�
                double extraAbilityValue = double.Parse(validValues[5]);    // �߰� ȹ�� �ɷ�ġ ��ġ
                string extraAbilityUnit = validValues[6];                   // ����
                string extraAbilitySymbol = validValues[7];                 // ���� ��ȣ
                double trainingCoin = double.Parse(validValues[8]);
                double levelUpCoin = double.Parse(validValues[9]);

                // TrainingData ��ü ���� �� Dictionary�� �߰�
                TrainingData trainingData = new TrainingData(growthDamage, growthHp, trainingCoin, levelUpCoin,
                    extraAbilityName, extraAbilityValue, extraAbilityUnit, extraAbilitySymbol);
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

    private double trainingCoin;
    public double TrainingCoin { get => trainingCoin; set => trainingCoin = value; }

    private double levelUpCoin;
    public double LevelUpCoin { get => levelUpCoin; set => levelUpCoin = value; }

    // �߰� �ɷ�ġ ���� ������
    private string extraAbilityName;    // �߰� ȹ�� �ɷ�ġ �̸�
    public string ExtraAbilityName { get => extraAbilityName; set => extraAbilityName = value; }

    private double extraAbilityValue;   // �߰� ȹ�� �ɷ�ġ ��ġ
    public double ExtraAbilityValue { get => extraAbilityValue; set => extraAbilityValue = value; }

    private string extraAbilityUnit;    // ����
    public string ExtraAbilityUnit { get => extraAbilityUnit; set => extraAbilityUnit = value; }

    private string extraAbilitySymbol;  // ���� ��ȣ
    public string ExtraAbilitySymbol { get => extraAbilitySymbol; set => extraAbilitySymbol = value; }

    public TrainingData(int growthDamage, int growthHp, double trainingCoin, double levelUpCoin,
        string extraAbilityName, double extraAbilityValue, string extraAbilityUnit, string extraAbilitySymbol)
    {
        GrowthDamage = growthDamage;
        GrowthHp = growthHp;
        TrainingCoin = trainingCoin;
        LevelUpCoin = levelUpCoin;
        ExtraAbilityName = extraAbilityName;
        ExtraAbilityValue = extraAbilityValue;
        ExtraAbilityUnit = extraAbilityUnit;
        ExtraAbilitySymbol = extraAbilitySymbol;
    }
}