using System;
using System.Collections;
using ThemeSelection;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Manager
{
    public enum Scene
    {
        Loading = 0,
        DefaultTheme = 1,
        DesertTheme = 2,
        BeachTheme = 3,
        SnowTheme = 4,
        Antarctica = 5,
        Egypt = 6,
        Factories = 7,
        Farm = 8,
        Forest = 9,
        Planet = 10,
        Town = 11,
    }

    public class SceneController : MonoBehaviour
    {
        public static SceneController Instance;

        public Scene currentScene = Scene.Loading;
        
        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(this.gameObject);

            DontDestroyOnLoad(this.gameObject);
        }

        
        public void GoTo_GamePlayScene_At_GameStart()
        {
            var theme = (Scene)ThemeSavedDataManager.EnvironmentThemeNumber;

            GameManager.Instance.challengeType = CurrentChallengeType.Level;
            LoadScene(theme, null, () =>
            {
                GameManager.Instance.HideAllPopUp();
                GameManager.Instance.Show_Screen(GeneralDataManager.Screen.Home);
                GameManager.Instance.homeScreen.SetActive(true);
                
            });
        }
        
        public void GoTo_GamePlayScene_At_SplashOff()
        {
            var theme = (Scene)ThemeSavedDataManager.EnvironmentThemeNumber;
            GameManager.Instance.challengeType = CurrentChallengeType.Level;
            LoadScene(theme, null, () =>
            {
                GameManager.Instance.HideAllPopUp();
                GameManager.Instance.Show_Screen(GeneralDataManager.Screen.Home);
                GameManager.Instance.homeScreen.SetActive(true);
            });
        }

        public void LoadScene(Scene sceneToLoad, UnityAction onstartSceneLoad = null,
            UnityAction afterSceneLoad = null)
        {
            StartCoroutine(Load(sceneToLoad, onstartSceneLoad, afterSceneLoad));
        }

        private IEnumerator Load(Scene sceneToLoad, UnityAction onstartSceneLoad = null,
            UnityAction afterSceneLoad = null)
        {
            GameManager.Instance.Enable_Loading_Panel();
            yield return new WaitForSeconds(1f);
            onstartSceneLoad?.Invoke();
            SceneManager.UnloadSceneAsync(currentScene.ToString(),UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
            currentScene = sceneToLoad;
            SceneManager.LoadSceneAsync(currentScene.ToString());

            afterSceneLoad?.Invoke();
        }
    }
}