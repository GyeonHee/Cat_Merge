using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum CatState
{
    isIdle,
    isWalk,
    isFaint,
    isGetCoin,
    isGrab,
    isBattle,
    isAttack
}

[System.Serializable]
public struct GradeOverrideData
{
    public int grade;
    public AnimatorOverrideController overrideController;
}

public class AnimatorManager : MonoBehaviour
{
    private Animator animator;
    public int catGrade;

    [Header("��޺� �ִϸ����� �������̵� ����Ʈ")]
    public List<GradeOverrideData> overrideDataList;
    public Dictionary<int, AnimatorOverrideController> overrideDict;

    private CatState currentState;


    void Awake()
    {
        animator = GetComponent<Animator>();

        // ��ųʸ� �ʱ�ȭ
        overrideDict = new Dictionary<int, AnimatorOverrideController>();
        foreach (var data in overrideDataList)
        {
            if (!overrideDict.ContainsKey(data.grade))
            {
                overrideDict.Add(data.grade, data.overrideController);
            }
        }

        // TitleScene�� ��� catGrade�� ����� ��� ����
        if (SceneManager.GetActiveScene().name == "TitleScene")
        {
            ApplyAnim(catGrade);
        }
    }

    public void ChangeState(CatState newState)
    {
        if (currentState == newState) return;

        ResetAllStateBools();
        SetBoolForState(newState);

        currentState = newState;
    }

    private void ResetAllStateBools()
    {
        animator.SetBool("isIdle", false);
        animator.SetBool("isWalk", false);
        animator.SetBool("isFaint", false);
        animator.SetBool("isGetCoin", false);
        animator.SetBool("isGrab", false);
        animator.SetBool("isBattle", false);
        animator.SetBool("isAttack", false);
    }

    private void SetBoolForState(CatState state)
    {
        animator.SetBool(state.ToString(), true);
    }

    public void ApplyAnimatorOverride(int grade)
    {
        if (overrideDict.ContainsKey(grade))
        {
            if (!animator.enabled) animator.enabled = true;
            animator.runtimeAnimatorController = overrideDict[grade];
        }
        else
        {
            animator.enabled = false;
        }
    }

    public void ApplyAnim(int grade)
    {
        ApplyAnimatorOverride(grade);
    }


}

