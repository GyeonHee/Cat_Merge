using UnityEngine;
using GoogleMobileAds;
using GoogleMobileAds.Api;
using System;

public class GoogleAdsManager : MonoBehaviour
{


    #region Variables

    // �׽�Ʈ ���� ���� ���� ���� ID ����
#if UNITY_ANDROID
    private string _adUnitId = "ca-app-pub-3940256099942544/5354046379";
#elif UNITY_IPHONE
    private string _adUnitId = "ca-app-pub-3940256099942544/6978759866";
#else
    private string _adUnitId = "unused";
#endif

    private RewardedInterstitialAd rewardedInterstitialAd;
    private bool isAdWatched = false;       // ���� ������ ��û�ߴ���
    private bool hasEarnedReward = false;   // ������ ���� �ڰ��� �ִ���
    private bool isAdShowing = false;       // ���� ���� �������� �ִ���

    private Action onAdCompleteCallback;    // ���� �Ϸ�� ������ �ݹ�

    #endregion


    #region Unity Methods

    public void Awake()
    {
        // Google Mobile Ads SDK �ʱ�ȭ
        MobileAds.Initialize((InitializationStatus initStatus) =>
        {
            // This callback is called once the MobileAds SDK is initialized.
        });
    }

    public void Start()
    {
        LoadRewardedInterstitialAd();
    }

    #endregion


    #region Load Ad

    public void LoadRewardedInterstitialAd()
    {
        // ���ο� ���� �ε��ϱ� ���� ���� ���� ����
        if (rewardedInterstitialAd != null)
        {
            rewardedInterstitialAd.Destroy();
            rewardedInterstitialAd = null;
        }

        var adRequest = new AdRequest();
        adRequest.Keywords.Add("unity-admob-sample");

        RewardedInterstitialAd.Load(_adUnitId, adRequest,
            (RewardedInterstitialAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogWarning("������ ���� ���� �ε� ����. ����: " + error);
                    return;
                }

                rewardedInterstitialAd = ad;
                RegisterEventHandlers(rewardedInterstitialAd);
            });
    }

    private void RegisterEventHandlers(RewardedInterstitialAd ad)
    {
        // ���� ���� ��
        ad.OnAdFullScreenContentClosed += () =>
        {
            HandleAdClosed();
        };

        // ���� ǥ�� ���н�
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogWarning("���� ǥ�� ����: " + error);
            HandleAdFailure();
        };

        // ���� ǥ�õ� ��
        ad.OnAdFullScreenContentOpened += () =>
        {
            isAdShowing = true;
            isAdWatched = false;
            hasEarnedReward = false;
        };

        // ���� ȹ���
        ad.OnAdPaid += (AdValue adValue) =>
        {
            isAdWatched = true;
        };
    }

    #endregion


    #region Show Ad

    // CashForAd ���� (ShopManager)
    public void ShowRewardedCashForAd()
    {
        ShowAd(() => ShopManager.Instance.OnCashForAdRewardComplete());
    }

    // DoubleCoinForAd ���� (ShopManager)
    public void ShowRewardedDoubleCoinForAd()
    {
        ShowAd(() => ShopManager.Instance.OnDoubleCoinAdRewardComplete());
    }

    private void ShowAd(Action onComplete)
    {
        if (rewardedInterstitialAd != null && rewardedInterstitialAd.CanShowAd())
        {
            onAdCompleteCallback = onComplete;

            // ���� ��û ���� ��ü ���� ������ ����
            GameManager.Instance.SaveGame();

            rewardedInterstitialAd.Show((Reward reward) =>
            {
                hasEarnedReward = true;
            });
        }
        else
        {
            Debug.LogWarning("���� ǥ���� �� �����ϴ�. ���ο� ���� �ε��մϴ�.");
            LoadRewardedInterstitialAd();
            ShopManager.Instance.ResetAdState();
        }
    }

    private void HandleAdClosed()
    {
        isAdShowing = false;

        // ���� ������ ���� ������ ���� �ڰ��� �ִ� ���
        if (isAdWatched && hasEarnedReward)
        {
            if (onAdCompleteCallback != null)
            {
                onAdCompleteCallback.Invoke();
            }
        }
        // ���� �߰��� �����߰ų� ������ ���� ���� ���
        else
        {
            ShopManager.Instance.ResetAdState();
        }

        // ���� �ʱ�ȭ
        isAdWatched = false;
        hasEarnedReward = false;
        onAdCompleteCallback = null;

        // ���ο� ���� �ε�
        LoadRewardedInterstitialAd();
    }

    private void HandleAdFailure()
    {
        isAdShowing = false;
        isAdWatched = false;
        hasEarnedReward = false;
        onAdCompleteCallback = null;

        ShopManager.Instance.ResetAdState();
        LoadRewardedInterstitialAd();
    }

    #endregion


}