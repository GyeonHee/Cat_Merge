using UnityEngine;
using System.Collections;
using System.Linq;

public class AutoMerge : MonoBehaviour
{
    private float startTime;                            // �ڵ� ���� ���� �ð�
    private float autoMergeDuration = 3.0f;             // �ڵ� ���� �⺻ ���� �ð�
    private float currentAutoMergeDuration;             // ���� �ڵ� ���� ���� �ð�
    private float plusAutoMergeDuration;                // �ڵ� ���� �߰� �ð�
    private float autoMergeInterval = 0.5f;             // �ڵ� ���� ����
    private float moveDuration = 0.2f;                  // ����̰� �̵��ϴ� �� �ɸ��� �ð� (�̵� �ӵ�)
    private bool isAutoMergeActive = false;             // �ڵ� ���� Ȱ��ȭ ����

    private void Awake()
    {
        plusAutoMergeDuration = autoMergeDuration;
        currentAutoMergeDuration = autoMergeDuration;
    }

    // �ڵ� ���� ����
    public void StartAutoMerge()
    {
        if (!isAutoMergeActive)
        {
            Debug.Log("�ڵ���������");
            startTime = Time.time;
            currentAutoMergeDuration = autoMergeDuration;
            StartCoroutine(AutoMergeCoroutine());
        }
        else
        {
            Debug.Log($"{plusAutoMergeDuration}�� �߰�");
            currentAutoMergeDuration += plusAutoMergeDuration;
        }
    }

    // �ڵ� ���� �ڷ�ƾ
    private IEnumerator AutoMergeCoroutine()
    {
        isAutoMergeActive = true;

        while (Time.time - startTime - currentAutoMergeDuration < 0)
        {
            // ����̵� ��޺��� ����
            var allCats = FindObjectsOfType<CatDragAndDrop>().OrderBy(cat => cat.catData.CatId).ToList();
            var groupedCats = allCats.GroupBy(cat => cat.catData.CatId).Where(group => group.Count() > 1).ToList();

            // �ռ� ������ ����̰� ������ ���� �ð� ���
            if (!groupedCats.Any())
            {
                yield return new WaitForSeconds(autoMergeInterval);
                continue;
            }

            // ��޺��� ����� �ռ�
            foreach (var group in groupedCats)
            {
                var catsInGroup = group.ToList();

                // 2�� �̻��� ����̰� ������ �ռ�
                while (catsInGroup.Count >= 2 && (Time.time - startTime - currentAutoMergeDuration < 0))
                {
                    var cat1 = catsInGroup[0];
                    var cat2 = catsInGroup[1];

                    // ���� ����� ����� Ȯ��
                    Cat nextCat = FindObjectOfType<CatMerge>().GetCatById(cat1.catData.CatId + 1);
                    if (nextCat == null)
                    {
                        Debug.LogWarning("���� ����� ����̰� ����");
                        catsInGroup.Remove(cat1);
                        catsInGroup.Remove(cat2);
                        break;
                    }

                    // �ռ� ��ġ ���
                    RectTransform parentRect = cat1.rectTransform.parent.GetComponent<RectTransform>();
                    Vector2 mergePosition = GetRandomPosition(parentRect);

                    // �� ����̰� ��� �ռ� ������ �������� �� �ռ� ����
                    StartCoroutine(MoveCatSmoothly(cat1, mergePosition));
                    StartCoroutine(MoveCatSmoothly(cat2, mergePosition));
                    yield return new WaitUntil(() => cat1.rectTransform.anchoredPosition == mergePosition && cat2.rectTransform.anchoredPosition == mergePosition);

                    // �ռ� ó��
                    Cat mergedCat = FindObjectOfType<CatMerge>().MergeCats(cat1.catData, cat2.catData);
                    if (mergedCat != null)
                    {
                        cat1.catData = mergedCat;
                        cat1.UpdateCatUI();
                        Destroy(cat2.gameObject);
                    }

                    // �ռ� �� ���� ����� ��� ����
                    catsInGroup.RemoveAt(0);
                    catsInGroup.RemoveAt(0);

                    // �ռ� �� �ڵ� ���� ���ݸ�ŭ ���
                    yield return new WaitForSeconds(autoMergeInterval);
                }
                if (Time.time - startTime - currentAutoMergeDuration >= 0) break;
            }
            yield return null;
        }

        isAutoMergeActive = false;
        Debug.Log("�ڵ� ���� ����");
    }

    // �θ� RectTransform���� ���� ��ġ ���
    private Vector2 GetRandomPosition(RectTransform parentRect)
    {
        float panelWidth = parentRect.rect.width;
        float panelHeight = parentRect.rect.height;
        float randomX = Random.Range(-panelWidth / 2, panelWidth / 2);
        float randomY = Random.Range(-panelHeight / 2, panelHeight / 2);
        return new Vector2(randomX, randomY);
    }

    // ����̰� �ε巴�� �̵��ϴ� �ڷ�ƾ
    private IEnumerator MoveCatSmoothly(CatDragAndDrop cat, Vector2 targetPosition)
    {
        Vector2 startPos = cat.rectTransform.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveDuration);
            cat.rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPosition, t);
            yield return null;
        }
    }


}
