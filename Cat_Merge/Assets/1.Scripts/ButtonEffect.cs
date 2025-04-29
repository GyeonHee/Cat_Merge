using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
public class ButtonEffect : MonoBehaviour
{
    [Header("������ ������")]
    public float scaleFactor = 0.9f;
    public float scaleSpeed = 10f; // ũ�� ���ϴ� �ӵ� (Ŀ������ ������)

    private class ButtonScaleData
    {
        public Vector3 originalScale;
        public Vector3 targetScale;
        public Transform transform;
    }

    private List<ButtonScaleData> buttonList = new List<ButtonScaleData>();

    void Start()
    {
        Button[] allButtons = GetComponentsInChildren<Button>(true);

        foreach (Button button in allButtons)
        {
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

    void Update()
    {
        foreach (var data in buttonList)
        {
            if (data.transform == null) continue; // ������ ������Ʈ�� ����

            data.transform.localScale = Vector3.Lerp(data.transform.localScale, data.targetScale, Time.deltaTime * scaleSpeed);
        }
    }

    private void AddButtonEvents(Button button, ButtonScaleData data)
    {
        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = button.gameObject.AddComponent<EventTrigger>();

        trigger.triggers = new List<EventTrigger.Entry>();

        // PointerDown
        EventTrigger.Entry entryDown = new EventTrigger.Entry();
        entryDown.eventID = EventTriggerType.PointerDown;
        entryDown.callback.AddListener((eventData) => OnButtonDown(data));
        trigger.triggers.Add(entryDown);

        // PointerUp
        EventTrigger.Entry entryUp = new EventTrigger.Entry();
        entryUp.eventID = EventTriggerType.PointerUp;
        entryUp.callback.AddListener((eventData) => OnButtonUp(data));
        trigger.triggers.Add(entryUp);

        // PointerExit
        EventTrigger.Entry entryExit = new EventTrigger.Entry();
        entryExit.eventID = EventTriggerType.PointerExit;
        entryExit.callback.AddListener((eventData) => OnButtonUp(data));
        trigger.triggers.Add(entryExit);
    }

    private void OnButtonDown(ButtonScaleData data)
    {
        data.targetScale = data.originalScale * scaleFactor;
    }

    private void OnButtonUp(ButtonScaleData data)
    {
        data.targetScale = data.originalScale;
    }
}
