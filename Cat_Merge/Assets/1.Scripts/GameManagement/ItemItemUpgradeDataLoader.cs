using System.Collections.Generic;
using System.IO;
using UnityEngine;

// ItemItemUpgradeDataLoader Script
[DefaultExecutionOrder(-2)]     // ��ũ��Ʈ ���� ���� ���� (2��°)
public class ItemItemUpgradeDataLoader : MonoBehaviour
{
    public static ItemItemUpgradeDataLoader Instance { get; private set; }

    // ����� �����͸� ������ Dictionary
    public Dictionary<int, List<(string title, int type, int step, float value, float fee)>> dataByNumber = new Dictionary<int, List<(string title, int type, int step, float value, float fee)>>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        ParseCSV("Item_Item_UpgradeDB");
    }

    public void ParseCSV(string fileName)
    {
        // Resources �������� ���� �б�
        TextAsset csvFile = Resources.Load<TextAsset>(fileName);

        if (csvFile == null)
        {
            Debug.LogError($"CSV file '{fileName}' not found in Resources folder!");
            return;
        }

        // ���� ���� �б�
        string[] lines = csvFile.text.Split('\n');

        // ù ��° ��(���) �����ϰ� ������ �Ľ�
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] values = line.Split(',');

            string title = values[0];
            int type = int.Parse(values[1]);
            int step = int.Parse(values[2]);
            float value = float.Parse(values[3]);
            float fee = float.Parse(values[4]);

            // ��ȣ���� ������ �߰�
            if (!dataByNumber.ContainsKey(type))
            {
                dataByNumber[type] = new List<(string title, int type, int step, float value, float fee)>();
            }
            dataByNumber[type].Add((title, type, step, value, fee));
        }

        // ������ Ȯ�� (������)
        //foreach (var entry in dataByNumber)
        //{
        //    Debug.Log($"Number: {entry.Key}");
        //    foreach (var item in entry.Value)
        //    {
        //        Debug.Log($"  Title: {item.title}, type: {item.type}, step: {item.step}, value: {item.value}, fee: {item.fee}");
        //    }
        //}
    }

    // Ư�� ��ȣ�� �ش��ϴ� ������ ��ȯ
    public List<(string title, int type, int step, float value, float fee)> GetDataByNumber(int typeNum)
    {
        if (dataByNumber.ContainsKey(typeNum))
        {
            return dataByNumber[typeNum];
        }
        else
        {
            Debug.LogWarning($"No data found for number {typeNum}");
            return null;
        }
    }

    // Resources �������� ��������Ʈ �ε� (������ ���ϵ�)
    //private Sprite LoadSprite(string path)
    //{
    //    Sprite sprite = Resources.Load<Sprite>("Sprites/Cats/" + path);
    //    if (sprite == null)
    //    {
    //        Debug.LogError($"�̹����� ã�� �� �����ϴ�: {path}");
    //    }
    //    return sprite;
    //}
}
