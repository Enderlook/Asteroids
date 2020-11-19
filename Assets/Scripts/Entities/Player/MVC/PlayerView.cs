using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerView : MonoBehaviour, IObserver
{
    PlayerModel model;
    
    private new SpriteRenderer renderer;
  
    void Awake()
    {
        model = GetComponent<PlayerModel>();
        
        renderer = GetComponent<SpriteRenderer>();
        
        
    }

  
    public void ColorWhite()
    {
       renderer.color = Color.white;
       
    }

    public void ColorOther()
    {
        renderer.color = new Color(.5f, .5f, .5f);
        
    }

    public void DeadSound()
    {
     
        model.deathSound.Play();
    }

    public void OnNotify(string eventType)
    {
        if (eventType == "Hit")
        {
            model.deathSound.Play();
            
            
        }

        if (eventType == "OtherColor")
        {
            ColorOther();
          
        }

        if (eventType == "WhiteColor")
        {
            ColorWhite();
           
        }
    }
}
