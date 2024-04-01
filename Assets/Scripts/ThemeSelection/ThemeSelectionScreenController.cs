using System;
using System.Linq;
using Manager;
using UnityEngine;

namespace ThemeSelection
{
    public enum ButtonState
    {
        Empty,
        SelectedFor24Hours,
        NotSelectedFor24Hours,
        Locked,
        Unlocked,
        Selected
    }

    [RequireComponent(typeof(PanelContoller))]
    public class ThemeSelectionScreenController : SingletonComponent<ThemeSelectionScreenController>
    {
        public Color selectionUiGradTop,
            selectionUiGradBottom,
            selectionUiDefault,
            carThemeNameColorLocked,
            carThemeNameColorUnlocked,
            themeButtonIconSelect;

        [HideInInspector] public int currentItemCategoryTheme;

        private int _carThemeNumber;
        private int _environmentThemeNumber;
        private int _roadNumber;
        private int _charecterNumber;
        private int _trailNumber;

        [SerializeField] private PanelContoller panelContoller;
        
        private void Awake()
        {
            Reset_CurrentItemCategoryTheme();
        }

        private void Start()
        {
            GameManager.Instance.earnPopUpShowCount++;
            _carThemeNumber = ThemeSavedDataManager.CarThemeNumber;
            _environmentThemeNumber = ThemeSavedDataManager.EnvironmentThemeNumber;
            _roadNumber = ThemeSavedDataManager.RoadThemeNumber;
            _charecterNumber = ThemeSavedDataManager.CharecterThemeNumber;
            _trailNumber = ThemeSavedDataManager.TrailThemeNumber;
        }

      
        public void Reset_CurrentItemCategoryTheme()
        {
            currentItemCategoryTheme = GetItemType() switch
            {
                ShopItemData.ItemType.Car => ThemeSavedDataManager.CarThemeNumber,
                ShopItemData.ItemType.Environment => ThemeSavedDataManager.EnvironmentThemeNumber,
                ShopItemData.ItemType.Road => ThemeSavedDataManager.RoadThemeNumber,
                ShopItemData.ItemType.Trail => ThemeSavedDataManager.TrailThemeNumber,
                ShopItemData.ItemType.Charecter => ThemeSavedDataManager.CharecterThemeNumber,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private ShopItemData.ItemType GetItemType()
        {
            return panelContoller.Last_Panel_Index() switch
            {
                0 => ShopItemData.ItemType.Car,
                1 => ShopItemData.ItemType.Environment,
                2 => ShopItemData.ItemType.Road,
                3 => ShopItemData.ItemType.Trail,
                4 => ShopItemData.ItemType.Charecter,
                _ => throw new ArgumentOutOfRangeException()
            };
        }


        public void Reset_Buttons()
        {
            foreach (var themeItemButtonController in
                     panelContoller.CurrentContent().GetComponentsInChildren<ThemeItemButtonController>())
            {
                themeItemButtonController.Set_Button_Ui();
            }
        }

        public void BackButtonClick()
        {
            if (ChangeTheme())
                SceneController.Instance.GoTo_GamePlayScene_At_GameStart();
            else
                GameManager.Instance.Hide_Popup();
        }

        public void Unlock_Item((int index, ShopItemData item) themeDetail)
        {
            if (!ThemeSavedDataManager.Get_Unlocked_List_For_Current_Category(themeDetail.item.itemType).Contains(themeDetail.index))
            {
                ThemeSavedDataManager.Add_In_UnlockedData(themeDetail.item, themeDetail.index);

                SoundManager.inst.Play("EventSound");
                ThemeSavedDataManager.Set_CategoryThemeNumber(themeDetail.item, themeDetail.index);
                Reset_CurrentItemCategoryTheme();
                Reset_Buttons();
            }
        }

        private bool ChangeTheme()
        {
            //change theme from challenge selection
            if (_carThemeNumber != ThemeSavedDataManager.CarThemeNumber)
                return true;
            if (_environmentThemeNumber != ThemeSavedDataManager.EnvironmentThemeNumber)
                return true;
            if (_roadNumber != ThemeSavedDataManager.RoadThemeNumber)
                return true;
            if (_charecterNumber != ThemeSavedDataManager.CharecterThemeNumber)
                return true;
            if (_trailNumber != ThemeSavedDataManager.TrailThemeNumber)
                return true;
            return false;
        }
        
    }
}