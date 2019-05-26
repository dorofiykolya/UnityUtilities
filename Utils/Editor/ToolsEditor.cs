using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Utils.Editor
{
  public class ToolsEditor : EditorWindow
  {
    [MenuItem("Assets/Tools/TextDisableAlignByGeometry")]
    public static void TextDisableAlignByGeometry()
    {
      var list = new List<Text>();
      foreach (var gameObject in Selection.gameObjects)
      {
        var temp = new List<Text>();
        gameObject.GetComponents(temp);
        gameObject.GetComponentsInChildren(temp);
        list.AddRange(temp);
      }
      foreach (var text in list)
      {
        text.alignByGeometry = false;
        EditorUtility.SetDirty(text);
      }
      AssetDatabase.SaveAssets();
    }

    [MenuItem("Tools/Delete Empty Folders")]
    public static void DeleteEmptyFolders()
    {
      var directories = Directory.GetDirectories(Application.dataPath, "*", SearchOption.AllDirectories);
      var index = 0;
      var total = directories.Length;
      foreach (var directory in directories)
      {
        if (Directory.Exists(directory))
        {
          EditorUtility.DisplayProgressBar("Scan", directory, index / (float)total);
          var files = Directory.GetFiles(directory, "*", SearchOption.AllDirectories);
          if (files == null || files.Length == 0 || files.All(s => Path.GetExtension(s) == ".meta"))
          {
            Directory.Delete(directory, true);
            if (File.Exists(directory + ".meta"))
            {
              File.Delete(directory + ".meta");
            }
            Debug.Log("DELETE DIRECTORY: " + directory);
            EditorUtility.DisplayProgressBar("Delete directory", directory, index / (float)total);
          }
        }
        index++;
      }
      EditorUtility.ClearProgressBar();
    }

    [MenuItem("Tools/Clear Prefs")]
    public static void ClearPlayerPrefs()
    {
      PlayerPrefs.DeleteAll();
    }

    [MenuItem("Tools/Clear Editor Prefs")]
    public static void ClearEditorPrefs()
    {
      EditorPrefs.DeleteAll();
    }

    private static Transform f(Transform g)
    {
      foreach (var item in g.GetComponents<Component>())
        if (item == null)
          return g;
      foreach (Transform childT in g)
        if (f(childT))
          return g;
      return null;
    }

    [MenuItem("Assets/Tools/Rename To Underscore")]
    private static void RenameToUnderscore()
    {
      var assets = Selection.objects;
      float index = 0f;
      float total = assets.Length;
      try
      {
        foreach (var asset in assets)
        {
          index++;
          EditorUtility.DisplayProgressBar("RENAME FILES", asset.name, index / total);

          var path = AssetDatabase.GetAssetPath(asset);
          string fileName = Path.GetFileNameWithoutExtension(path);
          var newFileName = Regex.Replace(fileName, "(?<=[a-z0-9])[A-Z]", m => "_" + m.Value);
          newFileName = newFileName.Replace("-", "_");
          newFileName = newFileName.ToLowerInvariant();
          AssetDatabase.RenameAsset(path, newFileName);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
      }
      finally
      {
        EditorUtility.ClearProgressBar();
      }
    }

    [MenuItem("Tools/Remove Missing Scripts (On Selected GameObject)")]
    public static void CleanupMissingScripts()
    {
      var go = Selection.gameObjects;

      int goCount = 0;
      int componentsCount = 0;
      int missingCount = 0;
      foreach (var g in go)
      {
        RemoveMissingScriptsInGO(g, ref goCount, ref componentsCount, ref missingCount);
      }

      Debug.Log(string.Format("Searched {0} GameObjects, {1} components, removed {2} missing scripts", goCount, componentsCount, missingCount));
    }

    private static void RemoveMissingScriptsInGO(GameObject g, ref int goCount, ref int componentsCount, ref int missingCount)
    {
      goCount++;
      // We must use the GetComponents array to actually detect missing components
      var components = g.GetComponents<Component>();

      // Create a serialized object so that we can edit the component list
      var serializedObject = new SerializedObject(g);
      // Find the component list property
      var prop = serializedObject.FindProperty("m_Component");

      // Track how many components we've removed
      int r = 0;

      // Iterate over all components
      for (int j = 0; j < components.Length; j++)
      {
        componentsCount++;
        // Check if the ref is null
        if (components[j] == null)
        {
          missingCount++;
          // If so, remove from the serialized component array
          prop.DeleteArrayElementAtIndex(j - r);
          // Increment removed count
          r++;
        }
      }

      // Apply our changes to the game object
      serializedObject.ApplyModifiedProperties();


      foreach (Transform childT in g.transform)
      {
        RemoveMissingScriptsInGO(childT.gameObject, ref goCount, ref componentsCount, ref missingCount);
      }
    }

    [MenuItem("Tools/Find Missing Scripts (On Selected GameObject)")]
    public static void FindInSelected()
    {
      GameObject[] go = Selection.gameObjects;
      int goCount = 0;
      int componentsCount = 0;
      int missingCount = 0;
      foreach (GameObject g in go)
      {
        FindInGO(g, ref goCount, ref componentsCount, ref missingCount);
      }
      Debug.Log(string.Format("Searched {0} GameObjects, {1} components, found {2} missing", goCount, componentsCount, missingCount));
    }

    private static void FindInGO(GameObject g, ref int goCount, ref int componentsCount, ref int missingCount)
    {
      goCount++;
      Component[] components = g.GetComponents<Component>();
      for (int i = 0; i < components.Length; i++)
      {
        componentsCount++;
        if (components[i] == null)
        {
          missingCount++;
          string name = g.name;
          Transform t = g.transform;
          while (t.parent != null)
          {
            name = t.parent.name + "/" + name;
            t = t.parent;
          }
          Debug.Log(name + " has an empty script attached in position: " + i, g);
        }
      }
      foreach (Transform childT in g.transform)
      {
        FindInGO(childT.gameObject, ref goCount, ref componentsCount, ref missingCount);
      }
    }

    public static string RelativePath(string absPath, string relTo)
    {
      absPath = absPath.Replace("\\", "/");
      relTo = relTo.Replace("\\", "/");
      string[] absDirs = absPath.Split('/');
      string[] relDirs = relTo.Split('/');
      // Get the shortest of the two paths 
      int len = absDirs.Length < relDirs.Length ? absDirs.Length : relDirs.Length;
      // Use to determine where in the loop we exited 
      int lastCommonRoot = -1; int index;
      // Find common root 
      for (index = 0; index < len; index++)
      {
        if (absDirs[index] == relDirs[index])
          lastCommonRoot = index;
        else break;
      }
      // If we didn't find a common prefix then throw 
      if (lastCommonRoot == -1)
      {
        throw new ArgumentException("Paths do not have a common base");
      }
      // Build up the relative path 
      StringBuilder relativePath = new StringBuilder();
      // Add on the .. 
      for (index = lastCommonRoot + 1; index < absDirs.Length; index++)
      {
        if (absDirs[index].Length > 0) relativePath.Append("../");
      }
      // Add on the folders 
      for (index = lastCommonRoot + 1; index < relDirs.Length - 1; index++)
      {
        relativePath.Append(relDirs[index] + "/");
      }
      relativePath.Append(relDirs[relDirs.Length - 1]);
      return relativePath.ToString();
    }
  }
}
