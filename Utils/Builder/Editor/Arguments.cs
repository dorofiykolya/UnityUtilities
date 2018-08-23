using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Assertions;

namespace Utils.BuildPipeline
{
  public class Arguments
  {
    private readonly Dictionary<string, string> _map;
    private readonly string _verboseKey;

    public Arguments(Dictionary<string, string> map, string verboseKey)
    {
      _map = map;
      _verboseKey = verboseKey;
    }

    public bool IsVerbose
    {
      get { return Contains(_verboseKey); }
      set
      {
        if (value) _map[_verboseKey] = "true";
        else _map.Remove(_verboseKey);
      }
    }

    public string this[string key]
    {
      get { return _map[key]; }
      set { _map[key] = value; }
    }

    public bool Remove(string key)
    {
      return _map.Remove(key);
    }

    public bool Contains(string key)
    {
      return _map.ContainsKey(key);
    }

    public void AssertKey(string key, string message = null)
    {
      if (message == null)
      {
        message = "command line build required argument:{0}";
      }
      Assert.IsTrue(_map.ContainsKey(key), string.Format(message, key));
    }

    public void AssertKeys(params string[] keys)
    {
      var required = new List<string>();
      foreach (var key in keys)
      {
        if (!_map.ContainsKey(key))
        {
          required.Add(key);
        }
      }

      var requiredKeys = string.Join(", ", required);
      var message = "command line build required arguments:{0}";

      Assert.IsTrue(required.Count == 0, string.Format(message, requiredKeys));
    }

    public override string ToString()
    {
      var builder = new StringBuilder();

      foreach (var pair in _map)
      {
        builder.Append(pair.Key);

        if (!string.IsNullOrEmpty(pair.Value))
        {
          builder.Append('=');
          builder.Append(pair.Value);
        }

        builder.AppendLine();
      }

      return builder.ToString();
    }

    public string ToString(string format)
    {
      var builder = new StringBuilder();

      foreach (var pair in _map)
      {
        builder.AppendFormat(format, pair.Key, pair.Value);
      }

      return builder.ToString();
    }

    public T GetValueByEnum<T>(string key) where T : struct
    {
      string value = this[key];
      T enumValue;
      if (Enum.TryParse<T>(value, out enumValue))
      {
        return enumValue;
      }

      throw new ArgumentException(string.Format("invalid '{0}' command value:'{1}'", key, value));
    }

    public bool TryGetValueByEnum<T>(string key, out T result) where T : struct
    {
      if (Contains(key))
      {
        result = GetValueByEnum<T>(key);
        return true;
      }

      result = default(T);
      return false;
    }
  }
}
