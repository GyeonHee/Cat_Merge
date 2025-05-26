using System.Collections.Generic;
using System.IO;
using UnityEngine;

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

// ü�´ܷ� �����͸� �ε��ϰ� �����ϴ� ��ũ��Ʈ
[DefaultExecutionOrder(-10)]
public class TrainingDataLoader : MonoBehaviour
{


    #region Variables

    // ����̺� �Ʒ� �����͸� ������ Dictionary (Key: CatId)
    public Dictionary<int, TrainingData> trainingDictionary = new Dictionary<int, TrainingData>();

    private readonly List<string> values = new List<string>(20);
    private readonly List<string> validValues = new List<string>(20);

    #endregion


    #region Unity Methods

    private void Awake()
    {
        LoadTrainingDataFromCSV();
    }

    #endregion


    #region Data Loading

    // CSV ������ �о� TrainingData ��ü�� ��ȯ �� Dictionary�� �����ϴ� �Լ�
    private void LoadTrainingDataFromCSV()
    {
        TextAsset csvFile = Resources.Load<TextAsset>("TrainingDB");
        if (csvFile == null)
        {
            //Debug.LogError("Training CSV ������ ������� �ʾҽ��ϴ�");
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

                ParseCSVLine(line, values);

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
                    ParseAndAddTrainingData(validValues, lineNumber);
                }
                catch (System.Exception ex)
                {
                    //Debug.LogError($"���� {lineNumber}: ������ ó�� �� ���� �߻� - {ex.Message}");
                }
            }
        }
    }

    // CSV ������ �Ľ��Ͽ� values ����Ʈ�� �����ϴ� �Լ�
    private void ParseCSVLine(string line, List<string> values)
    {
        values.Clear();
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

        string lastValue = line.Substring(startIndex).Trim();
        lastValue = lastValue.Trim('"');
        values.Add(lastValue);
    }

    // �Ľ̵� �����͸� TrainingData ��ü�� ��ȯ�Ͽ� Dictionary�� �߰��ϴ� �Լ�
    private void ParseAndAddTrainingData(List<string> values, int lineNumber)
    {
        int catId = int.Parse(values[1]);
        int growthDamage = int.Parse(values[2]);
        int growthHp = int.Parse(values[3]);
        string extraAbilityName = values[4];
        double extraAbilityValue = double.Parse(values[5]);
        string extraAbilityUnit = values[6];
        string extraAbilitySymbol = values[7];
        double trainingCoin = double.Parse(values[8]);
        double levelUpCoin = double.Parse(values[9]);

        TrainingData trainingData = new TrainingData(growthDamage, growthHp, trainingCoin, levelUpCoin,
            extraAbilityName, extraAbilityValue, extraAbilityUnit, extraAbilitySymbol);

        if (!trainingDictionary.ContainsKey(catId))
        {
            trainingDictionary.Add(catId, trainingData);
        }
        else
        {
            //Debug.LogWarning($"�ߺ��� CatId�� �߰ߵǾ����ϴ�: {catId}");
        }
    }

    #endregion


}
