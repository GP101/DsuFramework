#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Dsu.Extension
{
    public static class DsuEditorSettings
    {
        public static bool EnableWorldTransform
        {
            get => EditorPrefs.GetBool("Dsu_EnableWorldTransform", false);
            set => EditorPrefs.SetBool("Dsu_EnableWorldTransform", value);
        }

        public static bool HierarchyIconsEnabled
        {
            get => EditorPrefs.GetBool("HierarchyIcons_Enabled", true);
            set => EditorPrefs.SetBool("HierarchyIcons_Enabled", value);
        }

        public static bool ShowScriptIcon
        {
            get => EditorPrefs.GetBool("HierarchyIcons_ShowScriptIcon", true);
            set => EditorPrefs.SetBool("HierarchyIcons_ShowScriptIcon", value);
        }

        public static bool ShowScriptIconWithCount
        {
            get => EditorPrefs.GetBool("HierarchyIcons_ShowScriptIconWithCount", true);
            set => EditorPrefs.SetBool("HierarchyIcons_ShowScriptIconWithCount", value);
        }

        public static bool ShowUIIcon
        {
            get => EditorPrefs.GetBool("HierarchyIcons_ShowUIIcon", true);
            set => EditorPrefs.SetBool("HierarchyIcons_ShowUIIcon", value);
        }

        public static bool Show2DIcon
        {
            get => EditorPrefs.GetBool("HierarchyIcons_Show2DIcon", true);
            set => EditorPrefs.SetBool("HierarchyIcons_Show2DIcon", value);
        }

        public static bool ShowOtherIcons
        {
            get => EditorPrefs.GetBool("HierarchyIcons_ShowOtherIcons", true);
            set => EditorPrefs.SetBool("HierarchyIcons_ShowOtherIcons", value);
        }

        public static bool ShowLayer
        {
            get => EditorPrefs.GetBool("HierarchyIcons_ShowLayer", true);
            set => EditorPrefs.SetBool("HierarchyIcons_ShowLayer", value);
        }

        public static bool ShowTag
        {
            get => EditorPrefs.GetBool("HierarchyIcons_ShowTag", true);
            set => EditorPrefs.SetBool("HierarchyIcons_ShowTag", value);
        }

        public static int MaxIconCount
        {
            get => EditorPrefs.GetInt("HierarchyIcons_MaxIconCount", 8);
            set => EditorPrefs.SetInt("HierarchyIcons_MaxIconCount", Mathf.Max(0, value));
        }

        public static bool HighlightSpecial
        {
            get => EditorPrefs.GetBool("HierarchyIcons_HighlightSpecial", true);
            set => EditorPrefs.SetBool("HierarchyIcons_HighlightSpecial", value);
        }

        public static bool HighlightDisabled
        {
            get => EditorPrefs.GetBool("HierarchyIcons_HighlightDisabled", true);
            set => EditorPrefs.SetBool("HierarchyIcons_HighlightDisabled", value);
        }

        public static Color SpecialHighlightColor
        {
            get => LoadColor("HierarchyIcons_SpecialColor", Color.red);
            set => SaveColor("HierarchyIcons_SpecialColor", value);
        }

        public static Color DisabledHighlightColor
        {
            get => LoadColor("HierarchyIcons_DisabledColor", new Color(0f, 0.6f, 0f));
            set => SaveColor("HierarchyIcons_DisabledColor", value);
        }

        public static bool HighlightFolderWithCS
        {
            get => EditorPrefs.GetBool("ProjectFolder_HighlightCS", true);
            set => EditorPrefs.SetBool("ProjectFolder_HighlightCS", value);
        }

        public static Color HighlightFolderColor
        {
            get => LoadColor("ProjectFolder_HighlightCSColor", Color.yellow);
            set => SaveColor("ProjectFolder_HighlightCSColor", value);
        }

        public static bool EnableFolderPrefixHighlight
        {
            get => EditorPrefs.GetBool("ProjectFolder_EnablePrefixHighlight", true);
            set => EditorPrefs.SetBool("ProjectFolder_EnablePrefixHighlight", value);
        }

        public static string FolderPrefixToHighlight
        {
            get => EditorPrefs.GetString("ProjectFolder_HighlightPrefix", "");
            set => EditorPrefs.SetString("ProjectFolder_HighlightPrefix", value);
        }

        public static Color FolderPrefixHighlightColor
        {
            get => LoadColor("ProjectFolder_HighlightPrefixColor", Color.cyan);
            set => SaveColor("ProjectFolder_HighlightPrefixColor", value);
        }

        private static void SaveColor(string key, Color color)
        {
            EditorPrefs.SetFloat(key + "_R", color.r);
            EditorPrefs.SetFloat(key + "_G", color.g);
            EditorPrefs.SetFloat(key + "_B", color.b);
            EditorPrefs.SetFloat(key + "_A", color.a);
        }

        private static Color LoadColor(string key, Color defaultColor)
        {
            if (EditorPrefs.HasKey(key + "_R")) {
                return new Color(
                    EditorPrefs.GetFloat(key + "_R"),
                    EditorPrefs.GetFloat(key + "_G"),
                    EditorPrefs.GetFloat(key + "_B"),
                    EditorPrefs.GetFloat(key + "_A"));
            }
            return defaultColor;
        }
    }
}
#endif
