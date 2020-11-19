using System.Collections;
using System.Collections.Generic;
using Enderlook.Unity.Components.ScriptableSound;

using Asteroids.Scene;

using UnityEngine;

public partial class PlayerController : MonoBehaviour, IObservable
{
   

    private static PlayerController instance;

   
    private int lifes;

    private new Rigidbody2D rigidbody;

    private new Collider2D collider;

    private int scoreToNextLife;

    private float invulnerabilityTime;

    private PlayerModel model;
    private PlayerView view;

    private List<IObserver> observers = new List<IObserver>();

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
    private void Awake()
    {

      

        if (instance != null)
        {
            Debug.LogError($"{nameof(PlayerController)} can't have more than one instance at the same time.");
            Destroy(gameObject);
            return;
        }
        instance = this;
        view = GetComponent<PlayerView>();
        rigidbody = GetComponent<Rigidbody2D>();
        collider = GetComponent<Collider2D>();
       
        model = GetComponent<PlayerModel>();

        lifes = model.startingLifes;
        scoreToNextLife = model.scorePerLife;

        EventManager.Subscribe<GameManager.ScoreHasChangedEvent>(OnScoreChanged);

        Memento.TrackForRewind(this);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
    private void Update()
    {
        if (invulnerabilityTime > 0)
        {
            invulnerabilityTime -= Time.deltaTime;
            if (invulnerabilityTime <= 0)
            {
                collider.enabled = true;
                //view.ColorWhite();
                Notify("WhiteColor");
            }
            else
            {
                Notify("OtherColor");
                //view.ColorOther(); 
            }
           Notify("Hit");
        }
    }

    private void OnScoreChanged(GameManager.ScoreHasChangedEvent @event)
    {
        if (scoreToNextLife <= @event.NewScore)
        {
            model.newLife.Play();
            scoreToNextLife += model.scorePerLife;
            AddNewLife();
        }
    }

     public void AddNewLife()
    {
        lifes++;
        EventManager.Raise(HealthChangedEvent.Increase);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        
        //view.DeadSound();

        if (lifes == 0)
            EventManager.Raise(GameManager.LevelTerminationEvent.Lose);
        else
        {
            lifes--;
            EventManager.Raise(HealthChangedEvent.Decrease);
        }

        rigidbody.position = Vector2.zero;
        rigidbody.rotation = 0;
        rigidbody.velocity = default;

        BecomeInvulnerable();
    }

    private void BecomeInvulnerable()
    {
        collider.enabled = false;
        invulnerabilityTime = model.invulnerabilityDuration;
    }

    public void Suscribe(IObserver observer)
    {
        observers.Add(observer);
    }

    public void UnSubscribe(IObserver observer)
    {
        if (observers.Contains(observer))
            observers.Remove(observer);
       
    }

    private void Notify(string eventType)
    {
        foreach (var observer in observers)
        {
            observer.OnNotify(eventType);
        }
    }
}

