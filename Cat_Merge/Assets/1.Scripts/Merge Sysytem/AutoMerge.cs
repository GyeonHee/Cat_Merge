using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class AutoMerge : MonoBehaviour
{
    private float autoMergeDuration;            // �ڵ� ���� ���� �ð�
    private float autoMergeInterval;            // �ڵ� ���� ����
    private float moveDuration;                 // ����̰� �̵��ϴ� �� �ɸ��� �ð� (�̵� �ӵ�)
    private Vector2 mergePosition;              // �ڵ� ���� ��ġ
    private bool isAutoMergeActive;             // �ڵ� ���� Ȱ��ȭ ����

    private void Awake()
    {
        autoMergeDuration = 3.0f;
        autoMergeInterval = 0.5f;
        moveDuration = 0.2f;

        isAutoMergeActive = false;
    }

    public void StartAutoMerge()
    {
        if (!isAutoMergeActive)
        {
            StartCoroutine(AutoMergeCoroutine());
        }
    }

    private IEnumerator AutoMergeCoroutine()
    {
        isAutoMergeActive = true;
        float elapsedTime = 0f;

        // autoMergeDuration ���� �ڵ� ������ �����
        while (elapsedTime < autoMergeDuration)
        {
            // `autoMergeInterval`��ŭ ��ٸ� �� ����
            yield return new WaitForSeconds(autoMergeInterval);

            // ��� �ð� ������Ʈ
            elapsedTime += autoMergeInterval;

            // ����̵��� ��޺��� ����
            var allCats = FindObjectsOfType<CatDragAndDrop>().OrderBy(cat => cat.catData.CatId).ToList();
            var groupedCats = allCats.GroupBy(cat => cat.catData.CatId).Where(group => group.Count() > 1).ToList();

            // �ռ��� ����̰� ���� ��� ���
            if (!groupedCats.Any()) continue;

            // ��޺��� �ռ��� ����̸� ã�� �ռ�
            foreach (var group in groupedCats)
            {
                var catsInGroup = group.ToList();

                // 2�� �̻��� ����̰� ������ �ռ�
                while (catsInGroup.Count >= 2)
                {
                    CatDragAndDrop cat1 = catsInGroup[0];
                    CatDragAndDrop cat2 = catsInGroup[1];

                    // �ռ��� ������� ���� ����� Ȯ��
                    Cat nextCat = FindObjectOfType<CatMerge>().GetCatById(cat1.catData.CatId + 1);

                    // ���� ��� ����̰� ������ �ռ����� ����
                    if (nextCat == null)
                    {
                        Debug.LogWarning("���� ����� ����̰� ����");
                        break;
                    }

                    // �ռ� ��ġ ����
                    RectTransform parentRect = cat1.rectTransform.parent.GetComponent<RectTransform>();
                    mergePosition = GetRandomPosition(parentRect);

                    // �� ����̰� �ռ� ��ġ�� �̵�
                    StartCoroutine(MoveCatSmoothly(cat1, mergePosition));
                    StartCoroutine(MoveCatSmoothly(cat2, mergePosition));

                    // �� ����̰� ��� �������� �� �ռ� ����
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

                    // �ռ� �� ���ݸ�ŭ ��ٸ�
                    yield return new WaitForSeconds(autoMergeInterval);
                }
            }
        }

        // �ð��� ������ �ڵ� ���� ��Ȱ��ȭ
        isAutoMergeActive = false;
        Debug.Log("�ð� ����");
    }

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
