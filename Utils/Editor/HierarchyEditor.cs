using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Utils.Editor
{
  [InitializeOnLoad]
  public static class HierarchyEditor
  {
    private static readonly string PreferenceKey = typeof(HierarchyEditor).FullName + ".enabled";


    [PreferenceItem("Hierarchy")]
    private static void PreferancesGUI()
    {
      EditorPrefs.SetBool(PreferenceKey, EditorGUILayout.Toggle("Enable", EditorPrefs.GetBool(PreferenceKey)));
    }

    static HierarchyEditor()
    {
      EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemOnGui;
    }

    private static void HierarchyWindowItemOnGui(int instanceId, Rect selectionRect)
    {
      if (!EditorPrefs.GetBool(PreferenceKey)) return;

      var instance = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
      if (instance == null)
        return;

      selectionRect.xMin += selectionRect.width - 16f;
      instance.SetActive(GUI.Toggle(selectionRect, instance.activeSelf, GUIContent.none));
    }
  }
}
