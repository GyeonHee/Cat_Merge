using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

// ��ġ ������ �г� ���� ��ũ��Ʈ
public class ActivePanelManager : MonoBehaviour
{


    #region Variables

    public static ActivePanelManager Instance { get; private set; }

    // �г� �켱���� ����
    public enum PanelPriority
    {
        Low = 0,        // �Ϲ� �г�
        Medium = 1,     // �߿� �г�
        High = 2,       // �ý��� �г�
        Critical = 3    // �ʼ� �г� (���� ��)
    }

    private class PanelInfo
    {
        public GameObject Panel { get; }
        public Image ButtonImage { get; }
        public PanelPriority Priority { get; }

        public PanelInfo(GameObject panel, Image buttonImage, PanelPriority priority)
        {
            Panel = panel;
            ButtonImage = buttonImage;
            Priority = priority;
        }
    }

    private readonly Dictionary<string, PanelInfo> panels = new Dictionary<string, PanelInfo>();
    private readonly Stack<string> activePanelStack = new Stack<string>();
    public string ActivePanelName => activePanelStack.Count > 0 ? activePanelStack.Peek() : null;

    [Header("---[UI Color]")]
    private const float inactiveAlpha = 180f / 255f;

    // ��ư ���İ��� ������ Ư�� �гε�
    private readonly string[] alphaControlPanels = { "DictionaryMenu", "OptionMenu", "QuestMenu" };

    // �ӽ� ���� ������ ���� ��ü
    private readonly Stack<string> tempStack = new Stack<string>();

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
    }

    private void Start()
    {
        foreach (var panel in panels.Values)
        {
            panel.Panel.SetActive(false);
            if (panel.ButtonImage != null && ShouldControlButtonAlpha(panel))
            {
                UpdateButtonColor(panel.ButtonImage, false);
            }
        }
    }

    #endregion


    #region Panel Registration

    // �г� ��� �Լ�
    public void RegisterPanel(string panelName, GameObject panel, Image buttonImage = null, PanelPriority priority = PanelPriority.Low)
    {
        if (!panels.ContainsKey(panelName))
        {
            panels.Add(panelName, new PanelInfo(panel, buttonImage, priority));
        }
    }

    #endregion


    #region Panel Management

    // �г� ���� �Լ�
    public void OpenPanel(string panelName)
    {
        if (!panels.ContainsKey(panelName)) return;

        var newPanel = panels[panelName];

        // ���� Ȱ��ȭ�� �г��� �ִٸ�
        if (activePanelStack.Count > 0)
        {
            var currentPanelName = activePanelStack.Peek();
            var currentPanel = panels[currentPanelName];

            // �� �г��� �켱������ �� ���ų� ������ ���� �г��� ����
            if (newPanel.Priority >= currentPanel.Priority)
            {
                ClosePanel(currentPanelName);
            }
        }

        // �� �г� ����
        newPanel.Panel.SetActive(true);
        activePanelStack.Push(panelName);
        UpdateButtonColors();
    }

    // �г� �ݱ� �Լ�
    public void ClosePanel(string panelName)
    {
        if (!panels.ContainsKey(panelName)) return;

        PanelInfo panelInfo = panels[panelName];
        if (panelInfo.Panel.activeSelf)
        {
            panelInfo.Panel.SetActive(false);
            if (activePanelStack.Contains(panelName))
            {
                tempStack.Clear();
                while (activePanelStack.Count > 0)
                {
                    var currentPanel = activePanelStack.Pop();
                    if (currentPanel != panelName)
                    {
                        tempStack.Push(currentPanel);
                    }
                }
                while (tempStack.Count > 0)
                {
                    activePanelStack.Push(tempStack.Pop());
                }
            }

            // ������ �ֻ��� �г� Ȱ��ȭ
            if (activePanelStack.Count > 0)
            {
                panels[activePanelStack.Peek()].Panel.SetActive(true);
            }

            UpdateButtonColors();
        }
    }

    // �г� ��� �Լ�
    public void TogglePanel(string panelName)
    {
        if (!panels.ContainsKey(panelName)) return;

        if (IsPanelActive(panelName))
        {
            ClosePanel(panelName);
        }
        else
        {
            OpenPanel(panelName);
        }
    }

    // ��� �г� �ݱ� �Լ�
    public void CloseAllPanels()
    {
        while (activePanelStack.Count > 0)
        {
            ClosePanel(activePanelStack.Peek());
        }
    }

    #endregion


    #region UI Management

    // ��� ��ư ���� ������Ʈ �Լ�
    private void UpdateButtonColors()
    {
        foreach (var kvp in panels)
        {
            if (kvp.Value.ButtonImage != null && ShouldControlButtonAlpha(kvp.Value))
            {
                bool isActive = IsPanelActive(kvp.Key);
                UpdateButtonColor(kvp.Value.ButtonImage, isActive);
            }
        }
    }

    // ���� ��ư ���� ������Ʈ �Լ�
    private void UpdateButtonColor(Image buttonImage, bool isActive)
    {
        if (buttonImage == null) return;

        Color color = buttonImage.color;
        color.a = isActive ? 1f : inactiveAlpha;
        buttonImage.color = color;
    }

    #endregion


    #region Utility Functions

    // �ش� �г��� ��ư ���İ��� �����ؾ� �ϴ��� Ȯ���ϴ� �Լ�
    private bool ShouldControlButtonAlpha(PanelInfo panelInfo)
    {
        return alphaControlPanels.Any(panelName => panels.ContainsKey(panelName) && panels[panelName] == panelInfo);
    }

    // ���� Ȱ��ȭ�� �г��� �ִ��� Ȯ���ϴ� �Լ�
    public bool HasActivePanel()
    {
        return activePanelStack.Count > 0;
    }

    // Ư�� �г��� Ȱ��ȭ�Ǿ� �ִ��� Ȯ���ϴ� �Լ�
    public bool IsPanelActive(string panelName)
    {
        if (!panels.ContainsKey(panelName)) return false;
        return panels[panelName].Panel.activeSelf;
    }

    #endregion


}
