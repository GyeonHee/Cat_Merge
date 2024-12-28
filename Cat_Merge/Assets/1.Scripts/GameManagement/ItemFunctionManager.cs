using UnityEngine;
using System.Collections.Generic;
public class ItemFunctionManager : MonoBehaviour
{
    // Singleton Instance
    public static ItemFunctionManager Instance { get; private set; }

    //private int maxCats;                // ����� �ִ� ��
    //public int MaxCats { get => maxCats; set => maxCats = value; }

    //private int collectingTime;         // ��ȭ ȹ�� �ð�
    //public int CollectingTime { get => collectingTime; set => collectingTime = value; }

    //private int maxFoods;               // ���� �ִ�ġ
    //public int MaxFoods { get => maxFoods; set => maxFoods = value; }

    //private int producingFoodsTime;     // ���� ���� �ð�
    //public int ProducingFoodsTime { get => producingFoodsTime; set => producingFoodsTime = value; }

    //private int foodUpgrade;            // ���� ���׷��̵�
    //public int FoodUpgrade { get => foodUpgrade; set => foodUpgrade = value; }

    //private int foodUpgradeVer2;        // ���� ���׷��̵�2
    //public int FoodUpgradeVer2 { get => foodUpgradeVer2; set => foodUpgradeVer2 = value; }

    //private int autoFeedingTime;        // �ڵ� �����ֱ� �ð�
    //public int AutoFeedingTime { get => autoFeedingTime; set => autoFeedingTime = value; }

    public List<(int step, float value, float fee)> maxCatsList = new List<(int step, float value, float fee)>();

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

        InitListContents();
    }

    private void InitListContents()
    {
        var data = ItemItemUpgradeDataLoader.Instance.GetDataByNumber(1);
        if (data != null)
        {
            foreach (var item in data)
            {
                maxCatsList.Add((item.step, item.value, item.fee));
            }
        }
    }
}
