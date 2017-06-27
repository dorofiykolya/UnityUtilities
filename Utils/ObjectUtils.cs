using System;
using System.Collections;
using System.Reflection;

namespace Utils
{
  public class ObjectUtils
  {
    public static string Inspect(object value, bool asJson = false, bool includePrivate = true, int maxInspectIndent = 20)
    {
      if (asJson)
      {
        return InspectAsJson(value, includePrivate, 0, maxInspectIndent);
      }
      return Inspect(value, "", null, includePrivate, 0, maxInspectIndent);
    }

    private static string InspectAsJson(object obj, bool includePrivate, int indent, int maxInspectIndent)
    {
      if (indent >= maxInspectIndent) return "MAX_INSPECT_INDENT:" + maxInspectIndent + ", value:" + obj;
      if (obj == null)
      {
        return "null";
      }
      string result = "";
      Type type;
      if (obj is Type)
      {
        type = obj as Type;
      }
      else
      {
        type = obj.GetType();
      }
      if (type.IsValueType || obj is String || obj is Boolean || obj is int)
      {
        var isString = obj is string;
        if (isString) result += "\"";
        result += Convert.ToString(obj);
        if (isString) result += "\"";
      }
      else
      {
        if (obj is Array)
        {
          result += "[";
          var array = obj as Array;
          for (int i = 0; i < array.Length; i++)
          {
            if (i != 0) result += ",";
            result += InspectAsJson(array.GetValue(i), includePrivate, ++indent, maxInspectIndent);
          }
          result += "]";
        }
        else if (obj is IDictionary)
        {
          var dict = obj as IDictionary;
          ICollection keys = dict.Keys;
          result += "{";
          var index = 0;
          foreach (var key in keys)
          {
            if (index != 0) result += ",";
            index++;

            result += "{";
            result += InspectAsJson(key, includePrivate, ++indent, maxInspectIndent);
            result += ":";
            result += InspectAsJson(dict[key], includePrivate, ++indent, maxInspectIndent);
            result += "}";
          }
          result += "}";
        }
        else if (obj is ICollection)
        {
          var collection = obj as ICollection;
          var i = 0;
          result += "[";
          foreach (var key in collection)
          {
            if (i != 0) result += ",";
            result += InspectAsJson(key, includePrivate, ++indent, maxInspectIndent);
            i++;
          }
          result += "]";
        }
        else
        {
          var fieldFlag = BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetField;
          if (includePrivate) fieldFlag |= BindingFlags.NonPublic;

          FieldInfo[] fields = type.GetFields(fieldFlag);
          result += "{";
          var index = 0;
          foreach (var fieldInfo in fields)
          {
            if (index != 0) result += ",";
            index++;
            result += InspectAsJson(Convert.ToString(fieldInfo.Name), includePrivate, ++indent, maxInspectIndent);
            result += ":";
            result += InspectAsJson(Safe(() => fieldInfo.GetValue(obj)), includePrivate, ++indent, maxInspectIndent);
          }

          var propertyFlag = BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty;
          if (includePrivate) propertyFlag |= BindingFlags.NonPublic;

          PropertyInfo[] property = type.GetProperties(propertyFlag);
          index = 0;
          foreach (var propertyInfo in property)
          {
            if (index != 0) result += ",";
            index++;
            result += InspectAsJson(Convert.ToString(propertyInfo.Name), includePrivate, ++indent, maxInspectIndent);
            result += ":";
            result += InspectAsJson(Safe(() => propertyInfo.GetValue(obj, null)), includePrivate, ++indent, maxInspectIndent);
          }
          result += "}";
        }
      }
      return result;
    }

    private static string Inspect(object obj, string space, string objKey, bool includePrivate, int indent, int maxInspectIndent)
    {
      if (indent >= maxInspectIndent) return "MAX_INSPECT_INDENT:" + maxInspectIndent + ", value:" + obj;
      if (obj == null)
      {
        if (objKey != null)
        {
          return space + "#" + objKey + ": null";
        }
        return space + "null";
      }
      string result = space;
      Type type;
      if (obj is Type)
      {
        type = obj as Type;
      }
      else
      {
        type = obj.GetType();
      }
      if (objKey != null)
      {
        result += "#" + objKey + ": ";
      }
      result += type.Name + " ";
      if (type.IsValueType || obj is String || obj is Boolean || obj is int)
      {
        result += Convert.ToString(obj);
      }
      else
      {
        if (obj is Array)
        {
          result += "\n";
          var array = obj as Array;
          for (int i = 0; i < array.Length; i++)
          {
            result += Inspect(array.GetValue(i), space + "   ", Convert.ToString(i), includePrivate, ++indent, maxInspectIndent);
            result += "\n";
          }
        }
        else if (obj is IDictionary)
        {
          var dict = obj as IDictionary;
          ICollection keys = dict.Keys;
          result += "\n";
          foreach (var key in keys)
          {
            result += Inspect(dict[key], space + "   ", Convert.ToString(key), includePrivate, ++indent, maxInspectIndent);
            result += "\n";
          }
        }
        else if (obj is ICollection)
        {
          var collection = obj as ICollection;
          var i = 0;
          result += "\n";
          foreach (var key in collection)
          {
            result += Inspect(key, space + "   ", Convert.ToString(i), includePrivate, ++indent, maxInspectIndent);
            result += "\n";
            i++;
          }
        }
        else
        {
          var fieldFlag = BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetField;
          if (includePrivate) fieldFlag |= BindingFlags.NonPublic;

          FieldInfo[] fields = type.GetFields(fieldFlag);
          result += "\n";
          foreach (var fieldInfo in fields)
          {
            result += Inspect(fieldInfo.GetValue(obj), space + "   ", Convert.ToString(fieldInfo.Name), includePrivate, ++indent, maxInspectIndent);
            result += "\n";
          }

          var propertyFlag = BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty;
          if (includePrivate) propertyFlag |= BindingFlags.NonPublic;

          PropertyInfo[] property = type.GetProperties(propertyFlag);
          foreach (var propertyInfo in property)
          {
            result += Inspect(propertyInfo.GetValue(obj, null), space + "   ", Convert.ToString(propertyInfo.Name), includePrivate, ++indent, maxInspectIndent);
            result += "\n";
          }
        }
      }
      return result;
    }

    private static object Safe(Func<object> func)
    {
      try
      {
        return func();
      }
      catch (Exception e)
      {
        return e;
      }
    }
  }
}