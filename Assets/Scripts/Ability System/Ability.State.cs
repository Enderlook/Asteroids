using System;

namespace Asteroids.AbilitySystem
{
    public abstract partial class Ability
    {
        [Serializable]
        public readonly struct State
        {
            // This class is in charge of storing and setting the ability state to save the game.

            // Ability state the same as Ability memento, we can take advantage of that to keep DRY
            private readonly Memento memento;

            public State(Ability ability) => memento = new Memento(ability);

            public void Load(Ability ability) => memento.Load(ability);
        }
    }
}
