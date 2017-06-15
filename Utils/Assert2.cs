using System;

namespace Utils
{
  public static class Assert2
  {
    public static void NotNull(object value, string message = null)
    {
      if (value == null)
      {
        if (string.IsNullOrEmpty(message))
        {
          message = "this argument is required; it must not null";
        }
        throw new ArgumentNullException(message);
      }
    }

    public static void IsSubclassOf(Type type, Type parentType, string message = null)
    {
      NotNull(type);
      NotNull(parentType);
      if (!type.IsSubclassOf(parentType))
      {
        if (string.IsNullOrEmpty(message))
        {
          message = string.Format("{0}, , this argument is not a subclass of {1}", type, parentType);
        }
        throw new ArgumentException(message);
      }
    }

    public static void IsTrue(bool expression, string message = null)
    {
      if (!expression)
      {
        if (string.IsNullOrEmpty(message))
        {
          message = "this expression must be true";
        }
        throw new ArgumentException(message);
      }
    }

    public static void HasText(string text, string message = null)
    {
      if (String.IsNullOrEmpty(text))
      {
        if (string.IsNullOrEmpty(message))
        {
          message = "this String argument must have text; it must not be null, empty or blank";
        }
        throw new ArgumentException(message);
      }
    }

    public static void IsInstanceOf(object value, Type type, string message = null)
    {
      NotNull(value);
      NotNull(type);
      if (value == null || !(value.GetType().IsSubclassOf(type)) && value.GetType() != type)
      {
        if (string.IsNullOrEmpty(message))
        {
          message = "this argument is not of type \"" + type + "\"";
        }
        throw new ArgumentException(message);
      }
    }
  }
}
