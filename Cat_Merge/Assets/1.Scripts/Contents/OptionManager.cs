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
            if ((audioSource == Instance.bgmController.audioSource && !Instance.isBgmOn) || 
                (audioSource == Instance.sfxController.audioSource && !Instance.isSfxOn))
            {
                audioSource.volume = 0f;
            }
            else
            {
                audioSource.volume = volume;
            }
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
    }

    // ======================================================================================================================

    [Header("---[OptionManager]")]
    [SerializeField] private Button optionButton;               // �ɼ� ��ư
    [SerializeField] private Image optionButtonImage;           // �ɼ� ��ư �̹���
    [SerializeField] private GameObject optionMenuPanel;        // �ɼ� �޴� Panel
    [SerializeField] private Button optionBackButton;           // �ɼ� �ڷΰ��� ��ư
    private ActivePanelManager activePanelManager;              // ActivePanelManager

    [SerializeField] private GameObject[] mainOptionMenus;      // ���� �ɼ� �޴� Panels
    [SerializeField] private Button[] subOptionMenuButtons;     // ���� �ɼ� �޴� ��ư �迭

    // ======================================================================================================================
    // [Sound]

    [Header("---[BGM]")]
    [SerializeField] private Slider bgmSlider;                  // ����� ���� ���� �����̴�
    [SerializeField] private Button bgmSoundToggleButton;       // ����� On/Off ��ư
    [SerializeField] private RectTransform bgmSoundHandle;      // ����� ��� �ڵ�
    private SoundController bgmController;                      // ����� ��Ʈ�ѷ�
    private Coroutine bgmToggleCoroutine;                       // ����� ��� �ִϸ��̼� �ڷ�ƾ
    private bool isBgmOn = true;                                // ����� Ȱ��ȭ ����

    [Header("---[SFX]")]
    [SerializeField] private Slider sfxSlider;                  // ȿ���� ���� ���� �����̴�
    [SerializeField] private Button sfxSoundToggleButton;       // ȿ���� On/Off ��ư
    [SerializeField] private RectTransform sfxSoundHandle;      // ȿ���� ��� �ڵ�
    private SoundController sfxController;                      // ȿ���� ��Ʈ�ѷ�
    private Coroutine sfxToggleCoroutine;                       // ȿ���� ��� �ִϸ��̼� �ڷ�ƾ
    private bool isSfxOn = true;                                // ȿ���� Ȱ��ȭ ����

    [Header("---[Common]")]
    private float onX = 65f, offX = -65f;                       // ���� �ڵ� ��ư x��ǥ
    private float moveDuration = 0.2f;                          // ��� �ִϸ��̼� ���� �ð�

    // ======================================================================================================================

    [Header("---[Sub Menu UI Color]")]
    private string activeColorCode = "#5f5f5f";                 // Ȱ��ȭ���� Color
    private string inactiveColorCode = "#FFFFFF";               // ��Ȱ��ȭ���� Color

    // ======================================================================================================================

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
    }

    // OptionButton �ʱ� ���� �Լ�
    private void InitializeOptionButton()
    {
        optionButton.onClick.AddListener(() => activePanelManager.TogglePanel("OptionMenu"));
        optionBackButton.onClick.AddListener(() => activePanelManager.ClosePanel("OptionMenu"));
    }

    // Sound �ʱ� ���� �Լ�
    private void InitializeSoundControllers()
    {
        AudioClip bgmClip = Resources.Load<AudioClip>("Audios/BGM_Sound");
        AudioClip sfxClip = Resources.Load<AudioClip>("Audios/SFX_Sound");
        bgmController = new SoundController(gameObject, bgmSlider, true, bgmClip);
        sfxController = new SoundController(gameObject, sfxSlider, false, sfxClip);

        bgmController.SetVolume(bgmSlider.value);
        sfxController.SetVolume(sfxSlider.value);

        bgmController.Play();

        bgmSoundToggleButton.onClick.AddListener(() => ToggleSound(true));
        sfxSoundToggleButton.onClick.AddListener(() => ToggleSound(false));

        UpdateToggleUI(isBgmOn, true, true);
        UpdateToggleUI(isSfxOn, false, true);
    }

    // ======================================================================================================================
    // [���� ����]

    // ���� On/Off ��ư �Լ�
    public void ToggleSound(bool isBgm)
    {
        if (isBgm)
        {
            isBgmOn = !isBgmOn;
        }
        else
        {
            isSfxOn = !isSfxOn;
        }

        UpdateToggleUI(isBgm ? isBgmOn : isSfxOn, isBgm);
    }

    // ���� ��ư UI ������Ʈ �Լ�
    private void UpdateToggleUI(bool state, bool isBgm, bool instant = false)
    {
        float targetX = state ? onX : offX;
        float targetVolume = state ? (isBgm ? bgmSlider.value : sfxSlider.value) : 0.0f;
        RectTransform soundHandle = isBgm ? bgmSoundHandle : sfxSoundHandle;
        SoundController controller = isBgm ? bgmController : sfxController;

        if (instant)
        {
            soundHandle.anchoredPosition = new Vector2(targetX, soundHandle.anchoredPosition.y);
            controller.SetVolume(targetVolume);
        }
        else
        {
            // ���� ����� �ڷ�ƾ ������ ����
            ref Coroutine toggleCoroutine = ref (isBgm ? ref bgmToggleCoroutine : ref sfxToggleCoroutine);

            StopAndStartCoroutine(ref toggleCoroutine, AnimateToggle(soundHandle, targetX, controller, targetVolume));
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
        controller.SetVolume(targetVolume);
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


}
