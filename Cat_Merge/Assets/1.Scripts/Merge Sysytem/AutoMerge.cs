using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

// �ڵ� ���� Script
public class AutoMerge : MonoBehaviour
{
    private float startTime;                            // �ڵ� ���� ���� �ð�
    private float autoMergeDuration = 3.0f;             // �ڵ� ���� �⺻ ���� �ð�
    private float currentAutoMergeDuration;             // ���� �ڵ� ���� ���� �ð�
    private float plusAutoMergeDuration;                // �ڵ� ���� �߰� �ð�
    private float autoMergeInterval = 0.5f;             // �ڵ� ���� ����
    private float moveDuration = 0.2f;                  // ����̰� �̵��ϴ� �� �ɸ��� �ð� (�̵� �ӵ�)
    private bool isAutoMergeActive = false;             // �ڵ� ���� Ȱ��ȭ ����

    private HashSet<CatDragAndDrop> mergingCats = new HashSet<CatDragAndDrop>(); // ���� ���� ����� ����
    private GameManager gameManager;                    // gameManager

    private void Awake()
    {
        plusAutoMergeDuration = autoMergeDuration;
        currentAutoMergeDuration = autoMergeDuration;
        gameManager = GameManager.Instance;
    }

    // �ڵ� ���� ��ư Ŭ��
    public void OnClickedAutoMerge()
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

    // �ڵ� ���� ������ Ȯ���ϴ� �Լ�
    public bool IsMerging(CatDragAndDrop cat)
    {
        return mergingCats.Contains(cat);
    }

    // �ڵ� ���� �ڷ�ƾ
    private IEnumerator AutoMergeCoroutine()
    {
        isAutoMergeActive = true;

        // �ִ� ����� ���� ���� ����
        StartCoroutine(SpawnCatsWhileAutoMerge());

        while (Time.time - startTime - currentAutoMergeDuration < 0)
        {
            // mergingCats ���� ����
            mergingCats.RemoveWhere(cat => cat == null || cat.isDragging);

            var allCats = FindObjectsOfType<CatDragAndDrop>().OrderBy(cat => cat.catData.CatId).ToList();
            var groupedCats = allCats.GroupBy(cat => cat.catData.CatId).Where(group => group.Count() > 1).ToList();

            if (!groupedCats.Any())
            {
                yield return new WaitForSeconds(autoMergeInterval);
                continue;
            }

            foreach (var group in groupedCats)
            {
                var catsInGroup = group.ToList();

                while (catsInGroup.Count >= 2 && (Time.time - startTime - currentAutoMergeDuration < 0))
                {
                    var cat1 = catsInGroup[0];
                    var cat2 = catsInGroup[1];

                    // �ְ� ��� ��������� Ȯ��
                    if (IsMaxLevelCat(cat1.catData) || IsMaxLevelCat(cat2.catData))
                    {
                        catsInGroup.Remove(cat1);
                        catsInGroup.Remove(cat2);
                        continue;
                    }

                    // ����� ���� Ȯ��
                    if (cat1 == null || cat2 == null || cat1.isDragging || cat2.isDragging ||
                        mergingCats.Contains(cat1) || mergingCats.Contains(cat2))
                    {
                        catsInGroup.Remove(cat1);
                        catsInGroup.Remove(cat2);
                        continue;
                    }
                    mergingCats.Add(cat1);
                    mergingCats.Add(cat2);

                    RectTransform parentRect = cat1.rectTransform?.parent?.GetComponent<RectTransform>();
                    if (parentRect == null)
                    {
                        mergingCats.Remove(cat1);
                        mergingCats.Remove(cat2);
                        catsInGroup.Remove(cat1);
                        catsInGroup.Remove(cat2);
                        continue;
                    }

                    Vector2 mergePosition = GetRandomPosition(parentRect);

                    StartCoroutine(MoveCatSmoothly(cat1, mergePosition));
                    StartCoroutine(MoveCatSmoothly(cat2, mergePosition));

                    yield return new WaitUntil(() =>
                        cat1 == null || cat2 == null ||
                        (!mergingCats.Contains(cat1) && !mergingCats.Contains(cat2)) ||
                        (cat1.rectTransform.anchoredPosition == mergePosition && cat2.rectTransform.anchoredPosition == mergePosition)
                    );

                    if (cat1 != null && cat2 != null && mergingCats.Contains(cat1) && mergingCats.Contains(cat2))
                    {
                        Cat mergedCat = FindObjectOfType<CatMerge>().MergeCats(cat1.catData, cat2.catData);
                        if (mergedCat != null)
                        {
                            cat1.catData = mergedCat;
                            cat1.UpdateCatUI();
                            Destroy(cat2.gameObject);
                        }
                    }

                    mergingCats.Remove(cat1);
                    mergingCats.Remove(cat2);

                    catsInGroup.Remove(cat1);
                    catsInGroup.Remove(cat2);

                    yield return new WaitForSeconds(autoMergeInterval);
                }

                if (Time.time - startTime - currentAutoMergeDuration >= 0) break;
            }

            yield return null;
        }

        isAutoMergeActive = false;
        Debug.Log("�ڵ� ���� ����");
    }

    // �ڵ������� ����� �ִ�ġ�� ��ȯ�ϴ� �Լ�
    private IEnumerator SpawnCatsWhileAutoMerge()
    {
        CatSpawn catSpawn = FindObjectOfType<CatSpawn>();
        while (isAutoMergeActive)
        {
            while (gameManager.CanSpawnCat())
            {
                catSpawn.SpawnCat();
                yield return new WaitForSeconds(0.1f); // ����� �ڵ� ���� ����
            }

            yield return null;
        }
    }

    // �θ� RectTransform���� ���� ��ġ ��� �Լ�
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
        if (cat == null) yield break;

        Vector2 startPos = cat.rectTransform.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            if (cat == null) yield break;

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveDuration);
            cat.rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPosition, t);
            yield return null;
        }
    }

    // �ִ� ���� ����� Ȯ�� �Լ�
    private bool IsMaxLevelCat(Cat catData)
    {
        if (gameManager == null || gameManager.AllCatData == null)
        {
            return false;
        }

        return gameManager.AllCatData.All(cat => cat.CatId != catData.CatId + 1);
    }

    // Ư�� ������� mergingCats ���� ���� �Լ�
    public void StopMerging(CatDragAndDrop cat)
    {
        mergingCats.Remove(cat);
    }


}
