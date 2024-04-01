using Manager;
using UnityEngine;

public class CoinShowHide : MonoBehaviour
{
    [SerializeField] private bool isShow = true;
    
    private void OnEnable()
    {
       CancelInvoke();
       Invoke(nameof(CoinEnable),0.04f);
    }

    private void CoinEnable()
    {
        GameManager.Instance.coinObj.SetActive(isShow);
    }


    private void Enable_Coin()
    {
        GameManager.Instance.coinObj.SetActive(true);
    }

    private void OnDisable()
    {
        Enable_Coin();
    }
}
