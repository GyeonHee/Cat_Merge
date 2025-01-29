using UnityEngine;
using UnityEngine.UI;

public class OptionManager : MonoBehaviour
{
    // Singleton Instance
    public static OptionManager Instance { get; private set; }

    [Header("---[OptionManager]")]
    [SerializeField] private Button optionButton;                               // �ɼ� ��ư
    [SerializeField] private Image optionButtonImage;                           // �ɼ� ��ư �̹���
    [SerializeField] private GameObject optionMenuPanel;                        // �ɼ� �޴� Panel
    [SerializeField] private Button optionBackButton;                           // �ɼ� �ڷΰ��� ��ư
    private ActivePanelManager activePanelManager;                              // ActivePanelManager

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

    private void InitializeOptionManager()
    {
        InitializeOptionButton();
    }

    private void InitializeOptionButton()
    {
        optionButton.onClick.AddListener(() => activePanelManager.TogglePanel("OptionMenu"));
        optionBackButton.onClick.AddListener(() => activePanelManager.ClosePanel("OptionMenu"));
    }

    // ======================================================================================================================



}
