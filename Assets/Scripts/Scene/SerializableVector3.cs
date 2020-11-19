using System;

using UnityEngine;

namespace Asteroids.Scene
{
    [Serializable]
    public struct SerializableVector3
    {
        public float x;
        public float y;
        public float z;

        private SerializableVector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static implicit operator Vector3(SerializableVector3 vector)
            => new Vector3(vector.x, vector.y, vector.z);

        public static implicit operator SerializableVector3(Vector3 vector)
            => new SerializableVector3(vector.x, vector.y, vector.z);
    }
}