using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Utils.Editor
{
  public class CreateScriptableObjectWindowEditor : EditorWindow
  {
    [MenuItem("Assets/Tools/CreateScriptableObject")]
    private static void Create()
    {
      var monoScript = Selection.activeObject as MonoScript;
      if (monoScript)
      {
        if (typeof(ScriptableObject).IsAssignableFrom(monoScript.GetClass()))
        {
          GetWindow<CreateScriptableObjectWindowEditor>("Factory SO").Set(monoScript).ShowPopup();
        }
      }
    }

    [MenuItem("Assets/Tools/CreateScriptableObject", true)]
    private static bool CreateValidate()
    {
      var obj = Selection.activeObject as MonoScript;
      if (obj)
      {
        return typeof(ScriptableObject).IsAssignableFrom(obj.GetClass()) && !obj.GetClass().IsAbstract && !obj.GetClass().IsInterface;
      }
      return false;
    }

    private Type _scriptableObject;
    private string _path = "Assets";
    private string _name = "CreateScriptableObject";

    private CreateScriptableObjectWindowEditor Set(MonoScript script)
    {
      _name = script.name;
      _path = Path.GetDirectoryName(AssetDatabase.GetAssetPath(script));
      _scriptableObject = script.GetClass();
      return this;
    }

    private void OnGUI()
    {
      _path = EditorGUILayout.TextField("Path:", _path);
      _name = EditorGUILayout.TextField("Name:", _name);
      if (GUILayout.Button("Create"))
      {
        if (!Directory.Exists(_path))
        {
          Directory.CreateDirectory(_path);
        }
        CreateAssetInternal(_scriptableObject, Path.Combine(_path, _name + ".asset"));
      }
    }

    private static ScriptableObject CreateAssetInternal(Type scriptableObject, string path)
    {
      var asset = ScriptableObject.CreateInstance(scriptableObject);

      AssetDatabase.DeleteAsset(path);
      AssetDatabase.CreateAsset(asset, path);

      return asset;
    }
  }
}
