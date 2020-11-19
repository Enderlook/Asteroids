using System.Collections;
using System.Collections.Generic;
using Enderlook.Unity.Components.ScriptableSound;
using UnityEngine;

public class PlayerModel : MonoBehaviour
{
    [Tooltip("Amount of lifes the player start with.")]
    public int startingLifes;

    [SerializeField, Tooltip("Maximum amount of lifes the player can have.")]
    private int maxLifes;

    [SerializeField, Tooltip("Duration of invulnerability after lose a life.")]
    public float invulnerabilityDuration;

    [SerializeField, Min(1), Tooltip("Amount of points required to earn a new life. Up to a maximum of one life can be get per score increase.")]
    public int scorePerLife;

    [SerializeField, Tooltip("Sound played on death.")]
    public SimpleSoundPlayer deathSound;

    [SerializeField, Tooltip("Sound played on get new life.")]
    public SimpleSoundPlayer newLife;


    private int lifes;

    private static PlayerModel instance;
    public static int Lifes => instance.lifes;

    public static int MaxLifes => instance.maxLifes;
   
    
    private new SpriteRenderer renderer;
}
