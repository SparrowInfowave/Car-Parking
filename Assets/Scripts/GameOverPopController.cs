using System;
using GamePlay;
using Manager;
using ThemeSelection;
using UnityEngine;
using UnityEngine.UI;

public class GameOverPopController : MonoBehaviour
{
    [SerializeField] private GameObject watchVideoButton;
    [SerializeField] private GameObject challengeTextObj, retryTextObj;
    [SerializeField]
    private Text levelText,
        headerText,
        watchVideoText;

    private const int RewardMove = 5;
    private const int RewardTime = 15;
    internal bool isShownVideo = false;

    [SerializeField] private Animation watchVideoButtonHighlight; 
    private void Start()
    {
        SoundManager.inst.Play("GameOver");
        Set_Ui_And_Data();
    }

    private void Set_Ui_And_Data()
    {
        Set_Header();

        //Why
        if(GameManager.Instance.challengeType == CurrentChallengeType.Level && GameManager.Instance.gameOverReason == GameOverReason.HitPerson)
            watchVideoButton.SetActive(false);
        
        Set_WatchVideoText();
        SetLevelText();
        SetButton();
    }

    private void SetLevelText()
    {
        levelText.text = GameManager.Instance.challengeType switch
        {
            CurrentChallengeType.Level => "LEVEL " + (CommonGameData.LevelNumber + 1),
            CurrentChallengeType.BossChallenge => "BOSS CHALLENGE",
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    private void SetButton()
    {
        //homeButtonObj.SetActive(GameManager.Instance.challengeType == CurrentChallengeType.Challenge);
        retryTextObj.SetActive(GameManager.Instance.challengeType == CurrentChallengeType.Level);
        challengeTextObj.SetActive(GameManager.Instance.challengeType == CurrentChallengeType.BossChallenge);
    }

    private void Set_Header()
    {
        headerText.text = GameManager.Instance.gameOverReason switch
        {
            GameOverReason.HitPerson => "You just hit a person",
            GameOverReason.OutOfMoves => "Oops! out of moves",
            GameOverReason.OutOfTime => "Oops! out of time",
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private void Set_WatchVideoText()
    {
        watchVideoText.text = GameManager.Instance.gameOverReason switch
        {
            GameOverReason.HitPerson => "SKIP LEVEL",
            GameOverReason.OutOfMoves => "+" + RewardMove + " Moves",
            GameOverReason.OutOfTime => "+" + RewardTime + " SECOND",
            _ => throw new ArgumentOutOfRangeException()
        };

        watchVideoText.text = GameManager.Instance.gameOverReason switch
        {
            GameOverReason.HitPerson => GameManager.Instance.challengeType == CurrentChallengeType.BossChallenge
                ? "RETRY"
                : "SKIP LEVEL",
            GameOverReason.OutOfMoves => "+" + RewardMove + " Moves",
            GameOverReason.OutOfTime => "+" + RewardTime + " SECOND",
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public void PlayWatchVideoButtonHighlightAnim()
    {
        GetComponent<Animator>().enabled = false;
        watchVideoButtonHighlight.Play();
    }

    public void WatchButtonReward()
    {
        isShownVideo = true;
        watchVideoButton.SetActive(false);
        switch (GameManager.Instance.gameOverReason)
        {
            case GameOverReason.HitPerson:
                if (GameManager.Instance.challengeType == CurrentChallengeType.Level)
                    Set_NextLevel();
                else
                    Set_RetryChallenge();
                break;
            case GameOverReason.OutOfMoves:
                GameManager.Instance.targetMoves = RewardMove;
                GameManager.Instance.Show_Screen(GeneralDataManager.Screen.GamePlay);
                GameManager.Instance.Hide_Popup();
                break;
            case GameOverReason.OutOfTime:
                GameManager.Instance.targetTime = RewardTime;
                GameManager.Instance.Show_Screen(GeneralDataManager.Screen.GamePlay);
                GameManager.Instance.Hide_Popup();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void SkipLevelButton()
    {
        //AdsManager.inst.RequestAndLoadRewardedAd("GameOverVideo");
    }

    public void Retry_Button()
    {
        GameManager.Instance.challengeType = CurrentChallengeType.Level;
        GamePlayManager.Instance.Reset_Level();
    }

    public void ThemeButton()
    {
        GameManager.Instance.Show_Popup(GeneralDataManager.Popup.ThemeSelection, false);
    }
    
    public void HomeButton()
    {
        GameManager.Instance.challengeType = CurrentChallengeType.Level;
        SceneController.Instance.GoTo_GamePlayScene_At_GameStart();
    }

    private void Set_NextLevel()
    {
        GamePlayManager.Instance.Next_Level();
        GameManager.Instance.Hide_Popup();
    }

    private void Set_RetryChallenge()
    {
        SceneController.Instance.LoadScene((Scene)ThemeSavedDataManager.EnvironmentThemeNumber, null,
            () =>
            {
                GameManager.Instance.HideAllPopUp();
                GameManager.Instance.Show_Screen(GeneralDataManager.Screen.GamePlay);
                GamePlayController.Instance?.Reset_Level();
            });
    }

    private void OnDestroy()
    {
    }
}
