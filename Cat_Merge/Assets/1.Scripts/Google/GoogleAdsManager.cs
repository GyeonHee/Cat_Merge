using UnityEngine;
using GoogleMobileAds;
using GoogleMobileAds.Api;
using System;
using System.Collections;

// ���� ���� ���� ��ũ��Ʈ
public class GoogleAdsManager : MonoBehaviour
{


    #region Variables

    // ���� ���� ID
#if UNITY_ANDROID
    private string _productionAdUnitId = "ca-app-pub-6387288948977074/9379084963";
    private string _testAdUnitId = "ca-app-pub-3940256099942544/5354046379";
#elif UNITY_IPHONE
    // IPHONE�� �������� �����Ƿ� �Ѵ� �׽�Ʈ�� ��ü
    private string _productionAdUnitId = "ca-app-pub-3940256099942544/6978759866";
    private string _testAdUnitId = "ca-app-pub-3940256099942544/6978759866";
#else
    private string _productionAdUnitId = "unused";
    private string _testAdUnitId = "unused";
#endif

    private string _adUnitId;
    private bool isInitialized = false;
    private int retryAttempt;
    private const int MAX_RETRY_ATTEMPTS = 3;

    private RewardedInterstitialAd rewardedInterstitialAd;
    private bool isRewardEarned = false;
    private bool isLoadingAd = false;

    #endregion


    #region Unity Methods

    public void Awake()
    {
#if UNITY_EDITOR
        _adUnitId = _testAdUnitId;
#else
        _adUnitId = _productionAdUnitId;
#endif

        // Google Mobile Ads SDK �ʱ�ȭ
        MobileAds.Initialize((InitializationStatus initStatus) =>
        {
            isInitialized = true;

            StartCoroutine(InitialAdLoadRoutine());
        });
    }

    private IEnumerator InitialAdLoadRoutine()
    {
        int initialLoadAttempts = 0;
        const int MAX_INITIAL_ATTEMPTS = 3;

        while (initialLoadAttempts < MAX_INITIAL_ATTEMPTS)
        {
            if (!isLoadingAd && (rewardedInterstitialAd == null || !rewardedInterstitialAd.CanShowAd()))
            {
                LoadRewardedInterstitialAd();
                yield return new WaitForSeconds(2f);
            }
            else if (rewardedInterstitialAd != null && rewardedInterstitialAd.CanShowAd())
            {
                break;
            }
            initialLoadAttempts++;
        }
    }

    public void Start()
    {
        retryAttempt = 0;
    }

    #endregion


    #region Load Ad

    public void LoadRewardedInterstitialAd()
    {
        if (!isInitialized || isLoadingAd)
        {
            //Debug.Log("AdMob SDK�� ���� �ʱ�ȭ���� �ʾҽ��ϴ�.");
            return;
        }

        isLoadingAd = true;

        // ���ο� ���� �ε��ϱ� ���� ���� ���� ����
        if (rewardedInterstitialAd != null)
        {
            rewardedInterstitialAd.Destroy();
            rewardedInterstitialAd = null;
        }

        //Debug.Log($"������ ���� ���� �ε��մϴ�. �õ� #{retryAttempt + 1}");

        var adRequest = new AdRequest();
        //adRequest.Keywords.Add("unity-admob-sample");

        RewardedInterstitialAd.Load(_adUnitId, adRequest,
            (RewardedInterstitialAd ad, LoadAdError error) =>
            {
                isLoadingAd = false;

                if (error != null || ad == null)
                {
                    //Debug.Log($"������ ���� ���� �ε� ����. ����: {error?.GetMessage()}");

                    // ��õ� ����
                    if (retryAttempt < MAX_RETRY_ATTEMPTS)
                    {
                        retryAttempt++;
                        float delay = Mathf.Pow(2, retryAttempt);
                        //Debug.Log($"{delay}�� �� ��õ��մϴ�...");
                        Invoke(nameof(LoadRewardedInterstitialAd), delay);
                    }
                    else
                    {
                        //Debug.Log("�ִ� ��õ� Ƚ���� �ʰ��߽��ϴ�.");
                        ShopManager.Instance?.ResetAdState();
                    }
                    return;
                }

                //Debug.Log("������ ���� ���� ���������� �ε��Ǿ����ϴ�.");
                rewardedInterstitialAd = ad;
                retryAttempt = 0;

                // ���� �̺�Ʈ �ڵ鷯 ���
                RegisterEventHandlers(rewardedInterstitialAd);
            });
    }

    private void RegisterEventHandlers(RewardedInterstitialAd ad)
    {
        // ���� ���۵� �� ���� �÷��� �ʱ�ȭ
        ad.OnAdFullScreenContentOpened += () =>
        {
            isRewardEarned = false;
            //Debug.Log("���� ǥ�õǾ����ϴ�.");
        };

        // ���� ���� ��
        ad.OnAdFullScreenContentClosed += () =>
        {
            //Debug.Log("���� �������ϴ�.");
            if (!isRewardEarned)
            {
                ShopManager.Instance?.ResetAdState();
            }
            LoadRewardedInterstitialAd();
        };

        // ���� ǥ�� ���н�
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            //Debug.Log($"���� ǥ�� ����: {error.GetMessage()}");
            LoadRewardedInterstitialAd();
        };

        // ���� ȹ���
        ad.OnAdPaid += (AdValue adValue) =>
        {
            //string msg = string.Format("���� ������ ���޵Ǿ����ϴ�. (��ȭ: {0}, ��: {1}, ���е�: {2})", adValue.CurrencyCode, adValue.Value, adValue.Precision);
            //Debug.Log(msg);
        };
    }

    #endregion


    #region Show Ad

    // CashForAd ���� (ShopManager)
    public void ShowRewardedCashForAd()
    {
        if (!isInitialized)
        {
            //Debug.Log("AdMob SDK�� ���� �ʱ�ȭ���� �ʾҽ��ϴ�.");
            ShopManager.Instance?.ResetAdState();
            return;
        }

        if (rewardedInterstitialAd != null && rewardedInterstitialAd.CanShowAd())
        {
            try
            {
                rewardedInterstitialAd.Show((Reward reward) =>
                {
                    isRewardEarned = true;  // ���� ȹ�� �� �÷��� ����
                    //Debug.Log($"CashForAd ������ ���޵˴ϴ�: {reward.Type}");
                    ShopManager.Instance.OnCashForAdRewardComplete();
                });
            }
            catch (Exception e)
            {
                //Debug.Log($"���� ǥ�� �� ���� �߻�: {e.Message}");
                ShopManager.Instance?.ResetAdState();
                LoadRewardedInterstitialAd();
            }
        }
        else
        {
            //Debug.Log("���� �غ���� �ʾҽ��ϴ�. ���ο� ���� �ε��մϴ�.");
            ShopManager.Instance?.ResetAdState();
            LoadRewardedInterstitialAd();
        }
    }

    // DoubleCoinForAd ���� (ShopManager)
    public void ShowRewardedDoubleCoinForAd()
    {
        if (!isInitialized)
        {
            //Debug.Log("AdMob SDK�� ���� �ʱ�ȭ���� �ʾҽ��ϴ�.");
            ShopManager.Instance?.ResetAdState();
            return;
        }

        if (rewardedInterstitialAd != null && rewardedInterstitialAd.CanShowAd())
        {
            try
            {
                rewardedInterstitialAd.Show((Reward reward) =>
                {
                    //Debug.Log($"DoubleCoinForAd ������ ���޵˴ϴ�: {reward.Type}");
                    ShopManager.Instance.OnDoubleCoinAdRewardComplete();
                    LoadRewardedInterstitialAd();
                });
            }
            catch (Exception e)
            {
                //Debug.Log($"���� ǥ�� �� ���� �߻�: {e.Message}");
                ShopManager.Instance?.ResetAdState();
                LoadRewardedInterstitialAd();
            }
        }
        else
        {
            //Debug.Log("���� �غ���� �ʾҽ��ϴ�. ���ο� ���� �ε��մϴ�.");
            ShopManager.Instance?.ResetAdState();
            LoadRewardedInterstitialAd();
        }
    }

    #endregion


}
