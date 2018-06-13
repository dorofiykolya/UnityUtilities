using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Utils.Editor
{
  public class InspectScriptableObjectsWindowEditor : EditorWindow
  {
    [MenuItem("Assets/Tools/ScriptableObjectsFactory")]
    private static void ShowScriptableObjects()
    {
      var monoScripts = new List<MonoScript>();
      var csharpFiles = AssetDatabase.GetAllAssetPaths().Where(f => Path.GetExtension(f) == ".cs");
      foreach (var csharpFile in csharpFiles)
      {
        var script = AssetDatabase.LoadAssetAtPath<MonoScript>(csharpFile);
        if (script != null)
        {
          var type = script.GetClass();
          if (!typeof(UnityEditor.Editor).IsAssignableFrom(type) && !typeof(UnityEditor.EditorWindow).IsAssignableFrom(type) && typeof(ScriptableObject).IsAssignableFrom(type) && !type.IsAbstract &&
              !type.IsInterface)
          {
            monoScripts.Add(script);
          }
        }
      }
      monoScripts.Sort((s1, s2) => string.Compare(s1.name, s2.name));
      GetWindow<InspectScriptableObjectsWindowEditor>("ScriptableObjectFactory", true).Show(monoScripts, Selection.activeObject).ShowPopup();
    }

    private InspectScriptableObjectsWindowEditor Show(List<MonoScript> scripts, Object activeObject)
    {
      _scripts = scripts;
      _path = activeObject != null ? AssetDatabase.GetAssetPath(activeObject) : null;
      _scripts.RemoveAll(m => m == null);

      return this;
    }

    private List<MonoScript> _scripts;
    private Vector2 _position;
    private string _findText = string.Empty;
    private MonoScript[] _currentScrips;
    private string _path;

    private void OnGUI()
    {
      if (_scripts != null)
      {
        EditorGUILayout.BeginHorizontal();
        _findText = EditorGUILayout.TextField("Find: ", _findText, "SearchTextField");
        if (GUILayout.Button("x", EditorUtils.Styles.SearchCancelButton))
        {
          _findText = string.Empty;
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Separator();
        _currentScrips = _scripts.Where(s => s != null && s.name.ToLowerInvariant().Contains(_findText.ToLowerInvariant())).ToArray();
        _position = EditorGUILayout.BeginScrollView(_position);

        var style = new GUIStyle(EditorStyles.miniButton);
        style.alignment = TextAnchor.MiddleLeft;
        foreach (var script in _currentScrips)
        {
          if (script != null)
          {
            if (GUILayout.Button(script.name + " [" + AssetDatabase.GetAssetPath(script) + "]", style))
            {
              CreateScriptableObjectWindowEditor.Open(script, _path);
            }
          }
        }

        EditorGUILayout.EndScrollView();
      }
      else
      {
        GUILayout.Label("empty");
      }
    }
  }
}
