using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Menu : MonoBehaviour
{
   
    public void GoToGame()
   {
        SceneManager.LoadScene("Level");
   }


    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("SALE DEL JUEGO");
    }

    

}