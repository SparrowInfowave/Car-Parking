using System;
using System.Collections.Generic;
using DG.Tweening;
using GamePlay;
using Manager;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class LevelCompleteThemeNotAvailable : MonoBehaviour
{
    [SerializeField] private Text levelText, fillAmountText, coinText;
    [SerializeField] private GameObject themeItemObj, themeQuestionObj, watchVideoButtonObj;
    [SerializeField] private Image fillImage;
    
   [SerializeField] private Animation watchVideoAnimation;

    private bool _isCoinReward = false;
    [HideInInspector] public int givenCoin = 5;
    private void Start()
    {
        givenCoin = Random.Range(200, 301);
        Set_Data_And_Ui();
       
        var fillAmount = 1 - (1 / (float)CommonGameData.RoadAndTrailThemeRewardInterval);
        fillImage.fillAmount = fillAmount;
        fillAmountText.text = (Mathf.FloorToInt(fillAmount) * 100) + "%";
    }
    
    public void ShowRateUsPopUp()
    {
        GameManager.Instance.Show_Rate_Popup();
    }

    private void Set_Data_And_Ui()
    {
        switch (GameManager.Instance.challengeType)
        {
            case CurrentChallengeType.Level:
                levelText.text = "LEVEL " + (CommonGameData.LevelNumber + 1);
                CommonGameData.IncreaseLevelNumber();
                break;
            case CurrentChallengeType.UnblockChallenge:
                levelText.text = "CHALLENGE " + GameManager.Instance.currentChallengeNumber;
                ChallengeDataManager.NextChallenge();
                break;
            case CurrentChallengeType.BossChallenge:
            default:
                throw new ArgumentOutOfRangeException();
        }
        coinText.text = "+" + givenCoin.ToString();
    }
    
    //Used in animation
    private void PlayWatchVideoButtonAnimation()
    {
        GetComponent<Animator>().enabled = false;
        watchVideoAnimation.Play();
    }
    
    //Used in animation
    public void FillAnimation()
    {
        var current = 1;
        var previousFillAmount = 1f - (1 / (float)CommonGameData.RoadAndTrailThemeRewardInterval);
        DOTween.To(() => previousFillAmount, x =>
        {
            fillImage.fillAmount = x;
            fillAmountText.text = Mathf.CeilToInt(x * 100).ToString()+ "%";

        },current , 0.3f).SetEase(Ease.Linear).OnComplete(() =>
        {
            themeQuestionObj.GetComponent<CanvasGroup>().DOFade(0,0.4f).OnComplete(()=>themeQuestionObj.SetActive(false));
            themeItemObj.SetActive(true);
        });
    }
    
    public void NextButtonClick()
    {
        GameManager.Instance.LevelCompleteNextButtonClick();
    }
    
    public void ThemeButton()
    {
        GameManager.Instance.Show_Popup(GeneralDataManager.Popup.ThemeSelection, false);
    }

    public void HomeButton()
    {
        SceneController.Instance.GoTo_GamePlayScene_At_GameStart();
    }

    

    public void RewardButton()
    {
        //AdsManager.inst.RequestAndLoadRewardedAd("LevelCompleteThemeNotAvailable");
    }

    public void GiveReward()
    {
        CommonGameData.AddCoinWithAnimation(givenCoin);
        GetComponent<Animator>().SetTrigger("GiveCoin");
        GetComponent<Animator>().enabled = true;
    }
}
