using UnityEngine;

// ī�޶� ȭ�� ������ 9:16���� �����ϴ� ��ũ��Ʈ
public class CameraManager : MonoBehaviour
{
    private float targetAspect = 9f / 16f;

    private void Awake()
    {
        Camera cam = GetComponent<Camera>();
        float currentAspect = (float)Screen.width / Screen.height;
        float scaleHeight = currentAspect / targetAspect;

        Rect rect = cam.rect;

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

        cam.rect = rect;
    }
}
