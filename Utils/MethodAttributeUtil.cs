using System;
using System.Collections.Generic;
using System.Reflection;

namespace Utils
{
  public class MethodAttributeUtil
  {
    public static MethodInfo[] GetMethods(Type type, Type attributeType)
    {
      Assert2.IsSubclassOf(attributeType, typeof(Attribute));

      var result = new List<MethodInfo>();
      var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      foreach (var method in methods)
      {
        var attributes = method.GetCustomAttributes(attributeType, true);
        if (attributes.Length > 0)
        {
          result.Add(method);
        }
      }
      return result.ToArray();
    }
  }
}