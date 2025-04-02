using UnityEngine;

// ī�޶� ȭ�� ������ 9:16���� �����ϴ� ��ũ��Ʈ
public class CameraManager : MonoBehaviour
{
    private float targetAspect = 9f / 16f;
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = GetComponent<Camera>();

        SetupAspectRatio();
    }

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

    
}
