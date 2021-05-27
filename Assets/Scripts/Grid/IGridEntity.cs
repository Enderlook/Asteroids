using System;
using UnityEngine;

namespace Spatial
{
    public interface IGridEntity
    {
        event Action<IGridEntity> OnMove;

        Vector3 Position { get; set; }
    }
}