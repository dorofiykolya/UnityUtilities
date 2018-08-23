using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace Utils.BuildPipeline
{
  public class BuilderUtils
  {
    private const char CommandStartCharacter = '-';

    public static Arguments GetCommandLineArguments(string[] args)
    {
      Dictionary<string, string> commandToValueDictionary = new Dictionary<string, string>();

      for (int i = 0; i < args.Length; i++)
      {
        if (args[i].StartsWith(CommandStartCharacter.ToString()))
        {
          string command = args[i];
          string value = string.Empty;

          if (i < args.Length - 1 && !args[i + 1].StartsWith(CommandStartCharacter.ToString()))
          {
            value = args[i + 1];
            i++;
          }

          if (!commandToValueDictionary.ContainsKey(command))
          {
            commandToValueDictionary.Add(command, value);
          }
          else
          {
            Debug.Log("Duplicate command line argument " + command);
          }
        }
      }

      return new Arguments(commandToValueDictionary, BuilderArguments.Verbose);
    }

    public static Arguments GetCommandLineArguments()
    {
      string[] args = Environment.GetCommandLineArgs();
      return GetCommandLineArguments(args);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="args"></param>
    /// <param name="strict">check values</param>
    public static void AssertRequiredArguments<T>(Arguments args, bool strict)
    {
      var globalType = typeof(T);
      var consts = GetConstants(globalType);
      var requiredArgs = new List<string>();
      var invalidValues = new List<string>();
      foreach (var info in consts)
      {
        var attr = info.GetCustomAttributes(typeof(KeyAttribute), false).Cast<KeyAttribute>().FirstOrDefault();
        if (attr != null)
        {
          if (attr.Required)
          {
            string commandName = (string)info.GetValue(globalType);
            if (!args.Contains(commandName))
            {
              requiredArgs.Add(commandName);
            }
            else
            {
              if (strict)
              {
                var value = args[commandName];
                if (attr.AvailableValues.All(v => v != value))
                {
                  invalidValues.Add(invalidValues.Count + ": " + GetMessageNotValidArgs(commandName, args, attr.AvailableValues));
                }
              }
            }
          }
        }
      }
      args.AssertKeys(requiredArgs.ToArray());
      Assert.IsTrue(invalidValues.Count == 0, string.Join(Environment.NewLine, invalidValues.ToArray()));
    }

    public static string GetMessageNotValidArgs(string key, Arguments args, string[] expected = null)
    {
      var result = "command line not valid arg: '" + key + "', current not valid value: '" + args[key] + "'";
      if (expected != null && expected.Length != 0)
      {
        result += ", expected value: " + string.Join("|", expected);
      }
      return result;
    }

    public static string GetArgumentsDescription(Type type, int spaces = 0)
    {
      var consts = GetConstants(type);
      var builder = new StringBuilder();
      foreach (var info in consts)
      {
        var attr = info.GetCustomAttributes(typeof(KeyAttribute), false).Cast<KeyAttribute>().FirstOrDefault();
        if (attr != null)
        {
          var availableValues = attr.AvailableValues != null && attr.AvailableValues.Length != 0 ? '<' + string.Join("|", attr.AvailableValues) + '>' : "";
          var required = attr.Required ? "(required)" : "";
          var requiredValue = attr.RequiredValue ? "(required value)" : "";
          builder.Append("".PadLeft(spaces));
          builder.AppendFormat("{0} {1} {4} -- {3} {2} ", info.GetValue(type), required, attr.Description ?? "", availableValues, requiredValue);
          builder.AppendLine();
        }
      }
      return builder.ToString();
    }

    public static bool TryGetScriptingBackend(Arguments args, BuildTargetGroup targetGroup, out ScriptingImplementation result)
    {
      if (args.Contains(BuilderArguments.ScriptingBackend))
      {
        string scriptingBackendValue = args[BuilderArguments.ScriptingBackend];
        ScriptingImplementation enumValue;
        if (Enum.TryParse(scriptingBackendValue, out enumValue) && (enumValue == ScriptingImplementation.IL2CPP || enumValue == ScriptingImplementation.Mono2x || enumValue == ScriptingImplementation.WinRTDotNET))
        {
          if (enumValue == ScriptingImplementation.WinRTDotNET && targetGroup != BuildTargetGroup.WSA)
          {
            throw new ArgumentException(string.Format("invalid '{0}' command value '{1}' by BuildTargetGroup '{2}'", BuilderArguments.ScriptingBackend, scriptingBackendValue, targetGroup.ToString()));
          }
          result = enumValue;
          return true;
        }

        throw new ArgumentException(string.Format("invalid '{0}' command value:'{1}'", BuilderArguments.ScriptingBackend, scriptingBackendValue));
      }

      result = ScriptingImplementation.Mono2x;
      return false;
    }

    private static List<FieldInfo> GetConstants(Type type)
    {
      FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

      return fieldInfos.Where(fi => fi.IsLiteral && !fi.IsInitOnly).ToList();
    }
  }
}
