using System.Linq;
using UnityEngine;

namespace Asteroids.AbilitySystem
{
    [CreateAssetMenu(menuName = "Asteroids/Ability System/Abilities/Abilities Pack", fileName = "Ability Package")]
    public class AbilitiesPack : ScriptableObject
    {
        [SerializeField, Tooltip("Abilities")]
        private Ability[] abilities = null;

        public void Initialize(AbilitiesManager manager) => System.Array.ForEach(abilities, a => a.Initialize(manager));

        public void Update() => System.Array.ForEach(abilities, a => a.Update());
    }
}
