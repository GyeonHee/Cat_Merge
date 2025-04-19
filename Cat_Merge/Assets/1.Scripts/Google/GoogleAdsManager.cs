using UnityEngine;
using GoogleMobileAds;
using GoogleMobileAds.Api;
using System;

public class GoogleAdsManager : MonoBehaviour
{


    #region Variables

    // These ad units are configured to always serve test ads.
#if UNITY_ANDROID
    private string _adUnitId = "ca-app-pub-3940256099942544/5354046379";
#elif UNITY_IPHONE
    private string _adUnitId = "ca-app-pub-3940256099942544/6978759866";
#else
    private string _adUnitId = "unused";
#endif

    private RewardedInterstitialAd rewardedInterstitialAd;

    #endregion


    #region Unity Methods

    public void Awake()
    {
        // Initialize the Google Mobile Ads SDK.
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
        // Clean up the old ad before loading a new one.
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
                    return;
                }

                //Debug.Log("������ ���� ���� ���������� �ε��Ǿ����ϴ�. ����: " + ad.GetResponseInfo());

                rewardedInterstitialAd = ad;
            });
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
                ShopManager.Instance.OnAdRewardComplete();

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
