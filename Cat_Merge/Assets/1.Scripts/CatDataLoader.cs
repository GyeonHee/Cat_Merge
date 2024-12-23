using System.Collections.Generic;
using System.IO;
using UnityEngine;

// CatDataLoader Script
[DefaultExecutionOrder(-2)]     // ��ũ��Ʈ ���� ���� ����
public class CatDataLoader : MonoBehaviour
{
    // ����� �����͸� ������ Dictionary
    public Dictionary<int, Cat> catDictionary = new Dictionary<int, Cat>();

    // ======================================================================================================================

    private void Awake()
    {
        LoadCatDataFromCSV();

        // catDataDictionary.TryGetValue(1, out Cat cat) : 1�� �ε����� Cat ������ �������� �ڵ�
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
            if (lineNumber <= 2) continue;
            if (lineNumber >= 6) continue;

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
                int catId = int.Parse(validValues[0]);
                string catName = validValues[1];
                int catGrade = int.Parse(validValues[2]);
                int catDamage = int.Parse(validValues[3]);
                int catGetCoin = int.Parse(validValues[4]);
                int catHp = int.Parse(validValues[5]);
                Sprite catImage = LoadSprite(validValues[6]);
                string catExplain = validValues[7];

                // Cat ��ü ���� �� Dictionary�� �߰�
                Cat newCat = new Cat(catId, catName, catGrade, catDamage, catGetCoin, catHp, catImage, catExplain);
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

        //Debug.Log("����� ������ �ε� �Ϸ�: " + catDictionary.Count + "��");
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
