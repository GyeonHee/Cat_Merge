using UnityEngine;

public class CatMerge : MonoBehaviour
{
    private Cat[] allCatData;               // ��� ����� ������
    public Cat[] AllCatData => allCatData;

    private void Awake()
    {
        LoadAllCats();
    }

    // ����� ���� Load
    private void LoadAllCats()
    {
        allCatData = Resources.LoadAll<Cat>("Cats");
    }

    // ����� Merge �Լ�
    public Cat MergeCats(Cat cat1, Cat cat2)
    {
        if (cat1.CatId != cat2.CatId)
        {
            //Debug.LogWarning("����� �ٸ�");
            return null;
        }

        Cat nextCat = GetCatById(cat1.CatId + 1);
        if (nextCat != null)
        {
            Debug.Log($"�ռ� ���� : {nextCat.CatName}");
            return nextCat;
        }
        else
        {
            //Debug.LogWarning("�� ���� ����� ����̰� ����");
            return null;
        }
    }

    // ����� ID ��ȯ �Լ�
    public Cat GetCatById(int id)
    {
        foreach (Cat cat in allCatData)
        {
            if (cat.CatId == id)
                return cat;
        }
        return null;
    }


}
