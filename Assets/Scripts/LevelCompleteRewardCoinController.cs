using System;
using System.Collections.Generic;
using DG.Tweening;
using Manager;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class LevelCompleteRewardCoinController : SingletonComponent<LevelCompleteRewardCoinController>
{
    [SerializeField] private Text levelText, fillAmountText;
    [SerializeField] private Image headerImage;
    [SerializeField] public Text coinText, doubleRewardText;
    [SerializeField] private GameObject weatchAdButtonObj;
    [SerializeField] private Image fillImage;
    [HideInInspector] public int givenCoin = 5;

    [SerializeField] private List<Sprite> _leftHeaders = new List<Sprite>();

    [SerializeField] private Animation doubleRewardAnimation;

    [SerializeField] private RectTransform rewardSliderArrow;

    private void Start()
    {
        givenCoin = GameManager.Instance.levelOrChallengeCompleteReward;
        CommonGameData.AddCoin(givenCoin);
        Set_Data_And_Ui();

        var fillAmount = ((CommonGameData.CurrentCompletedLevel % CommonGameData.RoadAndTrailThemeRewardInterval) - 1) /
                         (float)CommonGameData.RoadAndTrailThemeRewardInterval;

        if (fillAmount < 0)
            fillAmount = (1 / (float)CommonGameData.RoadAndTrailThemeRewardInterval) *
                         (CommonGameData.RoadAndTrailThemeRewardInterval - 1);

        fillImage.fillAmount = fillAmount;
        fillAmountText.text = (Mathf.FloorToInt(fillAmount) * 100) + "%";
    }

    public void ShowRateUsPopUp()
    {
        GameManager.Instance.Show_Rate_Popup();
    }

    private void Set_Data_And_Ui()
    {
        SetLeftHeaderText();

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

        coinText.text = "0";
    }

    //Used in animation
    private void PlayDoubleRewardAnimation()
    {
        GetComponent<Animator>().enabled = false;
        doubleRewardAnimation.Play();
    }

    //Used in animation
    public void Add_Coin_WithAnim()
    {
        DOTween.To(() => 0, x => coinText.text = "+" + x.ToString(), givenCoin, 0.4f).SetEase(Ease.Linear);
    }

    //Used in animation
    public void FillAnimation()
    {
        var current = (CommonGameData.CurrentCompletedLevel % CommonGameData.RoadAndTrailThemeRewardInterval) /
                      (float)CommonGameData.RoadAndTrailThemeRewardInterval;

        if (current == 0)
            current = CommonGameData.RoadAndTrailThemeRewardInterval;

        var previousFillAmount = fillImage.fillAmount;
        DOTween.To(() => previousFillAmount, x =>
        {
            fillImage.fillAmount = x;
            fillAmountText.text = Mathf.FloorToInt(x * 100).ToString() + "%";
        }, current, 0.3f).SetEase(Ease.Linear);
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

    private void SetLeftHeaderText()
    {
        headerImage.sprite = _leftHeaders[Random.Range(0, _leftHeaders.Count)];
    }

    public void WatchVideoButton()
    {
        rewadSliderAnim.Kill();
        //AdsManager.inst.RequestAndLoadRewardedAd("LevelCompleteDoubleReward");
    }

    private int rewardMultiplier = 2;

    private Tween rewadSliderAnim = null;

    public void DoubleRewardSlider()
    {
        rewardSliderArrow.anchoredPosition = new Vector2(-260, 0);
        rewadSliderAnim = rewadSliderAnim =
            rewardSliderArrow.DOAnchorPosX(260, 0.7f).SetLoops(-1, LoopType.Yoyo).OnUpdate(OnSliderUpdate).SetEase(Ease.InOutSine);
    }

    private void OnSliderUpdate()
    {
        //y = 4.00115429 + -0.01154290112 * x
        rewardMultiplier = (int)(5.001539053f +  Mathf.Abs(rewardSliderArrow.anchoredPosition.x)*(-0.01539053482));
        var reward = givenCoin * rewardMultiplier;
        doubleRewardText.text = reward.ToString();
    }

    public void GiveDoubleReward()
    {
        var coin = rewardMultiplier * givenCoin;
        CommonGameData.AddCoin(coin);
        DOTween.To(() => givenCoin, x => coinText.text = "+" + x.ToString(), givenCoin + coin, 0.4f)
            .SetEase(Ease.Linear);
        weatchAdButtonObj.SetActive(false);
    }
}