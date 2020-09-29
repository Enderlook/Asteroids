using Asteroids.Entities.Player;
using Asteroids.Utils;

using UnityEngine;
using UnityEngine.UI;

namespace Asteroids.UI
{
    [DefaultExecutionOrder(1)]
    public class LifeUI : MonoBehaviour
    {
        [SerializeField, Tooltip("Sprite used for life icons.")]
        private Sprite lifeSprite;

        private Pool<Transform> pool;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Awake()
        {
            pool = new Pool<Transform>(PoolConstructor, PoolEnable, PoolDisable);
            EventManager.Subscribe(EventManager.Event.PlayerLostOneLife, OnDecrease);
            EventManager.Subscribe(EventManager.Event.PlayerGotNewLife, OnIncrease);

            for (int i = 0; i < Player.StartingLifes; i++)
                OnIncrease();
        }

        private void OnIncrease() => pool.Get();

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
            go.transform.SetParent(transform);
            return go.transform;
        }

        private void PoolEnable(Transform obj)
        {
            obj.gameObject.SetActive(true);
            obj.SetParent(transform);
        }

        private void PoolDisable(Transform obj)
        {
            obj.parent = null;
            obj.gameObject.SetActive(false);
        }
    }
}