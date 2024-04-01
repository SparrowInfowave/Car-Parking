using System;
using System.Collections.Generic;
using GamePlay;
using Manager;
using ThemeSelection;
using UnityEngine;
using UnityEngine.UI;

public class LevelCompleteCarSceneThemeRewardPopUpController : SingletonComponent<LevelCompleteCarSceneThemeRewardPopUpController>
{
    [SerializeField] private Animation unlockButtonAnimation;
    [SerializeField] private ParticleSystem _particleSystem;
    [SerializeField] private Image headerImage;
    [SerializeField] private Text loseText;
    [SerializeField] private GameObject sceneItemObj;
    [SerializeField] private GameObject carItemObj;

    [Header("Theme Car")] 
    [SerializeField] private Image carIcon;
    [SerializeField] private Text carName;
    
    [Space(10)]
    [Header("Theme Scene")] 
    [SerializeField] private Image sceneIcon;
    [SerializeField] private Text sceneName;
    
    private ShopItemData _shopItemData = null;

    [SerializeField] private List<Sprite> headerSprites = new List<Sprite>();


    private void Start()
    {
       
    }

    public void SetShopItemData(ShopItemData shopItemData)
    {
        _shopItemData = shopItemData;
        loseText.text = "LOSE";
        carItemObj.SetActive(_shopItemData.itemType == ShopItemData.ItemType.Car);
        sceneItemObj.SetActive(_shopItemData.itemType == ShopItemData.ItemType.Environment);
        
        switch (_shopItemData.itemType)
        {
            case ShopItemData.ItemType.Car:
                headerImage.sprite = headerSprites[0];
                carIcon.sprite = _shopItemData.icon;
                carName.text = _shopItemData.name;
                break;
            case ShopItemData.ItemType.Environment:
                headerImage.sprite = headerSprites[1];
                sceneIcon.sprite = _shopItemData.icon;
                sceneName.text = _shopItemData.name;
                break;
            case ShopItemData.ItemType.Road:
            case ShopItemData.ItemType.Trail:
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void PlayUnlockButtonAnimation()
    {
        GetComponent<Animator>().enabled = false;
        unlockButtonAnimation.Play();
    }

    public void PlayParticleAnimation()
    {
        _particleSystem.Play();
    }

    public void WatchVideoButton()
    {
        //AdsManager.inst.RequestAndLoadRewardedAd("CarOrSceneRewardTheme");
    }

    public void GiveReward()
    {
        GetComponent<Animator>().enabled = true;
        GetComponent<Animator>().SetTrigger("Play");
        ThemeSavedDataManager.Set_CategoryThemeNumber(_shopItemData,_shopItemData.index);
    }

    public void ShowRateUsPopUp()
    {
        GameManager.Instance.Show_Rate_Popup();
    }

    public void NextButtonClick()
    {
        switch (GameManager.Instance.challengeType)
        {
            case CurrentChallengeType.Level:
                GamePlayManager.Instance.Reset_Level();
                break;
            case CurrentChallengeType.UnblockChallenge:
                GameManager.Instance.PlayUnblockCarChallenge();
                break;
            case CurrentChallengeType.BossChallenge:
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    

    public void ThemeButton()
    {
        GameManager.Instance.Show_Popup(GeneralDataManager.Popup.ThemeSelection, false);
    }

    public void HomeButton()
    {
        SceneController.Instance.GoTo_GamePlayScene_At_GameStart();
    }
}