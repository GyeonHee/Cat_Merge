using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// OptionManager Script
public class OptionManager : MonoBehaviour
{
    // Singleton Instance
    public static OptionManager Instance { get; private set; }

    // ���� ��Ʈ�� ���� Ŭ����
    public class SoundController
    {
        private AudioSource audioSource;
        private Slider volumeSlider;
        private AudioClip soundClip;

        public SoundController(GameObject parent, Slider slider, bool loop, AudioClip clip)
        {
            audioSource = parent.AddComponent<AudioSource>();
            audioSource.loop = loop;
            volumeSlider = slider;
            soundClip = clip;

            audioSource.clip = soundClip;

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

            if ((isBgm && !Instance.bgmSettings.isOn) || (!isBgm && !Instance.sfxSettings.isOn))
            {
                audioSource.volume = 0f;
            }
            else
            {
                audioSource.volume = volume;
            }

            Instance.SetSoundToggleImage(isBgm);
        }

        // ���� ���
        public void Play()
        {
            audioSource.Play();
        }

        // ���� ����
        public void Stop()
        {
            audioSource.Stop();
        }

        // AudioSource getter �߰�
        public AudioSource GetAudioSource()
        {
            return audioSource;
        }
    }

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
    private string activeColorCode = "#5f5f5f";                 // Ȱ��ȭ���� Color
    private string inactiveColorCode = "#FFFFFF";               // ��Ȱ��ȭ���� Color

    // ======================================================================================================================
    // [��� ��ư ���� ����]

    private float onX = 65f, offX = -65f;                       // �ڵ� ��ư x��ǥ
    private float moveDuration = 0.2f;                          // ��� �ִϸ��̼� ���� �ð�

    // ======================================================================================================================
    // [Sound]

    [System.Serializable]
    private class SoundSettings
    {
        public Slider slider;                                   // ���� ���� �����̴�
        public Button toggleButton;                             // ��� ��ư
        public RectTransform handle;                            // ��� �ڵ�
        public Image onOffImage;                                // On/Off �̹���
        public SoundController controller;                      // ���� ��Ʈ�ѷ�
        public Coroutine toggleCoroutine;                       // ��� �ִϸ��̼� �ڷ�ƾ
        public bool isOn = true;                                // ��� ����
    }

    [Header("---[BGM]")]
    [SerializeField] private SoundSettings bgmSettings = new SoundSettings();

    [Header("---[SFX]")]
    [SerializeField] private SoundSettings sfxSettings = new SoundSettings();

    [Header("---[Common]")]
    private Sprite soundOnImage;                                // ���� On �̹���
    private Sprite soundOffImage;                               // ���� Off �̹���

    // ======================================================================================================================
    // [Display]

    [System.Serializable]
    private class ToggleSettings
    {
        public Button toggleButton;                             // ��� ��ư
        public RectTransform handle;                            // ��� �ڵ�
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

    // ======================================================================================================================

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
        optionMenuPanel.SetActive(false);

        InitializeOptionManager();
    }

    private void Start()
    {
        activePanelManager = FindObjectOfType<ActivePanelManager>();
        activePanelManager.RegisterPanel("OptionMenu", optionMenuPanel, optionButtonImage);
    }

    // ======================================================================================================================

    // ��� OptionManager ���� �Լ��� ����
    private void InitializeOptionManager()
    {
        InitializeOptionButton();
        InitializeSubMenuButtons();

        InitializeSoundControllers();
        InitializeDisplayControllers();
    }

    // OptionButton �ʱ�ȭ �Լ�
    private void InitializeOptionButton()
    {
        optionButton.onClick.AddListener(() => activePanelManager.TogglePanel("OptionMenu"));
        optionBackButton.onClick.AddListener(() => activePanelManager.ClosePanel("OptionMenu"));
    }

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
        soundOnImage = Resources.Load<Sprite>("Sprites/Cats/1");
        soundOffImage = Resources.Load<Sprite>("Sprites/Cats/2");
        bgmSettings.onOffImage.sprite = soundOnImage;
        sfxSettings.onOffImage.sprite = soundOnImage;

        // ��� ��ư �̺�Ʈ ����
        bgmSettings.toggleButton.onClick.AddListener(() => ToggleSound(true));
        sfxSettings.toggleButton.onClick.AddListener(() => ToggleSound(false));

        UpdateToggleUI(bgmSettings.isOn, true, true);
        UpdateToggleUI(sfxSettings.isOn, false, true);
    }

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
        UpdateToggleUI(settings.handle, settings.isOn, true);
    }

    // ======================================================================================================================
    // [���� �޴�]

    // ���� �޴� ��ư �ʱ�ȭ �� Ŭ�� �̺�Ʈ �߰� �Լ�
    private void InitializeSubMenuButtons()
    {
        for (int i = 0; i < (int)OptionMenuType.End; i++)
        {
            int index = i;
            subOptionMenuButtons[index].onClick.AddListener(() => ActivateMenu((OptionMenuType)index));
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
        string colorCode = isActive ? activeColorCode : inactiveColorCode;
        if (ColorUtility.TryParseHtmlString(colorCode, out Color color))
        {
            buttonImage.color = color;
        }
    }

    // ======================================================================================================================
    // [���� ����]

    // ���� On/Off ��� �Լ�
    public void ToggleSound(bool isBgm)
    {
        var settings = isBgm ? bgmSettings : sfxSettings;
        settings.isOn = !settings.isOn;
        SetSoundToggleImage(isBgm);
        UpdateToggleUI(settings.isOn, isBgm);
    }

    // ���� On/Off �̹��� ���� �Լ�
    private void SetSoundToggleImage(bool isBgm)
    {
        var settings = isBgm ? bgmSettings : sfxSettings;
        Image targetImage = settings.onOffImage;
        targetImage.sprite = (!settings.isOn || settings.slider.value == 0) ? soundOffImage : soundOnImage;
    }

    // ���� ��� UI ������Ʈ �Լ�
    private void UpdateToggleUI(bool state, bool isBgm, bool instant = false)
    {
        var settings = isBgm ? bgmSettings : sfxSettings;
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

    // ======================================================================================================================
    // [���÷��� ����]

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

    // ======================================================================================================================



}