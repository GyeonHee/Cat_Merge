using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour, ISaveable
{


    #region Variables

    public static TutorialManager Instance { get; private set; }

    [Header("---[Tutorial UI]")]
    [SerializeField] private GameObject tutorialPanel;          // Ʃ�丮�� �г�
    [SerializeField] private TextMeshProUGUI tutorialText;      // Ʃ�丮�� �ؽ�Ʈ
    [SerializeField] private Button tutorialButton;             // Ʃ�丮�� ��ư


    [Header("---[Tutorial Settings]")]
    [SerializeField] private float typingSpeed = 0.05f;         // Ÿ���� �ӵ�
    private int currentMainMessageIndex = 0;                    // ���� ���� �޽��� �ε���
    private int lastMainMessageIndex = 0;                       // ���������� �� ���� �޽��� �ε���
    private int currentDictionaryMessageIndex = 0;              // ���� ���� �޽��� �ε���
    private bool isTyping = false;                              // ���� Ÿ���� ������ ����
    private Coroutine typingCoroutine;                          // Ÿ���� �ڷ�ƾ
    private bool isWaitingForUserAction = false;                // ���� �׼� ��� ������ ����
    private bool isTutorialActive = false;                      // Ʃ�丮�� Ȱ��ȭ ����
    public bool IsTutorialActive => isTutorialActive;

    // Ʃ�丮�� �ܰ踦 �����ϴ� enum
    public enum TutorialStep
    {
        None,
        SpawnCat,           // �����ֱ� 2��
        MergeCat,           // ����� �ռ� 1��
        OpenItemMenu,       // ������ �г� ����
        BuyMaxCatItem,      // �ִ� ����� �� ���� ������ ����
        ShowCatCount,       // ����� ���� �� ǥ��
        ShowGaugeBar,       // �������� ǥ��
        AutoMerge           // �ڵ� �ռ� ǥ��
    }

    // ���� Ʃ�丮�� �ܰ�
    private TutorialStep currentTutorialStep = TutorialStep.None;

    // �� �ܰ躰 �Ϸ� ���� ī��Ʈ
    private int spawnCount = 0;
    private int mergeCount = 0;

    private string[] mainTutorialMessages = new string[]        // ���� Ʃ�丮�� �޽���
    {
        "�ݰ��ٿ�! �츮�� ����� ���ο� ����Ŀ�?",
        "��ħ �ߵƴٿ�! ��� �����ؾ����� �������� �˷��ְڴٿ�~",
        "�츮�� ���̸� �ָ� �ڿ������� ������ٿ�!\n'�����ֱ�'�� �� �� ���������!",
        "[USER_ACTION]SpawnCat",
        "�� ģ������ �Դٿ�!\n�̷������� �츮�� �θ� �� �ִٿ�~\n���̴� ���� �ð����� �����ȴٿ�~",
        "�̹����� �� ����̸� ���ĺ��ڿ�!\n����� �Ѹ����� ��ġ�� ���·�\n���� ����� ���� �巡���ϸ�...",
        "�� ���� ����� ����̰� �����Ѵٿ�!\n�ѹ� ���� �غ����!",
        "[USER_ACTION]MergeCat",
        "�Ǹ��ϴٿ�!\n�׸��� �츮�� ��ҿ� '����'�� �����ٿ�.\n�� ������ ���� ����� ���׷��̵��ϴµ�\n����Ѵٿ�!",
        "������ ����� �������� �����Ϸ� �����ڿ�!",
        "[USER_ACTION]OpenItemMenu",
        "[USER_ACTION]BuyMaxCatItem",
        "[USER_ACTION]ShowCatCount",
        "����̵��� �� ���� ���� �� �ְ� �ƴٿ�!\n����̰� �������� �� ���� ������ �����ϰ�..",
        "�� ������ �� �ִٿ�!\n�������� ���ϴ��� �ñ��ϳĿ�?",
        "������ �������� ���̳Ŀ�?\n�� �������� ���� ���� '��� ��'�� ��Ÿ����\n�츱 �������Ŵٿ�.",
        "[USER_ACTION]ShowGaugeBar",
        "�༮���� �ѾƳ� �� �ְ� �츱 ���ϰ�\n�����޶��! ���縦 �ϴ´ٿ�!",
        "���������� �ڵ��ռ��� ���� �˷��ְڴٿ�!\n���̾Ƹ� �Ҹ��ؼ� �ڵ����� ����̵���\n�θ��� �ռ����ִ� ����̴ٿ�!",
        "�̶� ���� �ռ��ϴ°͵� �����ϴ�\n�ڵ����� �ռ��߿��� ���� �ռ��ص� �ȴٿ�!",
        "[USER_ACTION]AutoMerge",
        "���� �� ���� ���̴ٿ�!\n�̰����� �������� ��հ� ��ܺ����!"
    };
    private bool isMainTutorialEnd = false;                     // ���� Ʃ�丮�� �Ϸ� ����
    

    private string[] dictionaryTutorialMessages = new string[]  // ���� Ʃ�丮�� �޽���
    {
        "������ ����óĿ�~\n���ݱ��� �󸶳� ���� ����̸�\n�ر��ߴ��� Ȯ���ϴ� �����̴ٿ�!",
        "ó������ ������ ����̰� ����������\n���̾Ƹ� �������� ��������\n������ �ռ��ش޶��~",
        "�׸��� ����̸��� �������\n'������'�� �����Ѵٿ�~",
        "����̸� �����ϰ�\n����� ���� ���� ���Ʒ���\n��ũ���غ��� �������� �� �� �ִٿ�~",
        "�������� ����̸� �������� �����ϰ�\n����̵��� �� ���� ���� �������ų�\n���翡�� ���� ȿ���� �ٰŴٿ�~",
        "���������� ���� ���� �ִ�\n����̸� ��ġ�ϸ� �� ũ�� ������ �� �ִٿ�!!"
    };
    [HideInInspector] public bool isDictionaryTutorialEnd = false;      // ���� Ʃ�丮�� �Ϸ� ����

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
            return;
        }

        InitializeTutorial();
    }

    #endregion


    #region Initialize

    // Ʃ�丮�� �ʱ�ȭ
    private void InitializeTutorial()
    {
        // �ʱ� ����
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }

        // ��ư �̺�Ʈ ���
        if (tutorialButton != null)
        {
            tutorialButton.onClick.AddListener(OnTutorialButtonClick);
        }
    }

    #endregion


    #region Tutorial System

    // Ʃ�丮�� ���� �Լ�
    public void StartTutorial()
    {
        if (tutorialPanel == null || tutorialText == null) return;
        if (isMainTutorialEnd) return;

        isTutorialActive = true;
        tutorialPanel.SetActive(true);

        // lastMainMessageIndex�� ���� ���� ��ġ ����
        currentMainMessageIndex = GetStartMessageIndex();

        // �� �ܰ躰 �ʱ�ȭ
        if (currentMainMessageIndex == 0)
        {
            spawnCount = 0;
            mergeCount = 0;
        }
        else if (currentMainMessageIndex <= 7)
        {
            mergeCount = 0;
        }

        // Ʃ�丮�� ���� �� �ý��� �Ͻ� ����
        PauseGameSystems();

        ShowCurrentMessage();
    }

    // ���� �޽��� ǥ�� �Լ�
    private void ShowCurrentMessage()
    {
        if (currentMainMessageIndex >= mainTutorialMessages.Length)
        {
            EndTutorial();
            return;
        }

        string currentMessage = mainTutorialMessages[currentMainMessageIndex];

        // ���� �׼��� �ʿ��� �޽������� Ȯ��
        if (currentMessage.StartsWith("[USER_ACTION]"))
        {
            string actionType = currentMessage.Substring(13); // "[USER_ACTION]" ������ ���ڿ�
            HandleUserAction(actionType);
            return;
        }

        // ���� Ÿ���� �ڷ�ƾ�� ���� ���̶�� ����
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        // ���ο� Ÿ���� �ڷ�ƾ ����
        typingCoroutine = StartCoroutine(TypeMessage(currentMessage));
    }

    // �޽��� Ÿ���� ȿ�� �ڷ�ƾ
    private IEnumerator TypeMessage(string message)
    {
        isTyping = true;
        tutorialText.text = "";

        foreach (char letter in message)
        {
            tutorialText.text += letter;
            yield return new WaitForSecondsRealtime(typingSpeed);
        }

        isTyping = false;
    }

    // ���� �׼� ó�� �Լ�
    private void HandleUserAction(string actionType)
    {
        isWaitingForUserAction = true;
        currentTutorialStep = (TutorialStep)Enum.Parse(typeof(TutorialStep), actionType);

        switch (currentTutorialStep)
        {
            case TutorialStep.SpawnCat:
                ResumeSpawnSystem();
                break;
            case TutorialStep.MergeCat:
                ResumeMergeSystem();
                break;
            case TutorialStep.OpenItemMenu:
                ResumeItemSystem();
                tutorialPanel.SetActive(false);
                break;
            case TutorialStep.BuyMaxCatItem:
                // ������ ���� ���
                break;
            case TutorialStep.ShowCatCount:
                StartCoroutine(ShowAndProceed());
                break;
            case TutorialStep.ShowGaugeBar:
                StartCoroutine(ShowAndProceed());
                break;
            case TutorialStep.AutoMerge:
                StartCoroutine(ShowAndProceed());
                break;
            default:
                break;
        }

        tutorialPanel.SetActive(false);
    }

    // ���� �ý��� ���� �Լ�
    public void OnCatSpawned()
    {
        if (currentTutorialStep != TutorialStep.SpawnCat) return;

        spawnCount++;
        if (spawnCount >= 2)
        {
            CompleteCurrentStep();
        }
    }

    // �ռ� �ý��� ���� �Լ�
    public void OnCatMerged()
    {
        if (currentTutorialStep != TutorialStep.MergeCat) return;

        mergeCount++;
        if (mergeCount >= 1)
        {
            CompleteCurrentStep();
        }
    }

    // ������ �г� ���� �Ϸ� üũ �Լ�
    public void OnOpenItemMenu()
    {
        if (currentTutorialStep != TutorialStep.OpenItemMenu) return;

        CompleteCurrentStep();
    }

    // ������ ���� �Ϸ� üũ �Լ�
    public void OnMaxCatItemPurchased()
    {
        if (currentTutorialStep != TutorialStep.BuyMaxCatItem) return;

        if (ActivePanelManager.Instance != null)
        {
            ActivePanelManager.Instance.ClosePanel("BottomItemMenu");
        }

        CompleteCurrentStep();
    }

    // 2�� ��� �� ���� �ܰ�� �����ϴ� �ڷ�ƾ
    private IEnumerator ShowAndProceed()
    {
        yield return new WaitForSeconds(2f);

        CompleteCurrentStep();
    }

    // ���� �ܰ� �Ϸ� ó�� �Լ�
    private void CompleteCurrentStep()
    {
        isWaitingForUserAction = false;
        PauseGameSystems();
        tutorialPanel.SetActive(true);

        currentMainMessageIndex++;
        lastMainMessageIndex = currentMainMessageIndex;
        ShowCurrentMessage();
    }

    // Ʃ�丮�� ��ư Ŭ�� ó�� �Լ�
    private void OnTutorialButtonClick()
    {
        if (isWaitingForUserAction) return;

        if (isTyping)
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }

            tutorialText.text = isDictionaryTutorialEnd ?
                mainTutorialMessages[currentMainMessageIndex] :
                dictionaryTutorialMessages[currentDictionaryMessageIndex];

            isTyping = false;
        }
        else
        {
            if (isDictionaryTutorialEnd)
            {
                currentMainMessageIndex++;
                lastMainMessageIndex = currentMainMessageIndex;
                ShowCurrentMessage();
            }
            else
            {
                currentDictionaryMessageIndex++;
                ShowDictionaryMessage();
            }
        }
    }

    // ���� �ý��� �Ͻ� ���� �Լ�
    private void PauseGameSystems()
    {
        // ���� ������ ����
        if (BattleManager.Instance != null)
        {
            if (BattleManager.Instance.enabled)
            {
                BattleManager.Instance.enabled = false;
            }
        }

        // �ڵ� ���� ���� ����
        if (SpawnManager.Instance != null)
        {
            SpawnManager.Instance.enabled = false;
        }

        // ��Ÿ �ý��۵� ����
        if (AutoMergeManager.Instance != null)
        {
            AutoMergeManager.Instance.enabled = false;
        }
    }

    // ���� �ý��� �簳 �Լ�
    private void ResumeSpawnSystem()
    {
        if (SpawnManager.Instance != null)
        {
            SpawnManager.Instance.enabled = true;
        }
    }

    // �ռ� �ý��� �簳 �Լ�
    private void ResumeMergeSystem()
    {
        if (MergeManager.Instance != null)
        {
            MergeManager.Instance.enabled = true;
        }
    }

    // ������ �ý��� �簳 �Լ�
    private void ResumeItemSystem()
    {
        if (ItemMenuManager.Instance != null)
        {
            ItemMenuManager.Instance.enabled = true;
        }
    }

    // ���� �ý��� �簳 �Լ�
    private void ResumeGameSystems()
    {
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.enabled = true;
        }

        if (SpawnManager.Instance != null)
        {
            SpawnManager.Instance.enabled = true;
        }

        if (AutoMergeManager.Instance != null)
        {
            AutoMergeManager.Instance.enabled = true;
        }
    }

    // Ʃ�丮�� ���� �Լ�
    private void EndTutorial()
    {
        isTutorialActive = false;
        tutorialPanel.SetActive(false);
        isMainTutorialEnd = true;

        // ��� �ý��� �簳
        ResumeGameSystems();

        NotificationManager.Instance.ShowNotification("Ʃ�丮�� �ϷẸ������ 100���̾Ƹ� ȹ���Ͽ����ϴ�.");
        GameManager.Instance.Cash += 100;
    }

    // Ʃ�丮�� �����߿� ���� ������������ �ٽ� �����ϴ� ��ġ�� �����ϴ� �Լ�
    private int GetStartMessageIndex()
    {
        // ���� �޽��� ��ġ�� ���� ������ ���� ��ġ ��ȯ
        if (lastMainMessageIndex <= 3)
        {
            return 0;
        }
        else if (lastMainMessageIndex <= 7)
        {
            return 4;
        }
        else if (lastMainMessageIndex <= 11)
        {
            return 8;
        }
        else if (lastMainMessageIndex <= 16)
        {
            return 13;
        }
        else if (lastMainMessageIndex <= 20)
        {
            return 17;
        }
        else if (lastMainMessageIndex <= 21)
        {
            return 21;
        }

        return 0;
    }

    #endregion


    #region Dictionary Tutorial

    // ���� Ʃ�丮�� ����
    public void StartDictionaryTutorial()
    {
        if (tutorialPanel == null || tutorialText == null) return;
        if (isDictionaryTutorialEnd) return;
        if (isTutorialActive) return;

        tutorialPanel.SetActive(true);
        currentDictionaryMessageIndex = 0;

        ShowDictionaryMessage();
    }

    // ���� Ʃ�丮�� �޽��� ǥ��
    private void ShowDictionaryMessage()
    {
        if (currentDictionaryMessageIndex >= dictionaryTutorialMessages.Length)
        {
            EndDictionaryTutorial();
            return;
        }

        string currentMessage = dictionaryTutorialMessages[currentDictionaryMessageIndex];

        // ���� Ÿ���� �ڷ�ƾ�� ���� ���̶�� ����
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        // ���ο� Ÿ���� �ڷ�ƾ ����
        typingCoroutine = StartCoroutine(TypeDictionaryMessage(currentMessage));
    }

    // ���� Ʃ�丮�� �޽��� Ÿ���� ȿ��
    private IEnumerator TypeDictionaryMessage(string message)
    {
        isTyping = true;
        tutorialText.text = "";

        foreach (char letter in message)
        {
            tutorialText.text += letter;
            yield return new WaitForSecondsRealtime(typingSpeed);
        }

        isTyping = false;
    }

    // ���� Ʃ�丮�� ����
    private void EndDictionaryTutorial()
    {
        isDictionaryTutorialEnd = true;
        tutorialPanel.SetActive(false);
    }

    #endregion


    #region Save System

    [Serializable]
    private class SaveData
    {
        public bool isMainTutorialEnd;
        public bool isDictionaryTutorialEnd;
        public int lastMainMessageIndex;
    }

    public string GetSaveData()
    {
        SaveData data = new SaveData
        {
            isMainTutorialEnd = this.isMainTutorialEnd,
            isDictionaryTutorialEnd = this.isDictionaryTutorialEnd,
            lastMainMessageIndex = this.lastMainMessageIndex
        };

        return JsonUtility.ToJson(data);
    }

    public void LoadFromData(string data)
    {
        if (string.IsNullOrEmpty(data)) return;

        SaveData savedData = JsonUtility.FromJson<SaveData>(data);

        this.isMainTutorialEnd = savedData.isMainTutorialEnd;
        this.isDictionaryTutorialEnd = savedData.isDictionaryTutorialEnd;
        this.lastMainMessageIndex = savedData.lastMainMessageIndex;
    }

    #endregion


}
