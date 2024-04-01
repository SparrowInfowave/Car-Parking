using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AddressableManager;
using UnityEngine;
using UnityEngine.Events;

namespace ThemeSelection
{
    public class ThemeItemLoad : MonoBehaviour
    {
        [SerializeField] private GameObject loader;
        [SerializeField] private GameObject contentTransform;
        [SerializeField] private ShopItemData.ItemType itemType = ShopItemData.ItemType.Car;

        private List<ShopItemData> _datas = new List<ShopItemData>();

        [SerializeField] private UnityEvent onLoadComplete;
        
        private void OnEnable()
        {
            if (_datas.Count == 0)
            {
                if (itemType == ShopItemData.ItemType.Car)
                    ThemeSavedDataManager.CheckCarThemeNumber();
                
                if (itemType == ShopItemData.ItemType.Environment)
                    ThemeSavedDataManager.CheckSceneThemeNumber();
                
                StartCoroutine(LoadItems());
            }
        }

        private IEnumerator LoadItems()
        {
            loader.SetActive(true);
            contentTransform.SetActive(false);

            yield return new WaitForEndOfFrame();

            _datas = Resources.LoadAll<ShopItemData>("ShopTheme/" + itemType.ToString() + "ThemeShopItem").ToList();
            _datas = _datas.OrderBy(x=>x.index).ToList();
            foreach (var item in _datas)
            {
                ThemeItemSpawner.Instance.SpawnItem(new ThemeItemSpawner.ThemeItemClass
                    { Parent = contentTransform.transform, ShopItemData = item });
            }
            
            yield return new WaitUntil(() => ThemeItemSpawner.Instance.CheckAllLoaded());

            yield return new WaitForSeconds(0.05f);

            foreach (var item in contentTransform.GetComponentsInChildren<ShopThemeItemUiManager>(true))
            {
               item.Set_Button_Data();
            }
            
            yield return new WaitForSeconds(0.05f);

            foreach (var item in contentTransform.GetComponentsInChildren<ThemeItemButtonController>(true))
            {
                item.Set_Button_Ui();
            }

            yield return new WaitForSeconds(0.05f);
            
            loader.SetActive(false);
            contentTransform.SetActive(true);
            onLoadComplete?.Invoke();
        }

        private void OnDisable()
        {
            for (int i = 0; i < contentTransform.transform.childCount; i++)
            {
                Destroy(contentTransform.transform.GetChild(i).gameObject);
            }
            _datas.Clear();
        }
    }
}