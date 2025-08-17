using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

// �г� ���� ��ũ��Ʈ
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
        public CanvasGroup CanvasGroup { get; }

        public PanelInfo(GameObject panel, Image buttonImage, PanelPriority priority)
        {
            Panel = panel;
            ButtonImage = buttonImage;
            Priority = priority;
            CanvasGroup = panel.GetComponent<CanvasGroup>();

            // CanvasGroup�� ������ �߰�
            if (CanvasGroup == null)
            {
                CanvasGroup = panel.AddComponent<CanvasGroup>();
            }
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

    // UI Disable GameObject���� CanvasGroup ������Ʈ��
    [Header("---[UI Disable]")]
    [SerializeField] private CanvasGroup mainUIPanelCanvasGroup;
    [SerializeField] private CanvasGroup mergeAutoFillAmountImgCanvasGroup;
    [SerializeField] private CanvasGroup spawnAutoFillAmountImgCanvasGroup;

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
        if (!panels.ContainsKey(panelName))
        {
            return;
        }

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
        if (newPanel.CanvasGroup != null)
        {
            newPanel.CanvasGroup.alpha = 1f;
            newPanel.CanvasGroup.blocksRaycasts = true;
        }

        activePanelStack.Push(panelName);
        UpdateButtonColors();
        UpdateUIDisableVisibility();
    }

    // �г� �ݱ� �Լ�
    public void ClosePanel(string panelName)
    {
        if (!panels.ContainsKey(panelName))
        {
            return;
        }

        PanelInfo panelInfo = panels[panelName];
        if (panelInfo.Panel.activeSelf)
        {
            // �г��� ��Ȱ��ȭ�ϵ� �׸��⸦ �����ϱ� ���� CanvasGroup alpha�� 0���� ����
            if (panelInfo.CanvasGroup != null)
            {
                panelInfo.CanvasGroup.alpha = 0f;
                panelInfo.CanvasGroup.blocksRaycasts = false;
            }

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
                var topPanel = panels[activePanelStack.Peek()];
                if (topPanel.CanvasGroup != null)
                {
                    topPanel.CanvasGroup.alpha = 1f;
                    topPanel.CanvasGroup.blocksRaycasts = true;
                }
            }

            UpdateButtonColors();
            UpdateUIDisableVisibility();
        }
    }

    // �г� ��� �Լ�
    public void TogglePanel(string panelName)
    {
        if (!panels.ContainsKey(panelName))
        {
            return;
        }

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
        if (buttonImage == null)
        {
            return;
        }

        Color color = buttonImage.color;
        color.a = isActive ? 1f : inactiveAlpha;
        buttonImage.color = color;
    }

    // UI Disable GameObject���� ���ü� ������Ʈ
    private void UpdateUIDisableVisibility()
    {
        // UI Disable�� ������ Ư�� �гε�
        string[] uiDisablePanels = { "BottomItemMenu", "BuyCatMenu", "ShopMenu", "DictionaryMenu", "QuestMenu", "OptionMenu" };

        // ���� Ȱ��ȭ�� �г� �߿��� UI Disable�� ������ �г��� �ִ��� Ȯ��
        bool hasUIDisablePanel = activePanelStack.Any(panelName => uiDisablePanels.Contains(panelName));

        // ���� �г� �̹��� ���ü� ����
        if (mainUIPanelCanvasGroup != null)
        {
            mainUIPanelCanvasGroup.alpha = hasUIDisablePanel ? 0f : 1f;
            mainUIPanelCanvasGroup.blocksRaycasts = !hasUIDisablePanel;
        }
        if (mergeAutoFillAmountImgCanvasGroup != null)
        {
            mergeAutoFillAmountImgCanvasGroup.alpha = hasUIDisablePanel ? 0f : 1f;
            mergeAutoFillAmountImgCanvasGroup.blocksRaycasts = !hasUIDisablePanel;
        }
        if (spawnAutoFillAmountImgCanvasGroup != null)
        {
            spawnAutoFillAmountImgCanvasGroup.alpha = hasUIDisablePanel ? 0f : 1f;
            spawnAutoFillAmountImgCanvasGroup.blocksRaycasts = !hasUIDisablePanel;
        }
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
        if (!panels.ContainsKey(panelName))
        {
            return false;
        }

        return panels[panelName].Panel.activeSelf && (panels[panelName].CanvasGroup == null || panels[panelName].CanvasGroup.alpha > 0f);
    }

    #endregion


}
