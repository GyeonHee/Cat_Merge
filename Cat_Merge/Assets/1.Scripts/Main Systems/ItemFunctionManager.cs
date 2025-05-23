using UnityEngine;
using System.Collections.Generic;

// ������ ��� �� ���׷��̵� ������ ���� ��ũ��Ʈ
[DefaultExecutionOrder(-8)]
public class ItemFunctionManager : MonoBehaviour
{


    #region Variables

    public static ItemFunctionManager Instance { get; private set; }

    // ������ ���׷��̵� ������ ����Ʈ��
    public List<(int step, float value, decimal fee)> maxCatsList;
    public List<(int step, float value, decimal fee)> reduceCollectingTimeList;
    public List<(int step, float value, decimal fee)> maxFoodsList;
    public List<(int step, float value, decimal fee)> reduceProducingFoodTimeList;
    public List<(int step, float value, decimal fee)> foodUpgradeList;
    public List<(int step, float value, decimal fee)> foodUpgrade2List;
    public List<(int step, float value, decimal fee)> autoCollectingList;
    public List<(int step, float value, decimal fee)> autoMergeList;

    // ����Ʈ �ʱ� �뷮 ����
    private const int INITIAL_LIST_CAPACITY = 50;

    #endregion


    #region Unity Methods

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

        InitializeLists();
        InitListContents();
    }

    #endregion


    #region Initialization

    // ����Ʈ���� �ʱ� �뷮�� �����ϴ� �Լ�
    private void InitializeLists()
    {
        maxCatsList = new List<(int, float, decimal)>(INITIAL_LIST_CAPACITY);
        reduceCollectingTimeList = new List<(int, float, decimal)>(INITIAL_LIST_CAPACITY);
        maxFoodsList = new List<(int, float, decimal)>(INITIAL_LIST_CAPACITY);
        reduceProducingFoodTimeList = new List<(int, float, decimal)>(INITIAL_LIST_CAPACITY);
        foodUpgradeList = new List<(int, float, decimal)>(INITIAL_LIST_CAPACITY);
        foodUpgrade2List = new List<(int, float, decimal)>(INITIAL_LIST_CAPACITY);
        autoCollectingList = new List<(int, float, decimal)>(INITIAL_LIST_CAPACITY);
        autoMergeList = new List<(int, float, decimal)>(INITIAL_LIST_CAPACITY);
    }

    // �� ������ ���׷��̵� �����͸� ����Ʈ�� �ε��ϴ� �Լ�
    private void InitListContents()
    {
        LoadItemData(1, maxCatsList);                       // ����� �ִ�ġ ����
        LoadItemData(2, reduceCollectingTimeList);          // ��ȭ ȹ�� �ð� ����
        LoadItemData(3, maxFoodsList);                      // ���� �ִ�ġ ����
        LoadItemData(4, reduceProducingFoodTimeList);       // ���� ���� �ð� ����
        LoadItemData(5, foodUpgradeList);                   // ���� ���׷��̵�
        LoadItemData(6, foodUpgrade2List);                  // ���� ���׷��̵�2
        LoadItemData(7, autoCollectingList);                // �ڵ� �����ֱ� �ð�
        LoadItemData(8, autoMergeList);                     // �ڵ� �ռ� �ð�
    }

    // ������ �����͸� �ε��Ͽ� ����Ʈ�� �߰��ϴ� �Լ�
    private void LoadItemData(int dataNumber, List<(int step, float value, decimal fee)> targetList)
    {
        var itemData = ItemUpgradeDataLoader.Instance.GetDataByNumber(dataNumber);
        if (itemData != null)
        {
            foreach (var item in itemData)
            {
                targetList.Add((item.step, item.value, item.fee));
            }
        }
    }

    #endregion


}
