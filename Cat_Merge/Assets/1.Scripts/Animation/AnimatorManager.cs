using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public enum CharacterState
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

    private CharacterState currentState;


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
    }

    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Alpha1)) ChangeState(CharacterState.isWalk);
        //if (Input.GetKeyDown(KeyCode.Alpha2)) ChangeState(CharacterState.isFaint);
        //if (Input.GetKeyDown(KeyCode.Alpha3)) ChangeState(CharacterState.isGetCoin);
        //if (Input.GetKeyDown(KeyCode.Alpha4)) ChangeState(CharacterState.isGrab);
        //if (Input.GetKeyDown(KeyCode.Alpha5)) ChangeState(CharacterState.isBattle);
        //if (Input.GetKeyDown(KeyCode.Alpha6)) ChangeState(CharacterState.isAttack);
    }
    public void ChangeState(CharacterState newState)
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

    private void SetBoolForState(CharacterState state)
    {
        animator.SetBool(state.ToString(), true);
    }

    public void ApplyAnimatorOverride(int grade)
    {
        if (overrideDict.ContainsKey(grade))
        {
            animator.runtimeAnimatorController = overrideDict[grade];
        }
        else
        {
            Debug.LogWarning($"�ش� ���({grade})�� �������̵� ��Ʈ�ѷ��� ��ϵ��� �ʾҽ��ϴ�!");
        }
    }

    public void ApplyAnim(int grade)
    {
        ApplyAnimatorOverride(grade);
    }
}

