using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GeneralAnimEvents : MonoBehaviour
{
   
    public void Disable_Animator_Component()
    {
        this.GetComponent<Animator>().enabled = false;
    }

    public void Disable_This_Object()
    {
        this.gameObject.SetActive(false);
    }

    public void Destroy_This_Object()
    {
        Destroy(this.gameObject);
    }
}
