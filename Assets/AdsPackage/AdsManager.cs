using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    private int rewardAmount = 0;

    private bool admobInitialized = false;

    private BannerView bannerAdView;
    private InterstitialAd interstitialAdView;
    private InterstitialAd interstitialVideoAdView;
    private RewardedAd rewardedAdView;
    private RewardedInterstitialAd rewardedInterstitialAdView;
    private AppOpenAd appOpenAdView;

    #region Keys
    //private string bannerAdId = "ca-app-pub-3940256099942544/6300978111";
    //private string interstitialAdId = "ca-app-pub-3940256099942544/1033173712";
    //private string interstitialVideoAdId = "ca-app-pub-3940256099942544/8691691433";
    //private string rewardedAdId = "ca-app-pub-3940256099942544/5224354917";
    //private string openAdId = "ca-app-pub-3940256099942544/3419835294";

    //TestID
    private string bannerAdId = "ca-app-pub-3940256099942544/6300978111";
    private string interstitialAdId = "ca-app-pub-3940256099942544/1033173712";
    private string interstitialVideoAdId = "ca-app-pub-3940256099942544/8691691433";
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
        //Toolbox.GameManager.Log("Ads=" + _str);

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
        deviceIds.Add("1c4bd141-d52c-4919-8fcc-32e5e38507fe");
        deviceIds.Add("efbd583e-e389-4272-8490-61496e215c2d");
#endif

        // Configure TagForChildDirectedTreatment and test device IDs.
        RequestConfiguration requestConfiguration =
            new RequestConfiguration.Builder()
            .SetTagForChildDirectedTreatment(TagForChildDirectedTreatment.Unspecified)
            .SetTestDeviceIds(deviceIds).build();

        MobileAds.SetRequestConfiguration(requestConfiguration);

        AppStateEventNotifier.AppStateChanged += OnAppStateChanged;

        MobileAds.Initialize(InitCompleteAction);
    }

    private void InitCompleteAction(InitializationStatus initstatus)
    {
        MobileAdsEventExecutor.ExecuteInUpdate(() =>
        {
            Log("Admob Initialized");

            admobInitialized = true;

            RequestAndLoadInterstitialAd();
            RequestAndLoadRewardedVideoAd();

        });
    }

    #region BANNER ADS

    public void HideBannerAd()
    {
        if (bannerAdView != null)
        {
            bannerAdView.Hide();
        }
    }
    public void ShowBannerWithSpecs(AdSize _size, AdPosition _pos)
    {
        bannerAdSize = _size;
        bannerAdPosition = _pos;

        if (NoAdsPurchased)
            return;

        ShowLoadedBanner();
    }

    public void RequestBannerAd()
    {

        if (!IsAdmobInitialized())
            return;

        HideBannerAd();

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

        //bannerAdView.LoadAd(CreateAdRequest());
        bannerAdView.LoadAd(CreateAdRequest());
    }

    public void ShowLoadedBanner() {

        if (bannerAdView != null)
            bannerAdView.Show();
        else
            RequestBannerAd();
    }

    #endregion

    #region INTERSTITIAL ADS

    public void RequestAndLoadInterstitialAd()
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

    public void ShowInterstitialAd()
    {

        if (interstitialAdView == null && !interstitialAdView.IsLoaded())
        {
            RequestAndLoadInterstitialAd();
        }
        else
        {
            interstitialAdView.Show();
        }
    }

    #endregion

    #region INTERSTITIAL Video ADS

    public void RequestAndLoadInterstitialVideoAd()
    {
        if (!IsAdmobInitialized())
            return;

        if (interstitialVideoAdView != null)
        {
            interstitialVideoAdView.Destroy();
        }

        interstitialVideoAdView = new InterstitialAd(interstitialVideoAdId);

        // Add Event Handlers
        interstitialVideoAdView.OnAdLoaded += (sender, args) =>
        {
            Log("Interstitial Video ad loaded.");
        };
        interstitialVideoAdView.OnAdFailedToLoad += (sender, args) =>
        {
            Log("Interstitial Video ad failed to load with error: " + args.LoadAdError.GetMessage());
        };
        interstitialVideoAdView.OnAdOpening += (sender, args) =>
        {
            Log("Interstitial Video ad opening.");
        };
        interstitialVideoAdView.OnAdClosed += (sender, args) =>
        {
            Log("Interstitial Video ad closed.");
        };
        interstitialVideoAdView.OnAdDidRecordImpression += (sender, args) =>
        {
            Log("Interstitial Video ad recorded an impression.");
        };
        interstitialVideoAdView.OnAdFailedToShow += (sender, args) =>
        {
            Log("Interstitial Video ad failed to show.");
        };
        interstitialVideoAdView.OnPaidEvent += (sender, args) =>
        {
            string msg = string.Format("{0} (currency: {1}, value: {2}",
                                        "Interstitial Video ad received a paid event.",
                                        args.AdValue.CurrencyCode,
                                        args.AdValue.Value);
            Log(msg);
        };

        interstitialVideoAdView.LoadAd(CreateAdRequest());
    }

    public void ShowInterstitialVideoAd()
    {

        if (interstitialVideoAdView == null && !interstitialVideoAdView.IsLoaded())
        {
            RequestAndLoadInterstitialVideoAd();
        }
        else
        {
            interstitialVideoAdView.Show();
        }
    }

    #endregion

    #region REWARDED ADS

    public bool IsRewardedVideoAdAvailable()
    {
        if (rewardedAdView != null && rewardedAdView.IsLoaded())
        {
            return true;
        }
        else {
            return false;
        }
    }

    public void RequestAndLoadRewardedVideoAd()
    {
        if (!IsAdmobInitialized() || IsRewardedVideoAdAvailable())
        {
            return;
        }

        rewardedAdView = new RewardedAd(rewardedAdId);

        rewardedAdView.OnAdLoaded += (sender, args) =>
        {
            Log("Reward ad loaded.");
        };
        rewardedAdView.OnAdFailedToLoad += (sender, args) =>
        {
            Log("Reward ad failed to load.Error: " + args.LoadAdError.GetMessage());
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
            RewardAdAwardHandling();
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

    public void ShowRewardedAdWithSpecs(RewardType _type, int _rewardQuantity)
    {
        if (rewardedAdView == null && !rewardedAdView.IsLoaded()) {

            RequestAndLoadRewardedVideoAd();

        }else
        {
            rewardType = _type;
            rewardAmount = _rewardQuantity;

            rewardedAdView.Show();
        }
    }

    private void RewardAdAwardHandling()
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
        if (!IsAdmobInitialized() || !IsAppOpenAdAvailable)
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

    bool IsAdmobInitialized()
    {
        if (admobInitialized)
            return true;
        else
        {
            Log("Admob not initialized!");
            return false;
        }
    }

    #endregion

}
