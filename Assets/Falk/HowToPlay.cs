using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HowToPlay : MonoBehaviour
{
    [SerializeField]
    private Button startButton;
    [SerializeField]
    private Button quitButton;
    [SerializeField]
    private Button howtoplay;

    public GameObject screen;


    public void HowPlay()
    {
        //start.SetActive(!start.activeSelf);
        startButton.gameObject.SetActive(!startButton.gameObject.activeSelf);
        quitButton.gameObject.SetActive(!quitButton.gameObject.activeSelf);
        howtoplay.gameObject.SetActive(!howtoplay.gameObject.activeSelf);

        screen.SetActive(!screen.activeSelf);
    }
}
