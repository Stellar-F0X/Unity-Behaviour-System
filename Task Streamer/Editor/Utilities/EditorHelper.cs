using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TaskStreamer.Tool
{
    public static class EditorHelper
    {
        public static T FindAssetByName<T>(string searchFilter) where T : Object
        {
            string[] guids = AssetDatabase.FindAssets(searchFilter);

            if (guids is null || guids.Length == 0)
            {
                return null;
            }

            foreach (var guid in guids)
            {
                string parentPath = AssetDatabase.GUIDToAssetPath(guid);

                if (File.Exists(parentPath))
                {
                    return AssetDatabase.LoadAssetAtPath<T>(parentPath);
                }
            }

            throw new FileNotFoundException($"Asset not found at filter: {searchFilter}");
        }


        public static string FindAssetPath(string searchFilter)
        {
            string[] guids = AssetDatabase.FindAssets(searchFilter);

            if (guids != null && guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);

                if (File.Exists(path))
                {
                    return path;
                }
            }

            throw new FileNotFoundException($"Asset not found at filter: {searchFilter}");
        }
        


        public static void DrawError(in Rect rect, in string message, in float iconSize = 12f)
        {
            Rect iconRect = new Rect(rect.x, rect.y + (rect.height - iconSize) * 0.5f, iconSize, iconSize);
            Rect textRect = new Rect(rect.x + iconSize + 2f, rect.y, rect.width - iconSize - 2f, rect.height);

            Texture warningImg = EditorGUIUtility.IconContent("console.warnicon").image;
            GUI.DrawTexture(iconRect, warningImg, ScaleMode.ScaleToFit);

            EditorGUI.LabelField(textRect, message);
        }
        


#region Custom Editor GUI Helpers
        public static GUIStyle GetHeaderStyle()
        {
            GUIStyle headerLabelStyle = new GUIStyle(EditorStyles.toolbar);
            headerLabelStyle.alignment = TextAnchor.MiddleLeft;
            headerLabelStyle.fontStyle = FontStyle.Bold;
            headerLabelStyle.fontSize = 13;
            return headerLabelStyle;
        }
        
        
        public static void DrawHeader(string header, GUIStyle headerLabelStyle, float startSpacing = 0f, float endSpacing = 0f)
        {
            if (Mathf.Approximately(startSpacing, 0f) == false)
            {
                EditorGUILayout.Space(startSpacing);
            }

            using (new GUIColorScope(new Color32(255, 255, 255, 255), GUIColorScope.EGUIColorScope.Background))
            {
                EditorGUILayout.LabelField(header, headerLabelStyle);
            }

            if (Mathf.Approximately(endSpacing, 0f) == false)
            {
                EditorGUILayout.Space(endSpacing);
            }
        }


        public static void DrawPropertiesRange(SerializedProperty start, SerializedProperty stop = null, bool includeChildren = true, bool startInclusive = true)
        {
            bool started = false;

            do
            {
                if (stop != null && SerializedProperty.EqualContents(start, stop))
                {
                    break;
                }

                if (started || startInclusive)
                {
                    EditorGUILayout.PropertyField(start, includeChildren);
                }

                started = true;
            }
            while (start.NextVisible(false));
        }


        public static bool HasRemainingPropertiesAfter(SerializedProperty startProperty)
        {
            if (startProperty == null)
            {
                return false;
            }

            SerializedProperty iterator = startProperty.Copy();

            int propertyCount = 0;

            while (iterator.NextVisible(false))
            {
                propertyCount++;
            }

            return propertyCount > 0;
        }
#endregion
    }
}