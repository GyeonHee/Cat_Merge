using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class QuestManager : MonoBehaviour
{
    private GameManager gameManager;                            // GameManager

    [Header("---[QuestManager]")]
    [SerializeField] private ScrollRect questScrollRect;        // ����Ʈ�� ��ũ�Ѻ�

    [SerializeField] private Button questButton;                // ����Ʈ ��ư
    [SerializeField] private Image questButtonImage;            // ����Ʈ ��ư �̹���

    [SerializeField] private GameObject questMenuPanel;         // ����Ʈ �޴� Panel
    [SerializeField] private Button questBackButton;            // ����Ʈ �ڷΰ��� ��ư
    private bool isQuestMenuOpen;                               // ����Ʈ �޴� Panel�� Ȱ��ȭ ����

    [Header("---[Spawn Quest UI]")]
    [SerializeField] private Slider spawnQuestSlider;           // ����� ���� Slider
    [SerializeField] private TextMeshProUGUI spawnCountText;    // "?/?" �ؽ�Ʈ
    [SerializeField] private Button rewardButton;               // ���� ��ư
    [SerializeField] private TextMeshProUGUI plusCashText;      // ���� ��ȭ ���� Text
    [SerializeField] private GameObject rewardDisabledBG;       // ���� ��ư ��Ȱ��ȭ BG
    private int spawnTargetCount = 1;                           // ��ǥ ���� Ƚ��
    private int increaseSpawnTargetCount = 2;                   // ��ǥ ���� Ƚ�� ����ġ
    private int questRewardCash = 5;                            // ����Ʈ ���� ĳ�� ��ȭ ����

    // ======================================================================================================================

    // Start
    private void Start()
    {
        gameManager = GameManager.Instance;

        questMenuPanel.SetActive(false);

        questButton.onClick.AddListener(ToggleQuestMenuPanel);
        questBackButton.onClick.AddListener(CloseDictionaryMenuPanel);

        rewardButton.onClick.AddListener(ReceiveReward);
        rewardButton.interactable = false;
        plusCashText.text = $"+{questRewardCash}";

        ResetScrollPositions();
        UpdateSpawnQuestUI();
    }

    // �ʱ� ��ũ�� ��ġ �ʱ�ȭ �Լ�
    private void ResetScrollPositions()
    {
        questScrollRect.verticalNormalizedPosition = 1f;
    }

    // ����Ʈ Panel ���� �ݴ� �Լ�
    private void ToggleQuestMenuPanel()
    {
        isQuestMenuOpen = !isQuestMenuOpen;
        questMenuPanel.SetActive(isQuestMenuOpen);
        UpdateButtonColor(questButtonImage, isQuestMenuOpen);
    }

    // ����Ʈ Panel �ݴ� �Լ� (questBackButton)
    private void CloseDictionaryMenuPanel()
    {
        isQuestMenuOpen = false;
        questMenuPanel.SetActive(false);
        UpdateButtonColor(questButtonImage, false);
    }

    // ��ư ������ Ȱ�� ���¿� ���� ������Ʈ�ϴ� �Լ�
    private void UpdateButtonColor(Image buttonImage, bool isActive)
    {
        string colorCode = isActive ? "#5f5f5f" : "#E2E2E2";
        if (ColorUtility.TryParseHtmlString(colorCode, out Color color))
        {
            buttonImage.color = color;
        }
    }

    // ======================================================================================================================

    // ���� ����Ʈ UI�� ������Ʈ�ϴ� �Լ�
    private void UpdateSpawnQuestUI()
    {
        int currentCount = gameManager.FeedCount;

        // ��ǥ�� 10 �̻��̸� ����Ʈ ���� ���� ó��
        if (spawnTargetCount >= 10)
        {
            spawnQuestSlider.value = spawnQuestSlider.maxValue;
            spawnCountText.text = "Complete";
            rewardButton.interactable = false;
            rewardDisabledBG.SetActive(true);
            return;
        }

        // Slider �� ����
        spawnQuestSlider.maxValue = spawnTargetCount;
        spawnQuestSlider.value = currentCount;

        // "?/?" �ؽ�Ʈ ������Ʈ
        spawnCountText.text = $"{currentCount}/{spawnTargetCount}";

        // ���� ��ư Ȱ��ȭ ���� üũ
        bool isComplete = currentCount >= spawnTargetCount;
        rewardButton.interactable = isComplete;
        rewardDisabledBG.SetActive(!isComplete);
    }

    // ���� ��ư Ŭ�� �� ȣ��Ǵ� �Լ�
    private void ReceiveReward()
    {
        int currentCount = gameManager.FeedCount;

        // ��ǥ�� 10 �̻��� ��� �� �̻� ���� ���� �Ұ�
        if (spawnTargetCount >= 10)
        {
            return;
        }

        if (currentCount >= spawnTargetCount)
        {
            // �ʰ��� Ƚ�� ���
            int excessCount = currentCount - spawnTargetCount;

            // ��ǥ ���� Ƚ�� ���� (����Ʈ ���̵� ���)
            spawnTargetCount += increaseSpawnTargetCount;

            // ����Ʈ �Ϸ� ó��
            gameManager.AddCash(questRewardCash);
            gameManager.ResetFeedCount(excessCount);
            gameManager.UpdateCashText();
            UpdateSpawnQuestUI();
        }
    }

    // ���� Ƚ���� ��� üũ�ϰ� UI ������Ʈ
    private void Update()
    {
        UpdateSpawnQuestUI();
    }

    // ======================================================================================================================


}
