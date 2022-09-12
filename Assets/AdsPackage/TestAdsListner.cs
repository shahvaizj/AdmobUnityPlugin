using UnityEngine;

public class TestAdsListner : MonoBehaviour
{
    public void Init()
    {
        AdsManager.instance.Initialize();
    }

    public void Admob_Destroy_Banner()
    {
        AdsManager.instance.DestroyBannerAd();
    }

    public void Admob_LoadnShow_Banner()
    {
        AdsManager.instance.AdmobRequestBannerAd();
    }

    public void Admob_Load_IAD()
    {
        AdsManager.instance.AdmobRequestAndLoadInterstitialAd();
    }

    public void Admob_Show_IAD()
    {
        AdsManager.instance.Admob_ShowInterstitialAd();
    }

    public void Admob_Load_RAD()
    {
        AdsManager.instance.AdmobRequestAndLoadRewardedAd();
    }

    public void Admob_Show_RAD()
    {
        AdsManager.instance.Admob_ShowRewardedAd();
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
