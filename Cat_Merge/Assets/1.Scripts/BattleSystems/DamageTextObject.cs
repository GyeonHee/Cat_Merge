using UnityEngine;
using TMPro;

// ������ �ؽ�Ʈ ������Ʈ ��ũ��Ʈ
public class DamageTextObject : MonoBehaviour
{


    #region Variables

    public TextMeshProUGUI text;
    public RectTransform rectTransform;

    #endregion


    #region Unity Methods

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        rectTransform = GetComponent<RectTransform>();
    }

    #endregion


}
