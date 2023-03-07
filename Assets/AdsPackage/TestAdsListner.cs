using UnityEngine;

public class TestAdsListner : MonoBehaviour
{
    public void Init()
    {
        AdsManager.instance.Initialize();
    }

    public void Admob_Destroy_Banner()
    {
        AdsManager.instance.HideBannerAd();
    }

    public void Admob_LoadnShow_Banner()
    {
        AdsManager.instance.ShowLoadedBanner();
    }

    public void Admob_Load_IAD()
    {
        AdsManager.instance.RequestAndLoadInterstitialAd();
    }

    public void Admob_Show_IAD()
    {
        AdsManager.instance.ShowInterstitialAd();
    }

    public void Admob_Load_VideoIAD()
    {
        AdsManager.instance.RequestAndLoadInterstitialVideoAd();
    }

    public void Admob_Show_VideoIAD()
    {
        AdsManager.instance.ShowInterstitialVideoAd();
    }

    public void Admob_Load_RAD()
    {
        AdsManager.instance.RequestAndLoadRewardedVideoAd();
    }

    public void Admob_Show_RAD()
    {
        AdsManager.instance.ShowRewardedAdWithSpecs( AdsManager.RewardType.FREECOINS, 100);
    }

    public void Admob_Load_OAD()
    {
        AdsManager.instance.RequestAndLoadAppOpenAd();
    }

    public void Admob_Show_OAD()
    {
        AdsManager.instance.ShowAppOpenAd();
    }
}
