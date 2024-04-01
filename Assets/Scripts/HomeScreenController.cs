using Manager;
using UnityEngine;
using UnityEngine.UI;

public class HomeScreenController : MonoBehaviour
{
    [SerializeField] private Text levelText;
    [SerializeField] GameObject moreGamesBtn, carDrivingAdBtn;

    private void Start()
    {
        moreGamesBtn.SetActive(Application.platform == RuntimePlatform.Android);
        carDrivingAdBtn.SetActive(Application.platform == RuntimePlatform.Android);
    }

    private void OnEnable()
    {
        GameManager.Instance.isGameStart = false;
        Set_LevelNumber();
    }

    public void Set_LevelNumber()
    {
        levelText.text = "LEVEL " + (CommonGameData.LevelNumber + 1);
    }

    public void ThemeSelection_Btn_Click()
    {
        GameManager.Instance.Show_Popup(GeneralDataManager.Popup.ThemeSelection);
    }
    

    public void Play_Button_Click()
    {
        GameManager.Instance.Show_Screen(GeneralDataManager.Screen.GamePlay);
    }
    
    public void Setting_Button_Click()
    {
        GameManager.Instance.Show_Popup(GeneralDataManager.Popup.SettingPopUp);
    }
    
    public void HotPack_Button_Click()
    {
        GameManager.Instance.Show_Popup(GeneralDataManager.Popup.HotPackPopUp);
    }

    public void MoreGame_Button_Click()
    {
        GameManager.Instance.Show_Popup(GeneralDataManager.Popup.MoreGameScreen);
    }

    public void CarDriving_Ad_Button_Click()
    {
        Application.OpenURL("https://play.google.com/store/apps/details?id=com.morefun.car.parking.driving.simulator");
    }

    public void Show_ChallengeSelectionScreen()
    {
        GameManager.Instance.PlayUnblockCarChallenge();
    }
}
