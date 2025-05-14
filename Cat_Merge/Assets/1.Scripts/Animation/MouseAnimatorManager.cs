using UnityEngine;

public enum MouseState
{
    isIdle,
    isFaint,
    isAttack1,
    isAttack2,
    isAttack3,
}

public class MouseAnimatorManager : MonoBehaviour
{


    #region Variables

    private Animator animator;
    private MouseState currentState;

    #endregion


    #region Unity Methods

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    #endregion


    #region State Management

    // �� ���� ���� �� �ִϸ��̼� ���� �Լ�
    public void ChangeState(MouseState newState)
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
        animator.SetBool("isFaint", false);
        animator.SetBool("isAttack1", false);
        animator.SetBool("isAttack2", false);
        animator.SetBool("isAttack3", false);
    }

    // Ư�� ������ bool ���� true�� �����ϴ� �Լ�
    private void SetBoolForState(MouseState state)
    {
        animator.SetBool(state.ToString(), true);
    }

    #endregion


}