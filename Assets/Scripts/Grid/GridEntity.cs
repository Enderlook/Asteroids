//IA2-P2
// The whole file
// ^- Don't touch that comment, used by the teacher

using Asteroids.Scene;

using System;
using UnityEngine;

#if SPATIAL_GRID
namespace Spatial
{
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class GridEntity : MonoBehaviour, IGridEntity
    {
        private Rigidbody2D rigidbody2D;
        private Action<IGridEntity> onMove;
        private Vector2 lastPosition;

        Vector2 IGridEntity.Position {
            get => rigidbody2D.position;
            set => rigidbody2D.position = value;
        }

        event Action<IGridEntity> IGridEntity.OnMove {
            add => onMove += value;
            remove => onMove -= value;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        protected virtual void Awake()
        {
            rigidbody2D = GetComponent<Rigidbody2D>();
            GameManager.SpatialGrid.UpdateEntity(this);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void FixedUpdate()
        {
            if (lastPosition != rigidbody2D.position)
            {
                lastPosition = rigidbody2D.position;
                onMove?.Invoke(this);
            }
        }
    }
}
#endif