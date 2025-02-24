using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ContentManager : MonoBehaviour
{
    #region Variables
    public static ContentManager Instance { get; private set; }

    [Header("---[Common UI Settings]")]
    [SerializeField] private Button contentButton;                  // ������ ��ư
    [SerializeField] private Image contentButtonImage;              // ������ ��ư �̹���
    [SerializeField] private GameObject contentPanel;               // ������ �г�
    [SerializeField] private Button contentBackButton;              // ������ �ڷΰ��� ��ư
    private ActivePanelManager activePanelManager;                  // ActivePanelManager

    [Header("---[Sub Menu Settings]")]
    [SerializeField] private GameObject[] mainContentMenus;         // ���� ������ �޴� Panels
    [SerializeField] private Button[] subContentMenuButtons;        // ���� ������ �޴� ��ư �迭

    [Header("---[UI Color Settings]")]
    private const string activeColorCode = "#FFCC74";               // Ȱ��ȭ���� Color
    private const string inactiveColorCode = "#FFFFFF";             // ��Ȱ��ȭ���� Color

    // ������ �޴� Ÿ�� ���� (���� �޴��� �����ϱ� ���� ���)
    private enum ContentMenuType
    {
        Training,       // ü�� �ܷ�
        Menu2,          // �� ��° �޴�
        Menu3,          // �� ��° �޴�
        End             // Enum�� ��
    }
    private ContentMenuType activeMenuType;                         // ���� Ȱ��ȭ�� �޴� Ÿ��
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
        contentPanel.SetActive(false);
        InitializeContentManager();
    }

    private void Start()
    {
        activePanelManager = FindObjectOfType<ActivePanelManager>();
        activePanelManager.RegisterPanel("ContentMenu", contentPanel, contentButtonImage);
    }
    #endregion

    #region Initialize
    // ��� ContentsManager ���� �Լ��� ����
    private void InitializeContentManager()
    {
        InitializeContentButton();
        InitializeSubMenuButtons();
    }

    // ContentButton �ʱ�ȭ �Լ�
    private void InitializeContentButton()
    {
        contentButton.onClick.AddListener(() =>
        {
            activePanelManager.TogglePanel("ContentMenu");
        });

        contentBackButton.onClick.AddListener(() =>
        {
            activePanelManager.ClosePanel("ContentMenu");
        });
    }
    #endregion

    #region Sub Menu System
    // ���� �޴� ��ư �ʱ�ȭ �� Ŭ�� �̺�Ʈ �߰� �Լ�
    private void InitializeSubMenuButtons()
    {
        for (int i = 0; i < (int)ContentMenuType.End; i++)
        {
            int index = i;
            subContentMenuButtons[index].onClick.AddListener(() =>
            {
                ActivateMenu((ContentMenuType)index);
            });
        }

        ActivateMenu(ContentMenuType.Training);
    }

    // ������ ���� �޴��� Ȱ��ȭ�ϴ� �Լ�
    private void ActivateMenu(ContentMenuType menuType)
    {
        activeMenuType = menuType;

        for (int i = 0; i < mainContentMenus.Length; i++)
        {
            mainContentMenus[i].SetActive(i == (int)menuType);
        }

        UpdateSubMenuButtonColors();
    }

    // ���� �޴� ��ư ������ ������Ʈ�ϴ� �Լ�
    private void UpdateSubMenuButtonColors()
    {
        for (int i = 0; i < subContentMenuButtons.Length; i++)
        {
            UpdateSubButtonColor(subContentMenuButtons[i].GetComponent<Image>(), i == (int)activeMenuType);
        }
    }

    // ���� �޴� ��ư ������ Ȱ�� ���¿� ���� ������Ʈ�ϴ� �Լ�
    private void UpdateSubButtonColor(Image buttonImage, bool isActive)
    {
        if (ColorUtility.TryParseHtmlString(isActive ? activeColorCode : inactiveColorCode, out Color color))
        {
            buttonImage.color = color;
        }
    }
    #endregion

    #region Battle System
    // ���� ���۽� ��ư �� ��� ��Ȱ��ȭ��Ű�� �Լ�
    public void StartBattleState()
    {
        contentButton.interactable = false;
        if (contentPanel.activeSelf)
        {
            contentPanel.SetActive(false);
        }
    }

    // ���� ����� ��ư �� ��� Ȱ��ȭ��Ű�� �Լ�
    public void EndBattleState()
    {
        contentButton.interactable = true;
    }
    #endregion

    #region Utility Functions
    // �ڷ�ƾ ���� �� �����Ű�� �Լ�
    private void StopAndStartCoroutine(ref Coroutine coroutine, IEnumerator routine)
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
        coroutine = StartCoroutine(routine);
    }
    #endregion
}
