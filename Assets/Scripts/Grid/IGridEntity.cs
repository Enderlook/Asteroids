using System;
using UnityEngine;

#if SPATIAL_GRID
namespace Spatial
{
    public interface IGridEntity
    {
        event Action<IGridEntity> OnMove;

        Vector2 Position { get; set; }
    }
}
#endif