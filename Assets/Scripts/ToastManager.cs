using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToastManager : MonoBehaviour
{
    public void Complete_Toast_Animation()
    {
        Destroy(this.gameObject);
    }
}
