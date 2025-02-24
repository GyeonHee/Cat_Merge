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

    [Header("---[Training Settings]")]
    public Button trainingLevelUpButton;                            // �Ʒ� ������ ��ư
    private int currentTrainingStage = 0;                           // ���� �Ʒ� �ܰ�
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
        InitializeTrainingSystem();
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

    private void InitializeTrainingSystem()
    {
        if (trainingLevelUpButton != null)
        {
            trainingLevelUpButton.onClick.AddListener(OnTrainingLevelUp);
            UpdateTrainingButtonUI();
        }
    }
    #endregion

    #region Training System
    // �Ʒ� ������ ��ư Ŭ�� ó��
    private void OnTrainingLevelUp()
    {
        TrainingDataLoader trainingLoader = FindObjectOfType<TrainingDataLoader>();
        if (trainingLoader != null)
        {
            // ���� �Ʒ� �ܰ� + 1�� ������ ��������
            int nextStage = currentTrainingStage + 1;
            if (trainingLoader.trainingDictionary.TryGetValue(nextStage, out TrainingData trainingData))
            {
                // GameManager���� ��� ����� ������ ��������
                Cat[] allCats = GameManager.Instance.AllCatData;

                // ��� ����̿��� ���� ���� ����
                foreach (Cat cat in allCats)
                {
                    cat.GrowStat(trainingData.GrowthDamage, trainingData.GrowthHp);
                }

                // ���� �Ʒ� �ܰ� ����
                currentTrainingStage = nextStage;

                // ���� ������ ����
                GameManager.Instance.SaveTrainingData(allCats);

                // �ʵ忡 �ִ� ��� ��������� ������Ʈ
                GameManager.Instance.UpdateAllCatsInField();

                // ��ư UI ������Ʈ
                UpdateTrainingButtonUI();
            }
        }
    }

    // ü�´ܷ� ��ưUI ������Ʈ
    private void UpdateTrainingButtonUI()
    {
        if (trainingLevelUpButton != null)
        {
            // ���� �ܰ谡 �����ϴ� ��쿡�� ��ư Ȱ��ȭ
            TrainingDataLoader trainingLoader = FindObjectOfType<TrainingDataLoader>();
            bool canLevelUp = trainingLoader != null && trainingLoader.trainingDictionary.ContainsKey(currentTrainingStage + 1);

            trainingLevelUpButton.interactable = canLevelUp;
        }
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

}
