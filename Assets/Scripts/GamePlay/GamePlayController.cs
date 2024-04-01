using System;
using System.Linq;
using Manager;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace GamePlay
{
    public class GamePlayController : SingletonComponent<GamePlayController>
    {
        [SerializeField] private MoveAndTimeController levelMoveTimeController, challengeMoveTimeController;

        [Header("Level")] [SerializeField] private Text onlyLevelLevelText;
        [SerializeField] private Text moveLevelLevelText;
        [SerializeField] private GameObject screenLevelMoveObj;
        [SerializeField] private GameObject screenLevelOnlyLevelObj;

        [Space(10)] [Header("Boss Challenge")] 
        [SerializeField] private Text bossChallengeText;
        [SerializeField] private GameObject screenBossChallengeMoveTextObj;
        [SerializeField] private GameObject screenBossChallengeTimerObject;
        
        [Space(10)] [Header("Challenge")]
        [SerializeField] private Text challengeText;

        [Space(10)] [SerializeField] private GameObject particleSystemObject1;
        [SerializeField] private GameObject particleSystemObject2;
        [SerializeField] private GameObject screenLevel;
        [SerializeField] private GameObject screenBossChallenge;
        [SerializeField] private GameObject screenChallenge;

        [Space(10)] [Header("Emoji")] public GameObject upEmoji;
        [SerializeField] private Sprite[] insideCarEmoji;
        [SerializeField] private Sprite[] waitCarEmoji;
        [SerializeField] private Sprite[] outsideCarEmoji;

        [SerializeField] private Animator screenLevelMoveHighlight;
        [SerializeField] private Animator screenBossChallengeMoveHighlight;
        [SerializeField] private GameObject skipHintObj;
        [SerializeField] private GameObject hintIconObj;
        private static readonly int Play = Animator.StringToHash("Play");

        private void OnEnable()
        {
            LevelGenerator.Instance?.CheckForLevelTutorial();
            Invoke(nameof(PlayHighLightAnimation),0.5f);
        }

        private void Start()
        {
            GameManager.Instance.isGameStart = true;
            SetLevelUi();
        }

        private void SetLevelUi()
        {
            screenLevel.SetActive(GameManager.Instance.challengeType == CurrentChallengeType.Level);
            screenBossChallenge.SetActive(GameManager.Instance.challengeType == CurrentChallengeType.BossChallenge);
            screenChallenge.SetActive(GameManager.Instance.challengeType == CurrentChallengeType.UnblockChallenge);
            switch (GameManager.Instance.challengeType)
            {
                case CurrentChallengeType.Level:
                    onlyLevelLevelText.text = moveLevelLevelText.text =
                        "Level " + (CommonGameData.LevelNumber + 1).ToString();
                    break;
                case CurrentChallengeType.BossChallenge:
                    bossChallengeText.text = "BOSS CHALLENGE";
                    break;
                case CurrentChallengeType.UnblockChallenge:
                    challengeText.text = "Challenge " + GameManager.Instance.currentChallengeNumber;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Set_Move_Time_Data();
        }

        public void Set_Move_Time_Data()
        {
            if(GetMoveTimeController() == null)return;
            
            if (GameManager.Instance.challengeType == CurrentChallengeType.Level)
            {
                var isLevelNotContainMove = CommonGameData.LevelNumber <=
                                            GameManager.Instance.startMoveInLevel;

                if (isLevelNotContainMove)
                {
                    screenLevelMoveObj.SetActive(false);
                    screenLevelOnlyLevelObj.SetActive(true);
                    return;
                }

                screenLevelMoveObj.SetActive(true);
                screenLevelOnlyLevelObj.SetActive(false);
            }

            GetMoveTimeController().Set_TargetMoves(GameManager.Instance.targetMoves);

            if (GameManager.Instance.challengeType != CurrentChallengeType.BossChallenge) return;

            screenBossChallengeMoveTextObj.SetActive(!GameManager.Instance.isGiantBossChallenge);
            screenBossChallengeTimerObject.SetActive(GameManager.Instance.isGiantBossChallenge);

            if (GameManager.Instance.isGiantBossChallenge)
            {
                GetMoveTimeController().Set_TimerText(GameManager.Instance.targetTime);
                GetMoveTimeController().Set_Slider();
            }
        }

        public void Start_For_OutOfMove_GameOver()
        {
            InvokeRepeating(nameof(Is_GameOver), 0f, 0.2f);
        }

        public void Is_GameOver()
        {
            if (LevelGenerator.Instance.vehicleControllers.Any(
                    x => x._currentCarState == VehicleController.CarState.OnPath) ||
                LevelGenerator.Instance.vehicleControllers.Any(
                    x => x._currentCarState == VehicleController.CarState.Exit) ||
                !GameManager.Instance.isGameStart)
            {
                return;
            }

            GameManager.Instance.StartCoroutine(GameManager.Instance.GameOver(0.1f, GameOverReason.OutOfMoves));
        }

        public void Check_Stop_Time_CountDown()
        {
            if (GameManager.Instance.challengeType != CurrentChallengeType.BossChallenge) return;
            if (!GameManager.Instance.isGiantBossChallenge) return;

            if (LevelGenerator.Instance.vehicleControllers.All(x =>
                    x._currentCarState != VehicleController.CarState.OnField))
            {
                GetMoveTimeController().Stop_CountDown();
            }
        }

        public void Check_Move_And_Time()
        {
            if(GameManager.Instance.challengeType == CurrentChallengeType.UnblockChallenge)return;
            
            if ((GameManager.Instance.challengeType == CurrentChallengeType.BossChallenge &&
                 !GameManager.Instance.isGiantBossChallenge) ||
                (GameManager.Instance.challengeType == CurrentChallengeType.Level &&
                 CommonGameData.LevelNumber > GameManager.Instance.startMoveInLevel))
            {
                GetMoveTimeController().Decrease_Move();
            }

            StartCountDown();
        }

        public void Reset_Level()
        {
            SetLevelUi();
            GameManager.Instance.isGameStart = true;
        }

        public void Reset_button_Click()
        {
            if (GamePlayManager.Instance.CanPressAnyButtonIn_GamePlay())
                GamePlayManager.Instance.Reset_Level();
        }

        public void Pause_button_Click()
        {
            if (GamePlayManager.Instance.CanPressAnyButtonIn_GamePlay())
                GameManager.Instance.Show_Screen(GeneralDataManager.Screen.Home);
        }

        public void ScreenBossChallenge_Home_button_Click()
        {
            if (GamePlayManager.Instance.CanPressAnyButtonIn_GamePlay())
            {
                GameManager.Instance.challengeType = CurrentChallengeType.Level;
                SceneController.Instance.GoTo_GamePlayScene_At_GameStart();
            }
        }

        public void OnEscape()
        {
            switch (GameManager.Instance.challengeType)
            {
                case CurrentChallengeType.Level:
                    Pause_button_Click();
                    break;
                case CurrentChallengeType.BossChallenge:
                case CurrentChallengeType.UnblockChallenge:
                    ScreenBossChallenge_Home_button_Click();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void StartCountDown()
        {
            GameManager.Instance.isGameStart = true;
            if (GameManager.Instance.challengeType == CurrentChallengeType.BossChallenge &&
                GameManager.Instance.isGiantBossChallenge)
            {
                GetMoveTimeController().StartDecrease_Time();
            }
        }

        public void CloseThisScreen()
        {
            Destroy(this.gameObject);
        }

        public void EnableParticleSystem()
        {
            particleSystemObject1.SetActive(true);
            particleSystemObject2.SetActive(true);
        }

        private MoveAndTimeController GetMoveTimeController()
        {
            return GameManager.Instance.challengeType switch
            {
                CurrentChallengeType.Level => levelMoveTimeController,
                CurrentChallengeType.BossChallenge => challengeMoveTimeController,
                _ => null
            };
        }
        
        public void HintButtonClick()
        {
            if (GamePlayManager.Instance.CanPressAnyButtonIn_GamePlay())
            {
                if (!LevelGenerator.Instance.isHintTutorial)
                    GameManager.Instance.Show_Popup(GeneralDataManager.Popup.HintBuyPopUp);
                else
                    LevelGenerator.Instance.UnlockCarRemoveHint();
            }
        }

        public Sprite[] GetEmojiPack(EmojiCarType emojiCarType)
        {
            return emojiCarType switch
            {
                EmojiCarType.InsideCar => insideCarEmoji,
                EmojiCarType.WaitCar => waitCarEmoji,
                EmojiCarType.OutSideCar => outsideCarEmoji,
                _ => throw new ArgumentOutOfRangeException(nameof(emojiCarType), emojiCarType, null)
            };
        }
        
        public void Check_HintObj()
        {
            skipHintObj.SetActive(LevelGenerator.Instance.isHintTutorial);
            hintIconObj.SetActive(!LevelGenerator.Instance.isHintTutorial);
        }
        
        public void PlayHighLightAnimation()
        {
            switch (GameManager.Instance.challengeType)
            {
                case CurrentChallengeType.Level:
                    if (GameManager.Instance.challengeType == CurrentChallengeType.Level &&
                        CommonGameData.LevelNumber > GameManager.Instance.startMoveInLevel)
                    {
                        screenLevelMoveHighlight.SetTrigger(Play);
                    }
                    break;
                case CurrentChallengeType.BossChallenge:
                    screenBossChallengeMoveHighlight.SetTrigger(Play);
                    break;
                case CurrentChallengeType.UnblockChallenge:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}