using System;
using System.Collections;
using Coffee.UIExtensions;
using Manager;
using UnityEngine;
using UnityEngine.UI;
using VLB;

namespace ThemeSelection
{
    public class ShopThemeItemUiManager : MonoBehaviour
    {
        [SerializeField] private GameObject  lockedObj, buttonsObj, borderObj, timerObj;
        [SerializeField] private Text nameText, infoText, coinPriceText, timerText;
        [SerializeField] private Image carIcon;
        public ShopItemData shopItemData;
        [SerializeField] private UIGradient _uiGradient;
        private ButtonState _currentButtonState = ButtonState.Empty;
        
        public void Set_Button_Data()
        {
            SetNameText();
            SetInfoText();
            SetCoinText();
            CheckForSoftMaskAble();
            carIcon.sprite = shopItemData.icon;
        }
        
        public void Theme_Selected()
        {
            if (_currentButtonState == ButtonState.Selected)
                return;

            var themeSelectController = ThemeSelectionScreenController.Instance;
            Set_TimerObj(false);
            lockedObj.SetActive(false);
            borderObj.SetActive(true);
            SetInfoObj(true);
            NameObjectActive(true);
            Set_ButtonsObject(false);
            SetIconOpacity(1);
            SetNameTextColor(themeSelectController.carThemeNameColorUnlocked);
            Set_Ui_Gradient(themeSelectController.selectionUiGradTop, themeSelectController.selectionUiGradBottom);

            _currentButtonState = ButtonState.Selected;
        }

        public void Theme_Not_Selected()
        {
            if (_currentButtonState == ButtonState.Unlocked)
                return;

            var themeSelectController = ThemeSelectionScreenController.Instance;
            Set_TimerObj(false);
            lockedObj.SetActive(false);
            borderObj.SetActive(false);
            SetInfoObj(true);
            Set_ButtonsObject(false);
            NameObjectActive(true);
            SetNameTextColor(themeSelectController.carThemeNameColorUnlocked);
            SetIconOpacity(1);
            Set_Ui_Gradient(themeSelectController.selectionUiDefault, themeSelectController.selectionUiDefault);

            _currentButtonState = ButtonState.Unlocked;
            
        }

        public void Theme_Locked()
        {
            if (_currentButtonState == ButtonState.Locked)
                return;

            var themeSelectController = ThemeSelectionScreenController.Instance;
            Set_TimerObj(false);
            lockedObj.SetActive(true);
            borderObj.SetActive(false);
            Set_ButtonsObject(true);
            SetInfoObj(false);
            NameObjectActive(false);
            SetNameTextColor(themeSelectController.carThemeNameColorLocked);
            SetIconOpacity(IconOpacity());
            Set_Ui_Gradient(themeSelectController.selectionUiDefault, themeSelectController.selectionUiDefault);

            _currentButtonState = ButtonState.Locked;
        }

        private float IconOpacity()
        {
            switch (shopItemData.itemType)
            {
                case ShopItemData.ItemType.Car:
                case ShopItemData.ItemType.Environment:
                case ShopItemData.ItemType.Road:
                case ShopItemData.ItemType.Trail:
                    return 0.7f;
                case ShopItemData.ItemType.Charecter:
                    return 0.8f;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        public void Theme_SelectedFor24Hours()
        {
            if (_currentButtonState == ButtonState.SelectedFor24Hours)
                return;

            var themeSelectController = ThemeSelectionScreenController.Instance;
            Set_TimerObj(true);
            lockedObj.SetActive(false);
            borderObj.SetActive(true);
            SetInfoObj(false);
            NameObjectActive(false);
            Set_ButtonsObject(false);
            
            SetIconOpacity(1);
            SetNameTextColor(themeSelectController.carThemeNameColorUnlocked);
            Set_Ui_Gradient(themeSelectController.selectionUiGradTop, themeSelectController.selectionUiGradBottom);

            _currentButtonState = ButtonState.SelectedFor24Hours;
        }
        
        public void Theme_NotSelectedFor24Hours()
        {
            if (_currentButtonState == ButtonState.NotSelectedFor24Hours)
                return;

            var themeSelectController = ThemeSelectionScreenController.Instance;
            Set_TimerObj(true);
            lockedObj.SetActive(false);
            borderObj.SetActive(false);
            SetInfoObj(false);
            Set_ButtonsObject(false);
            NameObjectActive(false);
            SetNameTextColor(themeSelectController.carThemeNameColorUnlocked);
            SetIconOpacity(1);
            Set_Ui_Gradient(themeSelectController.selectionUiDefault, themeSelectController.selectionUiDefault);
            
            _currentButtonState = ButtonState.NotSelectedFor24Hours;
        }
        
        private void Set_Ui_Gradient(Color colorTop, Color colorBottom)
        {
            if (_uiGradient == null)
                _uiGradient = GetComponent<UIGradient>();

            _uiGradient.color1 = colorTop;
            _uiGradient.color2 = colorBottom;
        }

        private void SetInfoText()
        {
            if(infoText != null)
                infoText.text = shopItemData.info;
        }
        
        private void SetNameText()
        {
            if(nameText != null)
                nameText.text = shopItemData.name;
        }

        private void NameObjectActive(bool isActive)
        {
            if(nameText == null)return;

            switch (shopItemData.itemType)
            {
                case ShopItemData.ItemType.Car:
                    nameText.gameObject.SetActive(isActive);
                    break;
                case ShopItemData.ItemType.Charecter:
                case ShopItemData.ItemType.Environment:
                    nameText.gameObject.SetActive(true);
                    break;
                case ShopItemData.ItemType.Road:
                case ShopItemData.ItemType.Trail:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private void SetInfoObj(bool isEnable)
        {
            if(infoText != null)
                infoText.gameObject.SetActive(isEnable);
        }
        
        private void SetCoinText()
        {
            if(coinPriceText != null)
                coinPriceText.text = shopItemData.coinPrice.ToString();
        }

        private void Set_ButtonsObject(bool isEnable)
        {
            if(buttonsObj != null)
                buttonsObj.SetActive(isEnable);
        }
        
        private void Set_TimerObj(bool isEnable)
        {
            if (timerObj == null) return;
            
            timerObj.SetActive(isEnable);
            if (timerObj.activeSelf)
            {
                CalculateTime();
            }
        }

        private int _totalSeconds = 60;
        private void CalculateTime()
        {
            CancelInvoke(nameof(StartTime));
            InvokeRepeating(nameof(StartTime),0f,1f);
        }

        private void StartTime()
        {
            var timeInFormate = GameManager.Instance.Int_To_Hour_Minute_Second_Time(_totalSeconds);
            timerText.text = timeInFormate;
            _totalSeconds--;

            if (_totalSeconds <= 0)
            {
                Theme_Locked();
            }
        }

        private void SetNameTextColor(Color color)
        {
            if(nameText != null)
                nameText.color = color;;
        }

        private void SetIconOpacity(float alpha)
        {
            var color = carIcon.color;
            carIcon.color = new Color(color.r, color.g, color.b, alpha);
        }

        private void CheckForSoftMaskAble()
        {
            switch (shopItemData.itemType)
            {
                case ShopItemData.ItemType.Car:
                case ShopItemData.ItemType.Environment:
                    break;
                case ShopItemData.ItemType.Road:
                case ShopItemData.ItemType.Trail:
                case ShopItemData.ItemType.Charecter:
                    var softMaskable = this.GetOrAddComponent<SoftMaskable>();
                    softMaskable.m_UseStencil = true;
                    softMaskable.SetMaskInteraction(SpriteMaskInteraction.None);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        

        public ShopItemData Get_ItemType()
        {
            return shopItemData;
        }
    }
}