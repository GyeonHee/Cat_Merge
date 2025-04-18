using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActivePanelManager : MonoBehaviour
{
    private class PanelInfo                                 // PanelInfo Class
    {
        public GameObject Panel { get; }
        public Image ButtonImage { get; }

        public PanelInfo(GameObject panel, Image buttonImage)
        {
            Panel = panel;
            ButtonImage = buttonImage;
        }
    }

    private Dictionary<string, PanelInfo> panels;           // Panel�� ��ư ������ ������ Dictionary
    private string activePanelName;                         // Ȱ��ȭ�� Panel �̸�
    public string ActivePanelName => activePanelName;

    [Header("---[UI Color]")]
    private const float inactiveAlpha = 180f / 255f;        // ��Ȱ��ȭ���� Alpha��

    private string[] topPanels = { "DictionaryMenu", "OptionMenu", "QuestMenu" };
    private string[] bottomPanels = { "BottomItemMenu", "BuyCatMenu" };

    // ======================================================================================================================

    private void Awake()
    {
        panels = new Dictionary<string, PanelInfo>();
        activePanelName = null;
    }

    private void Start()
    {
        if (panels != null)
        {
            foreach (var panel in panels.Values)
            {
                panel.Panel.SetActive(false);
                UpdateButtonColor(panel.ButtonImage, true);
            }
        }
    }

    // Panel�� ��ư ���
    public void RegisterPanel(string panelName, GameObject panel, Image buttonImage)
    {
        if (!panels.ContainsKey(panelName))
        {
            panels.Add(panelName, new PanelInfo(panel, buttonImage));
        }
    }

    // Panel ���ݴ� �Լ�
    public void TogglePanel(string panelName)
    {
        if (panels.ContainsKey(panelName))
        {
            // �����ִ� Panel ��ư�� �ѹ� �� �����ٸ�
            if (activePanelName == panelName)
            {
                ClosePanel(activePanelName);
                activePanelName = null;
                // Ư�� �гθ� ��ư ���� ����
                foreach (var kvp in panels)
                {
                    if (System.Array.Exists(topPanels, x => x == kvp.Key))
                    {
                        UpdateButtonColor(kvp.Value.ButtonImage, true);
                    }
                }
            }
            else
            {
                // ���� ���� �ִ� Panel �ݱ�
                if (activePanelName != null && activePanelName != panelName && panels.ContainsKey(activePanelName))
                {
                    ClosePanel(activePanelName);
                }

                // ���ο� Panel ����
                PanelInfo panelInfo = panels[panelName];
                panelInfo.Panel.SetActive(true);
                activePanelName = panelName;

                // Ư�� �гθ� ��ư ���� ������Ʈ
                foreach (var kvp in panels)
                {
                    if (System.Array.Exists(topPanels, x => x == kvp.Key))
                    {
                        UpdateButtonColor(kvp.Value.ButtonImage, kvp.Key == panelName);
                    }
                }
            }
        }
    }

    // Panel �ݴ� �Լ�
    public void ClosePanel(string panelName)
    {
        if (panels.ContainsKey(panelName))
        {
            PanelInfo panelInfo = panels[panelName];
            activePanelName = null;
            panelInfo.Panel.SetActive(false);

            // Ư�� �гθ� ��ư ���� ����
            foreach (var kvp in panels)
            {
                if (System.Array.Exists(topPanels, x => x == kvp.Key))
                {
                    UpdateButtonColor(kvp.Value.ButtonImage, true);
                }
            }
        }
    }

    // ��ư ���� ������Ʈ�ϴ� �Լ�
    private void UpdateButtonColor(Image buttonImage, bool isActive)
    {
        Color color = buttonImage.color;
        color.a = isActive ? 1f : inactiveAlpha;
        buttonImage.color = color;
    }

}
