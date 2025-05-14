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
    isAttack,
    isPick
}

[System.Serializable]
public struct GradeOverrideData
{
    public int grade;
    public AnimatorOverrideController overrideController;
}

public class AnimatorManager : MonoBehaviour
{


    #region Variables

    private Animator animator;
    public int catGrade;

    [Header("��޺� �ִϸ����� �������̵� ����Ʈ")]
    public List<GradeOverrideData> overrideDataList;
    public Dictionary<int, AnimatorOverrideController> overrideDict;

    private CatState currentState;

    #endregion


    #region Unity Methods

    private void Awake()
    {
        InitializeAnimator();
        InitializeOverrideDict();
        ApplyTitleSceneAnimation();
    }

    #endregion


    #region Initialize

    // �ִϸ����� ������Ʈ �ʱ�ȭ �Լ�
    private void InitializeAnimator()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    // �������̵� ��ųʸ� �ʱ�ȭ �Լ�
    private void InitializeOverrideDict()
    {
        overrideDict = new Dictionary<int, AnimatorOverrideController>();
        foreach (var data in overrideDataList)
        {
            if (!overrideDict.ContainsKey(data.grade))
            {
                overrideDict.Add(data.grade, data.overrideController);
            }
        }
    }

    // Ÿ��Ʋ �������� �ִϸ��̼� ���� �Լ�
    private void ApplyTitleSceneAnimation()
    {
        if (SceneManager.GetActiveScene().name == "TitleScene")
        {
            ApplyAnim(catGrade);
        }
    }

    #endregion


    #region State Management

    // ����� ���� ���� �� �ִϸ��̼� ���� �Լ�
    public void ChangeState(CatState newState)
    {
        if (currentState == newState) return;

        ResetAllStateBools();
        SetBoolForState(newState);

        currentState = newState;
    }

    // ��� ���� bool �� �ʱ�ȭ �Լ�
    private void ResetAllStateBools()
    {
        animator.SetBool("isIdle", false);
        animator.SetBool("isWalk", false);
        animator.SetBool("isFaint", false);
        animator.SetBool("isGetCoin", false);
        animator.SetBool("isGrab", false);
        animator.SetBool("isBattle", false);
        animator.SetBool("isAttack", false);
        animator.SetBool("isPick", false);
    }

    // Ư�� ������ bool ���� true�� �����ϴ� �Լ�
    private void SetBoolForState(CatState state)
    {
        animator.SetBool(state.ToString(), true);
    }

    #endregion


    #region Animation Override

    // Ư�� ����� �ִϸ����� �������̵� ��Ʈ�ѷ� ���� �Լ�
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

    // �ִϸ��̼� ���� �Լ�
    public void ApplyAnim(int grade)
    {
        ApplyAnimatorOverride(grade);
    }

    #endregion


}
