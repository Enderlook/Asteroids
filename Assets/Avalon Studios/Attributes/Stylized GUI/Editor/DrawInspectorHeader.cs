using UnityEngine;
using UnityEditor;

namespace AvalonStudios.Additions.Attributes.StylizedGUIs
{
    public partial class StylizedGUI
    {
        private static float headerHeight;
        public static float height { get; private set; }

        public static void DrawInspectorHeader(Rect rect, string banner, int fontSize = 0, float displacementY = 10)
        {
            headerHeight = 20;
            Rect headerFullRect = new Rect(rect.position.x, rect.position.y + displacementY, rect.width, rect.height);
            Rect headerBeginRect = new Rect(headerFullRect.position.x, headerFullRect.position.y, 10, headerHeight);
            Rect headerMidRect = new Rect(headerFullRect.position.x + 10, headerFullRect.position.y, headerFullRect.xMax - 32, headerHeight);
            Rect headerEndRect = new Rect(headerFullRect.xMax - 10, headerFullRect.position.y, 10, headerHeight);
            Rect titleRect = new Rect(headerFullRect.position.x, headerFullRect.position.y, headerFullRect.width, 18);
            height = displacementY;

            if (EditorGUIUtility.isProSkin)
                GUI.color = GUIStylesConstants.DarkGrayColor;
            else
                GUI.color = GUIStylesConstants.WitheColor;

            GUIStyle catergoryImageB = new GUIStyle();
            catergoryImageB.normal.background = GUIStylesConstants.StyleBackground(GUIStylesConstants.HEADER_BEGIN);
            EditorGUI.LabelField(headerBeginRect, GUIContent.none, catergoryImageB);

            GUIStyle catergoryImageM = new GUIStyle();
            catergoryImageM.normal.background = GUIStylesConstants.StyleBackground(GUIStylesConstants.HEADER_MIDDLE);
            EditorGUI.LabelField(headerMidRect, GUIContent.none, catergoryImageM);

            GUIStyle catergoryImageE = new GUIStyle();
            catergoryImageE.normal.background = GUIStylesConstants.StyleBackground(GUIStylesConstants.HEADER_MIDDLE);
            EditorGUI.LabelField(headerEndRect, GUIContent.none, catergoryImageE);

            GUI.color = Color.white;
            GUIStyle stylesConstants = GUIStylesConstants.TitleStyle(fontSize == 0 ? 0 : fontSize);
            GUI.Label(titleRect, banner, stylesConstants);
        }
    }
}
