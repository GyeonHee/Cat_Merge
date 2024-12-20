using System.Collections.Generic;
using System.IO;
using UnityEngine;

// catDataDictionary.TryGetValue(1, out Cat cat) : 1�� �ε����� Cat ������ �������� �ڵ�
// CatDataLoader Script
[DefaultExecutionOrder(-1)]     // ��ũ��Ʈ ���� ���� ����
public class CatDataLoader : MonoBehaviour
{
    // ����� �����͸� ������ Dictionary
    public Dictionary<int, Cat> catDictionary = new Dictionary<int, Cat>();

    private void Awake()
    {
        LoadCatDataFromCSV();
    }
    
    // CSV ������ �о� Cat ��ü�� ��ȯ �� Dictionary�� ����
    public void LoadCatDataFromCSV()
    {
        TextAsset csvFile = Resources.Load<TextAsset>("CatDB");
        if (csvFile == null)
        {
            Debug.LogError("CSV ������ ������� �ʾҽ��ϴ�!");
            return;
        }

        StringReader sr = new StringReader(csvFile.text);
        int lineNumber = 0;

        while (true)
        {
            string line = sr.ReadLine();
            if (line == null) break;

            lineNumber++;
            if (lineNumber <= 2) continue;
            if (lineNumber >= 6) continue;

            string[] values = line.Split(',');

            // ������ �Ľ�
            int catId = int.Parse(values[0]);
            string catName = values[1];
            int catGrade = int.Parse(values[2]);
            int catDamage = int.Parse(values[3]);
            int catGetCoin = int.Parse(values[4]);
            int catHp = int.Parse(values[5]);
            Sprite catImage = LoadSprite(values[6]);
            string catExplain = values[7];

            // Cat ��ü ���� �� Dictionary�� �߰�
            Cat newCat = new Cat(catId, catName, catGrade, catDamage, catGetCoin, catHp, catImage, catExplain);
            if (!catDictionary.ContainsKey(catId))
            {
                catDictionary.Add(catId, newCat);
                Debug.Log(newCat.CatId + ", " + newCat.CatName);
            }
            else
            {
                Debug.LogWarning($"�ߺ��� CatId�� �߰ߵǾ����ϴ�: {catId}");
            }
        }

        Debug.Log("����� ������ �ε� �Ϸ�: " + catDictionary.Count + "��");
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
