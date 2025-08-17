using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ContentManager : MonoBehaviour
{
    #region Variables
    public static ContentManager Instance { get; private set; }

    [Header("---[Common UI Settings]")]
    [SerializeField] private Button contentButton;              // ������ ��ư
    [SerializeField] private Image contentButtonImage;          // ������ ��ư �̹���
    [SerializeField] private GameObject contentPanel;           // ������ �г�
    [SerializeField] private Button contentBackButton;          // ������ �ڷΰ��� ��ư

    [Header("---[Sub Menu Settings]")]
    [SerializeField] private GameObject[] mainContentMenus;     // ���� ������ �޴� Panels
    [SerializeField] private Button[] subContentMenuButtons;    // ���� ������ �޴� ��ư �迭

    [Header("---[UI Color Settings]")]
    private const string activeColorCode = "#FFCC74";           // Ȱ��ȭ���� Color
    private const string inactiveColorCode = "#FFFFFF";         // ��Ȱ��ȭ���� Color

    // ������ �޴� Ÿ�� ���� (���� �޴��� �����ϱ� ���� ���)
    private enum ContentMenuType
    {
        Training,       // ü�� �ܷ�
        JellyDungeon,   // ���� ����
        Menu3,          // �� ��° �޴�
        End             // Enum�� ��
    }
    private ContentMenuType activeMenuType;                     // ���� Ȱ��ȭ�� �޴� Ÿ��

    [Header("---[Training Settings]")]
    public TextMeshProUGUI nextLevelAblityInformationText;      // ���� �ܰ� ���� �� ȹ���ϴ� �߰� �ɷ�ġ text

    public TextMeshProUGUI trainingLevelInformationText;        // ���� �ܰ� ���� text
    public TextMeshProUGUI trainingProgressText;                // �����Ȳ % text
    private float trainingProgress = 0f;                        // �����Ȳ

    public TextMeshProUGUI trainingLevelText;                   // ü�� �ܷ� N�ܰ� text
    public TextMeshProUGUI trainingDataText;                    // ü�� �ܷ� ��ġ text

    public TextMeshProUGUI levelUpCoinText;                     // ���� �ܰ� ��� �ʿ� ��ȭ text
    public Button levelUpButton;                                // ���� �ܰ� ��� ��ư
    public TextMeshProUGUI trainingCoinText;                    // ü�� �ܷ� �ʿ� ��ȭ text
    public Button trainingButton;                               // ü�� �ܷ� ��ư
    private int currentTrainingStage = 0;                       // ���� ü�´ܷ� �ܰ�
    private bool isTrainingCompleted = false;                   // ü�´ܷ� �Ϸ� ����
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
        ActivePanelManager.Instance.RegisterPanel("ContentMenu", contentPanel, contentButtonImage);
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
            ActivePanelManager.Instance.TogglePanel("ContentMenu");
        });

        contentBackButton.onClick.AddListener(() =>
        {
            ActivePanelManager.Instance.ClosePanel("ContentMenu");
        });
    }

    private void InitializeTrainingSystem()
    {
        // 0���� TraingingDB�� �ҷ��ͼ� ��� �����Ϳ� ���� (�⺻ ������)
        TrainingDataLoader trainingLoader = FindObjectOfType<TrainingDataLoader>();
        if (trainingLoader != null && trainingLoader.trainingDictionary.TryGetValue(0, out TrainingData trainingData))
        {
            Cat[] allCats = GameManager.Instance.AllCatData;
            foreach (Cat cat in allCats)
            {
                cat.GrowStat(trainingData.GrowthDamage, trainingData.GrowthHp);
            }
        }

        if (levelUpButton != null)
        {
            levelUpButton.onClick.AddListener(OnLevelUp);
        }
        if (trainingButton != null)
        {
            trainingButton.onClick.AddListener(OnTraining);
        }

        trainingProgressText.text = $"{trainingProgress}%";
        UpdateTrainingButtonUI();
    }
    #endregion

    #region Training System
    // �Ʒ� ������ ��ư Ŭ�� ó��
    private void OnLevelUp()
    {
        if (!isTrainingCompleted)
        {
            return;
        }

        TrainingDataLoader trainingLoader = FindObjectOfType<TrainingDataLoader>();
        if (trainingLoader != null)
        {
            int nextStage = currentTrainingStage + 1;
            if (trainingLoader.trainingDictionary.TryGetValue(nextStage, out TrainingData trainingData))
            {
                decimal requiredCoin = decimal.Parse(levelUpCoinText.text);
                if (GameManager.Instance.Coin < requiredCoin)
                {
                    NotificationManager.Instance.ShowNotification("��ȭ�� �����մϴ�!");
                    return;
                }
                GameManager.Instance.Coin -= requiredCoin;

                Cat[] allCats = GameManager.Instance.AllCatData;
                foreach (Cat cat in allCats)
                {
                    cat.GrowStat(trainingData.GrowthDamage, trainingData.GrowthHp);
                }
                currentTrainingStage = nextStage;

                // ���� ������ ���� �� �ʵ� ����� ���� ������Ʈ
                GameManager.Instance.SaveTrainingData(allCats);
                GameManager.Instance.UpdateAllCatsInField();

                isTrainingCompleted = false;
                levelUpButton.interactable = false;

                // ������ �� �Ʒ� ���� ��Ȳ�� 0%�� �ʱ�ȭ
                trainingProgress = 0f;
                trainingProgressText.text = $"{trainingProgress}%";

                UpdateTrainingButtonUI();
            }
        }
    }

    // ü�´ܷ� ��ư Ŭ�� ó��
    private void OnTraining()
    {
        TrainingDataLoader trainingLoader = FindObjectOfType<TrainingDataLoader>();
        if (trainingLoader != null && trainingLoader.trainingDictionary.TryGetValue(currentTrainingStage, out TrainingData trainingData))
        {
            decimal requiredCoin = decimal.Parse(trainingCoinText.text);
            if (GameManager.Instance.Coin < requiredCoin)
            {
                NotificationManager.Instance.ShowNotification("��ȭ�� �����մϴ�!");
                return;
            }
            GameManager.Instance.Coin -= requiredCoin;

            isTrainingCompleted = true;

            // �Ʒ� ���� ��Ȳ�� 100%�� ������Ʈ
            trainingProgress = 100f;
            trainingProgressText.text = $"{trainingProgress}%";

            UpdateTrainingButtonUI();
        }
    }

    // ü�´ܷ� ��ưUI ������Ʈ
    private void UpdateTrainingButtonUI()
    {
        TrainingDataLoader trainingLoader = FindObjectOfType<TrainingDataLoader>();
        if (trainingLoader != null)
        {
            // ������ ��ư ���� �� �ʿ� ��ȭ ������Ʈ
            bool canLevelUp = trainingLoader.trainingDictionary.ContainsKey(currentTrainingStage + 1);
            levelUpButton.interactable = canLevelUp && isTrainingCompleted;

            // ���� �ܰ��� ������ �ʿ� ��ȭ ǥ��
            if (canLevelUp)
            {
                levelUpCoinText.text = trainingLoader.trainingDictionary[currentTrainingStage + 1].LevelUpCoin.ToString("N0");
            }

            // ���� �ܰ��� �Ʒ� �ʿ� ��ȭ ǥ��
            if (trainingLoader.trainingDictionary.TryGetValue(currentTrainingStage + 1, out TrainingData nextData))
            {
                trainingCoinText.text = nextData.TrainingCoin.ToString("N0");
                trainingButton.interactable = !isTrainingCompleted;
            }
            else
            {
                trainingButton.interactable = false;
            }

            // ���� ���� �ɷ�ġ ���� ������Ʈ
            if (canLevelUp && trainingLoader.trainingDictionary.TryGetValue(currentTrainingStage + 1, out TrainingData nextLevelData))
            {
                if (nextLevelData.ExtraAbilityName == "����")
                {
                    nextLevelAblityInformationText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(new Color32(59, 125, 35, 255))}>[�⺻ �ɷ�ġ ����]</color>";
                }
                else
                {
                    nextLevelAblityInformationText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(new Color32(59, 125, 35, 255))}>[�� {nextLevelData.ExtraAbilityName} {nextLevelData.ExtraAbilitySymbol} {nextLevelData.ExtraAbilityValue} {nextLevelData.ExtraAbilityUnit} ��]</color>";
                }
            }

            // ��������� �� ������ ��ġ�� ���
            Dictionary<string, float> totalStats = new Dictionary<string, float>()
            {
                {"���� �ӵ� ����", 0},
                {"ġ��Ÿ Ȯ�� ����", 0},
                {"ġ��Ÿ ���� ����", 0},
                {"��ȭ ���޷� ����", 0},
                {"��ȭ ���� �ӵ� ����", 0},
                {"����� ���� ���� ����", 0}
            };

            // 0�ܰ���� ���� �ܰ������ ��� �߰� �ɷ�ġ�� �ջ�
            for (int i = 0; i <= currentTrainingStage; i++)
            {
                if (trainingLoader.trainingDictionary.TryGetValue(i, out TrainingData data))
                {
                    if (data.ExtraAbilityName != "����" && totalStats.ContainsKey(data.ExtraAbilityName))
                    {
                        totalStats[data.ExtraAbilityName] += (float)data.ExtraAbilityValue;
                    }
                }
            }

            // �Ʒ� ������ �ؽ�Ʈ ����
            string statsText = "";

            // �⺻ ���� ������ ǥ�� (���ݷ�, HP)
            if (trainingLoader.trainingDictionary.TryGetValue(currentTrainingStage, out TrainingData current))
            {
                statsText += $"+{current.GrowthDamage}\n";
                statsText += $"+{current.GrowthHp}\n";
            }

            // �߰� �ɷ�ġ ������ ǥ��
            statsText += $"-{totalStats["���� �ӵ� ����"]}��\n";
            statsText += $"+{totalStats["ġ��Ÿ Ȯ�� ����"]}%\n";
            statsText += $"+{totalStats["ġ��Ÿ ���� ����"]}%\n";
            statsText += $"+{totalStats["��ȭ ���޷� ����"]}%\n";
            statsText += $"-{totalStats["��ȭ ���� �ӵ� ����"]}��\n";
            statsText += $"+{totalStats["����� ���� ���� ����"]}����";

            trainingDataText.text = statsText;
            trainingLevelText.text = $"ü�� �ܷ� {currentTrainingStage}�ܰ�";
            trainingLevelInformationText.text = $"{currentTrainingStage + 1}�ܰ�";
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
