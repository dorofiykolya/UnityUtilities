using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine.Assertions;

namespace Utils.BuildPipeline
{
  public class Arguments
  {
    public delegate void ExistKey(string key, string value);

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

    public void OnExist(string key, ExistKey callback)
    {
      if (Contains(key))
      {
        callback(key, this[key]);
      }
    }

    public void SetStaticPropertiesFromFiels<T>(object fromObjectFields, bool checkContainsArgs = true) where T : class
    {
      Assert.IsNotNull(fromObjectFields);
      var valueType = fromObjectFields.GetType();

      Assert.IsTrue(valueType.IsClass);
      Assert.IsFalse(valueType.IsPrimitive);
      Assert.IsFalse(valueType.IsValueType);
      Assert.IsTrue(valueType != typeof(string));

      var staticType = typeof(T);
      Assert.IsTrue(staticType.IsClass);
      Assert.IsFalse(staticType.IsPrimitive);
      Assert.IsFalse(staticType.IsValueType);

      var fields = valueType.GetFields(BindingFlags.Instance | BindingFlags.GetField | BindingFlags.Public).ToDictionary(f => f.Name);

      var properties = staticType.GetProperties(BindingFlags.Static | BindingFlags.SetProperty | BindingFlags.Public);
      foreach (var propertyInfo in properties)
      {
        if (!checkContainsArgs || Contains(propertyInfo.Name))
        {
          if (fields.ContainsKey(propertyInfo.Name))
          {
            propertyInfo.SetValue(staticType, fields[propertyInfo.Name].GetValue(fromObjectFields), null);
          }
        }
      }
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

      var requiredKeys = string.Join(", ", required.ToArray());
      var message = "command line build required arguments:{0}";

      Assert.IsTrue(required.Count == 0, string.Format(message, requiredKeys));
    }

    public bool GetAsBool(string key)
    {
      bool boolResult = true;
      var boolValue = this[key];
      if (!string.IsNullOrEmpty(boolValue))
      {
        boolValue = boolValue.Trim();
        if (boolValue.Length != 0)
        {
          if (boolValue.ToLower() == "false") boolResult = false;
          if (boolValue.ToLower() == "0") boolResult = false;
          if (boolValue.ToLower() == "null") boolResult = false;
          if (boolValue.ToLower() == "no") boolResult = false;
          if (boolValue.ToLower() == "none") boolResult = false;
        }
      }

      return boolResult;
    }

    public T Fill<T>() where T : new()
    {
      var args = this;
      var type = typeof(T);
      var result = new T();
      var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetField | BindingFlags.SetField);
      foreach (var field in fields)
      {
        if (args.Contains(field.Name))
        {
          if (field.FieldType.IsPrimitive)
          {
            if (field.FieldType == typeof(bool))
            {
              bool boolResult = GetAsBool(field.Name);
              field.SetValue(result, boolResult);
            }
            else
            {
              field.SetValue(result, Convert.ChangeType(args[field.Name], field.FieldType));
            }
          }
          else if (field.FieldType == typeof(string))
          {
            field.SetValue(result, args[field.Name]);
          }
          else if (field.FieldType.IsEnum)
          {
            object enumValue = Enum.Parse(field.FieldType, args[field.Name]);
            field.SetValue(result, enumValue);
          }
          else
          {
            throw new ArgumentException(string.Format("type {0} not supported", field.FieldType));
          }
        }
      }
      return result;
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
      if (BuilderUtils.TryParse<T>(value, out enumValue))
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
