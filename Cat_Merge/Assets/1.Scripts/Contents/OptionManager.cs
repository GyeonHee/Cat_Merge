using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

// �ɼ� ��ũ��Ʈ
public class OptionManager : MonoBehaviour, ISaveable
{


    #region Variables

    public static OptionManager Instance { get; private set; }

    // ======================================================================================================================
    // [�ɼ� �޴� UI ��ҵ�]

    [Header("---[OptionManager]")]
    [SerializeField] private Button optionButton;               // �ɼ� ��ư
    [SerializeField] private Image optionButtonImage;           // �ɼ� ��ư �̹���
    [SerializeField] private GameObject optionMenuPanel;        // �ɼ� �޴� Panel
    [SerializeField] private Button optionBackButton;           // �ɼ� �ڷΰ��� ��ư
    private ActivePanelManager activePanelManager;              // ActivePanelManager

    [SerializeField] private GameObject[] mainOptionMenus;      // ���� �ɼ� �޴� Panels
    [SerializeField] private Button[] subOptionMenuButtons;     // ���� �ɼ� �޴� ��ư �迭

    // ======================================================================================================================
    // [���� �޴� UI ���� ����]

    [Header("---[Sub Menu UI Color]")]
    private const string activeColorCode = "#FFCC74";           // Ȱ��ȭ���� Color
    private const string inactiveColorCode = "#FFFFFF";         // ��Ȱ��ȭ���� Color

    // ======================================================================================================================
    // [��� ��ư ���� ����]

    private const float onX = 65f, offX = -65f;                 // �ڵ� ��ư x��ǥ
    private const float moveDuration = 0.2f;                    // ��� �ִϸ��̼� ���� �ð�

    // ======================================================================================================================
    // [Sound]

    // ���� ��Ʈ�� ���� Ŭ����
    public class SoundController
    {
        private readonly AudioSource audioSource;
        private readonly Slider volumeSlider;

        public SoundController(GameObject parent, Slider slider, bool loop, AudioClip clip)
        {
            audioSource = parent.AddComponent<AudioSource>();
            audioSource.loop = loop;
            audioSource.clip = clip;
            volumeSlider = slider;

            if (volumeSlider != null)
            {
                volumeSlider.value = 0.1f;
                volumeSlider.onValueChanged.AddListener(SetVolume);
            }
        }

        // ���� ���� �Լ�
        public void SetVolume(float volume)
        {
            bool isBgm = (audioSource == Instance.bgmSettings.controller.audioSource);
            audioSource.volume = (isBgm && !Instance.bgmSettings.isOn) || (!isBgm && !Instance.sfxSettings.isOn) ? 0f : volume;
            Instance.SetSoundToggleImage(isBgm);
        }

        public void Play() => audioSource.Play();
        public void PlayOneShot()
        {
            if (audioSource != null && audioSource.clip != null && Instance.sfxSettings.isOn)
            {
                audioSource.PlayOneShot(audioSource.clip, Instance.sfxSettings.slider.value);
            }
        }
        public void Stop() => audioSource.Stop();
        public AudioSource GetAudioSource() => audioSource;
    }

    [System.Serializable]
    private class SoundSettings
    {
        public Slider slider;                                   // ���� ���� �����̴�
        public Button toggleButton;                             // ��� ��ư
        public RectTransform handle;                            // ��� �ڵ�
        public Image onOffImage;                                // On/Off �̹���
        public Image toggleButtonImage;                         // ��� ��ư �̹���
        public SoundController controller;                      // ���� ��Ʈ�ѷ�
        public Coroutine toggleCoroutine;                       // ��� �ִϸ��̼� �ڷ�ƾ
        public bool isOn = true;                                // ��� ����
    }

    [Header("---[BGM]")]
    [SerializeField] private SoundSettings bgmSettings = new SoundSettings();

    [Header("---[SFX]")]
    [SerializeField] private SoundSettings sfxSettings = new SoundSettings();

    [Header("---[Common]")]
    private Sprite bgmOnImage;                                  // BGM On �̹���
    private Sprite bgmOffImage;                                 // BGM Off �̹���
    private Sprite sfxOnImage;                                  // SFX On �̹���
    private Sprite sfxOffImage;                                 // SFX Off �̹���

    private const string BGM_ON_IMAGE_PATH = "Sprites/UI/I_UI_Option/I_UI_BGM_v1.9";
    private const string BGM_OFF_IMAGE_PATH = "Sprites/UI/I_UI_Option/I_UI_BGM_v2.9";
    private const string SFX_ON_IMAGE_PATH = "Sprites/UI/I_UI_Option/I_UI_SFX_v1.9";
    private const string SFX_OFF_IMAGE_PATH = "Sprites/UI/I_UI_Option/I_UI_SFX_v2.9";

    // ======================================================================================================================
    // [Display]

    [System.Serializable]
    private class ToggleSettings
    {
        public Button toggleButton;                             // ��� ��ư
        public RectTransform handle;                            // ��� �ڵ�
        public Image toggleButtonImage;                         // ��� ��ư �̹���
        public Coroutine toggleCoroutine;                       // ��� �ִϸ��̼� �ڷ�ƾ
        public bool isOn = true;                                // ��� ����
    }

    [Header("---[Effect]")]
    [SerializeField] private ToggleSettings effectSettings = new ToggleSettings();

    [Header("---[Screen Shaking]")]
    [SerializeField] private ToggleSettings shakingSettings = new ToggleSettings();

    [Header("---[Saving Mode]")]
    [SerializeField] private ToggleSettings savingSettings = new ToggleSettings();

    // ======================================================================================================================
    // [System]

    [Header("---[System]")]
    [SerializeField] private Transform slotPanel;                   // ���� ��ư���� �θ� �г�
    [SerializeField] private Button[] slotButtons;                  // ���� ��ư �迭
    [SerializeField] private Button quitButton;                     // ������ ��ư
    [SerializeField] private GameObject informationPanel;           // ���� �гε��� �θ� �г�
    [SerializeField] private GameObject[] informationPanels;        // ���� �г� �迭
    [SerializeField] private Button informationPanelBackButton;     // ���� �г� �ڷΰ��� ��ư

    private Vector2[] originalButtonPositions;                      // ��ư���� ���� ��ġ
    private CanvasGroup slotPanelGroup;                             // ���� �г��� CanvasGroup
    private const float systemAnimDuration = 0.5f;                  // �ý��� �ִϸ��̼� ���� �ð�
    private int currentActivePanel = -1;                            // ���� Ȱ��ȭ�� �г� �ε���

    // ======================================================================================================================
    // [�ɼ� �޴� Ÿ�� ����]

    // Enum���� �޴� Ÿ�� ���� (���� �޴��� �����ϱ� ���� ���)
    private enum OptionMenuType
    {
        Sound,                                  // ���� �޴�
        Display,                                // ȭ�� �޴�
        System,                                 // �ý��� �޴�
        End                                     // Enum�� ��
    }
    private OptionMenuType activeMenuType;      // ���� Ȱ��ȭ�� �޴� Ÿ��


    private bool isDataLoaded = false;          // ������ �ε� Ȯ��

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
        optionMenuPanel.SetActive(false);

        activePanelManager = FindObjectOfType<ActivePanelManager>();
        activePanelManager.RegisterPanel("OptionMenu", optionMenuPanel, optionButtonImage);

        // GoogleManager���� �����͸� �ε����� ���� ��쿡�� �ʱ�ȭ
        if (!isDataLoaded)
        {
            InitializeOptionManager();
        }
    }

    #endregion


    #region Initialize

    // ��� OptionManager ���� �Լ��� ����
    private void InitializeOptionManager()
    {
        InitializeOptionButton();
        InitializeSubMenuButtons();
        InitializeSoundControllers();
        InitializeDisplayControllers();
        InitializeSystemSettings();
    }

    // OptionButton �ʱ�ȭ �Լ�
    private void InitializeOptionButton()
    {
        Action handleOptionMenu = () =>
        {
            if (activeMenuType == OptionMenuType.System && currentActivePanel != -1)
            {
                ResetSystemMenu();
            }
        };

        optionButton.onClick.AddListener(() =>
        {
            handleOptionMenu();
            activePanelManager.TogglePanel("OptionMenu");
        });

        optionBackButton.onClick.AddListener(() =>
        {
            handleOptionMenu();
            activePanelManager.ClosePanel("OptionMenu");
        });
    }

    #endregion


    #region ���� �޴�

    // ���� �޴� ��ư �ʱ�ȭ �� Ŭ�� �̺�Ʈ �߰� �Լ�
    private void InitializeSubMenuButtons()
    {
        for (int i = 0; i < (int)OptionMenuType.End; i++)
        {
            int index = i;
            subOptionMenuButtons[index].onClick.AddListener(() =>
            {
                if (activeMenuType == OptionMenuType.System && currentActivePanel != -1)
                {
                    ResetSystemMenu();
                }
                ActivateMenu((OptionMenuType)index);
            });
        }

        ActivateMenu(OptionMenuType.Sound);
    }

    // ������ ���� �޴��� Ȱ��ȭ�ϴ� �Լ�
    private void ActivateMenu(OptionMenuType menuType)
    {
        activeMenuType = menuType;

        for (int i = 0; i < mainOptionMenus.Length; i++)
        {
            mainOptionMenus[i].SetActive(i == (int)menuType);
        }

        UpdateSubMenuButtonColors();
    }

    // ���� �޴� ��ư ������ ������Ʈ�ϴ� �Լ�
    private void UpdateSubMenuButtonColors()
    {
        for (int i = 0; i < subOptionMenuButtons.Length; i++)
        {
            UpdateSubButtonColor(subOptionMenuButtons[i].GetComponent<Image>(), i == (int)activeMenuType);
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


    #region ���� ����

    // Sound �ʱ�ȭ �Լ�
    private void InitializeSoundControllers()
    {
        // Audio �ʱ�ȭ
        AudioClip bgmClip = Resources.Load<AudioClip>("Audios/BGM_Sound");
        AudioClip sfxClip = Resources.Load<AudioClip>("Audios/SFX_Sound");
        bgmSettings.controller = new SoundController(gameObject, bgmSettings.slider, true, bgmClip);
        sfxSettings.controller = new SoundController(gameObject, sfxSettings.slider, false, sfxClip);

        bgmSettings.controller.SetVolume(bgmSettings.slider.value);
        sfxSettings.controller.SetVolume(sfxSettings.slider.value);

        bgmSettings.controller.Play();

        // Image �ʱ�ȭ
        bgmOnImage = Resources.Load<Sprite>(BGM_ON_IMAGE_PATH);
        bgmOffImage = Resources.Load<Sprite>(BGM_OFF_IMAGE_PATH);
        sfxOnImage = Resources.Load<Sprite>(SFX_ON_IMAGE_PATH);
        sfxOffImage = Resources.Load<Sprite>(SFX_OFF_IMAGE_PATH);
        bgmSettings.onOffImage.sprite = bgmOnImage;
        sfxSettings.onOffImage.sprite = sfxOnImage;

        // ��� ��ư �̹��� �ʱ�ȭ
        bgmSettings.toggleButtonImage = bgmSettings.toggleButton.GetComponent<Image>();
        sfxSettings.toggleButtonImage = sfxSettings.toggleButton.GetComponent<Image>();
        UpdateToggleButtonImage(bgmSettings.toggleButtonImage, bgmSettings.isOn);
        UpdateToggleButtonImage(sfxSettings.toggleButtonImage, sfxSettings.isOn);

        // ��� ��ư �̺�Ʈ ����
        bgmSettings.toggleButton.onClick.AddListener(() => ToggleSound(true));
        sfxSettings.toggleButton.onClick.AddListener(() => ToggleSound(false));

        UpdateToggleUI(bgmSettings.isOn, true, true);
        UpdateToggleUI(sfxSettings.isOn, false, true);

        // ���� �����̴� �� ���� �� ���� �̺�Ʈ �߰�
        bgmSettings.slider.onValueChanged.AddListener(_ => {
            SetSoundToggleImage(true);
            GoogleSave();
        });
        sfxSettings.slider.onValueChanged.AddListener(_ => {
            SetSoundToggleImage(false);
            GoogleSave();
        });
    }

    // ���� On/Off ��� �Լ�
    public void ToggleSound(bool isBgm)
    {
        SoundSettings settings = isBgm ? bgmSettings : sfxSettings;
        settings.isOn = !settings.isOn;

        // SFX ����̰� Off���� On���� ����� ���� �Ҹ� ���
        if (!isBgm && settings.isOn)
        {
            if (sfxSettings.controller != null && sfxSettings.controller.GetAudioSource() != null)
            {
                AudioSource audioSource = sfxSettings.controller.GetAudioSource();
                audioSource.volume = sfxSettings.slider.value * 0.5f;
                audioSource.PlayOneShot(audioSource.clip);
            }
        }

        SetSoundToggleImage(isBgm);
        UpdateToggleUI(settings.isOn, isBgm);
        UpdateToggleButtonImage(settings.toggleButtonImage, settings.isOn);

        GoogleSave();
    }

    // ��� ��ư �̹��� ������Ʈ �Լ�
    private void UpdateToggleButtonImage(Image buttonImage, bool isOn)
    {
        string imagePath = isOn ? "Sprites/UI/I_UI_Option/I_UI_option_on_Frame.9" : "Sprites/UI/I_UI_Option/I_UI_option_off_Frame.9";
        buttonImage.sprite = Resources.Load<Sprite>(imagePath);
    }

    // ���� On/Off �̹��� ���� �Լ�
    private void SetSoundToggleImage(bool isBgm)
    {
        SoundSettings settings = isBgm ? bgmSettings : sfxSettings;
        if (isBgm)
        {
            settings.onOffImage.sprite = (!settings.isOn || settings.slider.value == 0) ? bgmOffImage : bgmOnImage;
        }
        else
        {
            settings.onOffImage.sprite = (!settings.isOn || settings.slider.value == 0) ? sfxOffImage : sfxOnImage;
        }
    }

    // ���� ��� UI ������Ʈ �Լ�
    private void UpdateToggleUI(bool state, bool isBgm, bool instant = false)
    {
        SoundSettings settings = isBgm ? bgmSettings : sfxSettings;
        float targetX = state ? onX : offX;
        float targetVolume = state ? settings.slider.value : 0.0f;

        if (instant)
        {
            settings.handle.anchoredPosition = new Vector2(targetX, settings.handle.anchoredPosition.y);
            settings.controller.SetVolume(targetVolume);
        }
        else
        {
            StopAndStartCoroutine(ref settings.toggleCoroutine, AnimateToggle(settings.handle, targetX, settings.controller, targetVolume));
        }

    }

    // �ڷ�ƾ ���� �� �����Ű�� �Լ�
    private void StopAndStartCoroutine(ref Coroutine coroutine, IEnumerator routine)
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
        coroutine = StartCoroutine(routine);
    }

    // ���� On/Off ��ư �ִϸ��̼� �ڷ�ƾ
    private IEnumerator AnimateToggle(RectTransform handle, float targetX, SoundController controller, float targetVolume)
    {
        yield return AnimateHandle(handle, targetX);
        controller.SetVolume(targetVolume);
    }

    #endregion


    #region ���÷��� ����

    // Display �ʱ�ȭ �Լ�
    private void InitializeDisplayControllers()
    {
        InitializeToggle(effectSettings, ToggleEffect);
        InitializeToggle(shakingSettings, ToggleShaking);
        InitializeToggle(savingSettings, ToggleSaving);
    }

    // ��� �ʱ�ȭ �Լ�
    private void InitializeToggle(ToggleSettings settings, System.Action toggleAction)
    {
        settings.toggleButton.onClick.AddListener(() => toggleAction());
        settings.toggleButtonImage = settings.toggleButton.GetComponent<Image>();
        UpdateToggleButtonImage(settings.toggleButtonImage, settings.isOn);
        UpdateToggleUI(settings.handle, settings.isOn, true);
    }

    // ����Ʈ ��� �Լ�
    private void ToggleEffect()
    {
        UpdateToggleState(effectSettings);
    }

    // ȭ�� ��鸲 ��� �Լ�
    private void ToggleShaking()
    {
        UpdateToggleState(shakingSettings);
    }

    // ���� ��� ��� �Լ�
    private void ToggleSaving()
    {
        UpdateToggleState(savingSettings);
    }

    // ��� ���� ������Ʈ �Լ�
    private void UpdateToggleState(ToggleSettings settings)
    {
        settings.isOn = !settings.isOn;
        UpdateToggleUI(settings.handle, settings.isOn);
        UpdateToggleButtonImage(settings.toggleButtonImage, settings.isOn);

        GoogleSave();
    }

    // ��� UI ������Ʈ �Լ�
    private void UpdateToggleUI(RectTransform handle, bool state, bool instant = false)
    {
        float targetX = state ? onX : offX;
        if (instant)
        {
            handle.anchoredPosition = new Vector2(targetX, handle.anchoredPosition.y);
        }
        else
        {
            StopAndStartCoroutine(ref effectSettings.toggleCoroutine, AnimateHandle(handle, targetX));
        }

    }

    // ��� �ڵ� �ִϸ��̼� �ڷ�ƾ
    private IEnumerator AnimateHandle(RectTransform handle, float targetX)
    {
        float elapsedTime = 0f;
        float startX = handle.anchoredPosition.x;

        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / moveDuration;
            handle.anchoredPosition = new Vector2(Mathf.Lerp(startX, targetX, t), handle.anchoredPosition.y);
            yield return null;
        }

        handle.anchoredPosition = new Vector2(targetX, handle.anchoredPosition.y);
    }

    #endregion


    #region �ý��� ����

    // System �ʱ�ȭ �Լ�
    private void InitializeSystemSettings()
    {
        // SlotPanel CanvasGroup �ʱ�ȭ
        slotPanelGroup = slotPanel.GetComponent<CanvasGroup>();
        if (slotPanelGroup == null)
        {
            slotPanelGroup = slotPanel.gameObject.AddComponent<CanvasGroup>();
        }

        // InformationPanel �ʱ� ����
        informationPanel.SetActive(false);

        // ��ư �� �г� �ʱ�ȭ
        originalButtonPositions = new Vector2[slotButtons.Length];
        for (int i = 0; i < slotButtons.Length; i++)
        {
            int buttonIndex = i;
            originalButtonPositions[i] = slotButtons[i].GetComponent<RectTransform>().anchoredPosition;
            slotButtons[i].onClick.AddListener(() => OnSlotButtonClick(buttonIndex));
            informationPanels[i].SetActive(false);

            // �� ��ư�� CanvasGroup �߰�
            CanvasGroup buttonGroup = slotButtons[i].gameObject.GetComponent<CanvasGroup>();
            if (buttonGroup == null)
            {
                buttonGroup = slotButtons[i].gameObject.AddComponent<CanvasGroup>();
            }

            // �� Information Panel�� Content Panel�� RectMask2D�� CanvasGroup �߰�
            Transform contentPanel = informationPanels[i].transform.GetChild(0);
            RectTransform contentRect = contentPanel.GetComponent<RectTransform>();
            if (contentRect.GetComponent<RectMask2D>() == null)
            {
                contentRect.gameObject.AddComponent<RectMask2D>();
            }
            if (contentRect.GetComponent<CanvasGroup>() == null)
            {
                contentRect.gameObject.AddComponent<CanvasGroup>();
            }
        }

        // ���� �ڷΰ��� ��ư �̺�Ʈ ����
        informationPanelBackButton.onClick.AddListener(() =>
        {
            if (currentActivePanel != -1)
            {
                StartCoroutine(HideInformationPanel(currentActivePanel));
            }
        });

        // quitButton�� CanvasGroup �߰�
        CanvasGroup exitButtonGroup = quitButton.gameObject.GetComponent<CanvasGroup>();
        if (exitButtonGroup == null)
        {
            exitButtonGroup = quitButton.gameObject.AddComponent<CanvasGroup>();
        }

        // informationPanelBackButton�� CanvasGroup �߰�
        CanvasGroup backButtonGroup = informationPanelBackButton.gameObject.GetComponent<CanvasGroup>();
        if (backButtonGroup == null)
        {
            backButtonGroup = informationPanelBackButton.gameObject.AddComponent<CanvasGroup>();
        }
        backButtonGroup.alpha = 0f;

        // �ý��� ��ư Ŭ�� �̺�Ʈ �߰�
        subOptionMenuButtons[(int)OptionMenuType.System].onClick.AddListener(() =>
        {
            ActivateMenu(OptionMenuType.System);
            ResetSystemMenu();
        });

        // Quit ��ư Ŭ�� �̺�Ʈ �߰�
        quitButton.onClick.AddListener(() => { GameManager.Instance.HandleQuitInput(); });
    }

    // �ý��� �޴� �ʱ�ȭ �Լ�
    private void ResetSystemMenu()
    {
        // ��� �ڷ�ƾ ����
        StopAllCoroutines();

        // ���� Ȱ��ȭ�� �г��� �ִٸ� ��Ȱ��ȭ
        if (currentActivePanel != -1)
        {
            informationPanels[currentActivePanel].SetActive(false);
        }

        slotPanel.gameObject.SetActive(true);
        informationPanel.SetActive(false);

        // ��� ��ư �ʱ�ȭ
        for (int i = 0; i < slotButtons.Length; i++)
        {
            slotButtons[i].gameObject.SetActive(true);
            RectTransform buttonRect = slotButtons[i].GetComponent<RectTransform>();
            buttonRect.anchoredPosition = originalButtonPositions[i];
            CanvasGroup buttonGroup = slotButtons[i].GetComponent<CanvasGroup>();
            buttonGroup.alpha = 1f;
        }

        // Quit ��ư �ʱ�ȭ
        quitButton.gameObject.SetActive(true);
        quitButton.GetComponent<CanvasGroup>().alpha = 1f;

        // back ��ư �ʱ�ȭ
        informationPanelBackButton.GetComponent<CanvasGroup>().alpha = 0f;

        currentActivePanel = -1;
    }

    // ���� ��ư Ŭ�� �̺�Ʈ ó�� �Լ�
    private void OnSlotButtonClick(int index)
    {
        if (currentActivePanel != -1)
        {
            return;
        }
        currentActivePanel = index;
        StartCoroutine(ShowInformationPanel(index));
    }

    // Information Panel ��ġ�� �ڷ�ƾ
    private IEnumerator ShowInformationPanel(int index)
    {
        // ���õ� ��ư�� ������ �ٸ� ��ư�� ���̵� �ƿ� �� ���õ� ��ư �̵� ���� ����
        List<IEnumerator> animations = new List<IEnumerator>();
        animations.Add(MoveButton(slotButtons[index].GetComponent<RectTransform>(), originalButtonPositions[index], new Vector2(0, 465)));
        for (int i = 0; i < slotButtons.Length; i++)
        {
            if (i != index)
            {
                animations.Add(FadeButton(slotButtons[i].GetComponent<CanvasGroup>(), 1f, 0f));
            }
        }
        animations.Add(FadeButton(quitButton.GetComponent<CanvasGroup>(), 1f, 0f));

        foreach (var anim in animations)
        {
            StartCoroutine(anim);
        }

        yield return new WaitForSeconds(systemAnimDuration);

        // �ִϸ��̼� �Ϸ� �� ��ư�� ��Ȱ��ȭ
        for (int i = 0; i < slotButtons.Length; i++)
        {
            if (i != index)
            {
                slotButtons[i].gameObject.SetActive(false);
            }
        }
        quitButton.gameObject.SetActive(false);

        // ���� �г� Ȱ��ȭ �� ��ġ�� �ִϸ��̼�
        informationPanel.SetActive(true);
        informationPanels[index].SetActive(true);

        // �г��� �ʱ� ũ��� ��ġ ����
        RectTransform panelRect = informationPanels[index].GetComponent<RectTransform>();
        Transform contentPanel = panelRect.GetChild(0);
        RectTransform contentRect = contentPanel.GetComponent<RectTransform>();
        CanvasGroup contentGroup = contentPanel.GetComponent<CanvasGroup>();

        // ������ �г��� Mask ����
        RectMask2D mask = contentRect.GetComponent<RectMask2D>();
        if (mask == null)
        {
            mask = contentRect.gameObject.AddComponent<RectMask2D>();
        }
        mask.enabled = true;

        // ������ �г� �ʱ� ���İ� ����
        contentGroup.alpha = 0f;

        panelRect.sizeDelta = new Vector2(panelRect.sizeDelta.x, 0);
        panelRect.anchoredPosition = new Vector2(panelRect.anchoredPosition.x, 480);
        float elapsedTime = 0f;

        // Back ��ư ���̵� �� ����
        StartCoroutine(FadeButton(informationPanelBackButton.GetComponent<CanvasGroup>(), 0f, 1f));

        while (elapsedTime < systemAnimDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / systemAnimDuration;
            float currentHeight = Mathf.Lerp(0, 960, t);
            panelRect.sizeDelta = new Vector2(panelRect.sizeDelta.x, currentHeight);
            panelRect.anchoredPosition = new Vector2(panelRect.anchoredPosition.x, Mathf.Lerp(480, 0, t));
            yield return null;
        }

        panelRect.sizeDelta = new Vector2(panelRect.sizeDelta.x, 960);
        panelRect.anchoredPosition = new Vector2(panelRect.anchoredPosition.x, 0);

        // �г��� �� ������ �� ������ ���̵� ��
        elapsedTime = 0f;
        while (elapsedTime < systemAnimDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / systemAnimDuration;
            contentGroup.alpha = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }
        contentGroup.alpha = 1f;
    }

    // Information Panel ���� �ڷ�ƾ
    private IEnumerator HideInformationPanel(int index)
    {
        // Back ��ư ���̵� �ƿ� ����
        StartCoroutine(FadeButton(informationPanelBackButton.GetComponent<CanvasGroup>(), 1f, 0f));

        RectTransform panelRect = informationPanels[index].GetComponent<RectTransform>();
        Transform contentPanel = panelRect.GetChild(0);
        CanvasGroup contentGroup = contentPanel.GetComponent<CanvasGroup>();

        // ���� ������ ���̵� �ƿ�
        float elapsedTime = 0f;
        while (elapsedTime < systemAnimDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / systemAnimDuration;
            contentGroup.alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }
        contentGroup.alpha = 0f;

        // �������� ������ ���̵� �ƿ��� �� �г� ����
        elapsedTime = 0f;
        while (elapsedTime < systemAnimDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / systemAnimDuration;
            float currentHeight = Mathf.Lerp(960, 0, t);
            panelRect.sizeDelta = new Vector2(panelRect.sizeDelta.x, currentHeight);
            panelRect.anchoredPosition = new Vector2(panelRect.anchoredPosition.x, Mathf.Lerp(0, 480, t));
            yield return null;
        }

        // Mask ��Ȱ��ȭ
        RectTransform contentRect = contentPanel.GetComponent<RectTransform>();
        RectMask2D mask = contentRect.GetComponent<RectMask2D>();
        if (mask != null)
        {
            mask.enabled = false;
        }

        informationPanels[index].SetActive(false);
        informationPanel.SetActive(false);

        // ��� ��ư Ȱ��ȭ
        for (int i = 0; i < slotButtons.Length; i++)
        {
            slotButtons[i].gameObject.SetActive(true);
        }
        quitButton.gameObject.SetActive(true);

        // ��ư �̵� �� ���̵� �� ���� ����
        List<IEnumerator> animations = new List<IEnumerator>();
        animations.Add(MoveButton(slotButtons[index].GetComponent<RectTransform>(), new Vector2(0, 465), originalButtonPositions[index]));
        for (int i = 0; i < slotButtons.Length; i++)
        {
            if (i != index)
            {
                animations.Add(FadeButton(slotButtons[i].GetComponent<CanvasGroup>(), 0f, 1f));
            }
        }
        animations.Add(FadeButton(quitButton.GetComponent<CanvasGroup>(), 0f, 1f));

        foreach (var anim in animations)
        {
            StartCoroutine(anim);
        }

        yield return new WaitForSeconds(systemAnimDuration);

        currentActivePanel = -1;
    }

    // ��ư �̵� �ִϸ��̼� �ڷ�ƾ
    private IEnumerator MoveButton(RectTransform buttonRect, Vector2 startPos, Vector2 endPos)
    {
        float elapsedTime = 0f;

        while (elapsedTime < systemAnimDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / systemAnimDuration;
            buttonRect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }

        buttonRect.anchoredPosition = endPos;
    }

    // ��ư ���̵� �ִϸ��̼� �ڷ�ƾ
    private IEnumerator FadeButton(CanvasGroup canvasGroup, float startAlpha, float endAlpha)
    {
        float elapsedTime = 0f;

        while (elapsedTime < systemAnimDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / systemAnimDuration;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            yield return null;
        }

        canvasGroup.alpha = endAlpha;
    }

    #endregion


    #region ��ư�� SFX �߰�

    // �� �ε� �̺�Ʈ�� ������ �߰�
    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // �� �ε� �̺�Ʈ���� ������ ����
    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // ���� �ε�� ������ ȣ��Ǵ� �Լ�
    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        StartCoroutine(AddSFXToAllButtonsDelayed());
    }

    // ������ ��ư SFX �߰��� ���� �ڷ�ƾ
    private IEnumerator AddSFXToAllButtonsDelayed()
    {
        // �� ������ ����Ͽ� ��� ������Ʈ�� �ʱ�ȭ�ǵ��� ��ٸ�
        yield return null;
        AddSFXToAllButtons();
    }

    // ��� ��ư�� SFX �߰��ϴ� �Լ�
    private void AddSFXToAllButtons()
    {
        // ���� ���� ��� ��ư ã�� (��Ȱ��ȭ�� �� ����)
        Button[] allButtons = Resources.FindObjectsOfTypeAll<Button>();

        foreach (Button button in allButtons)
        {
            // ���� �ִ� ���� ���ӿ�����Ʈ�� ��쿡�� SFX ���
            if (button.gameObject.scene.isLoaded)
            {
                RegisterDynamicButton(button);
            }
        }
    }

    // �������� ������ ��ư�� SFX �߰��ϴ� �Լ�
    public void RegisterDynamicButton(Button button)
    {
        if (button != null)
        {
            // ���� ������ ���� (�ߺ� ����)
            button.onClick.RemoveListener(PlayButtonClickSound);
            button.onClick.AddListener(PlayButtonClickSound);
        }
    }

    // ��ư Ŭ�� �� SFX ����ϴ� �Լ�
    public void PlayButtonClickSound()
    {
        if (sfxSettings.controller != null && sfxSettings.isOn)
        {
            sfxSettings.controller.PlayOneShot();
        }
    }

    #endregion


    #region Save System

    [Serializable]
    private class SaveData
    {
        public bool bgmIsOn;        // BGM Ȱ��ȭ ����
        public float bgmVolume;     // BGM ������
        public bool sfxIsOn;        // SFX Ȱ��ȭ ����
        public float sfxVolume;     // SFX ������

        public bool effectIsOn;     // ����Ʈ Ȱ��ȭ ����
        public bool shakingIsOn;    // ȭ�� ��鸲 Ȱ��ȭ ����
        public bool savingIsOn;     // ������� Ȱ��ȭ ����
    }

    public string GetSaveData()
    {
        SaveData data = new SaveData
        {
            bgmIsOn = bgmSettings.isOn,
            bgmVolume = bgmSettings.slider.value,
            sfxIsOn = sfxSettings.isOn,
            sfxVolume = sfxSettings.slider.value,

            effectIsOn = effectSettings.isOn,
            shakingIsOn = shakingSettings.isOn,
            savingIsOn = savingSettings.isOn
        };

        return JsonUtility.ToJson(data);
    }

    public void LoadFromData(string data)
    {
        if (string.IsNullOrEmpty(data)) return;

        SaveData savedData = JsonUtility.FromJson<SaveData>(data);

        // Sound Settings
        bgmSettings.isOn = savedData.bgmIsOn;
        bgmSettings.slider.value = savedData.bgmVolume;
        sfxSettings.isOn = savedData.sfxIsOn;
        sfxSettings.slider.value = savedData.sfxVolume;

        UpdateToggleUI(bgmSettings.isOn, true, true);
        UpdateToggleUI(sfxSettings.isOn, false, true);
        UpdateToggleButtonImage(bgmSettings.toggleButtonImage, bgmSettings.isOn);
        UpdateToggleButtonImage(sfxSettings.toggleButtonImage, sfxSettings.isOn);
        SetSoundToggleImage(true);
        SetSoundToggleImage(false);

        // Display Settings
        effectSettings.isOn = savedData.effectIsOn;
        shakingSettings.isOn = savedData.shakingIsOn;
        savingSettings.isOn = savedData.savingIsOn;

        UpdateToggleUI(effectSettings.handle, effectSettings.isOn, true);
        UpdateToggleUI(shakingSettings.handle, shakingSettings.isOn, true);
        UpdateToggleUI(savingSettings.handle, savingSettings.isOn, true);
        UpdateToggleButtonImage(effectSettings.toggleButtonImage, effectSettings.isOn);
        UpdateToggleButtonImage(shakingSettings.toggleButtonImage, shakingSettings.isOn);
        UpdateToggleButtonImage(savingSettings.toggleButtonImage, savingSettings.isOn);

        isDataLoaded = true;
    }

    private void GoogleSave()
    {
        if (GoogleManager.Instance != null)
        {
            GoogleManager.Instance.SaveGameState();
        }
    }

    #endregion


}
