using UnityEditor;
using UnityEngine;

namespace Utils.Editor
{
  [InitializeOnLoad]
  public static class HierarchyEditor
  {
    private static readonly string PreferenceKeyEnabled = typeof(HierarchyEditor).FullName + ".enabled";
    private static readonly string PreferenceKeyRightPadding = typeof(HierarchyEditor).FullName + ".rightPadding";

    [PreferenceItem("Hierarchy")]
    private static void PreferancesGUI()
    {
      EditorPrefs.SetBool(PreferenceKeyEnabled, EditorGUILayout.Toggle("Enable", EditorPrefs.GetBool(PreferenceKeyEnabled)));
      EditorPrefs.SetFloat(PreferenceKeyRightPadding, EditorGUILayout.FloatField("Right Padding", EditorPrefs.GetFloat(PreferenceKeyRightPadding, 0f)));
    }

    static HierarchyEditor()
    {
      EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemOnGui;
    }

    private static void HierarchyWindowItemOnGui(int instanceId, Rect selectionRect)
    {
      if (!EditorPrefs.GetBool(PreferenceKeyEnabled)) return;

      var instance = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
      if (instance == null)
        return;

      var rightPadding = EditorPrefs.GetFloat(PreferenceKeyRightPadding, 0f);
      selectionRect.xMin += selectionRect.width - 16f - rightPadding;
      instance.SetActive(GUI.Toggle(selectionRect, instance.activeSelf, GUIContent.none));
    }
  }
}
