using System;
using UnityEngine;

namespace Spatial
{
    public interface IGridEntity
    {
        event Action<IGridEntity> OnMove;

        Vector2 Position { get; set; }
    }
}