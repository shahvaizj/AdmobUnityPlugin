using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GoogleMobileAds.Api.Mediation.UnityAds;
public class AdsManager : MonoBehaviour
{
    public static AdsManager instance;

    private readonly TimeSpan APPOPEN_TIMEOUT = TimeSpan.FromHours(4);
    private DateTime appOpenExpireTime;

    public Text testTxt;
    private bool noAdsPurchased = false;
    private bool isShowingAppOpenAd;

    public enum AdType
    {
        BANNER,
        INTERSTITIAL,
        REWARDED
    };

    public enum RewardType
    {
        FREECOINS,
        DOUBLEREWARD,
        SKIPLEVEL,

    };
    public RewardType rewardType = RewardType.FREECOINS;

    public ScreenOrientation screenOrientation;

    private int rewardTypeVal = 0;
    private int rewardAmount = 0;

    private bool admobInitialized = false;

    private BannerView bannerAdView;
    private InterstitialAd interstitialAdView;
    private RewardedAd rewardedAdView;
    private AppOpenAd appOpenAdView;

    #region Keys
    private string bannerAdId = "ca-app-pub-3940256099942544/6300978111";
    private string interstitialAdId = "ca-app-pub-3940256099942544/1033173712";
    private string rewardedAdId = "ca-app-pub-3940256099942544/5224354917";
    private string openAdId = "ca-app-pub-3940256099942544/3419835294";
    #endregion

    [SerializeField] private AdSize bannerAdSize = AdSize.Banner;
    [SerializeField] private AdPosition bannerAdPosition = AdPosition.Top;

    [SerializeField] private bool initializeOnStart = true;

    public bool NoAdsPurchased { get => noAdsPurchased; set => noAdsPurchased = value; }

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        if (initializeOnStart)
            Initialize();
    }

    void Log(string _str)
    {

        Debug.Log("-- -- -- Ads=" + _str);

        if (testTxt)
        {
            testTxt.text = _str;
        }
    }

    public void Initialize()
    {
        Log("Initializing");

        MobileAds.SetiOSAppPauseOnBackground(true);

        List<String> deviceIds = new List<String>() { AdRequest.TestDeviceSimulator };

        // Add some test device IDs (replace with your own device IDs).
#if UNITY_IPHONE
        deviceIds.Add("");
#elif UNITY_ANDROID
        deviceIds.Add("");
#endif

        // Configure TagForChildDirectedTreatment and test device IDs.
        RequestConfiguration requestConfiguration =
            new RequestConfiguration.Builder()
            .SetTagForChildDirectedTreatment(TagForChildDirectedTreatment.True)
            .SetTagForUnderAgeOfConsent(TagForUnderAgeOfConsent.True)
            .SetTestDeviceIds(deviceIds).build();

        //EU consent and GDPR
        UnityAds.SetConsentMetaData("gdpr.consent", true);

        //CCPA consent
        UnityAds.SetConsentMetaData("privacy.consent", true);

        MobileAds.SetRequestConfiguration(requestConfiguration);

        MobileAds.Initialize(AdmobInitCompleteAction);
    }

    public void HideBannerAd()
    {
        DestroyBannerAd();
    }

    public void RequestBannerWithSpecs(AdSize _size, AdPosition _pos)
    {

        bannerAdSize = _size;
        bannerAdPosition = _pos;

        RequestAd(AdType.BANNER);
    }

    public void ShowRewardedAdWithSpecs(RewardType _type, int _rewardTypeVal, int _rewardQuantity)
    {
        rewardType = _type;
        rewardTypeVal = _rewardTypeVal;
        rewardAmount = _rewardQuantity;

        ShowAd(AdType.REWARDED);
    }

    public void RequestAd(AdType _type)
    {
        Log("Request Ad. Type = " + _type);

        switch (_type)
        {
            case AdType.BANNER:

                if (NoAdsPurchased)
                    return;

                AdmobRequestBannerAd();

                break;

            case AdType.INTERSTITIAL:

                if (NoAdsPurchased)
                    return;

                
                if (interstitialAdView != null && interstitialAdView.IsLoaded())
                {
                    //Dont loaded a loaded Ad
                }
                else {

                    AdmobRequestAndLoadInterstitialAd();
                }
                
                break;

            case AdType.REWARDED:

                if (!rewardedAdView.IsLoaded())
                    AdmobRequestAndLoadRewardedAd();

                break;
        }
    }

    public void ShowAd(AdType _type)
    {

        switch (_type)
        {

            case AdType.BANNER:

                if (NoAdsPurchased)
                    return;

                break;

            case AdType.INTERSTITIAL:

                if (NoAdsPurchased)
                    return;

                if (interstitialAdView != null && interstitialAdView.IsLoaded())
                {
                    Log("Showing Admob IAD");
                    Admob_ShowInterstitialAd();
                }

                break;

            case AdType.REWARDED:

                if (rewardedAdView != null && rewardedAdView.IsLoaded())
                {
                    Log("Showing Admob RAD");
                    Admob_ShowRewardedAd();
                }

                break;
        }
    }

    private void RewardAwardHandling()
    {
        switch (rewardType)
        {
            case RewardType.FREECOINS:

                break;

            case RewardType.DOUBLEREWARD:

                Log(">>>>Rewarded Double!");
                
                break;

            case RewardType.SKIPLEVEL:

                break;

            default:

                break;
        }
    }


    #region HELPER METHODS

    private AdRequest CreateAdRequest()
    {
        return new AdRequest.Builder().Build();
    }

    public void OnApplicationPause(bool paused)
    {
        // Display the app open ad when the app is foregrounded.
        if (!paused)
        {

        }
    }

    public bool IsRewardedAdAvailable() {

        return true;        
    }

    #endregion

    private void AdmobInitCompleteAction(InitializationStatus initstatus)
    {
        MobileAdsEventExecutor.ExecuteInUpdate(() =>
        {
            Log("Admob Initialized");

            admobInitialized = true;

            AdmobRequestAndLoadInterstitialAd();
            AdmobRequestAndLoadRewardedAd();
            RequestAndLoadAppOpenAd();
        });
    }

    bool IsAdmobInitialized() {

        if (admobInitialized)
            return true;
        else {

            Log("Admob not initialized!");
            return false;
        }
    }


    #region BANNER ADS

    public void AdmobRequestBannerAd()
    {

        if (!IsAdmobInitialized())
            return;

        if (bannerAdView != null)
        {
            bannerAdView.Destroy();
        }

        bannerAdView = new BannerView(bannerAdId, bannerAdSize, bannerAdPosition);

        bannerAdView.OnAdLoaded += (sender, args) =>
        {
            Log("Banner ad loaded.");
        };
        bannerAdView.OnAdFailedToLoad += (sender, args) =>
        {
            Log("Banner ad failed to load with error: " + args.LoadAdError.GetMessage());
        };
        bannerAdView.OnAdOpening += (sender, args) =>
        {
            Log("Banner ad opening.");
        };
        bannerAdView.OnAdClosed += (sender, args) =>
        {
            Log("Banner ad closed.");
        };
        bannerAdView.OnPaidEvent += (sender, args) =>
        {
            string msg = string.Format("{0} (currency: {1}, value: {2}",
                                        "Banner ad received a paid event.",
                                        args.AdValue.CurrencyCode,
                                        args.AdValue.Value);
            Log(msg);
        };

        bannerAdView.LoadAd(CreateAdRequest());
    }

    public void DestroyBannerAd()
    {
        if (bannerAdView != null)
        {
            bannerAdView.Destroy();
        }
    }

    #endregion

    #region INTERSTITIAL ADS

    public void AdmobRequestAndLoadInterstitialAd()
    {
        if (!IsAdmobInitialized())
            return;

        if (interstitialAdView != null)
        {
            interstitialAdView.Destroy();
        }

        interstitialAdView = new InterstitialAd(interstitialAdId);

        // Add Event Handlers
        interstitialAdView.OnAdLoaded += (sender, args) =>
        {
            Log("Interstitial ad loaded.");
        };
        interstitialAdView.OnAdFailedToLoad += (sender, args) =>
        {
            Log("Interstitial ad failed to load with error: " + args.LoadAdError.GetMessage());
        };
        interstitialAdView.OnAdOpening += (sender, args) =>
        {
            Log("Interstitial ad opening.");
        };
        interstitialAdView.OnAdClosed += (sender, args) =>
        {
            Log("Interstitial ad closed.");
        };
        interstitialAdView.OnAdDidRecordImpression += (sender, args) =>
        {
            Log("Interstitial ad recorded an impression.");
        };
        interstitialAdView.OnAdFailedToShow += (sender, args) =>
        {
            Log("Interstitial ad failed to show.");
        };
        interstitialAdView.OnPaidEvent += (sender, args) =>
        {
            string msg = string.Format("{0} (currency: {1}, value: {2}",
                                        "Interstitial ad received a paid event.",
                                        args.AdValue.CurrencyCode,
                                        args.AdValue.Value);
            Log(msg);
        };

        interstitialAdView.LoadAd(CreateAdRequest());
    }

    public void Admob_ShowInterstitialAd() {

        interstitialAdView.Show();
    }

    #endregion

    #region REWARDED ADS

    public void AdmobRequestAndLoadRewardedAd()
    {
        if (!IsAdmobInitialized()) {
            return;
        }

        rewardedAdView = new RewardedAd(rewardedAdId);

        rewardedAdView.OnAdLoaded += (sender, args) =>
        {
            Log("Reward ad loaded.");
        };
        rewardedAdView.OnAdFailedToLoad += (sender, args) =>
        {
            Log("Reward ad failed to load.");
        };
        rewardedAdView.OnAdOpening += (sender, args) =>
        {
            Log("Reward ad opening.");
        };
        rewardedAdView.OnAdFailedToShow += (sender, args) =>
        {
            Log("Reward ad failed to show with error: " + args.AdError.GetMessage());
        };
        rewardedAdView.OnAdClosed += (sender, args) =>
        {
            Log("Reward ad closed.");
        };
        rewardedAdView.OnUserEarnedReward += (sender, args) =>
        {
            Log("User earned Reward ad reward: " + args.Amount);
            RewardAwardHandling();
        };
        rewardedAdView.OnAdDidRecordImpression += (sender, args) =>
        {
            Log("Reward ad recorded an impression.");
        };
        rewardedAdView.OnPaidEvent += (sender, args) =>
        {
            string msg = string.Format("{0} (currency: {1}, value: {2}",
                                        "Rewarded ad received a paid event.",
                                        args.AdValue.CurrencyCode,
                                        args.AdValue.Value);
            Log(msg);
        };

        rewardedAdView.LoadAd(CreateAdRequest());
    }

    public void Admob_ShowRewardedAd()
    {
        rewardedAdView.Show();
    }

    #endregion

    #region APPOPEN ADS

    public bool IsAppOpenAdAvailable
    {
        get
        {
            return (!isShowingAppOpenAd
                    && appOpenAdView != null
                    && DateTime.Now < appOpenExpireTime);
        }
    }

    public void OnAppStateChanged(AppState state)
    {
        // Display the app open ad when the app is foregrounded.
        Log("App State is " + state);

        // OnAppStateChanged is not guaranteed to execute on the Unity UI thread.
        MobileAdsEventExecutor.ExecuteInUpdate(() =>
        {
            if (state == AppState.Foreground)
            {
                ShowAppOpenAd();
            }
        });
    }

    public void RequestAndLoadAppOpenAd()
    {
        Log("Requesting App Open ad.");

        // create new app open ad instance
        AppOpenAd.LoadAd(openAdId,
                         screenOrientation,
                         CreateAdRequest(),
                         OnAppOpenAdLoad);
    }

    private void OnAppOpenAdLoad(AppOpenAd ad, AdFailedToLoadEventArgs error)
    {
        if (error != null)
        {
            Log("App Open ad failed to load with error: " + error);
            return;
        }

        Log("App Open ad loaded. Please background the app and return.");
        this.appOpenAdView = ad;
        this.appOpenExpireTime = DateTime.Now + APPOPEN_TIMEOUT;
    }

    public void ShowAppOpenAd()
    {
        if (!IsAppOpenAdAvailable)
        {
            return;
        }

        // Register for ad events.
        this.appOpenAdView.OnAdDidDismissFullScreenContent += (sender, args) =>
        {
            Log("App Open ad dismissed.");
            isShowingAppOpenAd = false;
            if (this.appOpenAdView != null)
            {
                this.appOpenAdView.Destroy();
                this.appOpenAdView = null;
            }
        };
        this.appOpenAdView.OnAdFailedToPresentFullScreenContent += (sender, args) =>
        {
            Log("App Open ad failed to present with error: " + args.AdError.GetMessage());

            isShowingAppOpenAd = false;
            if (this.appOpenAdView != null)
            {
                this.appOpenAdView.Destroy();
                this.appOpenAdView = null;
            }
        };
        this.appOpenAdView.OnAdDidPresentFullScreenContent += (sender, args) =>
        {
            Log("App Open ad opened.");
        };
        this.appOpenAdView.OnAdDidRecordImpression += (sender, args) =>
        {
            Log("App Open ad recorded an impression.");
        };
        this.appOpenAdView.OnPaidEvent += (sender, args) =>
        {
            string msg = string.Format("{0} (currency: {1}, value: {2}",
                                        "App Open ad received a paid event.",
                                        args.AdValue.CurrencyCode,
                                        args.AdValue.Value);
            Log(msg);
        };

        isShowingAppOpenAd = true;
        appOpenAdView.Show();
    }

    #endregion
}
