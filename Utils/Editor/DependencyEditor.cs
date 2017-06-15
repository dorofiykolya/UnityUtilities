using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Utils.Editor
{
  public class DependencyEditor
  {
    private static readonly Dictionary<string, HashSet<string>> _dependencyMap = new Dictionary<string, HashSet<string>>();
    private static readonly Dictionary<string, HashSet<string>> _referencesMap = new Dictionary<string, HashSet<string>>();

    private static void FillDependency(Dictionary<string, HashSet<string>> dependencyMap, Dictionary<string, HashSet<string>> referencesMap)
    {
      var assets = AssetDatabase.GetAllAssetPaths();
      foreach (var asset in assets)
      {
        HashSet<string> set;
        if (!dependencyMap.TryGetValue(asset, out set))
        {
          dependencyMap[asset] = set = new HashSet<string>();
        }
        var dependencies = AssetDatabase.GetDependencies(asset);
        foreach (var dependency in dependencies)
        {
          set.Add(dependency);
          HashSet<string> references;
          if (!referencesMap.TryGetValue(dependency, out references))
          {
            referencesMap[dependency] = references = new HashSet<string>();
          }
          references.Add(asset);
        }
      }
    }

    public static string[] GetReferences(string asset)
    {
      _dependencyMap.Clear();
      _referencesMap.Clear();
      FillDependency(_dependencyMap, _referencesMap);
      Dictionary<string, HashSet<string>> map = _referencesMap;
      HashSet<string> set;
      if (map.TryGetValue(asset, out set))
      {
        return set.ToArray();
      }
      return new string[0];
    }

    [MenuItem("Assets/Tools/Log References")]
    private static void LogReferences()
    {
      if (Selection.activeObject != null)
      {
        var references = GetReferences(AssetDatabase.GetAssetPath(Selection.activeObject));
        foreach (var reference in references)
        {
          Debug.Log(reference);
        }
      }
    }
  }
}
