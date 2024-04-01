using System;
using UnityEngine;

public class AdLoaderPanelController : MonoBehaviour
{
    private void OnEnable()
    {
        Invoke(nameof(DisablePanel),20f);
    }

    private void DisablePanel()
    {
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(DisablePanel));
    }
}
