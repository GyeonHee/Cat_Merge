using UnityEngine;

// GameManager
public class GameManager : MonoBehaviour
{
    private Cat[] allCatData;               // ��� ����� ������ ����

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


}
