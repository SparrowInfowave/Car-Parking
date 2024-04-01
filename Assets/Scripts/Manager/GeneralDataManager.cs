using System.Collections.Generic;
using System.IO;
using GamePlay;
using Newtonsoft.Json;
using ThemeSelection;
using UnityEngine;

namespace Manager
{
    public class GeneralDataManager : MonoBehaviour
    {
        public static GeneralDataManager Instance;

        internal int watchVideoCoin = 50;

        [HideInInspector]
        public GameData oldGameData = new GameData();

        public int totalLevel = 575;
        public int totalBossChallenge = 530;
        public int totalChallenge = 530;

        private const string GameDataKey = "GameData";
        
        public static bool userGiveRating
        {
            get => PrefManager.GetBool(nameof(userGiveRating), false);
            set => PrefManager.SetBool(nameof(userGiveRating), value);
        }
        
        public static string shopSavedData
        {
            get => PrefManager.GetString(nameof(shopSavedData));
            set => PrefManager.SetString(nameof(shopSavedData), value);
        }
        
        public bool isUnlockCarTutorialShowed
        {
            get => PrefManager.GetBool(nameof(isUnlockCarTutorialShowed), false);
            set => PrefManager.SetBool(nameof(isUnlockCarTutorialShowed), value);
        }


        //Active popup enum
        public enum Popup
        {
            Empty,
            SettingPopUp,
            ExitPopUp,
            FreeCoinPopUp,
            RateUsPopUp,
            MoreGameScreen,
            MoreGameAdView,
            LevelCompleteCoinReward,
            LevelCompleteThemeReward,
            LevelCompleteThemeNotAvailable,
            LevelCompleteCarSceneRentPopUp,
            LevelCompleteCarSceneRentTimeFinished,
            BossChallengeComplete,
            BossChallengeStart,
            GameOver,
            ThemeSelection,
            HotPackPopUp,
            ShopPopUp,
            HintBuyPopUp
        }

        public static GameObject ActivePopUpObj;
        [HideInInspector] public List<Popup> currentOpenedPopupList = new List<Popup>();

        public enum Screen
        {
            Empty,
            Home,
            GamePlay
        }

        public static GameObject ActiveScreenObj;
        public static Screen CurrentScreen = Screen.Home;

        public bool testMode = false;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(this.gameObject);

            DontDestroyOnLoad(this.gameObject);

        }

        private void Start()
        {
            Input.multiTouchEnabled = false;
            Application.targetFrameRate = 1000;
            Load_Data();
            //Check_Level();
        }
        
        
        private void Load_Data()
        {
           
                CommonGameData.CurrentCompletedLevel = CommonGameData.LevelNumber;
            

            if (PlayerPrefs.HasKey(GameDataKey))
            {
                oldGameData = JsonConvert.DeserializeObject<GameData>(PlayerPrefs.GetString(GameDataKey));
                CommonGameData.SetOldData(oldGameData);
                ThemeSavedDataManager.SetOldData(oldGameData);
                PrefManager.DeleteKey(GameDataKey);
            }

            if (PrefManager.HasKey(nameof(ChallengeDataManager._challengeDataOldGameData)))
            {
                ChallengeDataManager._challengeDataOldGameData = JsonConvert.DeserializeObject<ChallengeDataNew>(
                    PrefManager.GetString(nameof(ChallengeDataManager._challengeDataOldGameData)));
                
                ChallengeDataManager.SetOldData(ChallengeDataManager._challengeDataOldGameData);
                PrefManager.DeleteKey(nameof(ChallengeDataManager._challengeDataOldGameData));
            }

            if (testMode)
            {
                CommonGameData.AddCoin(50000);
            }

            FindObjectOfType<HomeScreenController>()?.Set_LevelNumber();
        }

        [System.Serializable]
        public class GameData
        {
            public int Coin = 0;
            public int levelNumber = 0;
            public int MoreGamesAdShowCount = 0;
            public int CarThemeNumber = 1;
            public int EnvironmentThemeNumber = 1;
        }
    }
}