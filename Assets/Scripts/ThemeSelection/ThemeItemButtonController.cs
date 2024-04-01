using System;
using Manager;
using UnityEngine;


namespace ThemeSelection
{
    [RequireComponent(typeof(ShopThemeItemUiManager))]
    public class ThemeItemButtonController : MonoBehaviour
    {
        [SerializeField] private ShopThemeItemUiManager shopThemeItemUiManager;
        
        public void OnSelectThisTheme()
        {
            var itemData = shopThemeItemUiManager.shopItemData;
            
            if (Is_Locked())
            {
                switch (itemData.itemType)
                {
                    case ShopItemData.ItemType.Car:
                    case ShopItemData.ItemType.Environment:
                        break;
                    case ShopItemData.ItemType.Road:
                    case ShopItemData.ItemType.Trail:
                    case ShopItemData.ItemType.Charecter:
                    {
                        GameManager.Instance.MakeToast(itemData.itemType.ToString() +
                                                       " Is Locked!");
                        return;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (CommonGameData.Coin < itemData.coinPrice)
                {
                    GameManager.Instance.NotEnoughCoinToast();
                    return;
                }

                CommonGameData.DeductCoin(itemData.coinPrice);
                UnLockThisTheme();
                SetLastSelectedTheme(itemData);
            }
            else
            {
                if (ThemeSelectionScreenController.Instance.currentItemCategoryTheme == itemData.index) return;
                ThemeSavedDataManager.Set_CategoryThemeNumber(shopThemeItemUiManager.Get_ItemType(), itemData.index);
                ThemeSelectionScreenController.Instance.Reset_CurrentItemCategoryTheme();
                ThemeSelectionScreenController.Instance.Reset_Buttons();
                SetLastSelectedTheme(itemData);
            }
        }

        private static void SetLastSelectedTheme(ShopItemData itemData)
        {
           
        }

        public void Set_Button_Ui()
        {
           

            if (ThemeSelectionScreenController.Instance.currentItemCategoryTheme ==
                shopThemeItemUiManager.shopItemData.index)
            {
                shopThemeItemUiManager.Theme_Selected();
                return;
            }

            if (!Is_Locked())
            {
                shopThemeItemUiManager.Theme_Not_Selected();
                return;
            }

            shopThemeItemUiManager.Theme_Locked();
        }

        private void UnLockThisTheme()
        {
            ThemeSelectionScreenController.Instance.Unlock_Item((shopThemeItemUiManager.shopItemData.index,
                shopThemeItemUiManager.shopItemData));
        }

        private bool Is_Locked()
        {
            return ThemeSavedDataManager.Is_Theme_Locked(shopThemeItemUiManager.shopItemData.itemType,
                shopThemeItemUiManager.shopItemData.index);
        }
    }
}