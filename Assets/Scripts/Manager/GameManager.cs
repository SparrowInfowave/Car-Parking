using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GamePlay;
using ThemeSelection;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using static Manager.GeneralDataManager;
using Random = UnityEngine.Random;
using Screen = UnityEngine.Screen;

namespace Manager
{
    public enum CurrentChallengeType
    {
        Level,
        BossChallenge,
        UnblockChallenge
    }

    public enum GameOverReason
    {
        HitPerson,
        OutOfMoves,
        OutOfTime
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        public GameObject homeScreen,
            settingPopUpPrefab,
            exitPopUpPrefab,
            freeCoinPopupPrefab,
            adLoaderPanel,
            rateUsPopUpPrefab,
            moregameScreenPrefab,
            moreGame_AdViewPrefab,
            coinObj,
            loadingPanel,
            splashScreen,
            gamePlayScreen,
            levelCompleteCoinRewardPopUp,
            levelCompleteThemeRewardPopUp,
            levelCompleteThemeNotAvailable,
            levelCompleteCarSceneRentPopUp,
            levelCompleteCarSceneRentTimeFinished,
            bossChallengeStartPopUp,
            bosschallengeCompletePopUp,
            gameOverPopUp,
            themeSelection,
            hotpackPopUp,
            tutorialHand,
            shopPopUp,
            hintBuyPopUp;

        [Space(10)] public Transform popupsParent;
        public Transform screenParent;
        public Transform canvas;

        [Space(10)] [Header("Speed")] public float mainPathSpeed = 18f;
        public float inParkingSpeed = 200f;

        [SerializeField] public CurrentChallengeType challengeType;

        [HideInInspector] public int currentBossChallengeNumber;
        [HideInInspector] public int levelOrChallengeCompleteReward;
        [HideInInspector] public bool isGiantBossChallenge;

        [HideInInspector] public int currentChallengeNumber;

        [Space(10)] [Header("Other")] public int startMoveInLevel = 50;
        [HideInInspector] public bool isGameStart = false;
        [HideInInspector] public GameOverReason gameOverReason = GameOverReason.HitPerson;
        [HideInInspector] public int targetMoves;
        [HideInInspector] public int targetTime;
        [HideInInspector] public int earnPopUpShowCount = 0;

        [HideInInspector] public CurrentChallengeType lastSelectedTypeOnBossChallengeStart = CurrentChallengeType.Level;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(this.gameObject);

            DontDestroyOnLoad(this.gameObject);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && !loadingPanel.activeSelf)
            {
                if (adLoaderPanel.activeSelf)
                {
                    adLoaderPanel.SetActive(false);
                    return;
                }
                
                if (ActivePopUpObj != null)
                {
                    switch (GeneralDataManager.Instance.currentOpenedPopupList.Last())
                    {
                        case Popup.LevelCompleteCoinReward:
                            FindObjectOfType<LevelCompleteRewardCoinController>()?.HomeButton();
                            return;
                        case Popup.LevelCompleteThemeReward:
                            return;
                        case Popup.LevelCompleteThemeNotAvailable:
                            FindObjectOfType<LevelCompleteThemeNotAvailable>()?.HomeButton();
                            return;
                        case Popup.LevelCompleteCarSceneRentTimeFinished:
                            return;
                        case Popup.LevelCompleteCarSceneRentPopUp:
                            FindObjectOfType<LevelCompleteCarSceneThemeRewardPopUpController>()?.HomeButton();
                            return;
                        case Popup.BossChallengeComplete:
                            return;
                        case Popup.BossChallengeStart:
                            return;
                        case Popup.GameOver:
                            FindObjectOfType<GameOverPopController>()?.HomeButton();
                            return;
                        case Popup.ThemeSelection:
                            FindObjectOfType<ThemeSelection.ThemeSelectionScreenController>()?.BackButtonClick();
                            return;
                        default:
                            Hide_Popup();
                            return;
                    }
                }

                switch (CurrentScreen)
                {
                    case GeneralDataManager.Screen.Home:
                        Show_Popup(Popup.ExitPopUp);
                        break;
                    case GeneralDataManager.Screen.GamePlay:
                        GamePlayController.Instance?.OnEscape();
                        break;
                }
            }


            if (Input.GetKeyDown(KeyCode.A))
            {
                TakeScreenshot();
            }
        }

        private int number = 0;

        public void TakeScreenshot()
        {
            ScreenCapture.CaptureScreenshot("TestImage" + Screen.width + " x " + Screen.height + " " + number + ".png");
            number++;
        }

        private void Show_Home_Screen()
        {
            HideAllPopUp();
            if (screenParent.childCount > 0)
            {
                for (int i = 0; i < screenParent.childCount; i++)
                {
                    Destroy(screenParent.transform.GetChild(i).gameObject);
                }
            }

            homeScreen.SetActive(true);
            CurrentScreen = GeneralDataManager.Screen.Home;
        }

        public void Show_Screen(GeneralDataManager.Screen activeScreen, bool destroyPreviousScreen = true)
        {
            if (CurrentScreen == activeScreen) return;

            if (destroyPreviousScreen)
                if (ActiveScreenObj != null)
                    ActiveScreenObj.SendMessage("CloseThisScreen");

            CurrentScreen = activeScreen;

            switch (activeScreen)
            {
                case GeneralDataManager.Screen.Home:
                    Show_Home_Screen();
                    break;
                case GeneralDataManager.Screen.GamePlay:
                    homeScreen.SetActive(false);
                    ActiveScreenObj = Instantiate(gamePlayScreen, screenParent);
                    break;
            }
        }

        public void Hide_Screen()
        {
            Destroy(ActiveScreenObj);
            CurrentScreen = GeneralDataManager.Screen.Empty;
        }


        public void Show_Popup(Popup activePopup, bool isPreviousPopUpHide = true)
        {
            if (GeneralDataManager.Instance.currentOpenedPopupList.Contains(activePopup)) return;

            if (isPreviousPopUpHide)
                Hide_Popup();

            GeneralDataManager.Instance.currentOpenedPopupList.Add(activePopup);

            ActivePopUpObj = activePopup switch
            {
                Popup.SettingPopUp => Instantiate(settingPopUpPrefab, popupsParent),
                Popup.ExitPopUp => Instantiate(exitPopUpPrefab, popupsParent),
                Popup.FreeCoinPopUp => Instantiate(freeCoinPopupPrefab, popupsParent),
                Popup.RateUsPopUp => Instantiate(rateUsPopUpPrefab, popupsParent),
                Popup.MoreGameScreen => Instantiate(moregameScreenPrefab, popupsParent),
                Popup.MoreGameAdView => Instantiate(moreGame_AdViewPrefab, popupsParent),
                Popup.LevelCompleteCoinReward => Instantiate(levelCompleteCoinRewardPopUp, popupsParent),
                Popup.LevelCompleteThemeReward => Instantiate(levelCompleteThemeRewardPopUp, popupsParent),
                Popup.LevelCompleteThemeNotAvailable => Instantiate(levelCompleteThemeNotAvailable, popupsParent),
                Popup.BossChallengeStart => Instantiate(bossChallengeStartPopUp, popupsParent),
                Popup.BossChallengeComplete => Instantiate(bosschallengeCompletePopUp, popupsParent),
                Popup.GameOver => Instantiate(gameOverPopUp, popupsParent),
                Popup.ThemeSelection => Instantiate(themeSelection, popupsParent),
                Popup.HotPackPopUp => Instantiate(hotpackPopUp, popupsParent),
                Popup.HintBuyPopUp => Instantiate(hintBuyPopUp, popupsParent),
                Popup.ShopPopUp => Instantiate(shopPopUp, popupsParent),
                Popup.LevelCompleteCarSceneRentPopUp => Instantiate(levelCompleteCarSceneRentPopUp, popupsParent),
                Popup.LevelCompleteCarSceneRentTimeFinished => Instantiate(levelCompleteCarSceneRentTimeFinished,
                    popupsParent),
                _ => ActivePopUpObj
            };
        }

        public IEnumerator GameOver(float time, GameOverReason reason)
        {
            CancelInvoke(nameof(GamePlayController.Instance.Is_GameOver));
            if (GeneralDataManager.Instance.currentOpenedPopupList.Contains(Popup.GameOver) || !isGameStart)
                yield break;
            isGameStart = false;
            gameOverReason = reason;

            Stop_Car_Sounds();
            TouchDetectController.Inst.Set_ClickedObjectNull();
            yield return new WaitForSeconds(time);

            //Set in field Car
            var listToDestroyCar = new List<GameObject>();
            foreach (var item in LevelGenerator.Instance.vehicleControllers)
            {
                if (item._currentCarState != VehicleController.CarState.OnField)
                    listToDestroyCar.Add(item.gameObject);

                item.StopInFieldMovement();
            }

            foreach (var item in listToDestroyCar)
            {
                Destroy(item);
            }

            Show_Popup(Popup.GameOver);
            Hide_Screen();
        }


        public void Hide_Popup()
        {
            if (ActivePopUpObj == null)
            {
                GeneralDataManager.Instance.currentOpenedPopupList.Clear();
                return;
            }

            DestroyImmediate(ActivePopUpObj);

            ActivePopUpObj = popupsParent.childCount > 0
                ? popupsParent.GetChild(popupsParent.childCount - 1).gameObject
                : null;

            if (GeneralDataManager.Instance.currentOpenedPopupList.Count > 0)
                GeneralDataManager.Instance.currentOpenedPopupList.RemoveAt(GeneralDataManager.Instance
                    .currentOpenedPopupList.Count - 1);
        }

        public void HideAllPopUp()
        {
            var popUpList = new List<GameObject>();
            for (int i = 0; i < popupsParent.childCount; i++)
            {
                popUpList.Add(popupsParent.GetChild(i).gameObject);
            }

            if (popUpList.Count == 0) return;
            foreach (var item in popUpList)
            {
                DestroyImmediate(item);
            }

            GeneralDataManager.Instance.currentOpenedPopupList.Clear();
        }

        internal int ratePopUpLevel = 0;

        internal void Show_Rate_Popup()
        {
            ratePopUpLevel++;
            if (Application.internetReachability != NetworkReachability.NotReachable)
            {
                if (!userGiveRating && ratePopUpLevel > 3)
                {
                    var todayDate = DateTime.Now;
                    var lastDate = DateTime.Now;
                    var daysCount = 3;

                    if (!PlayerPrefs.HasKey("LastRateShowDate"))
                        PlayerPrefs.SetString("LastRateShowDate", DateTime.Now.AddDays(-1).ToString());

                    //PlayerPrefs.SetString("LastRateShowDate", new DateTime(2020, 5, 25).ToString());
                    if (PlayerPrefs.GetString("LastRateShowDate") != "")
                    {
                        lastDate = DateTime.Parse(PlayerPrefs.GetString("LastRateShowDate"));
                        var timeBetweenTwoDates = todayDate - lastDate;
                        daysCount = Mathf.Abs(timeBetweenTwoDates.Days);
                    }

                    if (daysCount >= 3)
                    {
                        PlayerPrefs.SetString("LastRateShowDate", todayDate.ToString());
                        Show_Popup(Popup.RateUsPopUp, false);
                    }
                }
            }
        }

     

        public void Show_Free_Coin_PopUp()
        {
            if (!loadingPanel.activeSelf) Show_Popup(Popup.FreeCoinPopUp, false);
        }

        //Make Toast Here.
        private GameObject _toast;

        internal void MakeToast(string toastMessage)
        {
            if (_toast != null)
                Destroy(_toast);
            _toast = Instantiate(Resources.Load<GameObject>("Prefab/Toast"), canvas);
            _toast.transform.GetChild(0).GetComponent<Text>().text = toastMessage;
        }

        public void NotEnoughCoinToast()
        {
            MakeToast("Not Enough Coin!");
        }

        public bool Is_Internet_Available()
        {
            return Application.internetReachability != NetworkReachability.NotReachable;
        }

        //public void Show_MoreGame_Ad(UnityAction callBack = null)
        //{
        //    if (purchaseAdsRemove)
        //    {
        //        callBack?.Invoke();
        //        return;
        //    }

        //    if (GeneralDataManager.Instance.gameData.MoreGamesAdShowCount >= 5 && ActivePopUpObj == null &&
        //        Is_Internet_Available())
        //    {
        //        Show_Popup(Popup.MoreGameAdView);
        //        FindObjectOfType<AdView_Controller>().onCloseCallBack = callBack;
        //        GeneralDataManager.Instance.gameData.MoreGamesAdShowCount = 0;
        //    }
        //    else
        //    {
        //        callBack?.Invoke();
        //    }
        //}

        public void Enable_Loading_Panel()
        {
            loadingPanel.SetActive(true);
            loadingPanel.GetComponent<LoadingPanelController>().CameraZoomOut();
        }

        public void Disable_Loading_Panel()
        {
            LoadingPanelController.Instance.Set_Disable();
            //loadingPanel.GetComponent<LoadingPanelController>().CameraZoomIn();
        }

        public bool IS_SplashScreenEnable()
        {
            return splashScreen != null && splashScreen.activeSelf;
        }

        public void Show_Shop_Screen()
        {
            Show_Popup(Popup.ShopPopUp, false);
        }

        private void Reset_Gravity()
        {
            Physics.gravity = new Vector3(0, -9.81f, 0);
        }

        public IEnumerator Set_Temporary_Gravity()
        {
            Physics.gravity = new Vector3(0, -9.81f, 0) * 20;
            yield return new WaitForSeconds(3f);
            Reset_Gravity();
        }

        public void Stop_Car_Sounds()
        {
            SoundManager.inst.Stop("CarDrift");
            SoundManager.inst.Stop("CarEngine");
        }

        public void Set_Remaining_Time(Text text, DateTime targetTime)
        {
            var timeSpan = targetTime - DateTime.Now;
            var time = string.Format("{0:D2} : {1:D2} : {2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
            text.text = time;
        }

        public string Int_To_Minute_Second_Time(int value)
        {
            var result = TimeSpan.FromSeconds(value);
            return result.ToString("mm':'ss");
        }

        public string Int_To_Hour_Minute_Second_Time(int value)
        {
            var result = TimeSpan.FromSeconds(value);
            return result.ToString("hh':'mm':'ss");
        }

        public void Set_BossChallenge_Data(int challengeNumber, bool giantChallenge, int reward)
        {
            currentBossChallengeNumber = challengeNumber;
            levelOrChallengeCompleteReward = reward;
            isGiantBossChallenge = giantChallenge;
        }

        public void Set_Challenge_Data(int challengeNumber)
        {
            currentChallengeNumber = challengeNumber;
        }

        public bool CanThemeRewardShow()
        {
            var themeRewardItemType = GetThemeTypeToUnlock();

            var allItem =
                Resources.LoadAll<ShopItemData>("ShopTheme/" + themeRewardItemType.ToString() + "ThemeShopItem");

            return (!ThemeSavedDataManager.IsAllItemUnlocked(themeRewardItemType, allItem));
        }

        public ShopItemData.ItemType GetThemeTypeToUnlock()
        {
            return CommonGameData.ThemeRewardItemTypeNumber switch
            {
                1 => ShopItemData.ItemType.Road,
                2 => ShopItemData.ItemType.Trail,
                _ => ShopItemData.ItemType.Road
            };
        }

      

        public void Play_Click_Sound()
        {
            SoundManager.inst.Play("Click");
        }

        public List<Vector3> Generate_Points(List<Vector3> keyPoints, int points = 20)
        {
            var smoothedPoints = new List<Vector3>();
            var smoothness = points;
            smoothedPoints.Add(keyPoints[0]);
            Vector3 p0, p1, p2;
            for (int j = 0; j < keyPoints.Count - 2; j++)
            {
                // determine control points of segment
                p0 = 0.5f * (keyPoints[j]
                             + keyPoints[j + 1]);
                p1 = keyPoints[j + 1];
                p2 = 0.5f * (keyPoints[j + 1]
                             + keyPoints[j + 2]);

                // set points of quadratic Bezier curve
                Vector3 position;
                float t;
                float pointStep = 1.0f / smoothness;
                for (int i = 0; i < smoothness; i++)
                {
                    t = i * pointStep;
                    position = (1.0f - t) * (1.0f - t) * p0
                               + 2.0f * (1.0f - t) * t * p1 + t * t * p2;
                    smoothedPoints.Add(position);
                }
            }

            smoothedPoints.Add(keyPoints.Last());

            return smoothedPoints;
        }

        public void LevelCompleteNextButtonClick()
        {
            if (GetCurrentChallengeTypeCurrentNumber() % CommonGameData.BossChallengeInterval == 0 &&
                GetCurrentChallengeTypeCurrentNumber() > 0)
            {
                Show_Popup(Popup.BossChallengeStart);
                return;
            }

            var checkCar = ThemeSavedDataManager.CheckCarThemeNumber();
            var checkScene = ThemeSavedDataManager.CheckSceneThemeNumber();
            if (checkCar || checkScene)
            {
                return;
            }

            if (CommonGameData.CurrentCompletedLevel >= 10 &&
                CommonGameData.CurrentCompletedLevel % CommonGameData.CarAndSceneThemeRewardInterval == 0 )
            {
                return;
            }
            
            switch (challengeType)
            {
                case CurrentChallengeType.Level:
                    GamePlayManager.Instance.Reset_Level();
                    break;
                case CurrentChallengeType.UnblockChallenge:
                    PlayUnblockCarChallenge();
                    break;
                case CurrentChallengeType.BossChallenge:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public int GetCurrentChallengeTypeCurrentNumber()
        {
            return challengeType switch
            {
                CurrentChallengeType.Level => CommonGameData.LevelNumber,
                CurrentChallengeType.BossChallenge => CommonGameData.BossChallengeNumber,
                CurrentChallengeType.UnblockChallenge => ChallengeDataManager.UnblockChallengeNumber,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public void PlayUnblockCarChallenge()
        {
            challengeType = CurrentChallengeType.UnblockChallenge;
            currentChallengeNumber = ChallengeDataManager.UnblockChallengeNumber;
            Set_Challenge_Data(currentChallengeNumber);
            SceneController.Instance.LoadScene((Scene)ThemeSavedDataManager.EnvironmentThemeNumber, null,
                () =>
                {
                    HideAllPopUp();
                    Show_Screen(GeneralDataManager.Screen.GamePlay);
                    GamePlayController.Instance?.Reset_Level();
                });
        }
    }
}