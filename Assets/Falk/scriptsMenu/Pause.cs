using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pause : MonoBehaviour
{
    ScreenManager screenManager;
    private void Awake()
    {
        screenManager = GetComponent<ScreenManager>();
    }
    public void ThePause()
    {
        if (screenManager.isScreenOn == true)
        {
            Time.timeScale = 1;

        }
        else
        {
            Time.timeScale = 0;
        }
    }
}
