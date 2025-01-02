using UnityEngine;
using UnityEngine.UI;

// ��ü�� �������ִ� ���� ������ ��� Script
public class MouseData : MonoBehaviour
{
    public Mouse mouseData;                     // �� ������
    private Image mouseImage;                   // �� �̹���

    private RectTransform rectTransform;        // RectTransform ����
    private RectTransform parentPanel;          // �θ� �г� RectTransform

    // ======================================================================================================================

    private void Awake()
    {
        mouseImage = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        parentPanel = rectTransform.parent.GetComponent<RectTransform>();
    }

    private void Start()
    {
        UpdateMouseUI(); 
    }

    // ======================================================================================================================

    // MouseUI �ֽ�ȭ�ϴ� �Լ�
    public void UpdateMouseUI()
    {
        mouseImage.sprite = mouseData.MouseImage;
    }

    // Mouse ������ ���� �Լ�
    public void SetMouseData(Mouse mouse)
    {
        mouseData = mouse;
        UpdateMouseUI();
    }

    // ======================================================================================================================
    // [��������]
    // Mouse Prefab�� MouseData, BossHitbox ��ũ��Ʈ ����


    // [���� �ൿ]
    // ��Ʈ�ڽ� �� �����ϴ� ����� �� N���� �����ϱ� (�ִϸ��̼��� ���߿� �߰��� ����)

    // HP�� �� ������ ���

    // 


}
