using UnityEngine;
using UnityEditor;

namespace AvalonStudios.Additions.Utils.IconContentHandle
{
    public sealed class IconContentHandle
    {
        public static GUIContent Help => EditorGUIUtility.IconContent("_Help");

        public static GUIContent TransformIcon => EditorGUIUtility.IconContent("Transform Icon");

        public static GUIContent ViewToolMove => EditorGUIUtility.IconContent("ViewToolMove");

        public static GUIContent MoveTool => EditorGUIUtility.IconContent("MoveTool");

        public static GUIContent RotateTool => EditorGUIUtility.IconContent("RotateTool");

        public static GUIContent ScaleTool => EditorGUIUtility.IconContent("ScaleTool");

        public static GUIContent RectTool => EditorGUIUtility.IconContent("RectTool");

        public static GUIContent TransformTool => EditorGUIUtility.IconContent("TransformTool");

        public static GUIContent InfoIcon => EditorGUIUtility.IconContent("console.infoicon");

        public static GUIContent WarningIcon => EditorGUIUtility.IconContent("console.warnicon");

        public static GUIContent ErrorIcon => EditorGUIUtility.IconContent("console.erroricon");

        public static GUIContent SpriteIcon => EditorGUIUtility.IconContent("Sprite Icon");
    }
}
