using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActivePanelManager : MonoBehaviour
{
    private class PanelInfo
    {
        public GameObject Panel { get; }
        public Image ButtonImage { get; }

        public PanelInfo(GameObject panel, Image buttonImage)
        {
            Panel = panel;
            ButtonImage = buttonImage;
        }
    }

    private Dictionary<string, PanelInfo> panels = new Dictionary<string, PanelInfo>();     // Panel�� ��ư ������ ������ Dictionary
    private string activePanelName = null;

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
                UpdateButtonColor(panelInfo.ButtonImage, true);

                activePanelName = panelName;
            }
        }
    }

    // Panel �ݴ� �Լ�
    public void ClosePanel(string panelName)
    {
        if (panels.ContainsKey(panelName))
        {
            PanelInfo panelInfo = panels[panelName];
            panelInfo.Panel.SetActive(false);
            UpdateButtonColor(panelInfo.ButtonImage, false);
        }
    }

    // ��ư ���� ������Ʈ
    private void UpdateButtonColor(Image buttonImage, bool isActive)
    {
        string colorCode = isActive ? "#5f5f5f" : "#FFFFFF";
        if (ColorUtility.TryParseHtmlString(colorCode, out Color color))
        {
            buttonImage.color = color;
        }
    }

}
