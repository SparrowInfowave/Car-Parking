using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "ShopItemData",fileName = "NewShopItemData")]
public class ShopItemData : ScriptableObject
{
    public enum ItemType
    {
        Car,
        Environment,
        Road,
        Trail,
        Charecter
    }

    public ItemType itemType = ItemType.Car;
    public int index = 1;
    public string name = "Cars";
    public string info = "Colorful Cars";
    public Sprite icon;
    public int coinPrice;
}
