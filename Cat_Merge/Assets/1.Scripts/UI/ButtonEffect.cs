using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

// ��ư Ŭ�� ȿ�� ��ũ��Ʈ
public class ButtonEffect : MonoBehaviour
{


    #region Variables

    [Header("������ ������")]
    public float scaleFactor = 0.9f;    // ��ư Ŭ���� ��ҵ� ũ�� ���� (1���� ���� ��)
    public float scaleSpeed = 10f;      // ũ�� ���ϴ� �ӵ� (Ŀ������ ������)

    private class ButtonScaleData
    {
        public Vector3 originalScale;   // ��ư�� ���� ũ��
        public Vector3 targetScale;     // ��ư�� ��ǥ ũ��
        public Transform transform;     // ��ư�� Transform ������Ʈ
    }

    private readonly List<ButtonScaleData> buttonList = new List<ButtonScaleData>();

    #endregion


    #region Unity Methods

    private void Start()
    {
        Button[] allButtons = GetComponentsInChildren<Button>(true);

        foreach (Button button in allButtons)
        {
            // Dictionary Panel�� Information Panel�� ���� ��ư���� ����
            if (IsButtonInExcludedArea(button))
            {
                continue;
            }

            var data = new ButtonScaleData
            {
                originalScale = button.transform.localScale,
                targetScale = button.transform.localScale,
                transform = button.transform
            };
            buttonList.Add(data);

            AddButtonEvents(button, data);
        }
    }

    private void Update()
    {
        foreach (var data in buttonList)
        {
            if (data.transform == null) 
            {
                continue;
            }

            data.transform.localScale = Vector3.Lerp(data.transform.localScale, data.targetScale, Time.deltaTime * scaleSpeed);
        }
    }

    #endregion


    #region Button Management

    // Normal Cat Dictionary�� Information Panel�� ������ �ִ� ��ư���� Ȯ���ϴ� �Լ�
    private bool IsButtonInExcludedArea(Button button)
    {
        Transform current = button.transform;

        bool foundNormalCatDictionary = false;
        bool foundInformationPanel = false;

        while (current != null)
        {
            string currentName = current.name;

            // Normal Cat Dictionary ���� üũ
            if (currentName == "Normal Cat Dictionary")
            {
                foundNormalCatDictionary = true;
            }
            // Information Panel üũ
            else if (currentName == "Information Panel")
            {
                foundInformationPanel = true;
            }

            // Normal Cat Dictionary�� ���� ������Ʈ�� ���
            if (foundNormalCatDictionary)
            {
                return true;
            }
            // Information Panel�� ���� ������Ʈ�� ���
            if (foundInformationPanel)
            {
                return true;
            }

            current = current.parent;
        }
        return false;
    }

    // ��ư�� �̺�Ʈ Ʈ���� �߰��ϴ� �Լ�
    private void AddButtonEvents(Button button, ButtonScaleData data)
    {
        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = button.gameObject.AddComponent<EventTrigger>();
        }

        trigger.triggers = new List<EventTrigger.Entry>();

        // PointerDown
        EventTrigger.Entry entryDown = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerDown
        };
        entryDown.callback.AddListener((eventData) => OnButtonDown(data));
        trigger.triggers.Add(entryDown);

        // PointerUp
        EventTrigger.Entry entryUp = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerUp
        };
        entryUp.callback.AddListener((eventData) => OnButtonUp(data));
        trigger.triggers.Add(entryUp);
    }

    // ��ư ���� ���� ó�� �Լ�
    private void OnButtonDown(ButtonScaleData data)
    {
        data.targetScale = data.originalScale * scaleFactor;
    }

    // ��ư �� ���� ó�� �Լ�
    private void OnButtonUp(ButtonScaleData data)
    {
        data.targetScale = data.originalScale;
    }

    #endregion


}
