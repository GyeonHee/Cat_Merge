using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{


    #region Variables

    [SerializeField] private Button startButton;

    #endregion


    #region Unity Methods

    private void Start()
    {
        Application.targetFrameRate = 60;

        startButton.gameObject.SetActive(false);
    }

    #endregion


    #region Game Start

    // ���� ���� ��ư Ȱ��ȭ �Լ�
    public void EnableStartButton()
    {
        startButton.gameObject.SetActive(true);
    }

    // ���� ���� ��ư Ŭ���� ȣ��� �Լ�
    public void OnGameStart()
    {
        GetComponent<TitleAnimationManager>().StopBlinkAnimation();
        GetComponent<TitleAnimationManager>().StopBreathingAnimation();
        GetComponent<TitleAnimationManager>().StopCatAutoMovement();
    }

    #endregion


}
