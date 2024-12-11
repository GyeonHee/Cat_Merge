using UnityEngine;

// ����� ���� Script
public class CatMerge : MonoBehaviour
{
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
            //Debug.Log($"�ռ� ���� : {nextCat.CatName}");
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
        GameManager gameManager = GameManager.Instance;

        foreach (Cat cat in gameManager.AllCatData)
        {
            if (cat.CatId == id)
                return cat;
        }
        return null;
    }


}
