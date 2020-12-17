using System;

using UnityEngine;

namespace AvalonStudios.Additions.Extensions
{
    public static class TypeExtensions
    {
        public static Vector3 GetVector3Value(this object obj)
        {
            Vector3 result = Vector3.zero;
            if (obj.GetType() == typeof(Vector3))
                result = obj.ToString().ToVector3();
            else
                Debug.LogError("The object is not a Vector3 type.");
            return result;
        }
    }
}
