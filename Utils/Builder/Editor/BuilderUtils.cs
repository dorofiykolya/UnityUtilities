using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace Utils.BuildPipeline
{
  public class BuilderUtils
  {
    private const string CommandStartCharacter = "-";

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
            string commandName = (string) info.GetValue(globalType);
            if (!args.Contains(commandName))
            {
              requiredArgs.Add(commandName);
            }
            else
            {
              if (strict)
              {
                var value = args[commandName];
                if (attr.RequiredValue && attr.AvailableValues != null && attr.AvailableValues.All(v => v != value))
                {
                  invalidValues.Add(invalidValues.Count + ": " +
                                    GetMessageNotValidArgs(commandName, args, attr.AvailableValues));
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
          var availableValues = attr.AvailableValues != null && attr.AvailableValues.Length != 0
            ? '<' + string.Join("|", attr.AvailableValues) + '>'
            : "";
          var required = attr.Required ? "(required)" : "";
          var requiredValue = attr.RequiredValue ? "(required value)" : "";
          builder.Append("".PadLeft(spaces));
          builder.AppendFormat(CommandStartCharacter + "{0} {1} {4} -- {3} {2} ", info.GetValue(type), required, attr.Description ?? "",
            availableValues, requiredValue);
          builder.AppendLine();
        }
      }

      return builder.ToString();
    }

    public static bool TryGetScriptingBackend(Arguments args, BuildTargetGroup targetGroup,
      out ScriptingImplementation result)
    {
      if (args.Contains(BuilderArguments.ScriptingBackend))
      {
        string scriptingBackendValue = args[BuilderArguments.ScriptingBackend];
        ScriptingImplementation enumValue;
        if (TryParse(scriptingBackendValue, out enumValue) &&
            (enumValue == ScriptingImplementation.IL2CPP || enumValue == ScriptingImplementation.Mono2x ||
             enumValue == ScriptingImplementation.WinRTDotNET))
        {
          if (enumValue == ScriptingImplementation.WinRTDotNET && targetGroup != BuildTargetGroup.WSA)
          {
            throw new ArgumentException(string.Format("invalid '{0}' command value '{1}' by BuildTargetGroup '{2}'",
              BuilderArguments.ScriptingBackend, scriptingBackendValue, targetGroup.ToString()));
          }

          result = enumValue;
          return true;
        }

        throw new ArgumentException(string.Format("invalid '{0}' command value:'{1}'",
          BuilderArguments.ScriptingBackend, scriptingBackendValue));
      }

      result = ScriptingImplementation.Mono2x;
      return false;
    }

    public static string PadLeftLines(string message, int spaces)
    {
      var builder = new StringBuilder();
      var reader = new StringReader(message);
      string line;
      bool first = true;
      while ((line = reader.ReadLine()) != null)
      {
        if (!first)
        {
          builder.AppendLine();
        }
        first = false;
        builder.Append("".PadLeft(spaces));
        builder.Append(line);
      }

      return builder.ToString();
    }

    public static bool TryParse<TEnum>(string value, out TEnum result) where TEnum : struct
    {
      try
      {
        result = (TEnum) Enum.Parse(typeof(TEnum), value);
      }
      catch
      {
        result = default(TEnum);
        return false;
      }

      return true;
    }

    public static bool TryParse(Type enumType, string value, out object result)
    {
      try
      {
        result = Enum.Parse(enumType, value);
      }
      catch
      {
        result = 0;
        return false;
      }

      return true;
    }

    public static bool TryParse(string input, out Version result)
    {
      return VersionUtils.TryParse(input, out result);
    }

    private static List<FieldInfo> GetConstants(Type type)
    {
      FieldInfo[] fieldInfos =
        type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy);

      return fieldInfos.Where(fi => fi.IsLiteral && !fi.IsInitOnly).ToList();
    }

    public class VersionUtils
    {
      private static readonly char[] SeparatorsArray = new char[1]
      {
        '.'
      };

      public static bool TryParse(string input, out Version result)
      {
        VersionUtils.VersionResult result1 = new VersionUtils.VersionResult();
        result1.Init("input", false);
        bool version = VersionUtils.TryParseVersion(input, ref result1);
        result = result1.m_parsedVersion;
        return version;
      }

      private static bool TryParseVersion(string version, ref VersionUtils.VersionResult result)
      {
        if (version == null)
        {
          result.SetFailure(VersionUtils.ParseFailureKind.ArgumentNullException);
          return false;
        }

        string[] strArray = version.Split(VersionUtils.SeparatorsArray);
        int length = strArray.Length;
        if (length < 2 || length > 4)
        {
          result.SetFailure(VersionUtils.ParseFailureKind.ArgumentException);
          return false;
        }

        int parsedComponent1;
        int parsedComponent2;
        if (!VersionUtils.TryParseComponent(strArray[0], "version", ref result, out parsedComponent1) ||
            !VersionUtils.TryParseComponent(strArray[1], "version", ref result, out parsedComponent2))
          return false;
        int num = length - 2;
        if (num > 0)
        {
          int parsedComponent3;
          if (!VersionUtils.TryParseComponent(strArray[2], "build", ref result, out parsedComponent3))
            return false;
          if (num - 1 > 0)
          {
            int parsedComponent4;
            if (!VersionUtils.TryParseComponent(strArray[3], "revision", ref result, out parsedComponent4))
              return false;
            result.m_parsedVersion =
              new Version(parsedComponent1, parsedComponent2, parsedComponent3, parsedComponent4);
          }
          else
            result.m_parsedVersion = new Version(parsedComponent1, parsedComponent2, parsedComponent3);
        }
        else
          result.m_parsedVersion = new Version(parsedComponent1, parsedComponent2);

        return true;
      }

      private static bool TryParseComponent(string component, string componentName,
        ref VersionUtils.VersionResult result, out int parsedComponent)
      {
        if (!int.TryParse(component, NumberStyles.Integer, (IFormatProvider) CultureInfo.InvariantCulture,
          out parsedComponent))
        {
          result.SetFailure(VersionUtils.ParseFailureKind.FormatException, component);
          return false;
        }

        if (parsedComponent >= 0)
          return true;
        result.SetFailure(VersionUtils.ParseFailureKind.ArgumentOutOfRangeException, componentName);
        return false;
      }

      internal enum ParseFailureKind
      {
        ArgumentNullException,
        ArgumentException,
        ArgumentOutOfRangeException,
        FormatException,
      }

      internal struct VersionResult
      {
        internal Version m_parsedVersion;
        internal VersionUtils.ParseFailureKind m_failure;
        internal string m_exceptionArgument;
        internal string m_argumentName;
        internal bool m_canThrow;

        internal void Init(string argumentName, bool canThrow)
        {
          this.m_canThrow = canThrow;
          this.m_argumentName = argumentName;
        }

        internal void SetFailure(VersionUtils.ParseFailureKind failure)
        {
          this.SetFailure(failure, string.Empty);
        }

        internal void SetFailure(VersionUtils.ParseFailureKind failure, string argument)
        {
          this.m_failure = failure;
          this.m_exceptionArgument = argument;
          if (this.m_canThrow)
            throw this.GetVersionParseException();
        }

        internal Exception GetVersionParseException()
        {
          switch (this.m_failure)
          {
            case VersionUtils.ParseFailureKind.ArgumentNullException:
              return (Exception) new ArgumentNullException(this.m_argumentName);
            case VersionUtils.ParseFailureKind.ArgumentException:
              return (Exception) new ArgumentException("Arg_VersionString");
            case VersionUtils.ParseFailureKind.ArgumentOutOfRangeException:
              return (Exception) new ArgumentOutOfRangeException(this.m_exceptionArgument,
                "ArgumentOutOfRange_Version");
            case VersionUtils.ParseFailureKind.FormatException:
              try
              {
                int.Parse(this.m_exceptionArgument, (IFormatProvider) CultureInfo.InvariantCulture);
              }
              catch (FormatException ex)
              {
                return (Exception) ex;
              }
              catch (OverflowException ex)
              {
                return (Exception) ex;
              }

              return (Exception) new FormatException("Format_InvalidString");
            default:
              return (Exception) new ArgumentException("Arg_VersionString");
          }
        }
      }
    }
  }
}
