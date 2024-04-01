using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using Manager;

namespace Manager
{
    public enum ButtonState
    {
        Empty,
        Locked,
        UnlockNow,
        UnlockedNotCompleted,
        Completed
    }

    public enum ButtonStateNew
    {
        Locked,
        PurchasedAndPlayed
    }

    public static class ChallengeDataManager
    {
        public static ChallengeDataNew _challengeDataOldGameData = new ChallengeDataNew();
        public static int challengeStartPrice = 100;
        public static int retryPrice = 20;

        public static int CityNameIndex
        {
            get => PrefManager.GetInt(CommonGameData.GetKeyName(nameof(ChallengeDataManager), nameof(CityNameIndex)),
                0);
            set => PrefManager.SetInt(CommonGameData.GetKeyName(nameof(ChallengeDataManager), nameof(CityNameIndex)),
                value);
        }

        public static int CurrentSceneNumber
        {
            get => PrefManager.GetInt(
                CommonGameData.GetKeyName(nameof(ChallengeDataManager), nameof(CurrentSceneNumber)), 1);
            set => PrefManager.SetInt(
                CommonGameData.GetKeyName(nameof(ChallengeDataManager), nameof(CurrentSceneNumber)), value);
        }

        public static int CurrentChallengeNumber
        {
            get => PrefManager.GetInt(
                CommonGameData.GetKeyName(nameof(ChallengeDataManager), nameof(CurrentChallengeNumber)), 1);
            set => PrefManager.SetInt(
                CommonGameData.GetKeyName(nameof(ChallengeDataManager), nameof(CurrentChallengeNumber)), value);
        }

        public static int UnblockChallengeNumber
        {
            get => PrefManager.GetInt(
                CommonGameData.GetKeyName(nameof(ChallengeDataManager), nameof(UnblockChallengeNumber)), 1);
            set => PrefManager.SetInt(
                CommonGameData.GetKeyName(nameof(ChallengeDataManager), nameof(UnblockChallengeNumber)), value);
        }

        public static ButtonStateNew CurrentState
        {
            get
            {
                return GetEnum(PrefManager.GetString(
                    CommonGameData.GetKeyName(nameof(ChallengeDataManager), nameof(CurrentState)),
                    ButtonStateNew.Locked.ToString()));
            }
            set
            {
                PrefManager.SetString(CommonGameData.GetKeyName(nameof(ChallengeDataManager), nameof(CurrentState)), value.ToString());
            }
        }

        public static bool IsAnimationPlayed
        {
            get => PrefManager.GetBool(
                CommonGameData.GetKeyName(nameof(ChallengeDataManager), nameof(IsAnimationPlayed)), true);
            set => PrefManager.SetBool(
                CommonGameData.GetKeyName(nameof(ChallengeDataManager), nameof(IsAnimationPlayed)), value);
        }

        private static string GetKey(int index)
        {
            return "Chapter" + index;
        }

        public static void Set_ChallengeDataForOldVersion()
        {
            for (int i = 1; i < 15; i++)
            {
                if (PrefManager.HasKey(GetKey(i)))
                {
                    var data = JsonConvert.DeserializeObject<Dictionary<int, ChallengeDataOld>>(
                        PrefManager.GetString(GetKey(i)));

                    if (data == null) continue;

                    foreach (var item in data)
                    {
                        CurrentChallengeNumber = item.Value.State switch
                        {
                            ButtonState.UnlockedNotCompleted => item.Key,
                            ButtonState.Completed => item.Key + 1,
                            _ => CurrentChallengeNumber
                        };
                    }

                    PlayerPrefs.DeleteKey(GetKey(i));
                }
                else
                {
                    break;
                }
            }
        }
        
        public static void NextChallenge()
        {
            UnblockChallengeNumber++;
            IsAnimationPlayed = false;
            CurrentState = ButtonStateNew.Locked;
        }
        

        private static ButtonStateNew GetEnum(string enumText)
        {
            return (ButtonStateNew)System.Enum.Parse(typeof(ButtonStateNew), enumText);
        }

        public static void SetOldData(ChallengeDataNew challengeDataNew)
        {
            CurrentChallengeNumber = challengeDataNew.CurrentChallengeNumber;
            CurrentSceneNumber = _challengeDataOldGameData.CurrentSceneNumber;
            IsAnimationPlayed = challengeDataNew.IsAnimationPlayed;
            CurrentState = challengeDataNew.State;
            CityNameIndex = challengeDataNew.CityNameIndex;
        }
    }

    public class ChallengeDataOld
    {
        public ButtonState State = ButtonState.Locked;
        public bool IsGiantLevel = false;
        public int Moves = 0;
        public int Time = 0;
    }

    public class ChallengeDataNew
    {
        public ButtonStateNew State = ButtonStateNew.Locked;
        public int CurrentChallengeNumber = 1;
        public int CurrentSceneNumber = 1;
        public int CityNameIndex = 0;
        public bool IsAnimationPlayed = true;
    }
}