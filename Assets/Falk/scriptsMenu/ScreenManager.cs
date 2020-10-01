using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenManager : MonoBehaviour
{
    [SerializeField]
    private GameObject screen;

    public bool isScreenOn;
    
    public void OnOff()
    {
        if(screen!=null)
        {
             screen.SetActive(!screen.activeSelf);
            isScreenOn = !isScreenOn;

        }
    }  
}
