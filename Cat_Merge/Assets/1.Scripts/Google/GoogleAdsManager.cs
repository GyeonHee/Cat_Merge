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
    private bool isInitialized = false;

    #endregion


    #region Unity Methods

    public void Awake()
    {
        // Google Mobile Ads SDK �ʱ�ȭ
        MobileAds.Initialize((InitializationStatus initStatus) =>
        {
            isInitialized = true;
        });
    }

    public void Start()
    {
        LoadRewardedInterstitialAd();
    }

    #endregion


    #region Load Ad

    // ��Ʈ��ũ �� �ʱ�ȭ ���� üũ �Լ� �߰�
    private bool CheckAdAvailability()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            //Debug.LogWarning("��Ʈ��ũ ���� �Ұ�");
            ShopManager.Instance.ResetAdState();
            return false;
        }

        if (!isInitialized)
        {
            //Debug.LogWarning("���� SDK�� �ʱ�ȭ���� ����");
            ShopManager.Instance.ResetAdState();
            return false;
        }

        return true;
    }

    public void LoadRewardedInterstitialAd()
    {
        if (!CheckAdAvailability()) return;

        // ���ο� ���� �ε��ϱ� ���� ���� ���� ����
        if (rewardedInterstitialAd != null)
        {
            rewardedInterstitialAd.Destroy();
            rewardedInterstitialAd = null;
        }

        //Debug.Log("������ ���� ���� �ε�");

        var adRequest = new AdRequest();
        adRequest.Keywords.Add("unity-admob-sample");

        RewardedInterstitialAd.Load(_adUnitId, adRequest,
            (RewardedInterstitialAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    //Debug.Log("������ ���� ���� �ε� ����. ����: " + error);
                    ShopManager.Instance.ResetAdState();
                    return;
                }

                //Debug.Log("������ ���� ���� ���������� �ε��Ǿ����ϴ�. ����: " + ad.GetResponseInfo());
                rewardedInterstitialAd = ad;
                RegisterEventHandlers(ad);
            });
    }

    // ���� �̺�Ʈ �ڵ鷯 ���
    private void RegisterEventHandlers(RewardedInterstitialAd ad)
    {
        // ���� ǥ�� ����
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            //Debug.LogError($"���� ǥ�� ����: {error.GetMessage()}");
            ShopManager.Instance.ResetAdState();
            LoadRewardedInterstitialAd();
        };

        // ���� ǥ�õ�
        ad.OnAdFullScreenContentOpened += () =>
        {
            //Debug.Log("���� ǥ�õ�");
        };

        // ���� ����
        ad.OnAdFullScreenContentClosed += () =>
        {
            //Debug.Log("���� ����");
            LoadRewardedInterstitialAd();
        };

        // ���� Ŭ��
        ad.OnAdClicked += () =>
        {
            //Debug.Log("���� Ŭ����");
        };

        // ���� ���� ���
        ad.OnAdImpressionRecorded += () =>
        {
            //Debug.Log("���� ������ ��ϵ�");
        };

        // ���� ���� �߻�
        ad.OnAdPaid += (AdValue adValue) =>
        {
            //Debug.Log($"���� ���� �߻�: ��ȭ={adValue.CurrencyCode}, �ݾ�={adValue.Value}");
        };
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            ShopManager.Instance.ResetAdState();
        }
    }

    #endregion


    #region Show Ad

    // CashForAd ���� (ShopManager)
    public void ShowRewardedCashForAd()
    {
        if (rewardedInterstitialAd != null && rewardedInterstitialAd.CanShowAd())
        {
            rewardedInterstitialAd.Show((Reward reward) =>
            {
                ShopManager.Instance.OnCashForAdRewardComplete();

                LoadRewardedInterstitialAd();
            });
        }
        else
        {
            LoadRewardedInterstitialAd();
        }
    }

    // DoubleCoinForAd ���� (ShopManager)
    public void ShowRewardedDoubleCoinForAd()
    {
        if (rewardedInterstitialAd != null && rewardedInterstitialAd.CanShowAd())
        {
            rewardedInterstitialAd.Show((Reward reward) =>
            {
                ShopManager.Instance.OnDoubleCoinAdRewardComplete();

                LoadRewardedInterstitialAd();
            });
        }
        else
        {
            LoadRewardedInterstitialAd();
        }
    }

    #endregion


}
