using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Utils.BuildPipeline
{
  public class ScriptingDefines
  {
    private static readonly char[] DefineSplits = new char[3]
    {
      ';',
      ',',
      ' '
    };

    private readonly BuildTargetGroup _targetGroup;
    private readonly bool _verbose;

    public ScriptingDefines(BuildTargetGroup targetGroup, bool verbose = false)
    {
      _targetGroup = targetGroup;
      _verbose = verbose;
    }

    public bool Add(string define)
    {
      var defines = GetDefines();
      var result = defines.Add(define);
      if (result)
      {
        if (_verbose)
        {
          Debug.Log("Add scripting define: " + define);
          Debug.Log("Defines: " + string.Join(";", defines));
        }
        Apply(defines);
      }

      return result;
    }

    public bool Remove(string define)
    {
      var defines = GetDefines();
      var result = defines.Remove(define);
      if (result)
      {
        if (_verbose)
        {
          Debug.Log("Remove scripting define: " + define);
          Debug.Log("Defines: " + string.Join(";", defines));
        }
        Apply(defines);
      }

      return result;
    }

    public bool Contains(string define)
    {
      var defines = GetDefines();
      return defines.Contains(define);
    }

    private HashSet<string> GetDefines()
    {
      var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(_targetGroup);

      var hashSet = new HashSet<string>();
      var splited = defines.Split(DefineSplits, StringSplitOptions.RemoveEmptyEntries);
      foreach (var item in splited)
      {
        hashSet.Add(item);
      }

      return hashSet;
    }

    private void Apply(HashSet<string> defines)
    {
      PlayerSettings.SetScriptingDefineSymbolsForGroup(_targetGroup, string.Join(";", defines));
    }
  }
}
