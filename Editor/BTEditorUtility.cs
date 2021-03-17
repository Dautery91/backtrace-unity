using UnityEditor;
using UnityEngine;

namespace Backtrace.Unity.Editor
{
    public static class BTEditorUtility
    {
        /// <summary>
        /// Draws a horizontal line across the EditorUI in a custom editor script (Window, Property, Inspector).
        /// </summary>
        /// <param name="color"></param>
        /// <param name="thickness"></param>
        /// <param name="padding"></param>
        public static void DrawHorizontalUILine(Color color, int thickness = 2, int padding = 2)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
            r.height = thickness;
            r.y += padding / 2;
            r.x -= 2;
            r.width += 6;
            EditorGUI.DrawRect(r, color);
        }

        public static GUILayoutOption[] DefineLayoutSizingConstraints(float minHeight, float maxHeight,
            float minWidth, float maxWidth)
        {
            return new GUILayoutOption[]
            {
                GUILayout.MinHeight(minHeight),
                GUILayout.MinWidth(minWidth),
                GUILayout.MaxHeight(maxHeight),
                GUILayout.MaxWidth(maxWidth)
            };
        }
        
        public static void DrawSubHeading(string subHeaderName)
        {
            BTEditorUtility.DrawHorizontalUILine(Color.grey,1,0);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField(subHeaderName, BTEditorUtility.SubHeaderTextStyle);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            BTEditorUtility.DrawHorizontalUILine(Color.grey,1,0);
        }

        public static void DrawHeader(Texture tex, Color bgColor, Rect position)
        {
            var texRect = GUILayoutUtility.GetRect(0f, 0f);
            var bgRect = GUILayoutUtility.GetRect(0f, 0f);
            texRect.width = tex.width / 2.2f;
            texRect.height = tex.height / 2.2f;
            bgRect.width = position.width;
            bgRect.height = texRect.height + 5;
            GUILayout.Space(texRect.height);
            EditorGUI.DrawRect(bgRect,bgColor);
            GUI.DrawTexture(texRect, tex);
        }

        #region Style and Layout Templates
        
        public static GUIStyle HeaderTextStyle { get; } = new GUIStyle()
        {
            alignment = TextAnchor.MiddleLeft,
            margin = new RectOffset(),
            padding = new RectOffset(),
            fontSize = 15,
            fontStyle = FontStyle.Bold
        };
        
        public static GUIStyle SubHeaderTextStyle { get; } = new GUIStyle()
        {
            alignment = TextAnchor.MiddleCenter,
            margin = new RectOffset(),
            padding = new RectOffset(),
            fontSize = 12,
            fontStyle = FontStyle.Bold
        };

        public static GUILayoutOption[] BaseFieldSizingLayoutOptions { get; } = new GUILayoutOption[]
        {
            GUILayout.MinHeight(10),
            GUILayout.MinWidth(60),
            GUILayout.MaxHeight(30),
            GUILayout.MaxWidth(600)
        };

        #endregion
    }
}