using UnityEngine;

// ī�޶� ȭ�� ������ 9:16���� �����ϴ� ��ũ��Ʈ
public class CameraManager : MonoBehaviour
{


    #region Variables

    private float targetAspect = 9f / 16f;
    private Camera mainCamera;

    #endregion


    #region Unity Methods

    private void Awake()
    {
        mainCamera = GetComponent<Camera>();
        SetupAspectRatio();
    }

    #endregion


    #region Camera Setup

    // ī�޶� ȭ�� ���� ���� �Լ�
    public void SetupAspectRatio()
    {
        float currentAspect = (float)Screen.width / Screen.height;
        float scaleHeight = currentAspect / targetAspect;

        Rect rect = mainCamera.rect;

        if (scaleHeight < 1)
        {
            rect.height = scaleHeight;
            rect.y = (1f - scaleHeight) * 0.5f;
        }
        else
        {
            float scaleWidth = 1f / scaleHeight;
            rect.width = scaleWidth;
            rect.x = (1f - scaleWidth) * 0.5f;
        }

        mainCamera.rect = rect;
    }

    #endregion


}
