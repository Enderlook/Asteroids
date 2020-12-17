using AvalonStudios.Additions.Extensions;

using System;
using System.Reflection;

using UnityEngine;

namespace AvalonStudios.Additions.Utils.SnapSettings
{
    public static class SnapSettingsHandle
    {
        private static Assembly assembly;

        private static readonly Action<Vector3> move;
        private static readonly Action<float> scale;
        private static readonly Action<float> rotation;

        public static readonly PropertyInfo moveInfo;
        public static readonly PropertyInfo scaleInfo;
        public static readonly PropertyInfo rotationInfo;

        private const string MOVE = "move";
        private const string SCALE = "scale";
        private const string ROTATION = "rotation";

        public static readonly Type snapSettings;
        private static object snapSettingsObject;

        static SnapSettingsHandle()
        {
            assembly = Assembly.Load("UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            snapSettings = assembly.GetType("UnityEditor.SnapSettings");

            moveInfo = snapSettings.GetProperty(MOVE);
            scaleInfo = snapSettings.GetProperty(SCALE);
            rotationInfo = snapSettings.GetProperty(ROTATION);

            move = (Action<Vector3>)moveInfo.GetSetMethod(true).CreateDelegate(typeof(Action<Vector3>));
            scale = (Action<float>)scaleInfo.GetSetMethod(true).CreateDelegate(typeof(Action<float>));
            rotation = (Action<float>)rotationInfo.GetSetMethod(true).CreateDelegate(typeof(Action<float>));

            snapSettingsObject = ScriptableObject.CreateInstance(snapSettings);
        }

        public static void ApplyMove(Vector3 snap) =>
            move(snap);

        public static void ApplyScale(float snap) =>
            scale(snap);

        public static void ApplyRotation(float snap) =>
            rotation(snap);

        public static void GetValues(ref Vector3 move, ref float scale, ref float rotation)
        {
            move = moveInfo.GetValue(snapSettingsObject, null).GetVector3Value();
            scale = float.Parse(scaleInfo.GetValue(snapSettingsObject, null).ToString(), System.Globalization.CultureInfo.InvariantCulture);
            rotation = float.Parse(rotationInfo.GetValue(snapSettingsObject, null).ToString(), System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
