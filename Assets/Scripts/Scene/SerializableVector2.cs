using System;

using UnityEngine;

namespace Asteroids.Scene
{
    [Serializable]
    public struct SerializableVector2
    {
        public float x;
        public float y;

        private SerializableVector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public static implicit operator Vector2(SerializableVector2 vector)
            => new Vector3(vector.x, vector.y);

        public static implicit operator SerializableVector2(Vector2 vector)
            => new SerializableVector2(vector.x, vector.y);
    }
}