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

      GetWindow<InspectScriptableObjectsWindowEditor>("ScriptableObjectFactory", true).Show(monoScripts).ShowPopup();
    }

    private InspectScriptableObjectsWindowEditor Show(List<MonoScript> scripts)
    {
      _scripts = scripts;

      return this;
    }

    private List<MonoScript> _scripts;
    private Vector2 _position;

    private void OnGUI()
    {
      if (_scripts != null)
      {
        _position = EditorGUILayout.BeginScrollView(_position);
        foreach (var script in _scripts)
        {
          if (script != null)
          {
            if (GUILayout.Button(script.name, EditorStyles.miniButton))
            {
              CreateScriptableObjectWindowEditor.Open(script);
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
