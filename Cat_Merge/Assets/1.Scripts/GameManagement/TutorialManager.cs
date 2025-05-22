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
    [SerializeField] private Image enterImage;                  // ����(��ġ) ��ư

    [SerializeField] private GameObject spawnBlockingPanel;     // ���� �Է� ���ܿ� �г� ([USER_ACTION]SpawnCat)
    [SerializeField] private GameObject mergeBlockingPanel;     // ���� �Է� ���ܿ� �г� ([USER_ACTION]MergeCat)
    [SerializeField] private GameObject openItemBlockingPanel;  // ���� �Է� ���ܿ� �г� ([USER_ACTION]OpenItemMenu)
    [SerializeField] private GameObject buyItemBlockingPanel;   // ���� �Է� ���ܿ� �г� ([USER_ACTION]BuyMaxCatItem)
    [SerializeField] private GameObject mainBlockingPanel;      // ���� �Է� ���ܿ� �г� ([USER_ACTION]ShowCatCount, [USER_ACTION]ShowGaugeBar, [USER_ACTION]ShowAutoMerge)


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


    [Header("---[Enter Image Settings]")]
    [SerializeField] private float fadeSpeed = 2f;              // ���̵� �ӵ�
    [SerializeField] private float minAlpha = 0.2f;             // �ּ� ����
    [SerializeField] private float maxAlpha = 1f;               // �ִ� ����
    private Coroutine enterImageBlinkCoroutine;                 // ���� �̹��� ������ �ڷ�ƾ


    [Header("---[Tutorial Arrow]")]
    [SerializeField] private GameObject tutorialTopArrow;       // Ʃ�丮�� ȭ��ǥ (���� ����Ŵ)
    [SerializeField] private GameObject tutorialBottomArrow;    // Ʃ�丮�� ȭ��ǥ (�Ʒ��� ����Ŵ)
    [SerializeField] private GameObject tutorialLeftArrow;      // Ʃ�丮�� ȭ��ǥ (������ ����Ŵ)
    private bool shouldShowArrow;                               // ȭ��ǥ ǥ�� ����
    private Coroutine arrowBlinkCoroutine;                      // ȭ��ǥ ������ �ڷ�ƾ
    private GameObject currentArrow;                            // ���� ��� ���� ȭ��ǥ

    // �ռ� ȭ��ǥ ���� ����
    private bool isMergeTutorialActive;                         // �ռ� Ʃ�丮�� Ȱ��ȭ ����
    private Coroutine mergeArrowCoroutine;                      // �ռ� ȭ��ǥ �ڷ�ƾ

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
        ShowAutoMerge       // �ڵ� �ռ� ǥ��
    }

    // ���� Ʃ�丮�� �ܰ�
    private TutorialStep currentTutorialStep = TutorialStep.None;
    public TutorialStep CurrentTutorialStep => currentTutorialStep;

    // �� �ܰ躰 �Ϸ� ���� ī��Ʈ
    private int spawnCount = 0;
    private int mergeCount = 0;

    private string[] mainTutorialMessages = new string[]        // ���� Ʃ�丮�� �޽���
    {
        "�ݰ��ٿ�! �츮�� ����� ���ο� ����Ŀ�?",
        "������ ��� �����ؾ�����\n�������� �˷��ְڴٿ�~",
        "�츮�� ���̸� �ָ� �ڿ������� ������ٿ�!\n'�����ֱ�'�� �� �� ���������!",
        "[USER_ACTION]SpawnCat",
        "�̷������� �츮�� �θ� �� �ִٿ�~\n���̴� ���� �ð����� �����Ǵ� �����϶��~",
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
        "[USER_ACTION]ShowAutoMerge",
        "���� �� ���� ���̴ٿ�!\n�̰����� �������� ��հ� ��ܺ����!"
    };
    private bool isMainTutorialEnd = false;                     // ���� Ʃ�丮�� �Ϸ� ����
    public bool IsMainTutorialEnd => isMainTutorialEnd;


    private string[] dictionaryTutorialMessages = new string[]  // ���� Ʃ�丮�� �޽���
    {
        "������ ����óĿ�~\n������ ���ݱ��� ���� ����̸�\nȮ���ϴ� �����̴ٿ�!",
        "ó������ ������ ����̰� ����������\n���̾Ƹ� �������� ��������\n������ �ռ��ش޶��~",
        "�׸��� ����̸��� �������\n'������'�� �����Ѵٿ�~",
        "����̸� �����ϰ�\n������ '����� ����' ���� ���Ʒ���\n��ũ���غ��� �������� �� �� �ִٿ�~",
        "�������� ����̸� �������� �����ϰ�\n����̵��� �� ���� ���� �������ų�\n���翡�� ���� ȿ���� �ٰŴٿ�~",
        "���������� ���� ���� �ִ�\n����̸� ��ġ�ϸ� �� ũ�� ������ �� ������\n���������!!"
    };
    private bool isDictionaryTutorialEnd = false;               // ���� Ʃ�丮�� �Ϸ� ����
    public bool IsDictionaryTutorialEnd => isDictionaryTutorialEnd;

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

        // ���ŷ �гε� �ʱ� ����
        if (spawnBlockingPanel != null)
        {
            spawnBlockingPanel.SetActive(false);
        }
        if (mergeBlockingPanel != null)
        {
            mergeBlockingPanel.SetActive(false);
        }
        if (openItemBlockingPanel != null)
        {
            openItemBlockingPanel.SetActive(false);
        }
        if (buyItemBlockingPanel != null)
        {
            buyItemBlockingPanel.SetActive(false);
        }
        if (mainBlockingPanel != null)
        {
            mainBlockingPanel.SetActive(false);
        }

        // ȭ��ǥ �̹��� �ʱ� ����
        if (tutorialTopArrow != null)
        {
            tutorialTopArrow.SetActive(false);
        }
        if (tutorialBottomArrow != null)
        {
            tutorialBottomArrow.SetActive(false);
        }
        if (tutorialLeftArrow != null)
        {
            tutorialLeftArrow.SetActive(false);
        }

        // ��ư �̺�Ʈ ���
        if (tutorialButton != null)
        {
            tutorialButton.onClick.AddListener(OnTutorialButtonClick);
        }

        // ���� �̹��� �ʱ�ȭ
        if (enterImage != null)
        {
            enterImage.color = new Color(1f, 1f, 1f, maxAlpha);
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
        StartEnterImageBlink();

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
            string actionType = currentMessage.Substring(13);
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
        shouldShowArrow = true;

        switch (currentTutorialStep)
        {
            case TutorialStep.SpawnCat:
                // ����� ��ȯ 2ȸ ���
                spawnBlockingPanel.SetActive(true);
                UpdateArrowPosition("Spawn Button");
                break;

            case TutorialStep.MergeCat:
                // ����� �ռ� 1ȸ ���
                mergeBlockingPanel.SetActive(true);
                isMergeTutorialActive = true;
                tutorialTopArrow.SetActive(true);
                StartMergeArrowMove();
                break;

            case TutorialStep.OpenItemMenu:
                // ������ �г� ���� ���
                openItemBlockingPanel.SetActive(true);
                UpdateArrowPosition("ItemMenu Button");
                break;

            case TutorialStep.BuyMaxCatItem:
                // ����� �ִ�ġ ���� ������ 1ȸ ���� ���
                buyItemBlockingPanel.SetActive(true);
                UpdateArrowPosition("IncreaseCatMaximumButton");
                break;

            case TutorialStep.ShowCatCount:
                StartCoroutine(ShowAndProceed());
                UpdateArrowPosition("CatCount");
                break;

            case TutorialStep.ShowGaugeBar:
                StartCoroutine(ShowAndProceed());
                UpdateArrowPosition("GaugeBar");
                break;

            case TutorialStep.ShowAutoMerge:
                StartCoroutine(ShowAndProceed());
                UpdateArrowPosition("AutoMerge");
                break;

            default:
                shouldShowArrow = false;
                break;
        }

        UpdateArrowVisibility();

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

    // ������ ���� ����/�ݱ� üũ �Լ�
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
        // ���ŷ �г� Ȱ��ȭ
        if (mainBlockingPanel != null)
        {
            mainBlockingPanel.SetActive(true);
        }

        yield return new WaitForSeconds(2f);

        // ���ŷ �г� ��Ȱ��ȭ
        if (mainBlockingPanel != null)
        {
            mainBlockingPanel.SetActive(false);
        }

        CompleteCurrentStep();
    }

    // ���� �ܰ� �Ϸ� ó�� �Լ�
    private void CompleteCurrentStep()
    {
        isWaitingForUserAction = false;
        shouldShowArrow = false;
        StopArrowMove();
        tutorialPanel.SetActive(true);

        // ���ŷ �гε� ��Ȱ��ȭ
        switch (currentTutorialStep)
        {
            case TutorialStep.SpawnCat:
                spawnBlockingPanel.SetActive(false);
                break;

            case TutorialStep.MergeCat:
                mergeBlockingPanel.SetActive(false);
                isMergeTutorialActive = false;
                break;

            case TutorialStep.OpenItemMenu:
                openItemBlockingPanel.SetActive(false);
                break;

            case TutorialStep.BuyMaxCatItem:
                buyItemBlockingPanel.SetActive(false);
                break;
        }

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

            tutorialText.text = isTutorialActive ? mainTutorialMessages[currentMainMessageIndex] : dictionaryTutorialMessages[currentDictionaryMessageIndex];
            isTyping = false;
        }
        else
        {
            if (isTutorialActive)
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

    // Ʃ�丮�� ���� �Լ�
    private void EndTutorial()
    {
        isTutorialActive = false;
        tutorialPanel.SetActive(false);
        isMainTutorialEnd = true;
        StopEnterImageBlink();

        // ���� ��ư ��ȣ�ۿ� ������Ʈ
        if (DictionaryManager.Instance != null)
        {
            DictionaryManager.Instance.UpdateDictionaryButtonInteractable();
        }

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


    #region Panel State

    // ȭ��ǥ ǥ�� ���� ������Ʈ
    private void UpdateArrowVisibility()
    {
        switch (currentTutorialStep)
        {
            case TutorialStep.SpawnCat:
                // spawnBlockingPanel�� �����Ƿ� �׻� ȭ��ǥ ǥ��
                shouldShowArrow = true;
                break;

            case TutorialStep.MergeCat:
                // mergeBlockingPanel�� �����Ƿ� �׻� ȭ��ǥ ǥ��
                shouldShowArrow = true;
                break;

            case TutorialStep.OpenItemMenu:
                // � �г��� ��� ItemMenu�� ���̱� ������ ȭ��ǥ �׻� ǥ��
                shouldShowArrow = true;
                break;

            case TutorialStep.BuyMaxCatItem:
                // ������ �г��� �������� ���� ȭ��ǥ ǥ��
                bool isItemPanelOpen = ActivePanelManager.Instance.IsPanelActive("BottomItemMenu");
                shouldShowArrow = isItemPanelOpen;
                break;

            case TutorialStep.ShowCatCount:
            case TutorialStep.ShowGaugeBar:
            case TutorialStep.ShowAutoMerge:
                // mainBlockingPanel�� �����Ƿ� �׻� ȭ��ǥ ǥ��
                shouldShowArrow = true;
                break;

            default:
                shouldShowArrow = false;
                break;
        }

        // ȭ��ǥ ���� ������Ʈ
        if (shouldShowArrow && currentArrow != null)
        {
            currentArrow.SetActive(true);
            StartArrowMove();
        }
        else if (!shouldShowArrow && currentArrow != null)
        {
            StopArrowMove();
        }
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
        StartEnterImageBlink();

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
        StopEnterImageBlink();

        NotificationManager.Instance.ShowNotification("���� Ʃ�丮���� �Ϸ��Ͽ����ϴ�.");
    }

    #endregion


    #region Enter Image Effect

    // ���� �̹��� ������ ����
    private void StartEnterImageBlink()
    {
        if (enterImage == null) return;

        // ���� �ڷ�ƾ�� ���� ���̶�� ����
        if (enterImageBlinkCoroutine != null)
        {
            StopCoroutine(enterImageBlinkCoroutine);
        }

        enterImageBlinkCoroutine = StartCoroutine(BlinkEnterImage());
    }

    // ���� �̹��� ������ ����
    private void StopEnterImageBlink()
    {
        if (enterImageBlinkCoroutine != null)
        {
            StopCoroutine(enterImageBlinkCoroutine);
            enterImageBlinkCoroutine = null;
        }

        // �̹��� �ʱ� ���·� ����
        if (enterImage != null)
        {
            enterImage.color = new Color(1f, 1f, 1f, maxAlpha);
        }
    }

    // ���� �̹��� ������ �ڷ�ƾ
    private IEnumerator BlinkEnterImage()
    {
        float currentAlpha = maxAlpha;
        bool fadeOut = true;

        while (true)
        {
            if (fadeOut)
            {
                currentAlpha = Mathf.MoveTowards(currentAlpha, minAlpha, fadeSpeed * Time.deltaTime);
                if (currentAlpha <= minAlpha)
                {
                    fadeOut = false;
                }
            }
            else
            {
                currentAlpha = Mathf.MoveTowards(currentAlpha, maxAlpha, fadeSpeed * Time.deltaTime);
                if (currentAlpha >= maxAlpha)
                {
                    fadeOut = true;
                }
            }

            enterImage.color = new Color(1f, 1f, 1f, currentAlpha);
            yield return null;
        }
    }

    #endregion


    #region Arrow Effect

    // ���� �ܰ迡 �´� ȭ��ǥ ����
    private GameObject GetCurrentArrow()
    {
        switch (currentTutorialStep)
        {
            case TutorialStep.SpawnCat:
            case TutorialStep.OpenItemMenu:
            case TutorialStep.ShowAutoMerge:
                return tutorialBottomArrow;
            case TutorialStep.BuyMaxCatItem:
            case TutorialStep.ShowCatCount:
                return tutorialTopArrow;
            case TutorialStep.ShowGaugeBar:
                return tutorialLeftArrow;
            case TutorialStep.MergeCat:
                return tutorialTopArrow;
            default:
                return null;
        }
    }

    // ȭ��ǥ ��ġ ������Ʈ
    private void UpdateArrowPosition(string targetButtonName)
    {
        // ���� ȭ��ǥ ��Ȱ��ȭ
        if (currentArrow != null)
        {
            currentArrow.SetActive(false);
        }

        // ���� �ܰ迡 �´� ȭ��ǥ ����
        currentArrow = GetCurrentArrow();
        if (currentArrow == null) return;

        RectTransform arrowRect = currentArrow.GetComponent<RectTransform>();
        if (arrowRect != null)
        {
            Vector2 targetPosition = GetArrowPosition(targetButtonName);

            // ȭ��ǥ Ȱ��ȭ �� ��ġ ����
            currentArrow.SetActive(true);
            arrowRect.localPosition = new Vector3(targetPosition.x, targetPosition.y, 0f);
            arrowRect.localScale = Vector3.one;
        }
    }

    // �� ��ư�� ȭ��ǥ ��ġ ��ȯ
    private Vector2 GetArrowPosition(string targetButtonName)
    {
        switch (targetButtonName)
        {
            case "Spawn Button":
                return new Vector2(370f, -725.5f);
            case "ItemMenu Button":
                return new Vector2(-445f, -725.5f);
            case "IncreaseCatMaximumButton":
                return new Vector2(330f, 270f);
            case "CatCount":
                return new Vector2(-320f, 785f);
            case "GaugeBar":
                return new Vector2(-360f, -320f);
            case "AutoMerge":
                return new Vector2(-350f, -565f);
            default:
                return Vector2.zero;
        }
    }

    // ȭ��ǥ ������ ����
    private void StartArrowMove()
    {
        if (currentTutorialStep == TutorialStep.MergeCat)
        {
            StartMergeArrowMove();
            return;
        }

        if (arrowBlinkCoroutine != null)
        {
            StopCoroutine(arrowBlinkCoroutine);
        }
        arrowBlinkCoroutine = StartCoroutine(MoveArrow());
    }

    // ȭ��ǥ ȭ��ǥ ������ ����
    private void StopArrowMove()
    {
        if (arrowBlinkCoroutine != null)
        {
            StopCoroutine(arrowBlinkCoroutine);
            arrowBlinkCoroutine = null;
        }

        if (mergeArrowCoroutine != null)
        {
            StopCoroutine(mergeArrowCoroutine);
            mergeArrowCoroutine = null;
        }

        isMergeTutorialActive = false;

        // ��� ȭ��ǥ ��Ȱ��ȭ
        if (tutorialTopArrow != null) tutorialTopArrow.SetActive(false);
        if (tutorialBottomArrow != null) tutorialBottomArrow.SetActive(false);
        if (tutorialLeftArrow != null) tutorialLeftArrow.SetActive(false);
        currentArrow = null;
    }

    // ȭ��ǥ ������ �ڷ�ƾ
    private IEnumerator MoveArrow()
    {
        if (currentArrow == null) yield break;

        RectTransform arrowRect = currentArrow.GetComponent<RectTransform>();
        if (arrowRect == null) yield break;

        Vector2 originalPosition = arrowRect.localPosition;
        float moveTime = 0f;
        float moveDuration = 1f; // �� ���� �պ��� �ɸ��� �ð�

        while (shouldShowArrow)
        {
            moveTime += Time.deltaTime;
            float normalizedTime = (moveTime % moveDuration) / moveDuration; // 0~1 ������ ��
            float moveProgress = Mathf.Sin(normalizedTime * Mathf.PI * 2) * 0.5f + 0.5f; // 0~1 ���̸� �ε巴�� �պ�

            Vector3 newPosition = originalPosition;

            if (currentArrow == tutorialTopArrow)
            {
                float yOffset = Mathf.Lerp(0, -50f, moveProgress);
                newPosition.y = originalPosition.y + yOffset;
            }
            else if (currentArrow == tutorialBottomArrow)
            {
                float yOffset = Mathf.Lerp(0, 50f, moveProgress);
                newPosition.y = originalPosition.y + yOffset;
            }
            else if (currentArrow == tutorialLeftArrow)
            {
                float xOffset = Mathf.Lerp(0, 50f, moveProgress);
                newPosition.x = originalPosition.x + xOffset;
            }

            arrowRect.localPosition = newPosition;
            yield return null;
        }

        // �������� ������ ���� ��ġ�� ����
        arrowRect.localPosition = originalPosition;
    }

    // �ռ� ȭ��ǥ �̵� ����
    private void StartMergeArrowMove()
    {
        if (mergeArrowCoroutine != null)
        {
            StopCoroutine(mergeArrowCoroutine);
        }

        isMergeTutorialActive = true;
        tutorialTopArrow.SetActive(true);
        mergeArrowCoroutine = StartCoroutine(MoveMergeArrow());
    }

    // �ռ� ȭ��ǥ �̵� �ڷ�ƾ
    private IEnumerator MoveMergeArrow()
    {
        float moveSpeed = 0.8f; // �̵� �ӵ�
        RectTransform arrowRect = tutorialTopArrow.GetComponent<RectTransform>();

        while (isMergeTutorialActive)
        {
            // Ȱ��ȭ�� ����̵��� ��ġ ��������
            var activeCats = SpawnManager.Instance.GetActiveCats();
            if (activeCats.Count < 2) yield return null;

            // ù ��°�� �� ��° ������� ��ġ ��������
            Vector2 startPos = activeCats[0].GetComponent<RectTransform>().anchoredPosition + Vector2.down * 100f;
            Vector2 endPos = activeCats[1].GetComponent<RectTransform>().anchoredPosition + Vector2.down * 100f;

            float progress = 0f;
            arrowRect.anchoredPosition = startPos;

            while (progress < 1f && isMergeTutorialActive)
            {
                progress += Time.deltaTime * moveSpeed;
                arrowRect.anchoredPosition = Vector2.Lerp(startPos, endPos, progress);
                yield return null;
            }
        }
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

        // ������ �ε� �� ���� ��ư ��ȣ�ۿ� ������Ʈ
        if (DictionaryManager.Instance != null)
        {
            DictionaryManager.Instance.UpdateDictionaryButtonInteractable();
        }
    }

    #endregion


}
