using System.Collections.Generic;
using System.IO;
using UnityEngine;

// ����� �����͸� �ε��ϰ� �����ϴ� ��ũ��Ʈ
[DefaultExecutionOrder(-10)]
public class CatDataLoader : MonoBehaviour
{


    #region Variables

    
    public Dictionary<int, Cat> catDictionary = new Dictionary<int, Cat>();     // ����� �����͸� ������ Dictionary

    private readonly List<string> values = new List<string>();
    private readonly List<string> validValues = new List<string>();

    #endregion


    #region Unity Methods

    private void Awake()
    {
        LoadCatDataFromCSV();
    }

    #endregion


    #region Data Loading

    // CSV ������ �о� Cat ��ü�� ��ȯ �� Dictionary�� �����ϴ� �Լ�
    public void LoadCatDataFromCSV()
    {
        TextAsset csvFile = Resources.Load<TextAsset>("CatDB");
        if (csvFile == null)
        {
            //Debug.LogError("CSV ������ ������� �ʾҽ��ϴ�");
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

            // CSV �Ľ� ���� List �ʱ�ȭ (����)
            values.Clear();
            validValues.Clear();

            // CSV �Ľ� - ����ǥ ������ ��ǥ�� �����ϰ� ���� �����ڸ� ó��
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
                ParseAndAddCatData(validValues, lineNumber);
            }
            catch (System.Exception ex)
            {
                //Debug.LogError($"���� {lineNumber}: ������ ó�� �� ���� �߻� - {ex.Message}");
            }
        }
    }

    // �Ľ̵� �����ͷ� Cat ��ü ���� �� �߰��ϴ� �Լ�
    private void ParseAndAddCatData(List<string> data, int lineNumber)
    {
        int catId = int.Parse(data[0]);
        string catName = data[1];
        int catGrade = int.Parse(data[2]);
        int catDamage = int.Parse(data[3]);
        int catGetCoin = int.Parse(data[4]);
        int catHp = int.Parse(data[5]);
        Sprite catImage = LoadSprite(data[6]);
        string catExplain = data[7];
        int catAttackSpeed = int.Parse(data[8]);
        int catArmor = int.Parse(data[9]);
        int catMoveSpeed = int.Parse(data[10]);
        int canOpener = int.Parse(data[11]);
        int catFirstOpenCash = int.Parse(data[12]);

        // Cat ��ü ���� �� Dictionary�� �߰�
        Cat newCat = new Cat(catId, catName, catGrade, catDamage, catGetCoin, catHp, catImage, catExplain, catAttackSpeed, catArmor, catMoveSpeed, canOpener, catFirstOpenCash);
        if (!catDictionary.ContainsKey(catId))
        {
            catDictionary.Add(catId, newCat);
        }
        else
        {
            //Debug.LogWarning($"�ߺ��� CatId�� �߰ߵǾ����ϴ�: {catId}");
        }
    }

    // Resources �������� ��������Ʈ �ε��ϴ� �Լ�
    private Sprite LoadSprite(string path)
    {
        Sprite sprite = Resources.Load<Sprite>("Sprites/Cats/" + path);
        if (sprite == null)
        {
            //Debug.LogError($"�̹����� ã�� �� �����ϴ�: {path}");
        }
        return sprite;
    }

    #endregion


}
