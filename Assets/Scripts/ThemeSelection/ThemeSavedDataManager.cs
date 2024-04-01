using System;
using System.Collections.Generic;
using Manager;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

namespace ThemeSelection
{
    public static class ThemeSavedDataManager
    {
        public static int CarThemeNumber
        {
            get => PrefManager.GetInt(CommonGameData.GetKeyName(nameof(ThemeSavedDataManager), nameof(CarThemeNumber)),
                1);
            set => PrefManager.SetInt(CommonGameData.GetKeyName(nameof(ThemeSavedDataManager), nameof(CarThemeNumber)),
                value);
        }

        public static int EnvironmentThemeNumber
        {
            get => PrefManager.GetInt(
                CommonGameData.GetKeyName(nameof(ThemeSavedDataManager), nameof(EnvironmentThemeNumber)), 1);
            set => PrefManager.SetInt(
                CommonGameData.GetKeyName(nameof(ThemeSavedDataManager), nameof(EnvironmentThemeNumber)), value);
        }

        public static int RoadThemeNumber
        {
            get => PrefManager.GetInt(
                CommonGameData.GetKeyName(nameof(ThemeSavedDataManager), nameof(RoadThemeNumber)), 1);
            set => PrefManager.SetInt(
                CommonGameData.GetKeyName(nameof(ThemeSavedDataManager), nameof(RoadThemeNumber)), value);
        }

        public static int TrailThemeNumber
        {
            get => PrefManager.GetInt(
                CommonGameData.GetKeyName(nameof(ThemeSavedDataManager), nameof(TrailThemeNumber)), 1);
            set => PrefManager.SetInt(
                CommonGameData.GetKeyName(nameof(ThemeSavedDataManager), nameof(TrailThemeNumber)), value);
        }
        
        public static int CharecterThemeNumber
        {
            get => PrefManager.GetInt(
                CommonGameData.GetKeyName(nameof(ThemeSavedDataManager), nameof(CharecterThemeNumber)), 1);
            set => PrefManager.SetInt(
                CommonGameData.GetKeyName(nameof(ThemeSavedDataManager), nameof(CharecterThemeNumber)), value);
        }

        public static List<int> Get_Unlocked_List_For_Current_Category(ShopItemData.ItemType itemType)
        {
            var keyName = GetKeyName(itemType);
            return PrefManager.HasKey(GetKeyName(itemType))
                ? JsonConvert.DeserializeObject<List<int>>(PrefManager.GetString(keyName))
                : new List<int> { 1 };
        }

        public static void Add_In_UnlockedData(ShopItemData item, int index)
        {
            var keyName = GetKeyName(item.itemType);
            var data = Get_Unlocked_List_For_Current_Category(item.itemType);
            data.Add(index);
            PrefManager.SetString(keyName, JsonConvert.SerializeObject(data));
            
            //Check in rent
            switch (item.itemType)
            {
                case ShopItemData.ItemType.Car:
                case ShopItemData.ItemType.Environment:
                case ShopItemData.ItemType.Road:
                case ShopItemData.ItemType.Trail:
                case ShopItemData.ItemType.Charecter:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
                
        }

        private static string GetKeyName(ShopItemData.ItemType itemType)
        {
            return itemType + "UnlockedData";
        }

        public static bool Is_Theme_Locked(ShopItemData.ItemType itemType, int index)
        {
            var keyName = GetKeyName(itemType);
            if (index == 1) return false;
            return !(PrefManager.HasKey(GetKeyName(itemType)) &&
                     JsonConvert.DeserializeObject<List<int>>(PrefManager.GetString(keyName))!.Contains(index));
        }

        public static void Set_CategoryThemeNumber(ShopItemData shopItemData, int index)
        {
            switch (shopItemData.itemType)
            {
                case ShopItemData.ItemType.Car:
                    CarThemeNumber = index;
                    break;
                case ShopItemData.ItemType.Environment:
                    EnvironmentThemeNumber = index;
                    break;
                case ShopItemData.ItemType.Road:
                    RoadThemeNumber = index;
                    break;
                case ShopItemData.ItemType.Trail:
                    TrailThemeNumber = index;
                    break;
                case ShopItemData.ItemType.Charecter:
                    CharecterThemeNumber = index;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static void SetOldData(GeneralDataManager.GameData oldGameData)
        {
            CarThemeNumber = oldGameData.CarThemeNumber;
            EnvironmentThemeNumber = oldGameData.EnvironmentThemeNumber;
        }

        public static bool IsAllItemUnlocked(ShopItemData.ItemType itemType, ShopItemData[] allItems = null)
        {
            allItems ??= Resources.LoadAll<ShopItemData>("ShopTheme/" + itemType.ToString() + "ThemeShopItem");

            var unlocked = Get_Unlocked_List_For_Current_Category(itemType);
            return allItems.Length == unlocked.Count;
        }

        public static bool CheckSceneThemeNumber()
        {
            
            return true;
        }

        public static bool CheckCarThemeNumber()
        {

          
            return true;
        }
    }
}