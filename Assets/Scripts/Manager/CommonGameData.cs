using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Manager
{
    public static class CommonGameData
    {
        public static int RoadAndTrailThemeRewardInterval => 8;
        public static int CarAndSceneThemeRewardInterval => 6;
        public const int GiantBossChallengeInterval = 5;
        public static int BossChallengeInterval => 3;

        public static int Coin
        {
            get => PrefManager.GetInt(GetKeyName(nameof(CommonGameData), nameof(Coin)), 0);
            set => PrefManager.SetInt(GetKeyName(nameof(CommonGameData), nameof(Coin)), value);
        }

        public static int MoreGamesAdShowCount
        {
            get => PrefManager.GetInt(GetKeyName(nameof(CommonGameData), nameof(MoreGamesAdShowCount)), 0);
            set => PrefManager.SetInt(GetKeyName(nameof(CommonGameData), nameof(MoreGamesAdShowCount)), value);
        }

        public static int LevelNumber
        {
            get => PrefManager.GetInt(GetKeyName(nameof(CommonGameData), nameof(LevelNumber)), 0);
            set => PrefManager.SetInt(GetKeyName(nameof(CommonGameData), nameof(LevelNumber)), value);
        }

        public static int CurrentCompletedLevel
        {
            get => PrefManager.GetInt(GetKeyName(nameof(CommonGameData), nameof(CurrentCompletedLevel)), 0);
            set => PrefManager.SetInt(GetKeyName(nameof(CommonGameData), nameof(CurrentCompletedLevel)), value);
        }

        public static int BossChallengeNumber
        {
            get => ChallengeDataManager.CurrentChallengeNumber;
            set => ChallengeDataManager.CurrentChallengeNumber = value;
        }

        public static int ThemeRewardItemTypeNumber
        {
            get => PrefManager.GetInt(GetKeyName(nameof(CommonGameData), nameof(ThemeRewardItemTypeNumber)), 0);
            set => PrefManager.SetInt(GetKeyName(nameof(CommonGameData), nameof(ThemeRewardItemTypeNumber)), value);
        }

        public static string GetKeyName(string mainClassName, string objectName)
        {
            return mainClassName + "_" + objectName;
        }

        public static void AddCoin(int addedCoin)
        {
            Coin += addedCoin;
        }

        public static void DeductCoin(int deductCoin)
        {
            Coin -= deductCoin;
        }

        public static void AddCoinWithAnimation(int addedCoin)
        {
            Coin += addedCoin;
        }

        public static void IncreaseLevelNumber()
        {
            LevelNumber++;
        }

        public static void IncreaseMoreGameAdShowCount()
        {
            MoreGamesAdShowCount++;
        }

        public static void IncreaseCurrentCompletedLevel()
        {
            CurrentCompletedLevel++;
        }
        
        public static void SetOldData(GeneralDataManager.GameData gameData)
        {
            Coin = gameData.Coin;
            LevelNumber = gameData.levelNumber;
            MoreGamesAdShowCount = gameData.MoreGamesAdShowCount;
        }

        public static void IncreaseThemeRewardItemNumber()
        {
            ThemeRewardItemTypeNumber++;

            if (ThemeRewardItemTypeNumber > 2)
                ThemeRewardItemTypeNumber = 1;
        }
        
        public static bool IsHapticVibrationOn
        {
            get => PrefManager.GetBool(nameof(IsHapticVibrationOn), true);
            set => PrefManager.SetBool(nameof(IsHapticVibrationOn), value);
        }


        public static int GetBossChallengeReward()
        {
            return Random.Range(200, 250);
        }

    }
}