using Asteroids.Entities.Player;
using Asteroids.Scene;
using Asteroids.Utils;

using UnityEngine;
using UnityEngine.UI;

namespace Asteroids.UI
{
    [DefaultExecutionOrder((int)ExecutionOrder.O3_LifeUI)]
    public class LifeUI : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField, Tooltip("Sprite used for life icons.")]
        private Sprite lifeSprite;
#pragma warning restore CS0649

        private Pool<Transform> pool;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Awake()
        {
            pool = new Pool<Transform>(PoolConstructor, PoolEnable, PoolDisable);
            EventManager.Subscribe<Player.HealthChangedEvent>(OnHealthChange);
            EventManager.Subscribe<GameSaver.LoadEvent>(OnLoad);

            for (int i = 0; i < Player.Lifes; i++)
                OnIncrease();
        }

        private void OnLoad()
        {
            while (transform.childCount > Player.Lifes)
                OnDecrease();

            while (transform.childCount < Player.Lifes)
                OnIncrease();
        }

        private void OnHealthChange(Player.HealthChangedEvent @event)
        {
            if (@event.HasIncreased)
                OnIncrease();
            else
                OnDecrease();
        }

        private void OnIncrease() => pool.Create();

        private void OnDecrease()
        {
            int childCount = transform.childCount;
            if (childCount > 0)
                pool.Store(transform.GetChild(childCount - 1));
        }

        private Transform PoolConstructor()
        {
            GameObject go = new GameObject("Life UI Slot");
            Image image = go.AddComponent<Image>();
            image.sprite = lifeSprite;
            image.preserveAspect = true;
            go.transform.SetParent(transform);
            go.GetComponent<RectTransform>().localScale = Vector3.one;
            return go.transform;
        }

        private void PoolEnable(Transform obj)
        {
            obj.gameObject.SetActive(true);
            obj.SetParent(transform);
        }

        private void PoolDisable(Transform obj)
        {
            obj.SetParent(null);
            obj.gameObject.SetActive(false);
        }
    }
}