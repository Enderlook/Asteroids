using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class music : MonoBehaviour
{
    /*[SerializeField]
    AudioClip audioClip;*/


    private void Awake()
    {

        int numMusic = FindObjectsOfType<music>().Length;

        if (numMusic != 1)
            Destroy(gameObject);
        else
            DontDestroyOnLoad(gameObject);
    }
    void Start()
    {
       
    }
    
    
    void Update()
    {
        
    }
}
